using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Vortex.Ui;

namespace FortniteOptimizer.UserControls
{
    public class PotatoPreviewOverlay : Control
    {
        public enum NvidiaPreviewMode
        {
            Creative,
            BattleRoyale
        }

        private readonly VortexBeforeAfterSlider _slider;
        private readonly VortexButton _creativeBtn;
        private readonly VortexButton _brBtn;
        private readonly Timer _fadeTimer;
        private float _fadeAlpha = 0f;
        private bool _isClosing = false;
        private Form _parentForm;

        private string _title = "POTATO GRAPHICS PREVIEW";
        private string _subtitle = "Before vs After Comparison • Drag the slider horizontally to compare";
        private Color _accentColor = Color.FromArgb(0, 150, 136);
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

        private bool _isNvidiaPreview;
        private string _nvidiaProfile = "Medium";
        private NvidiaPreviewMode _previewMode = NvidiaPreviewMode.Creative;

        public VortexBeforeAfterSlider Slider => _slider;

        public PotatoPreviewOverlay()
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
                _titleFont = new Font("Bahnschrift", 12.5f, FontStyle.Bold);
                _subFont = new Font("Bahnschrift", 8.5f, FontStyle.Regular);
            }
            catch
            {
                _titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
                _subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            }

            _slider = new VortexBeforeAfterSlider
            {
                SmoothMotion = true,
                SmoothSpeed = 0.35,
                SplitPosition = 0.5f,
                BorderRadius = 12,
                BorderSize = 1,
                BorderColor = Color.FromArgb(50, 65, 90),
                ShowSliderGlow = true,
                ShowBeforeTag = true,
                BeforeTagTitle = "BEFORE",
                BeforeTagValue = "DEFAULT TEXTURES",
                ShowAfterTag = true,
                AfterTagTitle = "AFTER",
                AfterTagValue = "POTATO BOOST",
                TagRadius = 10,
                HandleSize = 42
            };

            _creativeBtn = CreateModeButton("CREATIVE");
            _brBtn = CreateModeButton("BATTLE ROYALE");
            _creativeBtn.Click += (s, e) => SetPreviewMode(NvidiaPreviewMode.Creative);
            _brBtn.Click += (s, e) => SetPreviewMode(NvidiaPreviewMode.BattleRoyale);
            _creativeBtn.Visible = false;
            _brBtn.Visible = false;

            Controls.Add(_slider);
            Controls.Add(_creativeBtn);
            Controls.Add(_brBtn);

