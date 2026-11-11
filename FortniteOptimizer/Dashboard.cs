using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FortniteOptimizer.UserControls;
using Vortex.Ui;
using System.IO;
namespace FortniteOptimizer
{
    public partial class Dashboard : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(
            IntPtr hWnd,
            int Msg,
            int wParam,
            int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WM_WINDOWPOSCHANGED = 0x0047;
        private const int HTCAPTION = 0x2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const double RestOpacity = 0.96;

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        private static readonly FieldInfo RounderSuspendedField =
            typeof(FormRounder).GetField("shouldCloseDown", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo RounderLastLocationField =
            typeof(FormRounder).GetField("lastLocation", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool _dragSuspended;

        private const string CurrentAppVersion = "2.0";
        private const string LatestSuffix = " (Latest)";
        private const string VersionApiUrl =
            "https://api.github.com/repos/ToXTweaks/ToX-Fortnite-Optimizer/releases/latest";

        private readonly homePage Home = new homePage();
        private readonly tweakPage Tweak = new tweakPage();
        private readonly potatoPage Potato = new potatoPage();
        private readonly sweatsPage Sweats = new sweatsPage();
        private readonly creditsPage Credits = new creditsPage();

        private readonly UserControl[] allPages;
        private readonly VortexSidebarButton[] navButtons;

        private Timer latestTypeTimer;
        private int latestTypeIndex;

        public Dashboard()
        {
            InitializeComponent();
            ApplyAppIcon();

            Opacity = RestOpacity;
            DoubleBuffered = true;

            allPages = new UserControl[]
            {
                Home,
                Tweak,
                Potato,
                Sweats,
                Credits
            };

            navButtons = new[]
            {
                homeButton,
                tweaksButton,
                potatoButton,
                sweatsButton,
                creditsButton
            };

            InitializePages();
            ShowPage(Home, homeButton);

            appVersion.Text = "V" + CurrentAppVersion;
            BeginCheckLatestVersion();

            MouseDown += Dashboard_MouseDown;
        }

        private void ApplyAppIcon()
        {
            try
            {
                string iconPath = EmbeddedAssets.Extract(Path.Combine("Resources", "app.ico"));
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                    return;
                }

                using (var associated = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath))
                {
                    if (associated != null)
                        this.Icon = (System.Drawing.Icon)associated.Clone();
                }
            }
            catch { }
        }

        private void BeginCheckLatestVersion()
        {
            Task.Run(() =>
            {
                ReleaseUpdate update = null;
                bool upToDate = false;

                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    update = QueryNewerRelease(out upToDate);
                }
                catch
                {
                    return;
                }

                if (IsDisposed)
                    return;

                if (update != null)
                    PostToUi(() => OfferUpdate(update));
                else if (upToDate)
                    PostToUi(StartLatestTypewriter);
            });
        }

        private static ReleaseUpdate QueryNewerRelease(out bool upToDate)
        {
            upToDate = false;

            using (var client = new TimedWebClient(12000))
            {
                client.Headers.Add("User-Agent", "ToX Fortnite Optimizer");
                string json = client.DownloadString(VersionApiUrl);

                var tag = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                if (!tag.Success)
                    throw new InvalidOperationException();

                string remote = NormalizeVersion(tag.Groups[1].Value);
                if (CompareVersions(NormalizeVersion(CurrentAppVersion), remote) >= 0)
                {
                    upToDate = true;
                    return null;
                }

                string fileName;
                string url;
                if (!TryGetInstallerAsset(json, out fileName, out url))
                    throw new InvalidOperationException();

                return new ReleaseUpdate(remote, fileName, url);
            }
        }

        private static bool TryGetInstallerAsset(string json, out string fileName, out string url)
        {
            fileName = null;
            url = null;

            MatchCollection matches = Regex.Matches(
                json,
                "\"name\"\\s*:\\s*\"([^\"]+\\.exe)\"[\\s\\S]*?\"browser_download_url\"\\s*:\\s*\"(https://github\\.com/ToXTweaks/ToX-Fortnite-Optimizer/releases/download/[^\"]+)\"",
                RegexOptions.IgnoreCase);

            if (matches.Count == 0)
                return false;

            Match chosen = matches[0];
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value.IndexOf("installer", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    chosen = match;
                    break;
                }
            }

            string name = Path.GetFileName(chosen.Groups[1].Value);
            string downloadUrl = chosen.Groups[2].Value;
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;
            if (!IsOfficialDownload(downloadUrl))
                return false;

