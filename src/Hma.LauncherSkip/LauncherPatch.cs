using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hma.LauncherSkip
{
    internal static class LauncherPatch
    {
        private static int FindSetting(byte[] data)
        {
            MatchCollection matches = Regex.Matches(Encoding.ASCII.GetString(data),
                @"(?<=\nEnableSplashScreen=)[01](?=\r?\nBOOT_MOVIE=)");
            if (matches.Count != 1)
                throw new InvalidDataException(matches.Count == 0
                    ? "The launcher setting was not found in the expected configuration. No changes were made."
                    : "Multiple launcher settings were found. No changes were made.");
            return matches[0].Index;
        }

        internal static bool IsPatched(string path)
        {
            byte[] data = File.ReadAllBytes(ExecutablePath(path));
            return data[FindSetting(data)] == '0';
        }

        private static string ExecutablePath(string path)
        {
            path = Path.GetFullPath(path);
            return Directory.Exists(path) ? Path.Combine(path, "HMA.exe") : path;
        }

        internal static string Change(string path, bool skip)
        {
            path = ExecutablePath(path);
            byte[] data = File.ReadAllBytes(path);
            int setting = FindSetting(data);
            if ((data[setting] == '0') == skip) return skip ? "Executable already patched." : "Executable already unpatched.";

            string backup = path + ".bak";
            byte[] original = null;
            if (!skip || File.Exists(backup))
            {
                original = File.ReadAllBytes(backup);
                if (original[FindSetting(original)] != '1')
                    throw new InvalidDataException("The backup does not have its launcher enabled. No changes were made: " + backup);
            }

            if (!skip)
            {
                data[setting] = (byte)'1';
                if (!data.SequenceEqual(original))
                    throw new InvalidDataException("The backup does not match this executable apart from the launcher setting. No changes were made: " + backup);
                File.Replace(backup, path, null);
                return "HMA.exe restored and the old backup has been deleted.";
            }

            data[setting] = (byte)'0';
            string temporary = path + ".launcher-" + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllBytes(temporary, data);
                File.Replace(temporary, path, backup);
                return "Executable patched." + Environment.NewLine + "Original backup: " + Path.GetFileName(backup);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
