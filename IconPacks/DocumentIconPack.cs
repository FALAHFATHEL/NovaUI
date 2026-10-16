using System;

namespace NovaUI.IconPacks
{
    public sealed class DocumentIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Documents"; } }
        public override string FontFileName { get { return "Documents_icons.ttf"; } }
        public override Type IconClass { get { return typeof(DocumentIcons); } }
    }
}