using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using FortniteOptimizer;

namespace FortniteOptimizer.UserControls
{
    public partial class homePage : UserControl
    {
        private bool _isOptimized = false;
        private readonly string _settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToX Fortnite Optimizer");
        private readonly string _settingsFile;

        private const string ApplyAllTweaksCmd = @"reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""3"" /f
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

        private const string RevertAllTweaksCmd = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /f
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

        private const string ToxPowerPlanGuid = "77777777-7777-7777-7777-123456789000";
        private const string BalancedPowerPlanGuid = "381b4222-f694-41f0-9685-ff5bb260df2e";

        // Category switch groups mapped to the four home stats cards.
        private static readonly string[] PerformanceSwitches =
        {
            "vortexRoundedSwitch1",  // High CPU Priority
            "vortexRoundedSwitch2",  // High GPU Priority
            "vortexRoundedSwitch5",  // Disable Mitigations
            "vortexRoundedSwitch7",  // Increase CPU Utilization
            "vortexRoundedSwitch8",  // Increase GPU Speed
            "vortexRoundedSwitch13", // Optimize Processor Usage
            "vortexRoundedSwitch18"  // Optimized Game User Settings
        };

        private static readonly string[] NetworkSwitches =
        {
            "vortexRoundedSwitch3"   // QoS Optimization
        };

        private static readonly string[] LatencySwitches =
        {
            "vortexRoundedSwitch4",  // Disable Fullscreen Optimization
            "vortexRoundedSwitch6",  // High DPI Awareness
            "vortexRoundedSwitch9",  // KBM Responsiveness
            "vortexRoundedSwitch12"  // Prevent CPU Throttling
        };

        private static readonly string[] SystemSwitches =
        {
            "vortexRoundedSwitch10", // Heap Debug Checks
            "vortexRoundedSwitch11", // Run As Administrator
            "vortexRoundedSwitch14", // Disable Breakpoints
            "vortexRoundedSwitch15", // SEH Validation
            "vortexRoundedSwitch17", // Exception Chain Validation
            "vortexRoundedSwitch19", // High I/O Priority
            "vortexRoundedSwitch20"  // ToX Best Power Plan
        };

        public homePage()
        {
            InitializeComponent();
            _settingsFile = Path.Combine(_settingsFolder, "ToggleStates.txt");

            HookEvents();
            this.Load += HomePage_Load;
            this.VisibleChanged += HomePage_VisibleChanged;
        }

        private void HomePage_Load(object sender, EventArgs e)
        {
            RefreshStats();
        }

        private void HomePage_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                RefreshStats();
        }

        private void HookEvents()
        {
            vortexButton1.Click += VortexButton1_Click;
        }

