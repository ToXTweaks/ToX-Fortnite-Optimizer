using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fortnite_Optimizer.TweaksTabs
{
    public partial class Credits : UserControl
    {
        public Credits()
        {
            InitializeComponent();
        }

        private void ToXTweaks_Click(object sender, EventArgs e)
        {
            Process.Start("http://discord.gg/toxtweaks");
        }

        private void ToXCoding_Click(object sender, EventArgs e)
        {
            Process.Start("http://discord.gg/toxcoding");
        }

        private void PhantomWallet_Click(object sender, EventArgs e)
        {
            ToolTip.Show("Username: @toxtweaks", PhantomWallet);
        }

        private void PayPal_Click(object sender, EventArgs e)
        {
            Process.Start("https://paypal.me/toxtweaks");
        }

        private void Revolut_Click(object sender, EventArgs e)
        {
            Process.Start("https://revolut.me/toxtweaks");
        }

        private void Stripe_Click(object sender, EventArgs e)
        {
            Process.Start("https://buy.stripe.com/cN24hyfqf9lT2Va28a");
        }

        private void Purchase_Click(object sender, EventArgs e)
        {
            Process.Start("https://toxtweaks.com/");
        }
    }
}
