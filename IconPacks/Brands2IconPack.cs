using System;

namespace NovaUI.IconPacks
{
    public sealed class Brands2IconPack : IconPackBase
    {
        public override string DisplayName { get { return "Brands V2"; } }
        public override string FontFileName { get { return "Brands2_icons.ttf"; } }
        public override Type IconClass { get { return typeof(Brands2Icons); } }
    }
}