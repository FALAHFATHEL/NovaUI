using System;

namespace NovaUI.IconPacks
{
    public sealed class BrandsIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Brands V1"; } }
        public override string FontFileName { get { return "Brands.ttf"; } }
        public override Type IconClass { get { return typeof(BrandsIcons); } }
    }
}