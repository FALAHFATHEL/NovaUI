using System;

namespace NovaUI.IconPacks
{
    public sealed class ArrowIconPack : IconPackBase
    {
        public override string DisplayName { get { return "ARROWS"; } }
        public override string FontFileName { get { return "Arrows_icons.ttf"; } }
        public override Type IconClass { get { return typeof(ArrowIcons); } }
    }
}