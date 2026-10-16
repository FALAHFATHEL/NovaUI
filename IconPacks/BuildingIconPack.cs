using System;

namespace NovaUI.IconPacks
{
    public sealed class BuildingIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Buildings"; } }
        public override string FontFileName { get { return "Buildings_icons.ttf"; } }
        public override Type IconClass { get { return typeof(BuildingIcons); } }
    }
}