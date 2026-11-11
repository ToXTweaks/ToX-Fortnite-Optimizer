using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FortniteOptimizer
{
    /// <summary>
    /// AMD / Intel (any GPU) potato graphics via safe removal of optional HQ texture packs.
    /// Moves *ondemand-high* and pakchunk100fatiad* into a LocalAppData backup for full revert.
    /// Pure in-process C# (no PowerShell).
    /// </summary>
    internal static class PotatoTextureMod
    {
        private static readonly string[] CommonPaksPaths =
        {
            @"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks",
            @"D:\Epic Games\Fortnite\FortniteGame\Content\Paks",
            @"E:\Epic Games\Fortnite\FortniteGame\Content\Paks",
            @"F:\Epic Games\Fortnite\FortniteGame\Content\Paks",
            @"C:\Games\Fortnite\FortniteGame\Content\Paks",
            @"D:\Games\Fortnite\FortniteGame\Content\Paks"
        };

        public static string BackupDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ToX Fortnite Optimizer",
                    "PotatoBackup",
                    "AmdIntel");
            }
        }

        private static string ManifestPath
        {
            get { return Path.Combine(BackupDirectory, "manifest.txt"); }
        }

        public static void Apply()
        {
            string paks = FindPaksFolder();
            if (string.IsNullOrEmpty(paks) || !Directory.Exists(paks))
                throw new InvalidOperationException("Fortnite Paks folder was not found. Install Fortnite first.");

            Directory.CreateDirectory(BackupDirectory);

            var targets = new List<string>();
            targets.AddRange(Directory.GetFiles(paks, "*ondemand-high*"));
            targets.AddRange(Directory.GetFiles(paks, "pakchunk100fatiad*"));

            var safeTargets = targets
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path => IsSafePotatoTarget(Path.GetFileName(path)))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (safeTargets.Count == 0)
                return;

            var moved = new List<string>();

            foreach (string source in safeTargets)
            {
                string name = Path.GetFileName(source);
                string dest = Path.Combine(BackupDirectory, name);

                if (File.Exists(dest))
                {
                    try { File.Delete(dest); }
                    catch { }
                }

                MoveFilePreferDirect(source, dest);
                moved.Add(name);
            }

            File.WriteAllLines(ManifestPath, moved, Encoding.UTF8);
        }

        public static void Revert()
        {
            string paks = FindPaksFolder();
            if (string.IsNullOrEmpty(paks) || !Directory.Exists(paks))
                throw new InvalidOperationException("Fortnite Paks folder was not found. Install Fortnite first.");

            if (!Directory.Exists(BackupDirectory))
                return;

            var names = new List<string>();
            if (File.Exists(ManifestPath))
            {
                foreach (string line in File.ReadAllLines(ManifestPath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        names.Add(line.Trim());
                }
            }
            else
            {
                foreach (string file in Directory.GetFiles(BackupDirectory))
                {
                    string name = Path.GetFileName(file);
                    if (!string.Equals(name, "manifest.txt", StringComparison.OrdinalIgnoreCase))
                        names.Add(name);
                }
            }

            if (names.Count == 0)
                return;

            foreach (string name in names)
            {
                if (!IsSafePotatoTarget(name))
                    continue;

                string source = Path.Combine(BackupDirectory, name);
                if (!File.Exists(source))
                    continue;

                string dest = Path.Combine(paks, name);
                if (File.Exists(dest))
                {
                    try { File.Delete(dest); }
                    catch { }
                }

                MoveFilePreferDirect(source, dest);
            }
        }

        /// <summary>
        /// True only for optional HQ packs: names containing "ondemand-high"
        /// or starting with "pakchunk100fatiad". Never touches global.*, pakChunkEarly,
        /// or base pakchunk*-WindowsClient without those markers.
        /// </summary>
        internal static bool IsSafePotatoTarget(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            string name = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.IndexOf("ondemand-high", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (name.StartsWith("pakchunk100fatiad", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static void MoveFilePreferDirect(string source, string dest)
        {
            try
            {
                File.Move(source, dest);
            }
            catch (UnauthorizedAccessException)
            {
                MoveFileElevated(source, dest);
            }
            catch (IOException ex)
            {
                // Access denied is sometimes wrapped as IOException on locked/protected paths.
                if (ex.Message != null &&
                    ex.Message.IndexOf("access", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MoveFileElevated(source, dest);
                    return;
                }

                throw;
            }
        }

        private static void MoveFileElevated(string source, string dest)
        {
            string args = "/c move /Y \"" + source + "\" \"" + dest + "\"";
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Could not start elevated move for: " + Path.GetFileName(source));

                if (!process.WaitForExit(5 * 60 * 1000))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("Elevated move timed out for: " + Path.GetFileName(source));
                }

                if (process.ExitCode != 0 || !File.Exists(dest))
                    throw new InvalidOperationException("Elevated move failed for: " + Path.GetFileName(source));
            }
        }

        public static string FindPaksFolder()
        {
            foreach (string path in CommonPaksPaths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            try
            {
                string manifestsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Epic", "EpicGamesLauncher", "Data", "Manifests");

                if (Directory.Exists(manifestsDir))
                {
                    foreach (string file in Directory.GetFiles(manifestsDir, "*.item"))
                    {
                        string text = File.ReadAllText(file);
                        if (text.IndexOf("Fortnite", StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        Match match = Regex.Match(text, "\"InstallLocation\"\\s*:\\s*\"([^\"]+)\"");
                        if (!match.Success)
                            continue;

                        string install = match.Groups[1].Value.Replace("\\\\", "\\");
                        string paks = Path.Combine(install, "FortniteGame", "Content", "Paks");
                        if (Directory.Exists(paks))
                            return paks;
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
