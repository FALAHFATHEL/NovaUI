using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(UserControl))]
    public class NovaCustomForm : Component
    {
        private Form _targetForm;
        private int _borderRadius = 4; 

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        // خيارات النظام للزوايا:
        // 1 = زوايا حادة (Default)
        // 2 = زوايا مربعة (Sharp)
        // 3 = زوايا دائرية متوسّطة (Round)
        // 4 = زوايا دائرية ناعمة جداً وصغيرة (Small Round / Anti-aliased)
        [Category("Appearance")]
        [Description("درجة انحناء الحواف باستخدام محرك نظام ويندوز لتجنب التكسير.")]
        public int BorderRadiusPreference
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                ApplyRadius();
            }
        }

        [Category("Appearance")]
        [Description("حدد الفورم المستهدف لتطبيق الحواف الدائرية عليه.")]
        public Form TargetForm
        {
            get => _targetForm;
            set
            {
                _targetForm = value;
                if (_targetForm != null && !DesignMode)
                {
                    ApplyRadius();
                    _targetForm.Load += (s, e) => ApplyRadius();
                    _targetForm.Shown += (s, e) => ApplyRadius();
                }
            }
        }

        public NovaCustomForm()
        {
        }

        public NovaCustomForm(IContainer container)
        {
            container.Add(this);
        }

        private void ApplyRadius()
        {
            if (_targetForm == null)
                return;

            try
            {
                // استخدام ميزة ويندوز الأصلية (DWM) لأنها ترسم الحواف بدقة عالية جداً (Anti-aliased) 
                // وتمنع نهائياً أي تكسير بكسلات تظهر عند استخدام Region العادي.
                int preference = _borderRadius;
                DwmSetWindowAttribute(_targetForm.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
            catch
            {
                // تتطلب إصدارات ويندوز الحديثة (Windows 11 / Windows 10 الأحدث)
            }
        }
    }
}
