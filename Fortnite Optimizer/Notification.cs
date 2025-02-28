using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Fortnite_Optimizer
{
    public partial class Notification : Form
    {
        public Notification()
        {
            InitializeComponent();
        }
        public static string Content { get; set; }
        public void Lock(int X, int Y)
        {
            Location = new Point(X, Y);
        }
        private void closePopUp_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Notification_Load(object sender, EventArgs e)
        {
            msg.Text = Content;
        }
    }
}