            _fadeTimer = new Timer { Interval = 10 };
            _fadeTimer.Tick += OnFadeTimerTick;
        }

        private static VortexButton CreateModeButton(string text)
        {
            Font font;
            try { font = new Font("Bahnschrift SemiBold", 8.5f, FontStyle.Bold); }
            catch { font = new Font("Segoe UI", 8.5f, FontStyle.Bold); }

            return new VortexButton
            {
                Text = text,
                Size = new Size(130, 32),
                Cursor = Cursors.Hand,
                CornerCutSize = 8,
                TopLeftCut = true,
                TopRightCut = false,
                BottomLeftCut = false,
                BottomRightCut = true,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = font,
                ForeColor = Color.White,
                UseGradient = false,
                UseCompatibleTextRendering = true,
                EnableLoadingAnimation = false,
                TextAlign = TextAlign.Center,
                ImageAlign = ImageAlign.Center
            };
        }

        public static PotatoPreviewOverlay Show(Form parentForm, string gpuType, Image image = null, string nvidiaProfile = null)
        {
            if (parentForm == null || parentForm.IsDisposed) return null;

            foreach (Control c in parentForm.Controls)
            {
                if (c is PotatoPreviewOverlay existing)
                {
                    parentForm.Controls.Remove(existing);
                    existing.Dispose();
                    break;
                }
            }

            var overlay = new PotatoPreviewOverlay();
            overlay.ConfigureForGpu(gpuType, image, nvidiaProfile);
            overlay.AttachToForm(parentForm);
            overlay.StartOpen();
            return overlay;
        }

        public void ConfigureForGpu(string gpuType, Image image = null, string nvidiaProfile = null)
        {
            _isNvidiaPreview = string.Equals(gpuType, "NVIDIA", StringComparison.OrdinalIgnoreCase);
            _nvidiaProfile = NormalizeProfile(nvidiaProfile);
            _previewMode = NvidiaPreviewMode.Creative;

            if (_isNvidiaPreview)
            {
                _title = "NVIDIA POTATO GRAPHICS PREVIEW";
                _subtitle = "Before vs After • Drag the slider to compare";
                _accentColor = Color.FromArgb(76, 175, 80);

                _slider.BeforeTagTitle = "BEFORE";
                _slider.BeforeTagValue = "DEFAULT GRAPHICS";
                _slider.AfterTagTitle = "AFTER";
                _slider.AfterTagValueColor = Color.FromArgb(76, 220, 80);
                _slider.SliderGlowColor = Color.FromArgb(50, 76, 175, 80);
                _slider.HandleHoverBackColor = Color.FromArgb(40, 60, 45);
                _slider.EnableBeforeEffects = false;

                _creativeBtn.Visible = true;
                _brBtn.Visible = true;
                UpdateModeButtonStyles();
                ApplyNvidiaPreviewImages();
            }
            else
            {
                _title = "AMD / INTEL POTATO GRAPHICS PREVIEW";
                _subtitle = "Before vs After • Drag the slider to compare";
                _accentColor = Color.FromArgb(0, 210, 190);

                _slider.BeforeTagTitle = "BEFORE";
                _slider.BeforeTagValue = "DEFAULT TEXTURES";
                _slider.AfterTagTitle = "AFTER";
                _slider.AfterTagValue = "POTATO TEXTURES";
                _slider.AfterTagValueColor = Color.FromArgb(0, 210, 190);
                _slider.SliderGlowColor = Color.FromArgb(50, 0, 210, 190);
                _slider.HandleHoverBackColor = Color.FromArgb(30, 55, 55);
                _slider.EnableBeforeEffects = false;

                _creativeBtn.Visible = false;
                _brBtn.Visible = false;
                ApplyAmdPreviewImages();
            }

            _slider.BorderRadius = 16;
        }

        private static string NormalizeProfile(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile))
                return "Medium";

            string p = profile.Trim();
            if (p.StartsWith("Low", StringComparison.OrdinalIgnoreCase))
                return "Low";
            if (p.StartsWith("High", StringComparison.OrdinalIgnoreCase))
                return "High";
            return "Medium";
        }

        private void ApplyAmdPreviewImages()
        {
            Image oldBefore = _slider.BeforeImage;
            Image oldAfter = _slider.AfterImage;

            _slider.BeforeImage = LoadImageFromResources(
                "Resources\\Potato\\AmdIntel\\Before_AMD_Potato.jpg",
                "Before_AMD_Potato.jpg");
            _slider.AfterImage = LoadImageFromResources(
                "Resources\\Potato\\AmdIntel\\After_AMD_Potato.jpg",
                "After_AMD_Potato.jpg");

            if (oldBefore != null && !ReferenceEquals(oldBefore, _slider.BeforeImage))
            {
                try { oldBefore.Dispose(); } catch { }
            }
            if (oldAfter != null && !ReferenceEquals(oldAfter, _slider.AfterImage))
            {
                try { oldAfter.Dispose(); } catch { }
            }

            Invalidate();
        }

        private void ApplyNvidiaPreviewImages()
        {
            string viewSuffix = _previewMode == NvidiaPreviewMode.Creative ? "Creative" : "BR";
            string beforeName = "Potato_Before_" + viewSuffix + ".jpg";
            string afterName = "Potato_" + _nvidiaProfile + "_" + viewSuffix + ".jpg";

            Image oldBefore = _slider.BeforeImage;
            Image oldAfter = _slider.AfterImage;

            _slider.BeforeImage = LoadImageFromResources(
                "Resources\\Potato\\Nvidia\\Previews\\" + beforeName,
                beforeName);
            _slider.AfterImage = LoadImageFromResources(
                "Resources\\Potato\\Nvidia\\Previews\\" + afterName,
                afterName);

            if (oldBefore != null && !ReferenceEquals(oldBefore, _slider.BeforeImage))
            {
                try { oldBefore.Dispose(); } catch { }
            }
            if (oldAfter != null && !ReferenceEquals(oldAfter, _slider.AfterImage))
            {
                try { oldAfter.Dispose(); } catch { }
            }

            string modeLabel = _previewMode == NvidiaPreviewMode.Creative ? "CREATIVE" : "BATTLE ROYALE";
            _slider.AfterTagValue = _nvidiaProfile.ToUpperInvariant() + " • " + modeLabel;
            _subtitle = _nvidiaProfile + " potato • Drag the slider to compare";
            UpdateModeButtonStyles();
            Invalidate();
        }

        private void SetPreviewMode(NvidiaPreviewMode mode)
        {
            if (!_isNvidiaPreview || _previewMode == mode)
                return;

            _previewMode = mode;
            ApplyNvidiaPreviewImages();
        }

        private void UpdateModeButtonStyles()
        {
            ApplyModeButtonStyle(_creativeBtn, _previewMode == NvidiaPreviewMode.Creative);
            ApplyModeButtonStyle(_brBtn, _previewMode == NvidiaPreviewMode.BattleRoyale);
        }

        private static void ApplyModeButtonStyle(VortexButton btn, bool selected)
        {
            if (selected)
            {
                // Match NVIDIA APPLY button on potato page
                btn.ButtonColor = Color.FromArgb(150, 76, 175, 80);
                btn.HoverColor = Color.FromArgb(100, 76, 175, 80);
                btn.PressedColor = Color.FromArgb(80, 76, 175, 80);
                btn.ForeColor = Color.Black;
            }
            else
            {
                // Match muted PREVIEW-style inactive state
                btn.ButtonColor = Color.FromArgb(40, 35, 45, 58);
                btn.HoverColor = Color.FromArgb(55, 48, 62, 78);
                btn.PressedColor = Color.FromArgb(30, 28, 36, 48);
                btn.ForeColor = Color.FromArgb(200, 210, 220);
            }
            btn.Invalidate();
        }

        private static Image LoadImageFromResources(params string[] relativePaths)
        {
            try
            {
                foreach (var relative in relativePaths)
                {
                    Image embedded = FortniteOptimizer.EmbeddedAssets.LoadImage(relative);
                    if (embedded != null)
                        return embedded;
                }
            }
            catch { }

            return null;
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

            using (GraphicsPath formRoundedPath = CreateRoundRectPath(new Rectangle(0, 0, Width, Height), 10))
            {
                Region oldReg = this.Region;
                this.Region = new Region(formRoundedPath);
                oldReg?.Dispose();
            }

            int cardW = Math.Min(820, Math.Max(320, Width - 60));
            int cardH = Math.Min(500, Math.Max(260, Height - 60));
            int cardX = (Width - cardW) / 2;
            int cardY = (Height - cardH) / 2;

            _cardBounds = new Rectangle(cardX, cardY, cardW, cardH);
            _closeBtnBounds = new Rectangle(_cardBounds.Right - 42, _cardBounds.Top + 14, 28, 28);

            if (_isNvidiaPreview)
            {
                int btnW = 128;
                int btnH = 32;
                int gap = 8;
                int btnY = _cardBounds.Top + 14;
                int rightEdge = _closeBtnBounds.Left - 12;

                _brBtn.Bounds = new Rectangle(rightEdge - btnW, btnY, btnW, btnH);
                _creativeBtn.Bounds = new Rectangle(_brBtn.Left - gap - btnW, btnY, btnW, btnH);
                _creativeBtn.BringToFront();
                _brBtn.BringToFront();
            }

            int headerH = 62;
            int sliderX = _cardBounds.Left + 16;
            int sliderY = _cardBounds.Top + headerH;
            int sliderW = _cardBounds.Width - 32;
            int sliderH = _cardBounds.Height - headerH - 16;

            _slider.Bounds = new Rectangle(sliderX, sliderY, sliderW, sliderH);

            _cachedCardPath?.Dispose();
            _cachedCardPath = CreateRoundRectPath(_cardBounds, 14);

            _cachedShadowPath?.Dispose();
            _cachedShadowPath = CreateRoundRectPath(new Rectangle(_cardBounds.X - 3, _cardBounds.Y - 1, _cardBounds.Width + 6, _cardBounds.Height + 8), 16);

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
            if (!_isClosing)
            {
                _fadeAlpha += 0.35f;
                if (_fadeAlpha >= 1f)
                {
                    _fadeAlpha = 1f;
                    _fadeTimer.Stop();
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
                Invalidate(_closeBtnBounds);

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

            using (GraphicsPath formPath = CreateRoundRectPath(ClientRectangle, 10))
            {
                int backdropAlpha = (int)(220 * _fadeAlpha);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(backdropAlpha, 5, 8, 15)))
                {
                    g.FillPath(bgBrush, formPath);
                }
            }

            if (_cardBounds.Width <= 0 || _cardBounds.Height <= 0) return;

            if (_cachedCardPath != null)
            {
                if (_cachedShadowPath != null)
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb((int)(90 * _fadeAlpha), 0, 0, 0)))
                    {
                        g.FillPath(shadowBrush, _cachedShadowPath);
                    }
                }

                using (LinearGradientBrush cardBrush = new LinearGradientBrush(_cardBounds, _cardBgTop, _cardBgBottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(cardBrush, _cachedCardPath);
                }

                using (Pen borderPen = new Pen(Color.FromArgb((int)(160 * _fadeAlpha), _accentColor), 1.5f))
                {
                    g.DrawPath(borderPen, _cachedCardPath);
                }
            }

            int titleMaxRight = _isNvidiaPreview ? _creativeBtn.Left - 16 : _closeBtnBounds.Left - 12;
            int titleLeft = _cardBounds.Left + 36;
            int titleWidth = Math.Max(80, titleMaxRight - titleLeft);

            int dotSize = 8;
            int dotX = _cardBounds.Left + 20;
            int dotY = _cardBounds.Top + 24;
            using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentColor)))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), Color.White)))
            using (var titleFormat = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                g.DrawString(_title, _titleFont, titleBrush,
                    new RectangleF(titleLeft, _cardBounds.Top + 16, titleWidth, 20), titleFormat);
            }

            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb((int)(180 * _fadeAlpha), 170, 185, 205)))
            using (var subFormat = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                g.DrawString(_subtitle, _subFont, subBrush,
                    new RectangleF(titleLeft, _cardBounds.Top + 37, titleWidth, 16), subFormat);
            }

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

            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(r - d, y, d, d, 270, 90);
            path.AddArc(r - d, b - d, d, d, 0, 90);
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

                try
                {
                    _slider?.BeforeImage?.Dispose();
                    _slider?.AfterImage?.Dispose();
                    _slider?.Image?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}
