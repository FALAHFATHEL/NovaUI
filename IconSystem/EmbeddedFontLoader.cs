using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NovaUI
{
    internal static class EmbeddedFontLoader
    {
        private static readonly Assembly _asm = typeof(EmbeddedFontLoader).Assembly;

        public static byte[] Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            var allResources = _asm.GetManifestResourceNames();
            string fullName = allResources.FirstOrDefault(name =>
                name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));

            if (fullName == null)
            {
                string available = string.Join("\n  ",
                    allResources.Where(n => n.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)));
                throw new FileNotFoundException(
                    "لم يُعثر على المورد: " + fileName +
                    "\n\nالموارد المتاحة:\n  " + available);
            }

            using (var stream = _asm.GetManifestResourceStream(fullName))
            {
                if (stream == null)
                    throw new FileNotFoundException("المورد فارغ: " + fullName);

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static string DiscoverFamilyName(byte[] fontBytes)
        {
            if (fontBytes == null || fontBytes.Length == 0) return null;

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = System.Runtime.InteropServices.Marshal
                    .AllocCoTaskMem(fontBytes.Length);
                System.Runtime.InteropServices.Marshal
                    .Copy(fontBytes, 0, ptr, fontBytes.Length);

                using (var pfc = new System.Drawing.Text.PrivateFontCollection())
                {
                    pfc.AddMemoryFont(ptr, fontBytes.Length);
                    if (pfc.Families.Length > 0)
                        return pfc.Families[0].Name;
                }
            }
            catch { }
            finally
            {
                if (ptr != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
            }
            return null;
        }
    }
}