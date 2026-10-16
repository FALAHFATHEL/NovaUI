using System;

namespace NovaUI.IconPacks
{
    public sealed class OtherIconPack : IconPackBase
    {
        public override string DisplayName { get { return "OTHERS"; } }
        public override string FontFileName { get { return "Other_icons.ttf"; } }
        public override Type IconClass { get { return typeof(OtherIcons); } }
    }
}