using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace NovaUI
{
    public static class FontManager
    {
        private const int MaxLoadedFonts = 40;
        private const string FallbackFamily = "Segoe MDL2 Assets";

        private sealed class LoadedFont : IDisposable
        {
            public PrivateFontCollection Collection;
            public IntPtr MemoryPtr;
            public FontFamily Family;
            public string FamilyName;
            public DateTime LastAccess;
            public byte[] FontBytes;

            public void Dispose()
            {
                if (Collection != null) { Collection.Dispose(); Collection = null; }
                if (MemoryPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(MemoryPtr);
                    MemoryPtr = IntPtr.Zero;
                }
                Family = null;
                FontBytes = null;
            }
        }

        private static readonly Dictionary<string, LoadedFont> _loadedByPack =
            new Dictionary<string, LoadedFont>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _packToFamily =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _lock = new object();

        /// <summary>يضمن أن خط الحزمة محمّل (مع LRU).</summary>
        private static LoadedFont EnsureLoaded(IIconPack pack)
        {
            if (pack == null) return null;

            lock (_lock)
            {
                LoadedFont existing;
                if (_loadedByPack.TryGetValue(pack.DisplayName, out existing))
                {
                    existing.LastAccess = DateTime.UtcNow;
                    return existing;
                }

                // إخلاء الأقدم إن وصلنا للحد
                if (_loadedByPack.Count >= MaxLoadedFonts)
                {
                    string oldestKey = null;
                    DateTime oldestTime = DateTime.MaxValue;
                    foreach (var kv in _loadedByPack)
                    {
                        if (kv.Value.LastAccess < oldestTime)
                        {
                            oldestTime = kv.Value.LastAccess;
                            oldestKey = kv.Key;
                        }
                    }
                    if (oldestKey != null)
                    {
                        var toDispose = _loadedByPack[oldestKey];
                        _loadedByPack.Remove(oldestKey);
                        try { toDispose.Dispose(); } catch { }
                    }
                }

                // حمّل الخط الجديد
                var loaded = new LoadedFont();
                try
                {
                    var bytes = pack.FontData;
                    if (bytes == null || bytes.Length == 0) return null;

                    loaded.FontBytes = bytes;
                    loaded.MemoryPtr = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, loaded.MemoryPtr, bytes.Length);

                    loaded.Collection = new PrivateFontCollection();
                    loaded.Collection.AddMemoryFont(loaded.MemoryPtr, bytes.Length);

                    if (loaded.Collection.Families.Length == 0)
                    {
                        loaded.Dispose();
                        return null;
                    }

                    // ★ اختيار العائلة الصحيحة (مهم لملفات TTC المتعددة)
                    loaded.Family = SelectBestFamily(loaded.Collection, pack.DisplayName);
                    loaded.FamilyName = loaded.Family.Name;
                    loaded.LastAccess = DateTime.UtcNow;

                    _loadedByPack[pack.DisplayName] = loaded;
                    _packToFamily[pack.DisplayName] = loaded.FamilyName;

                    System.Diagnostics.Debug.WriteLine(
                        "[FontManager] ✓ حُمّل: " + pack.DisplayName +
                        " → \"" + loaded.FamilyName + "\"" +
                        " (families in file: " + loaded.Collection.Families.Length + ")");

                    return loaded;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[FontManager] ✗ فشل " + pack.DisplayName + ": " + ex.Message);
                    loaded.Dispose();
                    return null;
                }
            }
        }

        /// <summary>
        /// يختار العائلة الأنسب من ملف الخط.
        /// إن كان الملف يحتوي عدة عائلات (TTC)، يحاول مطابقة اسم الحزمة،
        /// وإلا يفضّل العائلة التي تحتوي Regular style، ثم الأولى.
        /// </summary>
        private static FontFamily SelectBestFamily(PrivateFontCollection collection, string packDisplayName)
        {
            var families = collection.Families;
            if (families == null || families.Length == 0) return null;
            if (families.Length == 1) return families[0];

            string packKey = ExtractKey(packDisplayName);
            foreach (var fam in families)
            {
                try
                {
                    if (fam.Name.IndexOf(packKey, StringComparison.OrdinalIgnoreCase) >= 0)
                        return fam;
                }
                catch { }
            }

            foreach (var fam in families)
            {
                try
                {
                    if (fam.IsStyleAvailable(FontStyle.Regular))
                        return fam;
                }
                catch { }
            }

            return families[0];
        }

        private static string ExtractKey(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "";
            int i = displayName.IndexOf(' ');
            return i > 0 ? displayName.Substring(0, i) : displayName;
        }

        /// <summary>
        /// ينشئ Font من العائلة بضمان:
        /// - وجود الـ Style فعلياً
        /// - عدم استبدال GDI+ للعائلة (font linking)
        /// </summary>
        private static Font CreateFontSafe(LoadedFont loaded, float size, FontStyle style)
        {
            if (loaded == null || loaded.Family == null) return null;

            // (1) اختيار Style متوفر فعلياً
            FontStyle effective = style;
            try
            {
                if (!loaded.Family.IsStyleAvailable(effective))
                {
                    if (loaded.Family.IsStyleAvailable(FontStyle.Regular))
                        effective = FontStyle.Regular;
                    else if (loaded.Family.IsStyleAvailable(FontStyle.Bold))
                        effective = FontStyle.Bold;
                    else if (loaded.Family.IsStyleAvailable(FontStyle.Italic))
                        effective = FontStyle.Italic;
                    else
                        effective = FontStyle.Regular;
                }
            }
            catch { effective = FontStyle.Regular; }

            Font font = null;
            try { font = new Font(loaded.Family, size, effective); }
            catch
            {
                try { font = new Font(loaded.Family, size); }
                catch { return null; }
            }
            if (font == null) return null;

            // (2) تحقق أن GDI+ لم يستبدل العائلة
            try
            {
                if (font.FontFamily == null ||
                    !string.Equals(font.FontFamily.Name, loaded.FamilyName,
                                   StringComparison.OrdinalIgnoreCase))
                {
                    font.Dispose();
                    return null;
                }
            }
            catch { }

            return font;
        }

        /// <summary>يكتشف اسم العائلة (يُستخدم للكاش فقط).</summary>
        public static string GetFamilyName(IIconPack pack)
        {
            if (pack == null) return null;

            lock (_lock)
            {
                LoadedFont loaded;
                if (_loadedByPack.TryGetValue(pack.DisplayName, out loaded))
                    return loaded.FamilyName;

                string cached;
                if (_packToFamily.TryGetValue(pack.DisplayName, out cached))
                    return cached;
            }

            try
            {
                var bytes = pack.FontData;
                var name = EmbeddedFontLoader.DiscoverFamilyName(bytes);
                if (!string.IsNullOrEmpty(name))
                {
                    lock (_lock) { _packToFamily[pack.DisplayName] = name; }
                    return name;
                }
            }
            catch { }
            return null;
        }

        public static IIconPack FindPackByFamilyName(string familyName)
        {
            if (string.IsNullOrEmpty(familyName)) return null;
            IconPackRegistry.EnsureInitialized();

            foreach (var p in IconPackRegistry.Packs)
            {
                if (string.Equals(GetFamilyName(p), familyName,
                                  StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }

        /// <summary>
        /// ★ API الموصى به: يُنشئ Font من اسم الحزمة (يمنع التعارض).
        /// </summary>
        public static Font GetIconFontByPack(string packName, float size,
            FontStyle style = FontStyle.Regular)
        {
            if (size <= 0) size = 9F;

            var pack = IconPackRegistry.GetByName(packName);
            if (pack != null)
            {
                var loaded = EnsureLoaded(pack);
                var f = CreateFontSafe(loaded, size, style);
                if (f != null) return f;
            }
            return GetFallbackFont(size, style);
        }

        /// <summary>
        /// يقبل اسم الحزمة **أو** اسم العائلة (يُفضّل اسم الحزمة).
        /// </summary>
        public static Font GetIconFont(string nameOrFamily, float size,
            FontStyle style = FontStyle.Regular)
        {
            if (size <= 0) size = 9F;

            var pack = IconPackRegistry.GetByName(nameOrFamily);
            if (pack == null)
                pack = FindPackByFamilyName(nameOrFamily);

            if (pack != null)
            {
                var loaded = EnsureLoaded(pack);
                var f = CreateFontSafe(loaded, size, style);
                if (f != null) return f;
            }
            return GetFallbackFont(size, style);
        }

        private static Font GetFallbackFont(float size, FontStyle style)
        {
            try { return new Font(FallbackFamily, size, style); }
            catch
            {
                try { return new Font(FontFamily.GenericSansSerif, size, style); }
                catch { return new Font(FontFamily.GenericSansSerif, 9F); }
            }
        }

        public static Font GetFont(string packName, float size,
            FontStyle style = FontStyle.Regular)
        {
            return GetIconFont(packName, size, style);
        }

        public static Font GetFont(string fontName, float size, bool flag)
        {
            return GetIconFont(fontName, size, FontStyle.Regular);
        }
    }
}