            fileName = name;
            url = downloadUrl;
            return true;
        }

        private static bool IsOfficialDownload(string downloadUrl)
        {
            Uri uri;
            if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out uri))
                return false;

            return string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                && string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
                && uri.AbsolutePath.StartsWith(
                    "/ToXTweaks/ToX-Fortnite-Optimizer/releases/download/",
                    StringComparison.OrdinalIgnoreCase);
        }

        private void OfferUpdate(ReleaseUpdate update)
        {
            if (update == null || IsDisposed)
                return;

            DialogResult answer = MessageBox.Show(
                this,
                "A new version of ToX Fortnite Optimizer is available (v" + update.Version + ").\r\n\r\nDo you want to install it now?",
                "ToX Fortnite Optimizer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes || IsDisposed)
                return;

            UseWaitCursor = true;
            Task.Run(() =>
            {
                try
                {
                    string installer = DownloadInstaller(update);
                    PostToUi(() => LaunchInstaller(installer));
                }
                catch
                {
                    PostToUi(() =>
                    {
                        UseWaitCursor = false;
                        if (IsDisposed)
                            return;

                        MessageBox.Show(
                            this,
                            "The update could not be downloaded.",
                            "ToX Fortnite Optimizer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    });
                }
            });
        }

        private static string DownloadInstaller(ReleaseUpdate update)
        {
            string folder = Path.Combine(Path.GetTempPath(), "ToX Fortnite Optimizer");
            Directory.CreateDirectory(folder);
            string destination = Path.Combine(folder, update.Version + "-" + update.FileName);

            using (var client = new TimedWebClient(180000))
            {
                client.Headers.Add("User-Agent", "ToX Fortnite Optimizer");
                client.DownloadFile(update.Url, destination);
            }

            if (!File.Exists(destination) || new FileInfo(destination).Length == 0)
                throw new IOException();

            return destination;
        }

        private void LaunchInstaller(string installer)
        {
            UseWaitCursor = false;
            if (IsDisposed)
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = installer,
                UseShellExecute = true
            });
            Close();
        }

        private void PostToUi(Action action)
        {
            if (action == null || IsDisposed)
                return;

            if (IsHandleCreated)
            {
                BeginInvoke(action);
                return;
            }

            EventHandler created = null;
            created = (sender, args) =>
            {
                HandleCreated -= created;
                if (!IsDisposed)
                    BeginInvoke(action);
            };
            HandleCreated += created;
        }

        private void StartLatestTypewriter()
        {
            if (appVersion.Text.EndsWith(LatestSuffix))
                return;

            latestTypeIndex = 0;
            latestTypeTimer = new Timer { Interval = 45 };
            latestTypeTimer.Tick += LatestTypeTimer_Tick;
            latestTypeTimer.Start();
        }

        private void LatestTypeTimer_Tick(object sender, EventArgs e)
        {
            if (latestTypeIndex >= LatestSuffix.Length)
            {
                latestTypeTimer.Stop();
                latestTypeTimer.Dispose();
                latestTypeTimer = null;
                return;
            }

            appVersion.Text = "V" + CurrentAppVersion + LatestSuffix.Substring(0, latestTypeIndex + 1);
            latestTypeIndex++;
        }

        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "0";

            version = version.Trim();
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                version = version.Substring(1);

            return version;
        }

        private static int CompareVersions(string a, string b)
        {
            var partsA = a.Split('.');
            var partsB = b.Split('.');
            int len = Math.Max(partsA.Length, partsB.Length);

            for (int i = 0; i < len; i++)
            {
                int na = i < partsA.Length && int.TryParse(partsA[i], out int xa) ? xa : 0;
                int nb = i < partsB.Length && int.TryParse(partsB[i], out int xb) ? xb : 0;
                if (na != nb)
                    return na.CompareTo(nb);
            }

            return 0;
        }

        private void Dashboard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ENTERSIZEMOVE)
                BeginDragLock();

            base.WndProc(ref m);

            if (m.Msg == WM_WINDOWPOSCHANGED && _dragSuspended)
                SyncOutline(m.LParam);
            else if (m.Msg == WM_EXITSIZEMOVE)
                EndDragLock();
        }

        private void BeginDragLock()
        {
            if (_dragSuspended)
                return;

            _dragSuspended = true;
            // Skip the rounder's deferred move. That queues the border a frame behind the window.
            RounderSuspendedField?.SetValue(formRounder1, true);
        }

        private void SyncOutline(IntPtr lParam)
        {
            WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if ((pos.flags & SWP_NOMOVE) != 0)
                return;

            MoveOutline(pos.x, pos.y);
        }

        private void MoveOutline(int x, int y)
        {
            Form outline = formRounder1?.roundedFormObj;
            if (outline != null && outline.IsHandleCreated && !outline.IsDisposed)
            {
                SetWindowPos(outline.Handle, IntPtr.Zero, x - 1, y - 1, 0, 0,
                    SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }

            Form underlay = formRounder1?.FakeForm;
            if (underlay != null && underlay.IsHandleCreated && !underlay.IsDisposed)
            {
                SetWindowPos(underlay.Handle, IntPtr.Zero, x, y, 0, 0,
                    SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }
        }

        private void EndDragLock()
        {
            if (!_dragSuspended)
                return;

            MoveOutline(Left, Top);
            RounderSuspendedField?.SetValue(formRounder1, false);
            RounderLastLocationField?.SetValue(formRounder1, Location);
            formRounder1?.UpdateRoundedFormRegion();
            _dragSuspended = false;
        }

        private void InitializePages()
        {
            SuspendLayout();

            foreach (var page in allPages)
            {
                page.Location = new Point(229, 42);
                page.Visible = false;
                Controls.Add(page);
            }

            sideBar.BringToFront();

            ResumeLayout(true);
        }

        public void ShowPage(UserControl uc, VortexSidebarButton activeBtn)
        {
            SuspendLayout();

            foreach (var page in allPages)
            {
                page.Visible = page == uc;
            }

            uc.BringToFront();
            sideBar.BringToFront();
            SetActiveButton(activeBtn);

            ResumeLayout(true);
        }

        private void SetActiveButton(VortexSidebarButton activeBtn)
        {
            if (navButtons == null)
                return;

            foreach (var btn in navButtons)
            {
                if (btn == activeBtn)
                {
                    btn.BackgroundColor = Color.FromArgb(200, 2, 109, 228);
                    btn.TextColor = Color.White;
                    btn.IconColor = Color.White;
                }
                else
                {
                    btn.BackgroundColor = Color.Empty;
                    btn.TextColor = Color.White;
                    btn.IconColor = Color.White;
                }
            }
        }

        private void homeButton_Click(object sender, EventArgs e)
        {
            ShowPage(Home, homeButton);
        }

        private void tweaksButton_Click(object sender, EventArgs e)
        {
            ShowPage(Tweak, tweaksButton);
        }

        private void potatoButton_Click(object sender, EventArgs e)
        {
            ShowPage(Potato, potatoButton);
        }

        private void sweatsButton_Click(object sender, EventArgs e)
        {
            ShowPage(Sweats, sweatsButton);
        }

        private void creditsButton_Click(object sender, EventArgs e)
        {
            ShowPage(Credits, creditsButton);
        }

        private void toxtweaksLogo_Click(object sender, EventArgs e)
        {
            OpenLink("https://toxtweaks.com/");
        }

        private void github_Click(object sender, EventArgs e)
        {
            OpenLink("https://github.com/ToXTweaks/ToX-Fortnite-Optimizer");
        }

        private void discord_Click(object sender, EventArgs e)
        {
            OpenLink("https://discord.gg/toxtweaks");
        }

        private void youtube_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.youtube.com/@toxtweaks");
        }

        private void twitter_Click(object sender, EventArgs e)
        {
            OpenLink("https://x.com/ToXTweaks");
        }

        private static void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (latestTypeTimer != null)
            {
                latestTypeTimer.Stop();
                latestTypeTimer.Dispose();
                latestTypeTimer = null;
            }

            base.OnFormClosed(e);
        }

        private sealed class ReleaseUpdate
        {
            public ReleaseUpdate(string version, string fileName, string url)
            {
                Version = version;
                FileName = fileName;
                Url = url;
            }

            public string Version { get; private set; }
            public string FileName { get; private set; }
            public string Url { get; private set; }
        }

        private sealed class TimedWebClient : WebClient
        {
            private readonly int _timeoutMs;

            public TimedWebClient(int timeoutMs)
            {
                _timeoutMs = timeoutMs;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request != null)
                    request.Timeout = _timeoutMs;
                return request;
            }
        }
    }
}