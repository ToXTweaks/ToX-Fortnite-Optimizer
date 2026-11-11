using System;
using System.Windows.Forms;

namespace FortniteOptimizer.UserControls
{
    public partial class sweatsPage : UserControl
    {
        public sweatsPage()
        {
            InitializeComponent();
            HookEvents();
        }

        private static string ProSettingsUrl(string playerSlug)
        {
            return "https://prosettings.net/players/" + playerSlug + "/";
        }

        private void HookEvents()
        {
            // 1. Peterbot
            github.Click += (s, e) => ShowProSettings("Peterbot", "Falcons Esports", ProSettingsUrl("peterbot"));
            vortexPictureBox2.Click += (s, e) => ShowProSettings("Peterbot", "Falcons Esports", ProSettingsUrl("peterbot"));

            // 2. MrSavage
            vortexIconButton1.Click += (s, e) => ShowProSettings("MrSavage", "XSET", ProSettingsUrl("mrsavage"));
            vortexPictureBox1.Click += (s, e) => ShowProSettings("MrSavage", "XSET", ProSettingsUrl("mrsavage"));

            // 3. Bugha
            vortexIconButton2.Click += (s, e) => ShowProSettings("Bugha", "Free Agent", ProSettingsUrl("bugha"));
            vortexPictureBox3.Click += (s, e) => ShowProSettings("Bugha", "Free Agent", ProSettingsUrl("bugha"));

            // 4. TaySon
            vortexIconButton3.Click += (s, e) => ShowProSettings("TaySon", "Free Agent", ProSettingsUrl("tayson"));
            vortexPictureBox4.Click += (s, e) => ShowProSettings("TaySon", "Free Agent", ProSettingsUrl("tayson"));

            // 5. Clix
            vortexIconButton4.Click += (s, e) => ShowProSettings("Clix", "XSET", ProSettingsUrl("clix"));
            vortexPictureBox5.Click += (s, e) => ShowProSettings("Clix", "XSET", ProSettingsUrl("clix"));

            // 6. Mongraal
            vortexIconButton5.Click += (s, e) => ShowProSettings("Mongraal", "Red Bull eSports", ProSettingsUrl("mongraal"));
            vortexPictureBox6.Click += (s, e) => ShowProSettings("Mongraal", "Red Bull eSports", ProSettingsUrl("mongraal"));

            // 7. Shxrk
            vortexIconButton6.Click += (s, e) => ShowProSettings("Shxrk", "Aurora Gaming", ProSettingsUrl("shxrk"));
            vortexPictureBox7.Click += (s, e) => ShowProSettings("Shxrk", "Aurora Gaming", ProSettingsUrl("shxrk"));

            // 8. Vico
            vortexIconButton7.Click += (s, e) => ShowProSettings("Vico", "BIG Clan", ProSettingsUrl("vico"));
            vortexPictureBox8.Click += (s, e) => ShowProSettings("Vico", "BIG Clan", ProSettingsUrl("vico"));

            // 9. Malibuca
            vortexIconButton8.Click += (s, e) => ShowProSettings("Malibuca", "BIG Clan", ProSettingsUrl("malibuca"));
            vortexPictureBox9.Click += (s, e) => ShowProSettings("Malibuca", "BIG Clan", ProSettingsUrl("malibuca"));
        }

        private void ShowProSettings(string playerName, string teamOrg, string url)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                ProSettingsOverlay.Show(parentForm, playerName, teamOrg, url);
            }
        }
    }
}
