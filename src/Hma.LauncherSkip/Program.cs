using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Hma.LauncherSkip
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                return RunInteractive();
            }
            catch (Exception error)
            {
                Console.WriteLine();
                WriteColor(error.Message, ConsoleColor.Red, error: true);
                return 1;
            }
            finally
            {
                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine();
                    WriteColor("Press Enter to exit...", ConsoleColor.White, newLine: false);
                    Console.ReadLine();
                }
            }
        }

        private static int RunInteractive()
        {
            PrintHeader();
            string detected = FindGame();
            string path = null;
            if (detected != null)
            {
                WriteColor("Detected: " + detected, ConsoleColor.Gray);
                bool? useDetected = Confirm("Use detected location? [y/n]: ");
                if (useDetected == null) return 0;
                if (useDetected == true) path = detected;
            }
            if (path == null)
            {
                if (detected != null) Console.WriteLine();
                WriteColor("Path to HMA.exe: ", ConsoleColor.Yellow, newLine: false);
                path = (Console.ReadLine() ?? "").Trim().Trim('"');
                if (path.Length == 0) return 0;
            }

            bool patched = LauncherPatch.IsPatched(path);
            Console.WriteLine();
            Console.WriteLine(patched ? "HMA.exe is already patched." : "HMA.exe is not patched.");
            bool confirmed = Confirm(patched ? "Revert the patch? [y/n]: " : "Patch it? [y/n]: ") == true;
            Console.WriteLine();
            if (confirmed)
                WriteColor(LauncherPatch.Change(path, !patched), ConsoleColor.Green);
            else
                WriteColor("Cancelled.", ConsoleColor.DarkGray);
            return 0;
        }

        private static bool? Confirm(string prompt)
        {
            while (true)
            {
                WriteColor(prompt, ConsoleColor.Yellow, newLine: false);
                string answer = Console.ReadLine();
                if (answer == null) return null;
                answer = answer.Trim();
                if (answer.Equals("y", StringComparison.OrdinalIgnoreCase) || answer.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (answer.Equals("n", StringComparison.OrdinalIgnoreCase) || answer.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        private static string FindGame()
        {
            foreach (string directory in new[] { Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory })
            {
                string local = Path.Combine(directory, "HMA.exe");
                if (File.Exists(local)) return Path.GetFullPath(local);
            }

            var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string steam = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            try
            {
                steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string ?? steam;
                libraries.Add(steam);
                string config = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
                if (File.Exists(config))
                    foreach (Match match in Regex.Matches(File.ReadAllText(config), "\"path\"\\s*\"([^\"]+)\""))
                        libraries.Add(match.Groups[1].Value.Replace(@"\\", @"\"));
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is SecurityException || error is ArgumentException)
            {
                libraries.Add(steam);
            }

            foreach (string library in libraries)
            {
                try
                {
                    string manifest = Path.Combine(library, "steamapps", "appmanifest_203140.acf");
                    if (!File.Exists(manifest)) continue;
                    Match install = Regex.Match(File.ReadAllText(manifest), "\"installdir\"\\s*\"([^\"]+)\"");
                    if (!install.Success) continue;
                    string game = Path.Combine(library, "steamapps", "common", install.Groups[1].Value, "HMA.exe");
                    if (File.Exists(game)) return Path.GetFullPath(game);
                }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is SecurityException || error is ArgumentException)
                {
                    // Try the next library; manual entry is always available.
                }
            }
            return null;
        }

        private static void PrintHeader()
        {
            string version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion.Split('+')[0];
            WriteColor("Hitman: Absolution launcher skip | V" + version + " | By Noklef", ConsoleColor.Cyan);
            Console.WriteLine();
        }

        private static void WriteColor(string text, ConsoleColor color, bool newLine = true, bool error = false)
        {
            bool redirected = error ? Console.IsErrorRedirected : Console.IsOutputRedirected;
            ConsoleColor previous = redirected ? default(ConsoleColor) : Console.ForegroundColor;
            try
            {
                if (!redirected) Console.ForegroundColor = color;
                TextWriter output = error ? Console.Error : Console.Out;
                output.Write(text + (newLine ? Environment.NewLine : ""));
            }
            finally { if (!redirected) Console.ForegroundColor = previous; }
        }
    }
}
