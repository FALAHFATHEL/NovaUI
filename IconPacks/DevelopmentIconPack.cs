using System;

namespace NovaUI.IconPacks
{
    public sealed class DevelopmentIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Development"; } }
        public override string FontFileName { get { return "Development_icons.ttf"; } }
        public override Type IconClass { get { return typeof(DevelopmentIcons); } }
    }
}