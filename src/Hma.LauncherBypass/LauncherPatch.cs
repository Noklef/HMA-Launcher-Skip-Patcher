using System;
using System.IO;
using System.Security.Cryptography;

namespace Hma.LauncherBypass
{
    internal static class LauncherPatch
    {
        private const int SettingOffset = 0xA7AE3A;
        private const string SupportedHash = "7655c1db63607612e358d2142241b96fc2c92db5d7f5fa7b290a43ec7764d9d6";

        private static string Hash(byte[] data)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
        }

        private static bool IsPatched(byte[] data)
        {
            if (data.Length <= SettingOffset || (data[SettingOffset] != '0' && data[SettingOffset] != '1'))
                throw new InvalidDataException("Unsupported executable. No changes were made.");

            byte[] normalized = (byte[])data.Clone();
            normalized[SettingOffset] = (byte)'1';
            string normalizedHash = Hash(normalized);
            if (normalizedHash != SupportedHash)
                throw new InvalidDataException("Unrecognized executable. No changes were made." + Environment.NewLine +
                    "Expected SHA-256 (launcher enabled): " + SupportedHash + Environment.NewLine +
                    "Found SHA-256 (launcher enabled):    " + normalizedHash);

            return data[SettingOffset] == '0';
        }

        internal static bool IsPatched(string path)
        {
            return IsPatched(File.ReadAllBytes(ExecutablePath(path)));
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
            if (IsPatched(data) == skip) return skip ? "Executable already patched." : "Executable already unpatched.";

            string backup = path + ".bak";
            if (!skip || File.Exists(backup))
            {
                if (Hash(File.ReadAllBytes(backup)) != SupportedHash)
                    throw new InvalidDataException("The backup is not the supported original executable. No changes were made: " + backup);
            }

            if (!skip)
            {
                File.Replace(backup, path, null);
                return "HMA.exe restored and the old backup has been deleted.";
            }

            data[SettingOffset] = (byte)'0';
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
