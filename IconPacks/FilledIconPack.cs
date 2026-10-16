using System;

namespace NovaUI.IconPacks
{
    public sealed class FilledIconPack : IconPackBase
    {
        public override string DisplayName { get { return "FILLED PACK"; } }
        public override string FontFileName { get { return "Filled_icons.ttf"; } }
        public override Type IconClass { get { return typeof(FilledIcons); } }
    }
}