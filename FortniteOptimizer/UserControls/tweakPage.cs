using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vortex.Ui;
using FortniteOptimizer;

namespace FortniteOptimizer.UserControls
{
    public partial class tweakPage : UserControl
    {
        private bool _isLoadingStates = true;
        private string _gamePath = string.Empty;
        private readonly string _settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToX Fortnite Optimizer");
        private readonly string _settingsFile;

        public tweakPage()
        {
            InitializeComponent();
            _settingsFile = Path.Combine(_settingsFolder, "ToggleStates.txt");

            HookEvents();
            this.Load += TweakPage_Load;
            this.VisibleChanged += TweakPage_VisibleChanged;
        }

        private void TweakPage_Load(object sender, EventArgs e)
        {
            SyncToggleStatesFromDisk();
        }

        private void TweakPage_VisibleChanged(object sender, EventArgs e)
        {
            // Optimize Now writes ToggleStates.txt from Home; refresh switches when Tweaks is shown again.
            if (Visible)
                SyncToggleStatesFromDisk();
        }

        private void SyncToggleStatesFromDisk()
        {
            _isLoadingStates = true;
            try
            {
                LoadToggleStates();
            }
            finally
            {
                _isLoadingStates = false;
            }
        }

        private void HookEvents()
        {
            // 1. High CPU Priority
            vortexRoundedSwitch1.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch1, "HIGH CPU PRIORITY", async (enabled) =>
            {
                string cmd = enabled
                    ? @"reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""3"" /f"
                    : @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 2. High GPU Priority (registry value name is the full exe path)
            vortexRoundedSwitch2.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch2, "HIGH GPU PRIORITY", async (enabled) =>
            {
                FortniteGamePath.SetHighGpuPriority(_gamePath, enabled);
                await Task.CompletedTask;
            }, true);

            // 3. QoS Optimization (policy matches the exe file name, which never changes)
            vortexRoundedSwitch3.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch3, "QOS OPTIMIZATION", async (enabled) =>
            {
                const string exeName = FortniteGamePath.ExeName;
                const string gameName = "FortniteClient-Win64-Shipping";

                string cmd = enabled
                    ? $@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Application Name"" /t REG_SZ /d ""{exeName}"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""DSCP value"" /t REG_SZ /d ""46"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP Prefix Length"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local Port"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Protocol"" /t REG_SZ /d ""UDP"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP Prefix Length"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote Port"" /t REG_SZ /d ""*"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""throttle Rate"" /t REG_SZ /d ""-1"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""version"" /t REG_SZ /d ""1.0"" /f"
                    : $@"REG DELETE ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 4. Disable Fullscreen Optimization
            vortexRoundedSwitch4.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch4, "DISABLE FULLSCREEN OPTIMIZATION", async (enabled) =>
            {
                FortniteGamePath.SetLayerFlag(_gamePath, "DISABLEDXMAXIMIZEDWINDOWEDMODE", enabled);
                await Task.CompletedTask;
            }, true);

            // 5. Disable Fortnite Mitigations
            vortexRoundedSwitch5.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch5, "DISABLE FORTNITE MITIGATIONS", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /t REG_BINARY /d ""220022222222202020222020202020202020002000000000"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /t REG_BINARY /d ""222222222222222222222222222222222222222222222222"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 6. Enable High DPI Awareness
            vortexRoundedSwitch6.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch6, "ENABLE HIGH DPI AWARENESS", async (enabled) =>
            {
                FortniteGamePath.SetLayerFlag(_gamePath, "HIGHDPIAWARE", enabled);
                await Task.CompletedTask;
            }, true);

            // 7. Increase CPU Utilization
            vortexRoundedSwitch7.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch7, "INCREASE CPU UTILIZATION", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /t REG_DWORD /d ""100"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 8. Increase GPU Speed
            vortexRoundedSwitch8.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch8, "INCREASE GPU SPEED", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /t REG_DWORD /d ""100"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 9. Increase Responsiveness of KBM Devices
            vortexRoundedSwitch9.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch9, "INCREASE KBM RESPONSIVENESS", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 10. Disable Extra Debugging Checks (Heap Validation)
            vortexRoundedSwitch10.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch10, "DISABLE HEAP DEBUG CHECKS", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 11. Run As Administrator
            vortexRoundedSwitch11.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch11, "RUN AS ADMINISTRATOR", async (enabled) =>
            {
                FortniteGamePath.SetLayerFlag(_gamePath, "RUNASADMIN", enabled);
                await Task.CompletedTask;
            }, true);

