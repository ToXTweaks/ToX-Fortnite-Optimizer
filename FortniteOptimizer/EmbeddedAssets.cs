using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FortniteOptimizer
{
    internal static class EmbeddedAssets
    {
        private const string ResourcePrefix = "FortniteOptimizer.";
        private static readonly object Gate = new object();

        public static Image LoadImage(string relativePath)
        {
            using (Stream stream = Open(relativePath))
            {
                if (stream == null)
                    return null;

                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    buffer.Position = 0;
                    using (var loaded = new Bitmap(buffer))
                        return new Bitmap(loaded);
                }
            }
        }

        public static string Extract(string relativePath)
        {
            string destination = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToX Fortnite Optimizer",
                "Embedded",
                relativePath);

            lock (Gate)
            {
                using (Stream stream = Open(relativePath))
                {
                    if (stream == null)
                        throw new FileNotFoundException("Embedded resource was not found.", relativePath);

                    string directory = Path.GetDirectoryName(destination);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    if (File.Exists(destination) && new FileInfo(destination).Length == stream.Length)
                        return destination;

                    using (FileStream file = File.Create(destination))
                        stream.CopyTo(file);
                }
            }

            return destination;
        }

        public static void PrepareWebView2Loader(Action<string> setLoaderFolder)
        {
            string rid;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    rid = "win-x86";
                    break;
                case Architecture.Arm64:
                    rid = "win-arm64";
                    break;
                default:
                    rid = "win-x64";
                    break;
            }

            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToX Fortnite Optimizer",
                "Native",
                rid);

            Directory.CreateDirectory(folder);
            string destination = Path.Combine(folder, "WebView2Loader.dll");
            ExtractNamed(ResourcePrefix + "Native." + rid + ".WebView2Loader.dll", destination);
            setLoaderFolder(folder);
        }

        private static void ExtractNamed(string resourceName, string destination)
        {
            lock (Gate)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException("Embedded resource was not found.", resourceName);

                    if (File.Exists(destination) && new FileInfo(destination).Length == stream.Length)
                        return;

                    using (FileStream file = File.Create(destination))
                        stream.CopyTo(file);
                }
            }
        }

        private static Stream Open(string relativePath)
        {
            string resourceName = ResourcePrefix + relativePath.Replace('\\', '.').Replace('/', '.');
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }
    }
}
