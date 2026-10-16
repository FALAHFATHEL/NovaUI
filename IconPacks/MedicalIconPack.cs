using System;

namespace NovaUI.IconPacks
{
    public sealed class MedicalIconPack : IconPackBase
    {
        public override string DisplayName { get { return "MEDICAL"; } }
        public override string FontFileName { get { return "Medical_icons.ttf"; } }
        public override Type IconClass { get { return typeof(MedicalIcons); } }
    }
}