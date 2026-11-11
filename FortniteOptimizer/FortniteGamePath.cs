using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FortniteOptimizer
{
    internal static class FortniteGamePath
    {
        public const string ExeName = "FortniteClient-Win64-Shipping.exe";

        private static readonly string SavedPathFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ToX Fortnite Optimizer",
            "FortnitePath.txt");

        private static string _cachedPath;

        public static string DefaultPath
        {
            get
            {
                string programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
                if (string.IsNullOrEmpty(programFiles))
                    programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                return Path.Combine(
                    programFiles,
                    "Epic Games",
                    "Fortnite",
                    "FortniteGame",
                    "Binaries",
                    "Win64",
                    ExeName);
            }
        }

        public static string FindExisting()
        {
            if (IsShippingExe(_cachedPath))
                return _cachedPath;

            string saved = ReadSaved();
            if (IsShippingExe(saved))
            {
                _cachedPath = saved;
                return saved;
            }

            if (File.Exists(DefaultPath))
            {
                Remember(DefaultPath);
                return DefaultPath;
            }

            string fromLauncher = FindFromEpicManifest();
            if (IsShippingExe(fromLauncher))
            {
                Remember(fromLauncher);
                return fromLauncher;
            }

            return null;
        }

        public static string ResolveOrPrompt(IWin32Window owner)
        {
            string found = FindExisting();
            if (!string.IsNullOrEmpty(found))
                return found;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select FortniteClient-Win64-Shipping.exe";
                dialog.Filter = "FortniteClient-Win64-Shipping.exe|FortniteClient-Win64-Shipping.exe";
                dialog.FileName = ExeName;
                dialog.CheckFileExists = true;

                string defaultDir = Path.GetDirectoryName(DefaultPath);
                if (!string.IsNullOrEmpty(defaultDir) && Directory.Exists(defaultDir))
                    dialog.InitialDirectory = defaultDir;

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;

                if (!IsShippingExe(dialog.FileName))
                {
                    MessageBox.Show(
                        owner,
                        "Select FortniteClient-Win64-Shipping.exe.",
                        "ToX Fortnite Optimizer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return null;
                }

                Remember(dialog.FileName);
                return dialog.FileName;
            }
        }

        public static void SetHighGpuPriority(string exePath, bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\DirectX\UserGpuPreferences"))
            {
                if (key == null)
                    return;

                key.DeleteValue(ExeName, false);

                if (!IsShippingExe(exePath))
                    return;

                if (enabled)
                    key.SetValue(exePath, "GpuPreference=2;");
                else
                    key.DeleteValue(exePath, false);
            }
        }

        public static void SetLayerFlag(string exePath, string flag, bool enabled)
        {
            if (!IsShippingExe(exePath) || string.IsNullOrEmpty(flag))
                return;

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"))
            {
                if (key == null)
                    return;

                var flags = new List<string>();
                string current = key.GetValue(exePath) as string;
                if (!string.IsNullOrEmpty(current))
                {
                    foreach (string part in current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (part == "~")
                            continue;
                        if (!ContainsFlag(flags, part))
                            flags.Add(part);
                    }
                }

                RemoveFlag(flags, flag);
                if (enabled)
                    flags.Add(flag);

                if (flags.Count == 0)
                    key.DeleteValue(exePath, false);
                else
                    key.SetValue(exePath, "~ " + string.Join(" ", flags.ToArray()));
            }
        }

        private static bool ContainsFlag(List<string> flags, string flag)
        {
            foreach (string existing in flags)
            {
                if (string.Equals(existing, flag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void RemoveFlag(List<string> flags, string flag)
        {
            for (int i = flags.Count - 1; i >= 0; i--)
            {
                if (string.Equals(flags[i], flag, StringComparison.OrdinalIgnoreCase))
                    flags.RemoveAt(i);
            }
        }

        private static bool IsShippingExe(string path)
        {
            return !string.IsNullOrEmpty(path)
                && File.Exists(path)
                && string.Equals(Path.GetFileName(path), ExeName, StringComparison.OrdinalIgnoreCase);
        }

        private static void Remember(string path)
        {
            _cachedPath = path;
            try
            {
                string folder = Path.GetDirectoryName(SavedPathFile);
                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllText(SavedPathFile, path);
            }
            catch { }
        }

        private static string ReadSaved()
        {
            try
            {
                if (!File.Exists(SavedPathFile))
                    return null;
                return File.ReadAllText(SavedPathFile).Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string FindFromEpicManifest()
        {
            try
            {
                string manifestsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Epic",
                    "EpicGamesLauncher",
                    "Data",
                    "Manifests");

                if (!Directory.Exists(manifestsDir))
                    return null;

                foreach (string file in Directory.GetFiles(manifestsDir, "*.item"))
                {
                    string text = File.ReadAllText(file);
                    if (text.IndexOf("Fortnite", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    Match match = Regex.Match(text, "\"InstallLocation\"\\s*:\\s*\"([^\"]+)\"");
                    if (!match.Success)
                        continue;

                    string install = match.Groups[1].Value.Replace("\\\\", "\\").Replace("/", "\\");
                    string exePath = Path.Combine(install, "FortniteGame", "Binaries", "Win64", ExeName);
                    if (File.Exists(exePath))
                        return exePath;
                }
            }
            catch { }

            return null;
        }
    }
}
