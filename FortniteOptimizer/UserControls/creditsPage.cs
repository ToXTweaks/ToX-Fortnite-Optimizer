using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace FortniteOptimizer.UserControls
{
    public partial class creditsPage : UserControl
    {
        private Timer creditsTimer;
        private float scrollY;
        private float totalCreditsHeight;
        private Bitmap creditsBitmap;
        private readonly Stopwatch scrollClock = new Stopwatch();
        private long lastTickMs;

        private const float ScrollPixelsPerSecond = 28f;

        private static readonly Font TitleFont = new Font("Bowlby One SC", 14F, FontStyle.Italic);
        private static readonly Font HeadingFont = new Font("Bahnschrift SemiBold", 10F, FontStyle.Bold);
        private static readonly Font NameFont = new Font("Bahnschrift SemiBold", 11F, FontStyle.Bold);
        private static readonly Font RoleFont = new Font("Bahnschrift", 8.5F);

        private static readonly Color TitleColor = Color.FromArgb(68, 168, 222);
        private static readonly Color HeadingColor = Color.FromArgb(1, 122, 241);
        private static readonly Color NameColor = Color.White;
        private static readonly Color RoleColor = Color.FromArgb(180, 180, 195);

        private struct CreditEntry
        {
            public string Text;
            public Font Font;
            public Color Color;
            public float SpacingAfter;

            public CreditEntry(string text, Font font, Color color, float spacingAfter = 4f)
            {
                Text = text;
                Font = font;
                Color = color;
                SpacingAfter = spacingAfter;
            }
        }

        private static readonly CreditEntry[] Credits = new CreditEntry[]
        {
            new CreditEntry("ToX Fortnite Optimizer", TitleFont, TitleColor, 2f),

            new CreditEntry("— CREATED BY —", HeadingFont, HeadingColor, 12f),
            new CreditEntry("ToX", NameFont, NameColor, 2f),
            new CreditEntry("Founder & Lead Developer", RoleFont, RoleColor, 30f),

            new CreditEntry("— UI DESIGNER —", HeadingFont, HeadingColor, 12f),
            new CreditEntry("Inx", NameFont, NameColor, 2f),
            new CreditEntry("Major Support For The V2 Re-Design", RoleFont, RoleColor, 30f),

            new CreditEntry("— SPECIAL THANKS —", HeadingFont, HeadingColor, 12f),
            new CreditEntry("ProSettings.net", NameFont, NameColor, 4f),
            new CreditEntry("ToXTweaks.com", NameFont, NameColor, 4f),

            new CreditEntry("— TESTERS —", HeadingFont, HeadingColor, 12f),
            new CreditEntry("Halel", NameFont, NameColor, 4f),
            new CreditEntry("Milo", NameFont, NameColor, 4f),
            new CreditEntry("XXNATY", NameFont, NameColor, 30f),

            new CreditEntry("— COMMUNITY —", HeadingFont, HeadingColor, 12f),
            new CreditEntry("Thank you to everyone who", RoleFont, RoleColor, 2f),
            new CreditEntry("supports ToX Tweaks!", RoleFont, RoleColor, 20f),
            new CreditEntry("Your feedback and support", RoleFont, RoleColor, 2f),
            new CreditEntry("keeps this project alive.", RoleFont, RoleColor, 40f),

            new CreditEntry("BUILT FOR VICTORY ROYALE!", TitleFont, TitleColor, 0f),
        };

        public creditsPage()
        {
            InitializeComponent();

            purchaseButton.Click += PurchaseButton_Click;

            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(creditsOverlay, true);

            creditsOverlay.Paint += CreditsOverlay_Paint;
            creditsOverlay.Resize += (s, e) => RebuildCreditsBitmap();
            VisibleChanged += CreditsPage_VisibleChanged;

            RebuildCreditsBitmap();
            scrollY = CreditsPanel.Height;

            creditsTimer = new Timer { Interval = 16 };
            creditsTimer.Tick += CreditsTimer_Tick;
            scrollClock.Start();
            lastTickMs = scrollClock.ElapsedMilliseconds;
            creditsTimer.Start();
        }

        private void CreditsPage_VisibleChanged(object sender, EventArgs e)
        {
            if (creditsTimer == null) return;

            if (Visible)
            {
                lastTickMs = scrollClock.ElapsedMilliseconds;
                creditsTimer.Start();
            }
            else
            {
                creditsTimer.Stop();
            }
        }

        private void RebuildCreditsBitmap()
        {
            int width = Math.Max(1, creditsOverlay.Width);
            totalCreditsHeight = MeasureCreditsHeight(width);
            int height = Math.Max(1, (int)Math.Ceiling(totalCreditsHeight) + 2);

            creditsBitmap?.Dispose();
            creditsBitmap = new Bitmap(width, height);

            using (var g = Graphics.FromImage(creditsBitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float y = 0f;
                foreach (var entry in Credits)
                {
                    var sz = g.MeasureString(entry.Text, entry.Font);
                    float x = (width - sz.Width) / 2f;

                    using (var brush = new SolidBrush(entry.Color))
                        g.DrawString(entry.Text, entry.Font, brush, x, y);

                    y += sz.Height + entry.SpacingAfter;
                }
            }
        }

        private float MeasureCreditsHeight(int width)
        {
            float h = 0;
            using (var bmp = new Bitmap(Math.Max(1, width), 1))
            using (var g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                foreach (var entry in Credits)
                {
                    var sz = g.MeasureString(entry.Text, entry.Font);
                    h += sz.Height + entry.SpacingAfter;
                }
            }
            return h;
        }

        private void CreditsTimer_Tick(object sender, EventArgs e)
        {
            long now = scrollClock.ElapsedMilliseconds;
            float dt = (now - lastTickMs) / 1000f;
            lastTickMs = now;

            if (dt <= 0f || dt > 0.25f)
                dt = 0.016f;

            scrollY -= ScrollPixelsPerSecond * dt;

            if (scrollY < -totalCreditsHeight)
                scrollY = creditsOverlay.Height;

            creditsOverlay.Invalidate();
        }

        private void CreditsOverlay_Paint(object sender, PaintEventArgs e)
        {
            if (creditsBitmap == null) return;

            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.DrawImage(creditsBitmap, 0f, scrollY, creditsBitmap.Width, creditsBitmap.Height);
        }

        private void PurchaseButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://toxtweaks.com/buy",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                creditsTimer?.Stop();
                creditsTimer?.Dispose();
                creditsBitmap?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void moredetailsButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://toxtweaks.com/premium",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
