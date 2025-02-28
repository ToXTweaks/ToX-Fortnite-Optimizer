using CuoreUI.Components;
using Fortnite_Optimizer.TweaksTabs;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DiscordRPC;
namespace Fortnite_Optimizer
{
    public partial class FNOptimizer : Form
    {
        private string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToXFNOptimizer", "AudioState.txt");
        public string appDataPath;
        private string settingsFile;
        public static FNOptimizer Instance { get; private set; }
        public FNOptimizer()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            InitializeDiscordRPC();
            UpdateStyles();
            Instance = this;
            TopMost = true;
            this.Opacity = 0.98;
        }
        private void Welcome()
        {
            if (File.Exists(configPath))
            {
                string audioState = File.ReadAllText(configPath).Trim();

                if (audioState == "ON")
                {
                    SoundPlayer player = new SoundPlayer(Properties.Resources.hi);
                    player.Play();
                }
                else if (audioState == "OFF")
                {
                    
                }
            }
            else
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.hi);
                player.Play();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            new Move(this, new Control[] { leftPanel, title1, title2, BGPanel, MainPanel, prempanel });
            TopMost = false;
            LowTaperFade.ShowSync(BGPanel);
            BGPanel.Visible = true;
            cDashboard.PerformClick();

            Welcome();
        }
        public async void DisplayFormInPanel(UserControl formToDisplay, Guna2Panel targetPanel, Guna2Transition transition)
        {
            transition.HideSync(targetPanel);
            await Task.Run(() =>
            {
                targetPanel.Invoke((Action)(() =>
                {
                    targetPanel.Controls.Clear();
                    formToDisplay.Dock = DockStyle.Fill;
                    targetPanel.Controls.Add(formToDisplay);
                    transition.ShowSync(targetPanel);
                    formToDisplay.Show();
                }));
            });
        }
        public async static void Select(Control container, Guna2Panel targetPanel, Guna2Button clickedButton, Guna2VSeparator separator, Guna2Transition transition, UserControl page)
        {
            var buttons = container.Controls.OfType<Guna2Button>().ToArray();
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                bool isClicked = button == clickedButton;
                button.ForeColor = isClicked ? Color.White : Color.FromArgb(150, 150, 150);

                switch (button.Name)
                {
                    case "cDashboard":
                        button.Image = isClicked ? Properties.Resources.dashboard : Properties.Resources.dashboard_;
                        break;
                    case "cTweaks":
                        button.Image = isClicked ? Properties.Resources.tweaks : Properties.Resources.tweaks_;
                        break;
                    case "cPotato":
                        button.Image = isClicked ? Properties.Resources.potato : Properties.Resources.potato_;
                        break;
                    case "cExtra":
                        button.Image = isClicked ? Properties.Resources.bonus : Properties.Resources.bonus_;
                        break;
                    case "cCredits":
                        button.Image = isClicked ? Properties.Resources.about : Properties.Resources.about_;
                        break;
                }

                if (isClicked)
                {
                    separator.Location = new Point(239, button.Location.Y);
                    transition.HideSync(targetPanel);
                    await Task.Run(() =>
                    {
                        targetPanel.Invoke((Action)(() =>
                        {
                            targetPanel.Controls.Clear();
                            page.Dock = DockStyle.Fill;
                            targetPanel.Controls.Add(page);
                            transition.ShowSync(targetPanel);
                            page.Show();
                        }));
                    });
                }
            }
        }
        private void logout_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #region Control Pages
        private void cDashboard_Click(object sender, EventArgs e)
        {
            Select(leftPanel, MainPanel, (Guna2Button)sender, guna2VSeparator1, LowTaperFade, new Dashboard());
        }
        private void cTweaks_Click(object sender, EventArgs e)
        {
            Select(leftPanel, MainPanel, (Guna2Button)sender, guna2VSeparator1, LowTaperFade, new Tweaks());
        }

        private void cPotato_Click(object sender, EventArgs e)
        {
            Select(leftPanel, MainPanel, (Guna2Button)sender, guna2VSeparator1, LowTaperFade, new Potato());

        }
        private void cExtra_Click(object sender, EventArgs e)
        {
            Select(leftPanel, MainPanel, (Guna2Button)sender, guna2VSeparator1, LowTaperFade, new Extra());

        }
        private void cCredits_Click(object sender, EventArgs e)
        {
            Select(leftPanel, MainPanel, (Guna2Button)sender, guna2VSeparator1, LowTaperFade, new Credits());

        }
        #endregion
        private Guna2CircleProgressBar progressBar;
        public async void ExecuteTweaks(Guna2ToggleSwitch @switch, string tweaks, string revert)
        {
            Form modalForm = new Form
            {
                BackColor = Color.FromArgb(32, 33, 42),
                Opacity = 0.8,
                FormBorderStyle = FormBorderStyle.None,
                Size = Size,
                StartPosition = FormStartPosition.Manual
            };

            modalForm.Location = new Point(
                Location.X + (Width - modalForm.Width) / 2,
                Location.Y + (Height - modalForm.Height) / 2
            );

            cuiFormRounder formRounder = new cuiFormRounder
            {
                Rounding = 10,
                OutlineColor = Color.FromArgb(32, 33, 42),
                TargetForm = modalForm
            };

            progressBar = new Guna2CircleProgressBar
            {
                Size = new Size(150, 150),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                FillColor = Color.FromArgb(40, 41, 52),
                ProgressColor = Color.FromArgb(80, 161, 248),
                ProgressColor2 = Color.FromArgb(80, 129, 249),
                FillThickness = 20,
                FillOffset = 0,
                ProgressThickness = 20,
                ProgressEndCap = System.Drawing.Drawing2D.LineCap.Round,
                ProgressStartCap = System.Drawing.Drawing2D.LineCap.Round,
            };

            modalForm.Controls.Add(progressBar);

            modalForm.Load += (s, ev) =>
            {
                progressBar.Location = new Point(
                    (modalForm.ClientSize.Width - progressBar.Width) / 2,
                    (modalForm.ClientSize.Height - progressBar.Height) / 2
                );
                timer1.Start();
            };

            modalForm.Show(this);

            await Task.Delay(1500);

            if (@switch.Checked)
            {
                await Task.Run(() => ExecuteBatchCommands(tweaks));
            }
            else
            {
                await Task.Run(() => ExecuteBatchCommands(revert));
            }

            timer1.Stop();

            modalForm.Close();
            modalForm.Dispose();

            if (File.Exists(configPath))
            {
                string audioState = File.ReadAllText(configPath).Trim();

                if (audioState == "ON")
                {
                    SoundPlayer player = new SoundPlayer(Properties.Resources.Apply);
                    player.Play();
                }
                else if (audioState == "OFF")
                {

                }
            }
            else
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Apply);
                player.Play();
            }

            if (@switch.Checked)
            {
                PopUp.Show("Tweaks Applied Successfully!");
            }
            else
            {
                PopUp.Show("Tweaks Reverted Successfully!");
            }

            SaveToggleStates(@switch);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar.Increment(5);
            if (progressBar.Value == progressBar.Maximum)
            {
                progressBar.Value = 0;
                return;
            }
        }
        private void premiumTweaks_Click(object sender, EventArgs e)
        {
            Process.Start("https://toxtweaks.com/");
        }
        public async Task ExecuteBatchCommands(string batchCommands)
        {
            await Task.Run(() =>
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe")
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (Process process = Process.Start(processInfo))
                {
                    using (StreamWriter sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine(batchCommands);
                        }
                    }
                    process.WaitForExit();
                }
            });
        }
        public void SaveToggleStates(Guna2ToggleSwitch toggleSwitch)
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            settingsFile = Path.Combine(appDataPath, "ToggleStates.txt");
            Dictionary<string, string> toggleStates = new Dictionary<string, string>();

            if (File.Exists(settingsFile))
            {
                foreach (string line in File.ReadAllLines(settingsFile))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        toggleStates[parts[0]] = parts[1];
                    }
                }
            }

            toggleStates[toggleSwitch.Name] = toggleSwitch.Checked ? "true" : "false";

            File.WriteAllLines(settingsFile, toggleStates.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }


        public void ApplyToggleStates()
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            settingsFile = Path.Combine(appDataPath, "ToggleStates.txt");
            if (!File.Exists(settingsFile)) return;

            string[] lines = File.ReadAllLines(settingsFile);

            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string toggleName = parts[0].Trim();
                    bool state = parts[1].Trim().ToLower() == "true";

                    Control[] controls = Controls.Find(toggleName, true);
                    if (controls.Length > 0 && controls[0] is Guna2ToggleSwitch toggleSwitch)
                    {
                        toggleSwitch.Checked = state;
                    }
                }
            }
        }
        
        static void InitializeDiscordRPC()
        {
            if (IsDiscordRunning())
            {
                // Create a Discord client with your client ID
                DiscordRpcClient client = new DiscordRpcClient("1344951286226878585");

                // Subscribe to events
                client.OnReady += (sender, e) =>
                {

                };

                client.OnPresenceUpdate += (sender, e) =>
                {

                };

                // Connect to the Discord RPC
                client.Initialize();

                // Create the buttons
                DiscordRPC.Button DiscordServerButton = new DiscordRPC.Button() { Label = "Try For Free!", Url = "https://github.com/ToXTweaks/ToX-Fortnite-Optimizer/releases/" };
                DiscordRPC.Button BuyNowButton = new DiscordRPC.Button() { Label = "Join Discord!", Url = "https://discord.gg/toxtweaks" };

                // Set the rich presence
                // Call this as many times as you want and anywhere in your code.
                client.SetPresence(new RichPresence()
                {
                    Details = "A Optimization Tool Specifically",
                    State = "Designed Only For Fortnite 🚀",
                    Assets = new Assets()
                    {
                        LargeImageKey = "https://raw.githubusercontent.com/ToXTweaks/Resources-Free/refs/heads/main/logo_fn_tox.png",
                        LargeImageText = "via ToX Tweaks",
                    },
                    Buttons = new DiscordRPC.Button[] { DiscordServerButton, BuyNowButton }
                });
            }
            else
            {

            }
        }

        private static bool IsDiscordRunning()
        {
            // Check if Discord is running by searching for its process
            Process[] processes = Process.GetProcessesByName("Discord");
            return processes.Length > 0;
        }
    }
}
