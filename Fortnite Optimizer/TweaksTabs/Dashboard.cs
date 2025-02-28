using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fortnite_Optimizer.TweaksTabs
{
    public partial class Dashboard : UserControl
    {
        private string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToXFNOptimizer", "AudioState.txt");
        private bool isAudioOn = true;
        private string settingsFile;

        public Dashboard()
        {
            InitializeComponent();
            if (File.Exists(configPath))
            {
                string state = File.ReadAllText(configPath).Trim();
                isAudioOn = state == "ON";
            }
            LoadStuff();
        }

        private void AudioEffects_Click(object sender, EventArgs e)
        {
            isAudioOn = !isAudioOn;
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, isAudioOn ? "ON" : "OFF");
            UpdateAudioButton();
        }

        private void UpdateAudioButton()
        {
            if (isAudioOn)
            {
                AudioEffects.PressedColor = Color.Lime;
                AudioEffects.Image = Properties.Resources.AudioON;
            }
            else
            {
                AudioEffects.PressedColor = Color.FromArgb(218, 27, 51);
                AudioEffects.Image = Properties.Resources.AudioOFF;
            }
        }
        private async void FetchDownloadCount()
        {
            string apiUrl = "https://api.github.com/repos/ToXTweaks/ToX-Fortnite-Optimizer/releases";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("ToX-Fortnite-Optimizer");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray releases = JArray.Parse(responseBody);

                    int totalDownloads = 0;

                    foreach (var release in releases)
                    {
                        JArray assets = release["assets"] as JArray;
                        if (assets != null)
                        {
                            foreach (var asset in assets)
                            {
                                totalDownloads += asset["download_count"].Value<int>();
                            }
                        }
                    }

                    downloads.Text = totalDownloads.ToString();
                }
            }
            catch (Exception ex)
            {
                downloads.Text = "Error: " + ex.Message;
            }
        }

        private static readonly HttpClient client = new HttpClient();

        private async void Updates_Checker_Click(object sender, EventArgs e)
        {
            string currentVersion = "1.0";
            string apiUrl = "https://api.github.com/repos/ToXTweaks/ToX-Fortnite-Optimizer/releases/latest";

            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                string latestVersionTag = json["tag_name"].Value<string>();
                string latestVersion = latestVersionTag.StartsWith("v") ? latestVersionTag.Substring(1) : latestVersionTag;

                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    DialogResult dialogResult = MessageBox.Show(
                        $"A new version ({latestVersion}) is available! Do you want to download it?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (dialogResult == DialogResult.Yes)
                    {
                        Process.Start(json["html_url"].Value<string>());
                    }
                }
                else
                {
                    MessageBox.Show("You are using the latest version.", "Up to Date", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for updates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OptimizationsCounter()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string settingsFile = Path.Combine(appDataPath, "ToggleStates.txt");

            int totalOptions = 18;
            int trueCount = 0;

            if (File.Exists(settingsFile))
            {
                string[] lines = File.ReadAllLines(settingsFile);
                trueCount = lines.Count(line => line.Split('=').LastOrDefault()?.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) == true);
            }

            OptiCount.Text = $"{trueCount}/{totalOptions}";

            int percentage = (int)Math.Round((trueCount / (double)totalOptions) * 100);
            OptiPercentage.Text = $"{percentage}%";
        }

        private async void UpdateCheck()
        {
            string currentVersion = "1.0";
            string apiUrl = "https://api.github.com/repos/ToXTweaks/ToX-Fortnite-Optimizer/releases/latest";

            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                string latestVersionTag = json["tag_name"].Value<string>();
                string latestVersion = latestVersionTag.StartsWith("v") ? latestVersionTag.Substring(1) : latestVersionTag;

                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    Version.Text = "Outdated";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for updates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void LoadStuff()
        {
            await Task.Run(() => UpdateAudioButton());
            await Task.Run(() => FetchDownloadCount());
            await Task.Run(() => OptimizationsCounter());
            await Task.Run(() => UpdateCheck());
            await Task.Run(() => UpdateProcessAndThreadCounts());
        }

        private void ApplyAllTweaks_Click(object sender, EventArgs e)
        {
            string tweaks = @"reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""3"" /f
reg add ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""FortniteClient-Win64-Shipping.exe"" /t REG_SZ /d ""GpuPreference=2;"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /t REG_BINARY /d ""220022222222202020222020202020202020002000000000"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /t REG_BINARY /d ""222222222222222222222222222222222222222222222222"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /t REG_DWORD /d ""100"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /t REG_DWORD /d ""100"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /t REG_DWORD /d ""3"" /f
";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(NvApply, tweaks, revert);
        }

        private void RevertAllTweaks_Click(object sender, EventArgs e)
        {
            string tweaks = @"";
            string revert = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /f
reg delete ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""FortniteClient-Win64-Shipping.exe"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /f
";
            FNOptimizer.Instance.ExecuteTweaks(NvRevert, tweaks, revert);
        }
        private void UpdateProcessAndThreadCounts()
        {
            int processCount = Process.GetProcesses().Length;
            ProcessCount.Text = $"{processCount}";

            Task.Run(() =>
            {
                int threadCount = 0;
                foreach (Process process in Process.GetProcesses())
                    threadCount += process.Threads.Count;
                UpdateThreadCountUI(threadCount);
            });

            int processCoun = int.Parse(ProcessCount.Text);
            int threadsCount = int.Parse(ThreadsCount.Text);

            if (processCount < 100)
            {
                PStatus.Text = "Good";
            }
            else if (processCount >= 100 && processCount <= 150)
            {
                PStatus.Text = "Average";
            }
            else if (processCount > 150)
            {
                PStatus.Text = "Bad";
            }

            if (threadsCount < 2000)
            {
                TStatus.Text = "Good";
            }
            else if (threadsCount >= 2000 && threadsCount <= 2500)
            {
                TStatus.Text = "Average";
            }
            else if (threadsCount > 2500)
            {
                TStatus.Text = "Bad";
            }

        }

        private void UpdateThreadCountUI(int threadCount)
        {
            if (InvokeRequired) { Invoke((MethodInvoker)(() => UpdateThreadCountUI(threadCount))); return; }
            ThreadsCount.Text = $"{threadCount}";
        }
    }
}
