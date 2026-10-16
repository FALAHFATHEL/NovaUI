using System;

namespace NovaUI.IconPacks
{
    public sealed class DevicesIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Devices"; } }
        public override string FontFileName { get { return "Devices_icons.ttf"; } }
        public override Type IconClass { get { return typeof(DevicesIcons); } }
    }
}