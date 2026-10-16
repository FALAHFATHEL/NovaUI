using System;

namespace NovaUI.IconPacks
{
    public sealed class SportsIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Sports"; } }
        public override string FontFileName { get { return "Game_Sports_icons.ttf"; } }
        public override Type IconClass { get { return typeof(SportsIcons); } }
    }
}