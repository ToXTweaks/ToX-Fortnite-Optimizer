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
    public partial class Extra : UserControl
    {
        public Extra()
        {
            InitializeComponent();
        }

        private void ApplyGameUserSettings_Click(object sender, EventArgs e)
        {
            string tweaks = @"REM Set the URL of the new settings file
set ""DOWNLOAD_URL=https://github.com/ToXTweaks/Resources-Free/raw/refs/heads/main/GameUserSettings.ini""

REM Set the path where the new settings file will be downloaded
set ""TEMP_SETTINGS_PATH=%TEMP%\GameUserSettings.ini""

REM Set the path where the Fortnite user settings are located
set ""FORTNITE_SETTINGS_PATH=%LOCALAPPDATA%\FortniteGame\Saved\Config\WindowsClient\GameUserSettings.ini""

REM Download the new settings file using curl
echo Downloading new settings file...
curl -L -o ""%TEMP_SETTINGS_PATH%"" ""%DOWNLOAD_URL%""

REM Check if download was successful
if not exist ""%TEMP_SETTINGS_PATH%"" (
    echo Failed to download new settings file.
    exit /b 1
)

REM Delete the old settings file
echo Deleting old settings file...
if exist ""%FORTNITE_SETTINGS_PATH%"" (
    del ""%FORTNITE_SETTINGS_PATH%""
) else (
    echo Old settings file not found.
)

REM Move the new settings file to the final location
echo Moving new settings file...
move /Y ""%TEMP_SETTINGS_PATH%"" ""%FORTNITE_SETTINGS_PATH%""
";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(NvApply, tweaks, revert);
        }

        private void RevertGameUserSettings_Click(object sender, EventArgs e)
        {
            string tweaks = @"";
            string revert = @"";
            FNOptimizer.Instance.ExecuteTweaks(NvRevert, tweaks, revert);
        }
    }
}
