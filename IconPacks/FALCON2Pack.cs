using System;

namespace NovaUI.IconPacks
{
    public sealed class FALCON2Pack : IconPackBase
    {
        public override string DisplayName { get { return "FALCON V2"; } }
        public override string FontFileName { get { return "FALCON_Icons_Collection_Pack_2.ttf"; } }
        public override Type IconClass { get { return typeof(FALCON2ICONS); } }
    }
}