            // 12. Prevent Windows From Throttling CPU Usage
            vortexRoundedSwitch12.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch12, "PREVENT CPU THROTTLING", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 13. Optimize Processor Usage
            vortexRoundedSwitch13.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch13, "OPTIMIZE PROCESSOR USAGE", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 14. Disable Breakpoints
            vortexRoundedSwitch14.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch14, "DISABLE BREAKPOINTS", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 15. Reduce Error Checking Steps (SEH Validation)
            vortexRoundedSwitch15.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch15, "DISABLE SEH VALIDATION", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 17. Remove Extra Windows Security Checks (Exception Chain Validation)
            vortexRoundedSwitch17.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch17, "DISABLE EXCEPTION CHAIN VALIDATION", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d ""1"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 18. Optimized Game User Settings
            vortexRoundedSwitch18.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch18, "OPTIMIZED GAME USER SETTINGS", async (enabled) =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string fnConfigDir = Path.Combine(localAppData, "FortniteGame", "Saved", "Config", "WindowsClient");
                        string targetIni = Path.Combine(fnConfigDir, "GameUserSettings.ini");
                        string backupIni = Path.Combine(fnConfigDir, "GameUserSettings.ini.bak");

                        if (!Directory.Exists(fnConfigDir))
                        {
                            Directory.CreateDirectory(fnConfigDir);
                        }

                        if (enabled)
                        {
                            // Create backup if target exists
                            if (File.Exists(targetIni) && !File.Exists(backupIni))
                            {
                                File.Copy(targetIni, backupIni, true);
                            }

                            string downloadUrl = "https://github.com/ToXTweaks/Resources-Free/raw/refs/heads/main/GameUserSettings.ini";
                            string tempFile = Path.Combine(Path.GetTempPath(), "GameUserSettings.ini");

                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(downloadUrl, tempFile);
                            }

