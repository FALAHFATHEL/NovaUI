using System;

namespace NovaUI.IconPacks
{
    public sealed class SystemIconPack : IconPackBase
    {
        public override string DisplayName { get { return "SYSTEM"; } }
        public override string FontFileName { get { return "System_icons.ttf"; } }
        public override Type IconClass { get { return typeof(SystemIcons); } }
    }
}