using System;

namespace NovaUI.IconPacks
{
    public sealed class EditorIconPack : IconPackBase
    {
        public override string DisplayName { get { return "EDITOR"; } }
        public override string FontFileName { get { return "Editor_icons.ttf"; } }
        public override Type IconClass { get { return typeof(EditorIcons); } }
    }
}