        private async void VortexButton1_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm == null) return;

            bool apply = !_isOptimized;
            string title = apply ? "APPLY ALL TWEAKS" : "REVERT ALL TWEAKS";
            string cmdToRun = apply ? ApplyAllTweaksCmd : RevertAllTweaksCmd;

            string gamePath = apply
                ? FortniteGamePath.ResolveOrPrompt(parentForm)
                : FortniteGamePath.FindExisting();
            if (apply && string.IsNullOrEmpty(gamePath))
                return;

            bool success = false;
            try
            {
                success = await TweakProgressOverlay.ExecuteWithOverlayAsync(parentForm, title, apply, async () =>
                {
                    await ExecuteBatchCommandAsync(cmdToRun);
                    ApplyPathTweaks(gamePath, apply);
                    await ApplyToxBestPowerPlanAsync(apply);
                    SaveAllToggleStates(apply);
                });
            }
            catch
            {
                success = false;
            }

            if (success)
            {
                _isOptimized = apply;
                RefreshStats();
            }
        }

        private static void ApplyPathTweaks(string gamePath, bool enabled)
        {
            FortniteGamePath.SetHighGpuPriority(gamePath, enabled);
            FortniteGamePath.SetLayerFlag(gamePath, "DISABLEDXMAXIMIZEDWINDOWEDMODE", enabled);
            FortniteGamePath.SetLayerFlag(gamePath, "HIGHDPIAWARE", enabled);
            FortniteGamePath.SetLayerFlag(gamePath, "RUNASADMIN", enabled);
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
                            return;

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

        private void RefreshStats()
        {
            Dictionary<string, bool> states = LoadToggleStates();

            int performance = CalcCategoryPercent(states, PerformanceSwitches);
            int network = CalcCategoryPercent(states, NetworkSwitches);
            int latency = CalcCategoryPercent(states, LatencySwitches);
            int system = CalcCategoryPercent(states, SystemSwitches);

            // PERFORMANCE (blue) -> bar2 / label10
            ApplyStat(vortexStripedProgressBar2, vortexLabel10, performance);
            // NETWORK (green) -> bar1 / label13
            ApplyStat(vortexStripedProgressBar1, vortexLabel13, network);
            // LATENCY (cyan) -> bar4 / label19
            ApplyStat(vortexStripedProgressBar4, vortexLabel19, latency);
            // SYSTEM (purple) -> bar3 / label16
            ApplyStat(vortexStripedProgressBar3, vortexLabel16, system);

            int enabledTotal = 0;
            int allTotal = PerformanceSwitches.Length + NetworkSwitches.Length + LatencySwitches.Length + SystemSwitches.Length;
            foreach (var kv in states)
            {
                if (kv.Value) enabledTotal++;
            }

            _isOptimized = allTotal > 0 && enabledTotal >= Math.Max(5, (allTotal * 3) / 4);
            UpdateOptimizeButton();
        }

        private void UpdateOptimizeButton()
        {
            if (_isOptimized)
            {
                vortexButton1.Text = "REVERT ALL TWEAKS";
                vortexButton1.ButtonColor = Color.FromArgb(235, 94, 40);
                vortexButton1.ForeColor = Color.White;
            }
            else
            {
                vortexButton1.Text = "OPTIMIZE NOW!";
                vortexButton1.ButtonColor = Color.FromArgb(252, 209, 39);
                vortexButton1.ForeColor = Color.Black;
            }
        }

        private static void ApplyStat(Vortex.Ui.VortexStripedProgressBar bar, Vortex.Ui.VortexLabel label, int percent)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            bar.Value = percent;
            label.Text = "+ " + percent + "% OPTIMIZED";
        }

        private static int CalcCategoryPercent(Dictionary<string, bool> states, string[] switches)
        {
            if (switches == null || switches.Length == 0)
                return 0;

            int on = 0;
            foreach (string name in switches)
            {
                bool enabled;
                if (states.TryGetValue(name, out enabled) && enabled)
                    on++;
            }

            return (int)Math.Round((on * 100.0) / switches.Length);
        }

        private Dictionary<string, bool> LoadToggleStates()
        {
            var states = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(_settingsFile))
                    return states;

                foreach (string line in File.ReadAllLines(_settingsFile))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    states[parts[0].Trim()] = parts[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }

            return states;
        }

        private void SaveAllToggleStates(bool isChecked)
        {
            try
            {
                if (!Directory.Exists(_settingsFolder))
                {
                    Directory.CreateDirectory(_settingsFolder);
                }

                string val = isChecked ? "true" : "false";
                string[] switchNames = new[]
                {
                    "vortexRoundedSwitch1",
                    "vortexRoundedSwitch2",
                    "vortexRoundedSwitch3",
                    "vortexRoundedSwitch4",
                    "vortexRoundedSwitch5",
                    "vortexRoundedSwitch6",
                    "vortexRoundedSwitch7",
                    "vortexRoundedSwitch8",
                    "vortexRoundedSwitch9",
                    "vortexRoundedSwitch10",
                    "vortexRoundedSwitch11",
                    "vortexRoundedSwitch12",
                    "vortexRoundedSwitch13",
                    "vortexRoundedSwitch14",
                    "vortexRoundedSwitch15",
                    "vortexRoundedSwitch17",
                    "vortexRoundedSwitch18",
                    "vortexRoundedSwitch19",
                    "vortexRoundedSwitch20"
                };

                List<string> lines = new List<string>();
                foreach (string name in switchNames)
                {
                    lines.Add($"{name}={val}");
                }

                File.WriteAllLines(_settingsFile, lines);
            }
            catch { }
        }
    }
}
