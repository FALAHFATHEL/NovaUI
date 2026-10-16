using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace NovaUI
{
    /// <summary>
    /// محرر خاصية IconGlyph: يعرض زر "..." في نافذة الخصائص،
    /// وعند الضغط عليه يفتح IconPickerForm.
    /// </summary>
    public class IconGlyphEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
            ITypeDescriptorContext context,
            IServiceProvider provider,
            object value)
        {
            if (provider == null) return value;

            var edSvc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (edSvc == null) return value;

            string currentGlyph = value as string ?? "";
            string currentPack = GetCurrentProperty(context, "IconPackName") as string ?? "";

            using (var picker = new IconPickerForm(currentPack, currentGlyph))
            {
                if (edSvc.ShowDialog(picker) != DialogResult.OK)
                    return value;

                ApplyPropertyChange(provider, context, "IconFontName", picker.LoadedFontFamilyName);
                ApplyPropertyChange(provider, context, "IconPackName", picker.SelectedPackName);

                return picker.SelectedGlyph;
            }
        }

        private static object GetCurrentProperty(ITypeDescriptorContext context, string propName)
        {
            if (context == null || context.Instance == null) return null;

            var pd = TypeDescriptor.GetProperties(context.Instance)[propName];
            if (pd == null) return null;

            try { return pd.GetValue(context.Instance); }
            catch { return null; }
        }

        private static void ApplyPropertyChange(
            IServiceProvider provider,
            ITypeDescriptorContext context,
            string propName,
            object newValue)
        {
            if (context == null || context.Instance == null) return;

            var pd = TypeDescriptor.GetProperties(context.Instance)[propName];
            if (pd == null) return;

            if (pd.IsReadOnly) return;

            object oldValue;
            try { oldValue = pd.GetValue(context.Instance); }
            catch { return; }

            if (Equals(oldValue, newValue)) return;

            var changeSvc = provider != null
                ? provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService
                : null;

            if (changeSvc != null)
            {
                try { changeSvc.OnComponentChanging(context.Instance, pd); }
                catch { }
            }

            try { pd.SetValue(context.Instance, newValue); }
            catch { return; }

            if (changeSvc != null)
            {
                try { changeSvc.OnComponentChanged(context.Instance, pd, oldValue, newValue); }
                catch { }
            }
        }
    }
}
