using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace FortniteOptimizer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            EmbeddedAssets.PrepareWebView2Loader(CoreWebView2Environment.SetLoaderDllFolderPath);
            AppFonts.LoadAll();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Dashboard());
        }
    }
}
