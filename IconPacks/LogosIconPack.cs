using System;

namespace NovaUI.IconPacks
{
    public sealed class LogosIconPack : IconPackBase
    {
        public override string DisplayName { get { return "LOGOS"; } }
        public override string FontFileName { get { return "Logos_icons.ttf"; } }
        public override Type IconClass { get { return typeof(LogosIcons); } }
    }
}