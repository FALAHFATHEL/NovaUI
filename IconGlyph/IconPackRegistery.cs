using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NovaUI
{
    public static class IconPackRegistry
    {
        private static readonly List<IIconPack> _packs = new List<IIconPack>();
        private static readonly Dictionary<string, IIconPack> _byName =
            new Dictionary<string, IIconPack>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> _iconsCache =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();
        private static bool _initialized;

        public static IReadOnlyList<IIconPack> Packs
        {
            get { EnsureInitialized(); return _packs; }
        }

        /// <summary>مسح الـ assembly الذي يحتوي هذا الـ registry عن الحزم.</summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                _initialized = true;
                ScanAssembly(typeof(IconPackRegistry).Assembly);
            }
        }

        public static void ScanAssembly(Assembly asm)
        {
            if (asm == null) return;
            foreach (var t in SafeTypes(asm))
            {
                if (t.IsAbstract || t.IsInterface) continue;
                if (!typeof(IIconPack).IsAssignableFrom(t)) continue;
                try
                {
                    var inst = (IIconPack)Activator.CreateInstance(t);
                    Register(inst);
                }
                catch { /* تجاهل الحزم المعطوبة */ }
            }
        }

        public static void Register(IIconPack pack)
        {
            if (pack == null || string.IsNullOrEmpty(pack.DisplayName)) return;
            lock (_lock)
            {
                if (_byName.ContainsKey(pack.DisplayName)) return;
                _packs.Add(pack);
                _byName[pack.DisplayName] = pack;
            }
        }

        public static IIconPack GetByName(string displayName)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(displayName)) return null;
            IIconPack p;
            return _byName.TryGetValue(displayName, out p) ? p : null;
        }

        /// <summary>استخراج قاموس (الاسم → الرمز) من كلاس الثوابت مرة واحدة.</summary>
        public static Dictionary<string, string> GetIcons(IIconPack pack)
        {
            if (pack == null) return new Dictionary<string, string>();

            Dictionary<string, string> cache;
            if (_iconsCache.TryGetValue(pack.DisplayName, out cache)) return cache;

            var result = new Dictionary<string, string>();
            var t = pack.IconClass;
            if (t != null)
            {
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (f.FieldType != typeof(string)) continue;
                    string val = null;
                    try
                    {
                        if (f.IsLiteral) val = f.GetRawConstantValue() as string;
                        else val = f.GetValue(null) as string;
                    }
                    catch { }
                    if (!string.IsNullOrEmpty(val)) result[f.Name] = val;
                }
            }
            _iconsCache[pack.DisplayName] = result;
            return result;
        }

        private static IEnumerable<Type> SafeTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(x => x != null); }
        }
    }
}