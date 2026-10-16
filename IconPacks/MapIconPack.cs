using System;

namespace NovaUI.IconPacks
{
    public sealed class MapIconPack : IconPackBase
    {
        public override string DisplayName { get { return "MAPs"; } }
        public override string FontFileName { get { return "Map_icons.ttf"; } }
        public override Type IconClass { get { return typeof(MapIcons); } }
    }
}