                            if (File.Exists(tempFile))
                            {
                                File.Copy(tempFile, targetIni, true);
                                File.Delete(tempFile);
                            }
                        }
                        else
                        {
                            // Revert from backup if exists
                            if (File.Exists(backupIni))
                            {
                                File.Copy(backupIni, targetIni, true);
                            }
                        }
                    }
                    catch { }
                });
            });

            // 19. High I/O Priority
            vortexRoundedSwitch19.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch19, "HIGH I/O PRIORITY", async (enabled) =>
            {
                string cmd = enabled
                    ? @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /t REG_DWORD /d ""3"" /f"
                    : @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /f";
                await ExecuteBatchCommandAsync(cmd);
            });

            // 20. ToX Best Power Plan
            vortexRoundedSwitch20.CheckedChanged += (s, e) => HandleSwitch(vortexRoundedSwitch20, "TOX BEST POWER PLAN", async (enabled) =>
            {
                await ApplyToxBestPowerPlanAsync(enabled);
            });
        }

        private async void HandleSwitch(VortexRoundedSwitch sw, string tweakTitle, Func<bool, Task> tweakAction, bool needsGamePath = false)
        {
            if (_isLoadingStates) return;

            Form parentForm = this.FindForm();
            if (parentForm == null) return;

            bool isEnabling = sw.Checked;

            if (needsGamePath)
            {
                string path = FortniteGamePath.ResolveOrPrompt(parentForm);
                if (string.IsNullOrEmpty(path))
                {
                    RevertSwitch(sw);
                    return;
                }

                _gamePath = path;
            }

            bool applied = await TweakProgressOverlay.ExecuteWithOverlayAsync(parentForm, tweakTitle, isEnabling, async () =>
            {
                if (tweakAction != null)
                    await tweakAction(isEnabling);
            });

            if (applied)
                SaveToggleState(sw.Name, isEnabling);
            else
                RevertSwitch(sw);
        }

        private void RevertSwitch(VortexRoundedSwitch sw)
        {
            _isLoadingStates = true;
            try
            {
                sw.Checked = !sw.Checked;
            }
            finally
            {
                _isLoadingStates = false;
            }
        }

        private static async Task ExecuteBatchCommandAsync(string batchCommands)
        {
            if (string.IsNullOrWhiteSpace(batchCommands)) return;

            await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + batchCommands)
                    {
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    };

                    using (Process process = Process.Start(procStartInfo))
                    {
                        process?.WaitForExit(10000);
                    }
                }
                catch { }
            });
        }

        private const string ToxPowerPlanGuid = "77777777-7777-7777-7777-123456789000";
        private const string BalancedPowerPlanGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";

        private static async Task ApplyToxBestPowerPlanAsync(bool enabled)
        {
            await Task.Run(() =>
            {
                try
                {
                    string powPath = FortniteOptimizer.EmbeddedAssets.Extract(
                        Path.Combine("Resources", "PowerPlan", "ToX_Best_Power_Plan.pow"));

                    if (enabled)
                    {
                        if (!File.Exists(powPath))
                            throw new FileNotFoundException("ToX Best Power Plan file was not found.", powPath);

                        // Re-import cleanly, then activate (matches Apply Power Plan.bat).
                        RunElevatedCmd(
                            "powercfg /delete " + ToxPowerPlanGuid + " >nul 2>&1 & " +
                            "powercfg /import \"" + powPath + "\" " + ToxPowerPlanGuid + " >nul 2>&1 & " +
                            "powercfg /s " + ToxPowerPlanGuid + " >nul 2>&1");

                        if (IsIntelHybridCpu())
                        {
                            RunElevatedCmd(
                                @"Reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DefaultHeteroCpuPolicy"" /t REG_DWORD /d 1 /f >nul 2>&1 & " +
                                @"Reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DefaultDynamicHeteroCpuPolicy"" /t REG_DWORD /d 1 /f >nul 2>&1 & " +
                                @"Reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DynamicHeteroCpuPolicyImportant"" /t REG_DWORD /d 1 /f >nul 2>&1 & " +
                                @"Reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DynamicHeteroCpuPolicyImportantShort"" /t REG_DWORD /d 1 /f >nul 2>&1");
                        }
                    }
                    else
                    {
                        RunElevatedCmd(
                            "powercfg /s " + BalancedPowerPlanGuid + " >nul 2>&1 & " +
                            "powercfg /delete " + ToxPowerPlanGuid + " >nul 2>&1");

                        RunElevatedCmd(
                            @"Reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DefaultHeteroCpuPolicy"" /f >nul 2>&1 & " +
                            @"Reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DefaultDynamicHeteroCpuPolicy"" /f >nul 2>&1 & " +
                            @"Reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DynamicHeteroCpuPolicyImportant"" /f >nul 2>&1 & " +
                            @"Reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"" /v ""DynamicHeteroCpuPolicyImportantShort"" /f >nul 2>&1");
                    }
                }
                catch { }
            });
        }

        private static void RunElevatedCmd(string commands)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + commands,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                process?.WaitForExit(20000);
            }
        }

        private static bool IsIntelHybridCpu()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject cpu in results)
                    {
                        string name = (cpu["Name"] as string) ?? string.Empty;
                        if (name.IndexOf("12th", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("13th", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("14th", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("Ultra", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void SaveToggleState(string switchName, bool isChecked)
        {
            try
            {
                if (!Directory.Exists(_settingsFolder))
                {
                    Directory.CreateDirectory(_settingsFolder);
                }

                Dictionary<string, string> states = new Dictionary<string, string>();
                if (File.Exists(_settingsFile))
                {
                    foreach (string line in File.ReadAllLines(_settingsFile))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            states[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }

                states[switchName] = isChecked ? "true" : "false";

                List<string> lines = new List<string>();
                foreach (var kvp in states)
                {
                    lines.Add($"{kvp.Key}={kvp.Value}");
                }
                File.WriteAllLines(_settingsFile, lines);
            }
            catch { }
        }

        private void LoadToggleStates()
        {
            try
            {
                if (!File.Exists(_settingsFile)) return;

                string[] lines = File.ReadAllLines(_settingsFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string name = parts[0].Trim();
                        bool state = parts[1].Trim().ToLowerInvariant() == "true";

                        Control[] controls = this.Controls.Find(name, true);
                        if (controls.Length > 0 && controls[0] is VortexRoundedSwitch sw)
                        {
                            if (sw.Checked != state)
                                sw.Checked = state;
                        }
                    }
                }
            }
            catch { }
        }
    }
}

