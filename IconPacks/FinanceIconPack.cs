using System;

namespace NovaUI.IconPacks
{
    public sealed class FinanceIconPack : IconPackBase
    {
        public override string DisplayName { get { return "FINANCE"; } }
        public override string FontFileName { get { return "Finance_icons.ttf"; } }
        public override Type IconClass { get { return typeof(FinanceIcons); } }
    }
}