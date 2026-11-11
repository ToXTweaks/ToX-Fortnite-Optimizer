using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FortniteOptimizer.UserControls
{
    public partial class potatoPage : UserControl
    {
        private sealed class NvidiaProfileOption
        {
            public string DisplayName { get; }
            public string FileName { get; }

            public NvidiaProfileOption(string displayName, string fileName)
            {
                DisplayName = displayName;
                FileName = fileName;
            }

            public override string ToString() => DisplayName;
        }

        private static readonly NvidiaProfileOption[] NvidiaProfiles =
        {
            new NvidiaProfileOption("Low", "Fortnite_Low.nip"),
            new NvidiaProfileOption("Medium (Recommended)", "Fortnite_Medium (Recommended).nip"),
            new NvidiaProfileOption("High", "Fortnite_High.nip")
        };

        private bool _isBusy;

        public potatoPage()
        {
            InitializeComponent();
            SetupNvidiaProfileDropdown();
            HookAmdIntelEvents();
        }

        private void SetupNvidiaProfileDropdown()
        {
            nvidiaProfileCombo.Items.Clear();
            foreach (var profile in NvidiaProfiles)
                nvidiaProfileCombo.Items.Add(profile);

            nvidiaProfileCombo.SelectedIndex = 1; // Medium (Recommended)
        }

        private void HookAmdIntelEvents()
        {
            vortexButton1.Click += vortexButton1_Click; // AMD/Intel APPLY
            vortexButton2.Click += vortexButton2_Click; // AMD/Intel REVERT
        }

        private void previewAMD_Click(object sender, EventArgs e)
        {
            ShowPreviewModal("AMD");
        }

        private void previewNvidia_Click(object sender, EventArgs e)
        {
            var selected = nvidiaProfileCombo.SelectedItem as NvidiaProfileOption;
            string profileName = selected != null ? selected.DisplayName : "Medium";
            ShowPreviewModal("NVIDIA", profileName);
        }

        private async void vortexButton1_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;

            if (string.IsNullOrEmpty(FortniteOptimizer.PotatoTextureMod.FindPaksFolder()))
            {
                MessageBox.Show(
                    "Fortnite install was not found.\n\nExpected under Epic Games\\Fortnite\\FortniteGame\\Content\\Paks",
                    "AMD / Intel Potato Graphics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "This will safely move optional high-quality Fortnite texture packs (~6–10 GB) to a backup folder so the game looks much more \"potato\".\n\n" +
                "Core game files are never deleted. You can REVERT anytime.\n\n" +
                "Close Fortnite before continuing. Epic may re-download textures later.\n\nContinue?",
                "AMD / Intel Potato Graphics",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            await RunAmdIntelPotatoAsync(true);
        }

        private async void vortexButton2_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;
            await RunAmdIntelPotatoAsync(false);
        }

        private async Task RunAmdIntelPotatoAsync(bool applying)
        {
            Form parentForm = FindForm();
            if (parentForm == null) return;

            _isBusy = true;
            try
            {
                string title = applying ? "AMD / INTEL POTATO GRAPHICS" : "REVERT AMD / INTEL POTATO";
                await TweakProgressOverlay.ExecuteWithOverlayAsync(parentForm, title, applying, async () =>
                {
                    await Task.Run(() =>
                    {
                        if (applying)
                            FortniteOptimizer.PotatoTextureMod.Apply();
                        else
                            FortniteOptimizer.PotatoTextureMod.Revert();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to " + (applying ? "apply" : "revert") + " AMD / Intel potato graphics:\n" + ex.Message,
                    "AMD / Intel Potato Graphics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async void vortexButton4_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;

            var selected = nvidiaProfileCombo.SelectedItem as NvidiaProfileOption;
            if (selected == null)
            {
                MessageBox.Show("Select a potato graphics profile first.", "NVIDIA Potato Graphics",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await RunNvidiaProfileAsync(selected.FileName, "NVIDIA POTATO GRAPHICS", true);
        }

        private async void vortexButton3_Click(object sender, EventArgs e)
        {
            if (_isBusy) return;
            await RunNvidiaProfileAsync("Fortnite_Default.nip", "REVERT NVIDIA POTATO", false);
        }

        private async Task RunNvidiaProfileAsync(string nipFileName, string title, bool applying)
        {
            Form parentForm = FindForm();
            if (parentForm == null) return;

            _isBusy = true;
            try
            {
                await TweakProgressOverlay.ExecuteWithOverlayAsync(parentForm, title, applying, async () =>
                {
                    await Task.Run(() => ImportNvidiaProfile(nipFileName));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to apply NVIDIA potato graphics:\n" + ex.Message,
                    "NVIDIA Potato Graphics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private static void ImportNvidiaProfile(string nipFileName)
        {
            string inspectorPath = FortniteOptimizer.EmbeddedAssets.Extract(Path.Combine("Resources", "Potato", "Nvidia", "nvidiaProfileInspector.exe"));
            string nipPath = FortniteOptimizer.EmbeddedAssets.Extract(Path.Combine("Resources", "Potato", "Nvidia", nipFileName));
            string nvidiaDir = Path.GetDirectoryName(inspectorPath);

            if (!File.Exists(inspectorPath))
                throw new FileNotFoundException("nvidiaProfileInspector.exe was not found.", inspectorPath);

            if (!File.Exists(nipPath))
                throw new FileNotFoundException("Selected NVIDIA profile was not found.", nipPath);

            // nvidiaProfileInspector 2.4.x imports on startup when given a .nip path.
            // It may leave the UI open, so we wait briefly then close it.
            var psi = new ProcessStartInfo
            {
                FileName = inspectorPath,
                Arguments = "\"" + nipPath + "\"",
                WorkingDirectory = nvidiaDir,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Could not start NVIDIA Profile Inspector.");

                if (!process.WaitForExit(5000))
                {
                    try
                    {
                        if (!process.HasExited)
                            process.CloseMainWindow();
                    }
                    catch { }

                    if (!process.WaitForExit(2000))
                    {
                        try
                        {
                            if (!process.HasExited)
                                process.Kill();
                        }
                        catch { }
                    }
                }
            }
        }

        private void ShowPreviewModal(string gpuType, string nvidiaProfile = null)
        {
            Form parentForm = FindForm();
            if (parentForm != null)
            {
                PotatoPreviewOverlay.Show(parentForm, gpuType, null, nvidiaProfile);
            }
        }
    }
}
