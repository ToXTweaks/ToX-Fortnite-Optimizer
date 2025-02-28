using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fortnite_Optimizer.TweaksTabs
{
    public partial class Tweaks : UserControl
    {
        private string gamePath = string.Empty;
        private bool isClickHandled = false;
        public Tweaks()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            tweaksPage.AutoScroll = false;
            guna2VScrollBar1.Location = new Point(690, 1);
            guna2VScrollBar1.Scroll += (s, e) =>
            {
                guna2VScrollBar1.SmallChange = 1;
                guna2VScrollBar1.LargeChange = 1;
                tweaksPage.Invalidate();
                tweaksPage.Update();
                tweaksPage.PerformLayout();
            };
            guna2VScrollBar1.Parent = tweaksPage;
        }
        private void Tweaks_Load(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Helpers.PanelScrollHelper panelScrollHelper =
                           new Guna.UI2.WinForms.Helpers.PanelScrollHelper(tweaksPage, guna2VScrollBar1, false);
            tweaksPage.BringToFront();
            guna2VScrollBar1.BringToFront();
            FNOptimizer.Instance.ApplyToggleStates();
            this.Focus();
        }

        private void HCP_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""3"" /f";
            string revert = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""CpuPriorityClass"" /f";
            FNOptimizer.Instance.ExecuteTweaks(HCP_Switch, tweaks, revert);
        }

        
        private void HGP_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"reg add ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""FortniteClient-Win64-Shipping.exe"" /t REG_SZ /d ""GpuPreference=2;"" /f";
            string revert = @"reg delete ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""FortniteClient-Win64-Shipping.exe"" /f";
            FNOptimizer.Instance.ExecuteTweaks(HGP_Switch, tweaks, revert);
        }

        private async void QP_Switch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Fortnite Executable|FortniteClient-Win64-Shipping.exe";
                openFileDialog.Title = "Select the Fortnite Executable (.exe)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePath = openFileDialog.FileName;
                }
            }

            await Task.Run(() => FortQOSTweak(gamePath));

            string tweaks = @"";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(QP_Switch, tweaks, revert);
        }

        private async void DFO_Switch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Fortnite Executable|FortniteClient-Win64-Shipping.exe";
                openFileDialog.Title = "Select the Fortnite Executable (.exe)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePath = openFileDialog.FileName;
                }
            }

            await Task.Run(() => FortDisableFSO(gamePath));

            string tweaks = @"";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(DFO_Switch, tweaks, revert);
        }

        private void DFM_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /t REG_BINARY /d ""220022222222202020222020202020202020002000000000"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /t REG_BINARY /d ""222222222222222222222222222222222222222222222222"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationOptions"" /f
Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""MitigationAuditOptions"" /f
";
            FNOptimizer.Instance.ExecuteTweaks(DFM_Switch, tweaks, revert);
        }

        private async void EHDA_Switch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Fortnite Executable|FortniteClient-Win64-Shipping.exe";
                openFileDialog.Title = "Select the Fortnite Executable (.exe)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePath = openFileDialog.FileName;
                }
            }

            await Task.Run(() => FortHDA(gamePath));

            string tweaks = @"";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(EHDA_Switch, tweaks, revert);
        }

        private void ICU_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /t REG_DWORD /d ""100"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""CpuUtilization"" /f";
            FNOptimizer.Instance.ExecuteTweaks(ICU_Switch, tweaks, revert);
        }

        private void IGS_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /t REG_DWORD /d ""100"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""GpuSpeed"" /f";
            FNOptimizer.Instance.ExecuteTweaks(IGS_Switch, tweaks, revert);
        }

        private void ITROKD_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""InputBoosted"" /f";
            FNOptimizer.Instance.ExecuteTweaks(ITROKD_Switch, tweaks, revert);
        }

        private void OPU_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""ProcOptimize"" /f";
            FNOptimizer.Instance.ExecuteTweaks(OPU_Switch, tweaks, revert);
        }

        private void PWFTCU_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe"" /v ""throttlinginsensitive"" /f";
            FNOptimizer.Instance.ExecuteTweaks(PWFTCU_Switch, tweaks, revert);
        }

        private async void RAD_Switch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Fortnite Executable|FortniteClient-Win64-Shipping.exe";
                openFileDialog.Title = "Select the Fortnite Executable (.exe)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePath = openFileDialog.FileName;
                }
            }

            await Task.Run(() => FortRunAsAdmin(gamePath));

            string tweaks = @"";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(RAD_Switch, tweaks, revert);
        }

        private void DEDC_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableHeapValidation"" /f";
            FNOptimizer.Instance.ExecuteTweaks(DEDC_Switch, tweaks, revert);
        }

        private void REWSC_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableExceptionChainValidation"" /f";
            FNOptimizer.Instance.ExecuteTweaks(REWSC_Switch, tweaks, revert);
        }

        private void RECS_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableSEHValidation"" /f";
            FNOptimizer.Instance.ExecuteTweaks(RECS_Switch, tweaks, revert);
        }

        private void DB_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /t REG_DWORD /d ""1"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""DisableBreakpoint"" /f";
            FNOptimizer.Instance.ExecuteTweaks(DB_Switch, tweaks, revert);
        }

        private void HIOP_Switch_Click(object sender, EventArgs e)
        {
            string tweaks = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /t REG_DWORD /d ""3"" /f";
            string revert = @"Reg.exe delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions"" /v ""IoPriority"" /f";
            FNOptimizer.Instance.ExecuteTweaks(HIOP_Switch, tweaks, revert);
        }
        private void ExecuteBatchCommand(string command)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = new Process { StartInfo = procStartInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }
        private void FortQOSTweak(string gamePath)
        {
            string gameExeName = Path.GetFileName(gamePath);
            string gameName = Path.GetFileNameWithoutExtension(gamePath);

            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Application Name"" /t REG_SZ /d ""{gameExeName}"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""DSCP value"" /t REG_SZ /d ""46"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP Prefix Length"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local Port"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Protocol"" /t REG_SZ /d ""UDP"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP Prefix Length"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote Port"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""throttle Rate"" /t REG_SZ /d ""-1"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""version"" /t REG_SZ /d ""1.0"" /f");
        }
        private void FortDisableFSO(string gamePath)
        {
            string gameExeName = Path.GetFileName(gamePath);
            string gameName = Path.GetFileNameWithoutExtension(gamePath);

            ExecuteBatchCommand($@"reg add ""HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" /v ""{gamePath}"" /t REG_SZ /d ""~ DISABLEDXMAXIMIZEDWINDOWEDMODE"" /f");
        }
        private void FortHDA(string gamePath)
        {
            string gameExeName = Path.GetFileName(gamePath);
            string gameName = Path.GetFileNameWithoutExtension(gamePath);

            ExecuteBatchCommand($@"reg add ""HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" /v ""{gamePath}"" /t REG_SZ /d ""~ RUNASADMIN"" /f");
        }
        private void FortRunAsAdmin(string gamePath)
        {
            string gameExeName = Path.GetFileName(gamePath);
            string gameName = Path.GetFileNameWithoutExtension(gamePath);

            ExecuteBatchCommand($@"reg add ""HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" /v ""{gamePath}"" /t REG_SZ /d ""~ RUNASADMIN"" /f");
        }
    }
}
