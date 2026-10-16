using System;

namespace NovaUI.IconPacks
{
    public sealed class DesignIconPack : IconPackBase
    {
        public override string DisplayName { get { return "DESIGN"; } }
        public override string FontFileName { get { return "Design_icons.ttf"; } }
        public override Type IconClass { get { return typeof(DesignIcons); } }
    }
}