using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FortniteOptimizer.UserControls
{
    public class TweakProgressOverlay : Control
    {
        private readonly Timer _animTimer;
        private float _fadeAlpha = 0f;
        private bool _isClosing = false;
        private Form _parentForm;

        // Customization
        private string _actionTitle = "APPLYING TWEAK";
        private string _tweakTitle = "HIGH CPU PRIORITY";
        private string _statusText = "Applying system optimizations...";
        private Color _accentColor = Color.FromArgb(2, 109, 228);
        private Color _accentGlow = Color.FromArgb(0, 210, 190);
        private Color _cardBgTop = Color.FromArgb(22, 28, 42);
        private Color _cardBgBottom = Color.FromArgb(10, 12, 18);

        private Rectangle _cardBounds = Rectangle.Empty;
        private Rectangle _progressTrackBounds = Rectangle.Empty;

        private float _targetProgress = 0f;
        private float _currentProgress = 0f;
        private bool _isCompleted = false;

        private GraphicsPath _cachedCardPath;
        private GraphicsPath _cachedShadowPath;
        private Font _titleFont;
        private Font _subFont;
        private Font _statusFont;
        private Font _percentFont;

        public TweakProgressOverlay()
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
                _subFont = new Font("Bahnschrift", 9.5f, FontStyle.Bold);
                _statusFont = new Font("Bahnschrift", 8.5f, FontStyle.Regular);
                _percentFont = new Font("Bahnschrift", 9.5f, FontStyle.Bold);
            }
            catch
            {
                _titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
                _subFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                _statusFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                _percentFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }

            _animTimer = new Timer { Interval = 12 };
            _animTimer.Tick += OnAnimTimerTick;
        }

        public static async Task<bool> ExecuteWithOverlayAsync(Form parentForm, string tweakTitle, bool isEnabling, Func<Task> tweakAction)
        {
            if (parentForm == null || parentForm.IsDisposed)
            {
                if (tweakAction != null) await tweakAction();
                return true;
            }


            foreach (Control c in parentForm.Controls)
            {
                if (c is TweakProgressOverlay existing)
                {
                    parentForm.Controls.Remove(existing);
                    existing.Dispose();
                    break;
                }
            }

            var overlay = new TweakProgressOverlay();
            overlay.Configure(tweakTitle, isEnabling);
            overlay.AttachToForm(parentForm);
            overlay.StartOpen();

            overlay.SetProgress(25f, "Initializing tweak configuration...");
            await Task.Delay(120);

            overlay.SetProgress(60f, isEnabling ? "Writing registry & system settings..." : "Reverting changes & restoring defaults...");

            try
            {
                if (tweakAction != null)
                {
                    await tweakAction();
                }
            }
            catch (Exception ex)
            {
                overlay.SetProgress(100f, "Error: " + ex.Message);
                await Task.Delay(600);
                overlay.CloseOverlay();
                return false;
            }

            overlay.SetProgress(88f, "Finalizing optimizations...");
            await Task.Delay(130);

            overlay.SetCompleted(isEnabling ? "Tweak Applied Successfully! \u2713" : "Tweak Reverted Successfully! \u2713");
            await Task.Delay(400);

            overlay.CloseOverlay();
            return true;
        }

        public void Configure(string tweakTitle, bool isEnabling)
        {
            _tweakTitle = tweakTitle.ToUpperInvariant();
            _actionTitle = isEnabling ? "APPLYING TWEAK" : "REVERTING TWEAK";
            _statusText = isEnabling ? "Applying system optimizations..." : "Restoring default configuration...";
            _accentColor = isEnabling ? Color.FromArgb(2, 109, 228) : Color.FromArgb(235, 94, 40);
            _accentGlow = isEnabling ? Color.FromArgb(0, 210, 190) : Color.FromArgb(255, 170, 80);
            _currentProgress = 0f;
            _targetProgress = 0f;
            _isCompleted = false;
        }

        public void SetProgress(float progressPercent, string status)
        {
            _targetProgress = Math.Max(0f, Math.Min(100f, progressPercent));
            if (!string.IsNullOrEmpty(status))
            {
                _statusText = status;
            }
            Invalidate();
        }

        public void SetCompleted(string completedMessage)
        {
            _targetProgress = 100f;
            _currentProgress = 100f;
            _isCompleted = true;
            _statusText = completedMessage;
            _accentColor = Color.FromArgb(16, 185, 129); // Green
            _accentGlow = Color.FromArgb(52, 211, 153);
            Invalidate();
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

            int cardW = Math.Min(520, Math.Max(320, Width - 80));
            int cardH = 220;
            int cardX = (Width - cardW) / 2;
            int cardY = (Height - cardH) / 2;

            _cardBounds = new Rectangle(cardX, cardY, cardW, cardH);
            _progressTrackBounds = new Rectangle(_cardBounds.X + 35, _cardBounds.Y + 115, _cardBounds.Width - 70, 14);

            _cachedCardPath?.Dispose();
            _cachedCardPath = CreateRoundRectPath(_cardBounds, 14);

            _cachedShadowPath?.Dispose();
            _cachedShadowPath = CreateRoundRectPath(new Rectangle(_cardBounds.X - 4, _cardBounds.Y - 2, _cardBounds.Width + 8, _cardBounds.Height + 10), 16);
        }

        public void StartOpen()
        {
            _fadeAlpha = 0.35f;
            _isClosing = false;
            Visible = true;
            Focus();
            _animTimer.Start();
        }

        public void CloseOverlay()
        {
            if (_isClosing) return;
            _isClosing = true;
            _animTimer.Start();
        }

        private void OnAnimTimerTick(object sender, EventArgs e)
        {
            if (_currentProgress < _targetProgress)
            {
                _currentProgress += (_targetProgress - _currentProgress) * 0.35f + 1f;
                if (_currentProgress > _targetProgress) _currentProgress = _targetProgress;
            }

            if (!_isClosing)
            {
                _fadeAlpha += 0.35f;
                if (_fadeAlpha >= 1f)
                {
                    _fadeAlpha = 1f;
                }
            }
            else
            {
                _fadeAlpha -= 0.40f;
                if (_fadeAlpha <= 0f)
                {
                    _fadeAlpha = 0f;
                    _animTimer.Stop();
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

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (GraphicsPath formPath = CreateRoundRectPath(ClientRectangle, 10))
            {
                int backdropAlpha = (int)(225 * _fadeAlpha);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(backdropAlpha, 5, 8, 15)))
                {
                    g.FillPath(bgBrush, formPath);
                }
            }

            if (_cardBounds.Width <= 0 || _cardBounds.Height <= 0) return;

            if (_cachedShadowPath != null)
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb((int)(100 * _fadeAlpha), 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, _cachedShadowPath);
                }
            }

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

            int dotSize = 9;
            int dotX = _cardBounds.Left + 25;
            int dotY = _cardBounds.Top + 26;
            using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentColor)))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            using (Pen glowPen = new Pen(Color.FromArgb((int)(90 * _fadeAlpha), _accentGlow), 2f))
            {
                g.DrawEllipse(glowPen, dotX - 2, dotY - 2, dotSize + 4, dotSize + 4);
            }

            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), Color.White)))
            {
                g.DrawString(_actionTitle, _titleFont, titleBrush, dotX + 18, _cardBounds.Top + 20);
            }

            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb((int)(200 * _fadeAlpha), 170, 185, 210)))
            {
                g.DrawString(_tweakTitle, _subFont, subBrush, dotX + 18, _cardBounds.Top + 45);
            }

            string percentText = string.Format("{0}%", (int)_currentProgress);
            SizeF percentSize = g.MeasureString(percentText, _percentFont);
            float percentX = _progressTrackBounds.Right - percentSize.Width;
            float percentY = _progressTrackBounds.Top - 24;

            using (SolidBrush percentBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentGlow)))
            {
                g.DrawString(percentText, _percentFont, percentBrush, percentX, percentY);
            }

            using (SolidBrush progLabelBrush = new SolidBrush(Color.FromArgb((int)(150 * _fadeAlpha), 140, 155, 175)))
            {
                g.DrawString("PROGRESS", _statusFont, progLabelBrush, _progressTrackBounds.Left, percentY + 2);
            }

            using (GraphicsPath trackPath = CreateRoundRectPath(_progressTrackBounds, 7))
            {
                using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), 18, 24, 38)))
                {
                    g.FillPath(trackBrush, trackPath);
                }

                using (Pen trackBorderPen = new Pen(Color.FromArgb((int)(80 * _fadeAlpha), 35, 45, 70), 1f))
                {
                    g.DrawPath(trackBorderPen, trackPath);
                }
            }

            float fillRatio = Math.Max(0f, Math.Min(1f, _currentProgress / 100f));
            int fillWidth = (int)(_progressTrackBounds.Width * fillRatio);

            if (fillWidth > 6)
            {
                Rectangle fillRect = new Rectangle(_progressTrackBounds.X, _progressTrackBounds.Y, fillWidth, _progressTrackBounds.Height);
                using (GraphicsPath fillPath = CreateRoundRectPath(fillRect, 7))
                {
                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(fillRect, _accentColor, _accentGlow, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(fillBrush, fillPath);
                    }
                }
            }

            float statusY = _progressTrackBounds.Bottom + 16;
            Color statusColor = _isCompleted
                ? Color.FromArgb((int)(255 * _fadeAlpha), 52, 211, 153) // Emerald Green
                : Color.FromArgb((int)(180 * _fadeAlpha), 160, 175, 200);

            using (SolidBrush statusBrush = new SolidBrush(statusColor))
            {
                g.DrawString(_statusText, _statusFont, statusBrush, _progressTrackBounds.Left, statusY);
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
                _animTimer?.Stop();
                _animTimer?.Dispose();
                _cachedCardPath?.Dispose();
                _cachedShadowPath?.Dispose();
                _titleFont?.Dispose();
                _subFont?.Dispose();
                _statusFont?.Dispose();
                _percentFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}