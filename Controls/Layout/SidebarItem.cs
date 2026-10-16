using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace NovaUI
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("عنصر واحد داخل NovaSidebar — إما عنصر عادي أو عنوان قسم فرعي.")]
    public class SidebarItem
    {
        private string _text = "";
        private string _icon = "";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private string _badge = "";
        private string _tag = "";
        private bool _isSectionHeader = false;

        public SidebarItem() { }

        public SidebarItem(string text, string icon, string tag = null, string badge = null)
        {
            _text = text ?? "";
            _icon = icon ?? "";
            _tag = tag ?? "";
            _badge = badge ?? "";
        }

        [Category("SidebarItem")]
        [Description("نص العنصر، أو نص العنوان الفرعي عند تفعيل IsSectionHeader.")]
        [DefaultValue("")]
        public string Text { get => _text; set => _text = value ?? ""; }

        [Category("SidebarItem")]
        [Description("عند التفعيل يظهر العنصر كعنوان قسم فرعي (غير قابل للنقر ولا يعرض أيقونة).")]
        [DefaultValue(false)]
        public bool IsSectionHeader
        {
            get => _isSectionHeader;
            set => _isSectionHeader = value;
        }

        [Category("SidebarItem")]
        [Description("رمز الأيقونة (يُعبَّأ تلقائياً من المنتقي).")]
        [DefaultValue("")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string Icon { get => _icon; set => _icon = value ?? ""; }

        [Category("SidebarItem")]
        [Description("اسم عائلة الخط (تلقائي).")]
        [DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get => _iconFontName;
            set => _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value;
        }

        [Category("SidebarItem")]
        [Description("اسم حزمة الأيقونات (تلقائي).")]
        [DefaultValue("")]
        public string IconPackName
        {
            get => _iconPackName;
            set => _iconPackName = value ?? "";
        }

        [Category("SidebarItem")]
        [Description("شارة اختيارية (رقم إشعارات). اتركها فارغة لإخفائها.")]
        [DefaultValue("")]
        public string Badge { get => _badge; set => _badge = value ?? ""; }

        [Category("SidebarItem")]
        [Description("مفتاح الصفحة المرتبطة (مثال: pageHome، pageUsers).")]
        [DefaultValue("")]
        public string Tag { get => _tag; set => _tag = value ?? ""; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_text)) return "(فارغ)";
            return _isSectionHeader ? $"▼ {_text}" : _text;
        }
    }
}