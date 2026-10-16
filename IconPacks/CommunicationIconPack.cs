using System;

namespace NovaUI.IconPacks
{
    public sealed class CommunicationIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Communication"; } }
        public override string FontFileName { get { return "Communication_icons.ttf"; } }
        public override Type IconClass { get { return typeof(CommunicationIcons); } }
    }
}