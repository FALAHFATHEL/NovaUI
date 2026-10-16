using System;

namespace NovaUI.IconPacks
{
    public sealed class MediaIconPack : IconPackBase
    {
        public override string DisplayName { get { return "MEDIA"; } }
        public override string FontFileName { get { return "Media_icons.ttf"; } }
        public override Type IconClass { get { return typeof(MediaIcons); } }
    }
}