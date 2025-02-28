using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fortnite_Optimizer.TweaksTabs
{
    public partial class Potato : UserControl
    {
        public Potato()
        {
            InitializeComponent();
        }

        private void NvidiaPotatoApply_Click(object sender, EventArgs e)
        {
            string tweaks = @"
curl -g -k -L -# -o ""%tmp%\nvidiaProfileInspector.zip"" ""https://github.com/Orbmu2k/nvidiaProfileInspector/releases/latest/download/nvidiaProfileInspector.zip"">nul 2>&1 
powershell -NoProfile Expand-Archive '%tmp%\nvidiaProfileInspector.zip' -DestinationPath 'C:\ToX-Fortnite-Optimizer\Resources\'>nul 2>&1 
curl -g -k -L -# -o ""C:\ToX-Fortnite-Optimizer\Resources\LowGraphics.nip"" ""https://github.com/ToXTweaks/Resources-Free/raw/refs/heads/main/LowGraphics.nip"" >nul 
start """" /D ""C:\ToX-Fortnite-Optimizer\Resources"" nvidiaProfileInspector.exe LowGraphics.nip";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(NvApply, tweaks, revert);
        }

        private void NvidiaPotatoRevert_Click(object sender, EventArgs e)
        {
            string tweaks = @"";
            string revert = @"curl -g -k -L -# -o ""%tmp%\nvidiaProfileInspector.zip"" ""https://github.com/Orbmu2k/nvidiaProfileInspector/releases/latest/download/nvidiaProfileInspector.zip"">nul 2>&1 
powershell -NoProfile Expand-Archive '%tmp%\nvidiaProfileInspector.zip' -DestinationPath 'C:\ToX-Fortnite-Optimizer\Resources\'>nul 2>&1 
curl -g -k -L -# -o ""C:\ToX-Fortnite-Optimizer\Resources\PotatoRevert.nip"" ""https://github.com/ToXTweaks/Resources-Free/raw/refs/heads/main/PotatoRevert.nip"" >nul 
start """" /D ""C:\ToX-Fortnite-Optimizer\Resources"" nvidiaProfileInspector.exe PotatoRevert.nip";
            FNOptimizer.Instance.ExecuteTweaks(NvRevert, tweaks, revert);
        }

        private void AmdPotatoApply_Click(object sender, EventArgs e)
        {
            
        }

        private void NvidiaPreview_Click(object sender, EventArgs e)
        {

        }
    }
}
