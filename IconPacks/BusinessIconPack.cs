using System;

namespace NovaUI.IconPacks
{
    public sealed class BusinessIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Bussiness"; } }
        public override string FontFileName { get { return "Business_icons.ttf"; } }
        public override Type IconClass { get { return typeof(BusinessIcons); } }
    }
}