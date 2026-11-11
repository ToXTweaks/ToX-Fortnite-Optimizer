using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace FortniteOptimizer.UserControls
{
    public class ProSettingsOverlay : Control
    {
        private readonly Timer _fadeTimer;
        private float _fadeAlpha = 0f;
        private bool _isClosing = false;
        private Form _parentForm;

        // Customization
        private string _playerName = "PETERBOT";
        private string _teamOrg = "Falcons Esports";
        private string _targetUrl = "https://prosettings.net/players/shxrk/";
        private Color _accentColor = Color.FromArgb(2, 109, 228);
        private Color _accentGlow = Color.FromArgb(0, 210, 190);
        private Color _cardBgTop = Color.FromArgb(22, 28, 42);
        private Color _cardBgBottom = Color.FromArgb(10, 12, 18);

        private Rectangle _cardBounds = Rectangle.Empty;
        private Rectangle _closeBtnBounds = Rectangle.Empty;
        private bool _isCloseHovered = false;

        private GraphicsPath _cachedCardPath;
        private GraphicsPath _cachedShadowPath;
        private GraphicsPath _cachedClosePath;
        private Font _titleFont;
        private Font _subFont;
        private Font _statusFont;
        private WebView2 _webView;
        private static CoreWebView2Environment _sharedEnvironment;
        private bool _isPageLoaded = false;
        private float _spinnerAngle = 0f;

        public ProSettingsOverlay()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor, true);

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Visible = false;

            try
            {
                _titleFont = new Font("Bahnschrift", 12f, FontStyle.Bold);
                _subFont = new Font("Bahnschrift", 8.5f, FontStyle.Regular);
                _statusFont = new Font("Bahnschrift", 9f, FontStyle.Regular);
            }
            catch
            {
                _titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
                _subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                _statusFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            }

            _webView = new WebView2
            {
                Visible = false,
                TabStop = false,
                DefaultBackgroundColor = Color.FromArgb(14, 18, 28)
            };
            Controls.Add(_webView);

            _fadeTimer = new Timer { Interval = 10 };
            _fadeTimer.Tick += OnFadeTimerTick;
        }

        public static ProSettingsOverlay Show(Form parentForm, string playerName, string teamOrg, string url)
        {
            if (parentForm == null || parentForm.IsDisposed) return null;

            // Remove existing overlay if any
            foreach (Control c in parentForm.Controls)
            {
                if (c is ProSettingsOverlay existing)
                {
                    parentForm.Controls.Remove(existing);
                    existing.Dispose();
                    break;
                }
            }

            var overlay = new ProSettingsOverlay();
            overlay.Configure(playerName, teamOrg, url);
            overlay.AttachToForm(parentForm);
            overlay.StartOpen();
            overlay.InitializeAndNavigate();

            return overlay;
        }

        public void Configure(string playerName, string teamOrg, string url)
        {
            _playerName = playerName.ToUpperInvariant();
            _teamOrg = teamOrg;
            _targetUrl = url;
            _isPageLoaded = false;
        }

        private async void InitializeAndNavigate()
        {
            try
            {
                if (_sharedEnvironment == null)
                {
                    string userDataFolder = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ToX Fortnite Optimizer",
                        "WebViewCache"
                    );

                    var options = new CoreWebView2EnvironmentOptions(
                        "--disable-background-networking --disable-features=Translate,OptimizationHints,MediaRouter --enable-gpu-rasterization --enable-zero-copy"
                    );

                    _sharedEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                }

                await _webView.EnsureCoreWebView2Async(_sharedEnvironment);

                // Configure settings for speed
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

                // Block ad / gambling network requests before they load.
                try { _webView.CoreWebView2.RemoveWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All); } catch { }
                _webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                _webView.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested;
                _webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;

                // Inject CSS/DOM ad-blocker BEFORE document parses.
                string earlyInjectScript = @"
                    (function() {
                        const css = `
                            header.site-header, footer.site-footer, .site-header, .site-footer,
                            .e-con.e-flex.e-con-boxed.e-con-equal, .e-con.e-flex.e-con-boxed,
                            .e-con-inner.e-flex, .e-con.e-flex, .sticky-share,
                            iframe[src*='ads'], iframe[src*='doubleclick'], iframe[src*='googlesyndication'],
                            iframe[src*='casino'], iframe[src*='bet'], iframe[src*='gambling'],
                            iframe[src*='stake'], iframe[src*='roobet'], iframe[src*='gamble'],
                            .google-auto-placed, ins.adsbygoogle, #ezmobfooter, .ad-banner,
                            div[id*='ad-'], div[class*='ad-'], div[class*='adsbygoogle'],
                            div[class*='sponsor'], div[class*='promo'], aside[class*='ad'],
                            [data-ad], [data-ad-slot], [data-google-query-id],
                            .cookie-banner, .cookie-consent, #cookie-law-info-bar,
                            #onetrust-banner-sdk, #onetrust-consent-sdk, .onetrust-pc-dark-filter,
                            #CybotCookiebotDialog, #CybotCookiebotDialogBodyUnderlay,
                            .qc-cmp2-container, .qc-cmp-ui-container, .qc-cmp2-summary-section,
                            [id*='sp_message'], [class*='sp_veil'], [class*='message-overlay'],
                            .fc-consent-root, .fc-dialog-overlay, .fc-dialog-container, .fc-dialog,
                            .fc-iframe-overlay, #google-fc-dialog, iframe[id*='googlefc'],
                            iframe[src*='fundingchoices'], iframe[src*='privacy-mgmt'],
                            iframe[src*='sourcepoint'], iframe[src*='consentmanager'],
                            iframe[src*='cookiebot'], iframe[src*='cookielaw'],
                            #didomi-host, .didomi-popup-container, #cmplz-cookiebanner-container,
                            .cky-consent-container, #BorlabsCookieBox, #BorlabsCookieWidget,
                            .privacy-settings, [aria-label*='Privacy'], [aria-modal='true'],
                            div[role='dialog'], .modal-backdrop, .consent-modal, .consent-overlay,
                            [href*='casino'], [href*='gambling'], [href*='stake.com'],
                            [href*='roobet'], [href*='gamble'], [href*='betting'],
                            a[href*='bet365'], a[href*='draftkings'], a[href*='fanduel'] {
                                display: none !important;
                                visibility: hidden !important;
                                height: 0 !important;
                                max-height: 0 !important;
                                overflow: hidden !important;
                                pointer-events: none !important;
                            }
                            body {
                                background-color: #0d111a !important;
                                color: #e2e8f0 !important;
                            }
                        `;

                        const style = document.createElement('style');
                        style.id = 'fortnite-optimizer-adblock';
                        style.textContent = css;

                        const apply = () => {
                            if (document.head && !document.getElementById('fortnite-optimizer-adblock')) {
                                document.head.appendChild(style);
                            }
                            stripAds();
                        };

                        const isAdNode = (el) => {
                            if (!el || el.nodeType !== 1) return false;
                            const tag = (el.tagName || '').toLowerCase();
                            if (tag === 'ins' && (el.className || '').toString().indexOf('adsbygoogle') >= 0) return true;
                            if (tag === 'iframe') {
                                const src = (el.getAttribute('src') || '').toLowerCase();
                                return /(ads|doubleclick|googlesyndication|casino|bet|gambling|stake|roobet|gamble)/.test(src);
                            }
                            const id = (el.id || '').toLowerCase();
                            const cls = (el.className || '').toString().toLowerCase();
                            const href = ((el.getAttribute && el.getAttribute('href')) || '').toLowerCase();
                            const text = ((el.innerText || el.textContent || '') + '').toLowerCase().slice(0, 200);
                            if (/(^|[-_])ad(s|[-_]|$)|sponsor|promo-banner|ezmob|google-auto-placed/.test(id + ' ' + cls)) return true;
                            if (/(casino|gambling|stake\.com|roobet|bet365|draftkings|fanduel|sportsbook)/.test(href)) return true;
                            if (/(casino|gambling|claim bonus|deposit|sportsbook|slot machine)/.test(text) && (cls.indexOf('widget') >= 0 || cls.indexOf('banner') >= 0 || tag === 'aside')) return true;
                            return false;
                        };

                        const dismissPrivacy = () => {
                            try {
                                const kill = (el) => {
                                    if (!el || el === document.body || el === document.documentElement || el === document.head) return;
                                    try { el.remove(); } catch (e) {
                                        try { el.style.setProperty('display', 'none', 'important'); } catch (e2) {}
                                    }
                                };

                                const clickAccept = (root) => {
                                    if (!root || !root.querySelectorAll) return;
                                    const buttons = root.querySelectorAll('button, a, [role=""button""], input[type=""button""]');
                                    for (const btn of buttons) {
                                        const t = ((btn.innerText || btn.textContent || btn.value || '') + '').replace(/\s+/g, ' ').trim().toLowerCase();
                                        if (t === 'accept all' || t === 'allow all' || t === 'i agree' || t === 'agree' || t.indexOf('accept all') === 0) {
                                            try { btn.click(); } catch (e) {}
                                        }
                                    }
                                    root.querySelectorAll('*').forEach((el) => {
                                        if (el.shadowRoot) clickAccept(el.shadowRoot);
                                    });
                                    root.querySelectorAll('iframe').forEach((frame) => {
                                        try { if (frame.contentDocument) clickAccept(frame.contentDocument); } catch (e) {}
                                    });
                                };

                                clickAccept(document);

                                document.querySelectorAll('[role=""dialog""], [aria-modal=""true""], .qc-cmp2-container, .qc-cmp-ui-container, #onetrust-consent-sdk, #onetrust-banner-sdk, #CybotCookiebotDialog, #didomi-host, .fc-consent-root, .fc-dialog-overlay, .fc-dialog-container, #google-fc-dialog, [id*=""sp_message""], [id*=""googlefc""], #cmplz-cookiebanner-container, .cky-consent-container, #BorlabsCookieBox').forEach(kill);

                                const consentKey = /fundingchoices|google-fc|googlefc|fc-consent|fc-dialog|fc-iframe|onetrust|cookiebot|cookieyes|cky-|cmplz|borlabs|didomi|sp_message|sp-message|qc-cmp|privacy-mgmt|sourcepoint|consent-root|consent-modal|cookie-banner|cookie-consent/;
                                const nodes = document.querySelectorAll('div, section, aside, iframe');
                                for (const el of nodes) {
                                    const id = (el.id || '').toLowerCase();
                                    const cls = (typeof el.className === 'string' ? el.className : '').toLowerCase();
                                    const src = ((el.getAttribute && el.getAttribute('src')) || '').toLowerCase();
                                    if (consentKey.test(id + ' ' + cls + ' ' + src)) {
                                        kill(el);
                                        continue;
                                    }

                                    let style;
                                    try { style = window.getComputedStyle(el); } catch (e) { continue; }
                                    const z = parseInt(style.zIndex || '0', 10);
                                    if (!(z >= 1000 && (style.position === 'fixed' || style.position === 'absolute'))) continue;

                                    const rect = el.getBoundingClientRect();
                                    if (rect.width < 180 || rect.height < 100) continue;
                                    const text = ((el.innerText || '') + '').slice(0, 600).toLowerCase();
                                    const looksLikeConsent = text.indexOf('privacy') >= 0 || text.indexOf('cookie') >= 0 || text.indexOf('consent') >= 0 || text.indexOf('accept all') >= 0 || text.indexOf('partners') >= 0;
                                    const emptyShell = text.trim().length < 40 && rect.width > window.innerWidth * 0.45 && rect.height > window.innerHeight * 0.35;
                                    if (looksLikeConsent || emptyShell || (el.tagName === 'IFRAME' && rect.width > window.innerWidth * 0.4)) {
                                        kill(el);
                                    }
                                }

                                if (document.body) document.body.style.setProperty('overflow', 'auto', 'important');
                                document.documentElement.style.setProperty('overflow', 'auto', 'important');
                            } catch (e) {}
                        };

                        const stripAds = () => {
                            try {
                                dismissPrivacy();
                                document.querySelectorAll('iframe, ins.adsbygoogle, [data-ad], [data-ad-slot], [id*=""ad""], [class*=""ad-""], [class*=""ads""], aside, a[href]').forEach((el) => {
                                    if (isAdNode(el)) {
                                        el.remove();
                                    }
                                });
                            } catch (e) {}
                        };

                        if (document.head) apply();
                        else document.addEventListener('DOMContentLoaded', apply);

                        const observer = new MutationObserver(() => stripAds());
                        const startObserver = () => {
                            if (document.documentElement) {
                                observer.observe(document.documentElement, { childList: true, subtree: true });
                            }
                        };
                        if (document.documentElement) startObserver();
                        else document.addEventListener('DOMContentLoaded', startObserver);

                        setInterval(stripAds, 500);
                        setTimeout(dismissPrivacy, 200);
                        setTimeout(dismissPrivacy, 800);
                        setTimeout(dismissPrivacy, 2000);
                    })();
                ";

                await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(earlyInjectScript);

                _webView.CoreWebView2.DOMContentLoaded += (sender, args) =>
                {
                    _isPageLoaded = true;
                    _webView.Visible = true;
                    Invalidate();
                };

                _webView.CoreWebView2.NavigationCompleted += (sender, args) =>
                {
                    _isPageLoaded = true;
                    _webView.Visible = true;
                    Invalidate();
                };

                _webView.CoreWebView2.Navigate(_targetUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load WebView: " + ex.Message, "Pro Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CloseOverlay();
            }
        }

        private static readonly string[] BlockedHostFragments =
        {
            "doubleclick.net",
            "googlesyndication.com",
            "googleadservices.com",
            "pagead2.googlesyndication",
            "adservice.google",
            "adnxs.com",
            "adsrvr.org",
            "amazon-adsystem.com",
            "taboola.com",
            "outbrain.com",
            "rainbet.com",
            "criteo.com",
            "pubmatic.com",
            "rubiconproject.com",
            "openx.net",
            "moatads.com",
            "scorecardresearch.com",
            "quantserve.com",
            "exoclick.com",
            "popads.net",
            "propellerads.com",
            "adsterra.com",
            "mgid.com",
            "revcontent.com",
            "ezmob",
            "casino",
            "gambling",
            "stake.com",
            "roobet.com",
            "rollbit.com",
            "duelbits.com",
            "bc.game",
            "gamdom.com",
            "csgoempire",
            "thunderpick",
            "bet365",
            "draftkings",
            "fanduel",
            "sportsbet",
            "betting",
            "gamble",
            "luckyblock",
            "metawin",
            "shuffle.com",
            "fundingchoicesmessages.google.com",
            "fundingchoices.google.com",
            "google.com/fundingchoices",
            "privacy-mgmt.com",
            "sp-prod.net",
            "sourcepoint",
            "consentmanager.net",
            "consentmanager.mgr",
            "cmp.quantcast.com",
            "quantcast.mgr.consensu.org",
            "consent.cookiebot.com",
            "consent.cookiebot.eu",
            "cookiebot.com",
            "cdn.cookielaw.org",
            "cookielaw.org",
            "onetrust.com",
            "usercentrics.eu",
            "cmp.osano.com"
        };

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            try
            {
                string uri = e.Request?.Uri;
                if (string.IsNullOrEmpty(uri))
                    return;

                string lower = uri.ToLowerInvariant();
                for (int i = 0; i < BlockedHostFragments.Length; i++)
                {
                    if (lower.IndexOf(BlockedHostFragments[i], StringComparison.Ordinal) >= 0)
                    {
                        e.Response = _webView.CoreWebView2.Environment.CreateWebResourceResponse(
                            null, 204, "Blocked", "Content-Type: text/plain");
                        return;
                    }
                }
            }
            catch { }
        }

        public void AttachToForm(Form form)
        {
            _parentForm = form;
            Bounds = new Rectangle(0, 0, form.ClientSize.Width, form.ClientSize.Height);
            form.Controls.Add(this);
            BringToFront();
            UpdateLayout();

            form.Resize += OnParentFormResize;
        }

        private void OnParentFormResize(object sender, EventArgs e)
        {
            if (_parentForm != null)
            {
                Bounds = new Rectangle(0, 0, _parentForm.ClientSize.Width, _parentForm.ClientSize.Height);
                UpdateLayout();
                Invalidate();
            }
        }

        private void UpdateLayout()
        {
            if (Width <= 0 || Height <= 0) return;

            // Clip this control's region to match the parent form's 10px rounded corners
            using (GraphicsPath formRoundedPath = CreateRoundRectPath(new Rectangle(0, 0, Width, Height), 10))
            {
                Region oldReg = this.Region;
                this.Region = new Region(formRoundedPath);
                oldReg?.Dispose();
            }

            int cardW = Math.Min(880, Math.Max(400, Width - 50));
            int cardH = Math.Min(540, Math.Max(300, Height - 50));
            int cardX = (Width - cardW) / 2;
            int cardY = (Height - cardH) / 2;

            _cardBounds = new Rectangle(cardX, cardY, cardW, cardH);
            _closeBtnBounds = new Rectangle(_cardBounds.Right - 42, _cardBounds.Top + 14, 28, 28);

            int webX = _cardBounds.Left + 12;
            int webY = _cardBounds.Top + 54;
            int webW = _cardBounds.Width - 24;
            int webH = _cardBounds.Height - 66;

            _webView.Bounds = new Rectangle(webX, webY, webW, webH);

            _cachedCardPath?.Dispose();
            _cachedCardPath = CreateRoundRectPath(_cardBounds, 14);

            _cachedShadowPath?.Dispose();
            _cachedShadowPath = CreateRoundRectPath(new Rectangle(_cardBounds.X - 4, _cardBounds.Y - 2, _cardBounds.Width + 8, _cardBounds.Height + 10), 16);

            _cachedClosePath?.Dispose();
            _cachedClosePath = CreateRoundRectPath(_closeBtnBounds, 8);
        }

        public void StartOpen()
        {
            _fadeAlpha = 0.35f;
            _isClosing = false;
            Visible = true;
            Focus();
            _fadeTimer.Start();
        }

        public void CloseOverlay()
        {
            if (_isClosing) return;
            _isClosing = true;
            _fadeTimer.Start();
        }

        private void OnFadeTimerTick(object sender, EventArgs e)
        {
            if (!_isPageLoaded)
            {
                _spinnerAngle = (_spinnerAngle + 12f) % 360f;
            }

            if (!_isClosing)
            {
                _fadeAlpha += 0.35f;
                if (_fadeAlpha >= 1f)
                {
                    _fadeAlpha = 1f;
                    if (_isPageLoaded)
                    {
                        _fadeTimer.Stop();
                    }
                }
            }
            else
            {
                _fadeAlpha -= 0.40f;
                if (_fadeAlpha <= 0f)
                {
                    _fadeAlpha = 0f;
                    _fadeTimer.Stop();
                    DisposeAndRemove();
                    return;
                }
            }

            Invalidate();
        }

        private void DisposeAndRemove()
        {
            if (_parentForm != null)
            {
                _parentForm.Resize -= OnParentFormResize;
                _parentForm.Controls.Remove(this);
            }
            Dispose();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_closeBtnBounds.Contains(e.Location))
            {
                CloseOverlay();
                return;
            }

            // Click outside card to close
            if (!_cardBounds.Contains(e.Location))
            {
                CloseOverlay();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool wasHovered = _isCloseHovered;
            _isCloseHovered = _closeBtnBounds.Contains(e.Location);

            if (wasHovered != _isCloseHovered)
            {
                Invalidate(_closeBtnBounds);
            }

            Cursor = _isCloseHovered ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                CloseOverlay();
                e.Handled = true;
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CloseOverlay();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 1. Full Dim Backdrop (clipped to rounded form)
            using (GraphicsPath formPath = CreateRoundRectPath(ClientRectangle, 10))
            {
                int backdropAlpha = (int)(225 * _fadeAlpha);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(backdropAlpha, 5, 8, 15)))
                {
                    g.FillPath(bgBrush, formPath);
                }
            }

            if (_cardBounds.Width <= 0 || _cardBounds.Height <= 0) return;

            // 2. Drop Shadow
            if (_cachedShadowPath != null)
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb((int)(100 * _fadeAlpha), 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, _cachedShadowPath);
                }
            }

            // 3. Center Modal Card
            if (_cachedCardPath != null)
            {
                using (LinearGradientBrush cardBrush = new LinearGradientBrush(_cardBounds, _cardBgTop, _cardBgBottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(cardBrush, _cachedCardPath);
                }

                using (Pen borderPen = new Pen(Color.FromArgb((int)(170 * _fadeAlpha), _accentColor), 1.5f))
                {
                    g.DrawPath(borderPen, _cachedCardPath);
                }
            }

            // 4. Header Dot Indicator
            int dotSize = 8;
            int dotX = _cardBounds.Left + 20;
            int dotY = _cardBounds.Top + 24;
            using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentGlow)))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Player Title ("PETERBOT • PRO SETTINGS")
            string titleText = $"{_playerName} • PRO SETTINGS";
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), Color.White)))
            {
                g.DrawString(titleText, _titleFont, titleBrush, dotX + 16, _cardBounds.Top + 16);
            }

            // Team Subtitle
            string subText = $"{_teamOrg} • Fortnite Sensitivity, Keybinds & Hardware Settings";
            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb((int)(180 * _fadeAlpha), 170, 185, 205)))
            {
                g.DrawString(subText, _subFont, subBrush, dotX + 16, _cardBounds.Top + 37);
            }

            // 5. Close Button (✕)
            if (_closeBtnBounds.Width > 0 && _cachedClosePath != null)
            {
                Color closeBg = _isCloseHovered
                    ? Color.FromArgb((int)(220 * _fadeAlpha), 220, 40, 60)
                    : Color.FromArgb((int)(80 * _fadeAlpha), 40, 50, 70);

                using (SolidBrush closeBgBrush = new SolidBrush(closeBg))
                {
                    g.FillPath(closeBgBrush, _cachedClosePath);
                }

                using (Pen xPen = new Pen(Color.FromArgb((int)(255 * _fadeAlpha), Color.White), 1.8f))
                {
                    xPen.StartCap = LineCap.Round;
                    xPen.EndCap = LineCap.Round;

                    int pad = 8;
                    g.DrawLine(xPen, _closeBtnBounds.Left + pad, _closeBtnBounds.Top + pad, _closeBtnBounds.Right - pad, _closeBtnBounds.Bottom - pad);
                    g.DrawLine(xPen, _closeBtnBounds.Right - pad, _closeBtnBounds.Top + pad, _closeBtnBounds.Left + pad, _closeBtnBounds.Bottom - pad);
                }
            }

            // 6. Modern Loading Indicator while page prepares
            if (!_isPageLoaded && _cardBounds.Width > 0 && _cardBounds.Height > 0)
            {
                int webX = _cardBounds.Left + 12;
                int webY = _cardBounds.Top + 54;
                int webW = _cardBounds.Width - 24;
                int webH = _cardBounds.Height - 66;

                int cx = webX + webW / 2;
                int cy = webY + webH / 2 - 15;

                int spinnerR = 18;
                using (Pen trackPen = new Pen(Color.FromArgb((int)(30 * _fadeAlpha), 255, 255, 255), 3f))
                {
                    g.DrawEllipse(trackPen, cx - spinnerR, cy - spinnerR, spinnerR * 2, spinnerR * 2);
                }

                using (Pen spinPen = new Pen(Color.FromArgb((int)(240 * _fadeAlpha), _accentGlow), 3f))
                {
                    spinPen.StartCap = LineCap.Round;
                    spinPen.EndCap = LineCap.Round;
                    g.DrawArc(spinPen, cx - spinnerR, cy - spinnerR, spinnerR * 2, spinnerR * 2, _spinnerAngle, 100);
                }

                string loadingText = "Loading pro configuration & settings...";
                if (_statusFont != null)
                {
                    SizeF loadSize = g.MeasureString(loadingText, _statusFont);
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb((int)(170 * _fadeAlpha), 160, 175, 200)))
                    {
                        g.DrawString(loadingText, _statusFont, textBrush, cx - loadSize.Width / 2, cy + spinnerR + 15);
                    }
                }
            }
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = radius * 2f;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            float x = rect.X;
            float y = rect.Y;
            float r = rect.Right;
            float b = rect.Bottom;

            // Top-left
            path.AddArc(x, y, d, d, 180, 90);
            // Top-right
            path.AddArc(r - d, y, d, d, 270, 90);
            // Bottom-right
            path.AddArc(r - d, b - d, d, d, 0, 90);
            // Bottom-left
            path.AddArc(x, b - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fadeTimer?.Stop();
                _fadeTimer?.Dispose();
                _cachedCardPath?.Dispose();
                _cachedShadowPath?.Dispose();
                _cachedClosePath?.Dispose();
                _titleFont?.Dispose();
                _subFont?.Dispose();
                _statusFont?.Dispose();
                _webView?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
