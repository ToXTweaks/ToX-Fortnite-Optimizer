using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FortniteOptimizer
{
    internal static class AppFonts
    {
        private static readonly PrivateFontCollection PrivateFonts = new PrivateFontCollection();
        private static readonly List<IntPtr> NativeHandles = new List<IntPtr>();
        private static readonly List<GCHandle> PinnedFontData = new List<GCHandle>();
        private static readonly List<byte[]> FontBuffers = new List<byte[]>();
        private static bool _loaded;

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr AddFontMemResourceEx(
            IntPtr pbFont,
            uint cbFont,
            IntPtr pdv,
            ref uint pcFonts);

        public static void LoadAll()
        {
            if (_loaded)
                return;

            _loaded = true;

            LoadEmbeddedFont("FortniteOptimizer.Resources.Fonts.Bahnschrift.ttf");
            LoadEmbeddedFont("FortniteOptimizer.Resources.Fonts.BowlbyOneSC-Regular.ttf");
            LoadEmbeddedFont("FortniteOptimizer.Resources.Fonts.BurbankBigCondensedBlack.otf");
        }

        private static void LoadEmbeddedFont(string resourceName)
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return;

                    byte[] data = new byte[stream.Length];
                    int offset = 0;
                    int read;
                    while (offset < data.Length &&
                           (read = stream.Read(data, offset, data.Length - offset)) > 0)
                        offset += read;

                    if (offset != data.Length)
                        return;

                    // Keep buffers alive for process lifetime (PrivateFontCollection / GDI).
                    FontBuffers.Add(data);

                    GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    PinnedFontData.Add(handle);

                    IntPtr pointer = handle.AddrOfPinnedObject();
                    uint installed = 0;
                    IntPtr gdiHandle = AddFontMemResourceEx(pointer, (uint)data.Length, IntPtr.Zero, ref installed);
                    if (gdiHandle != IntPtr.Zero)
                        NativeHandles.Add(gdiHandle);

                    PrivateFonts.AddMemoryFont(pointer, data.Length);
                }
            }
            catch
            {
                // Fail soft per font so missing/corrupt resources do not block startup.
            }
        }
    }
}
