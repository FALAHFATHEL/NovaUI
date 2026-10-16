using System;

namespace NovaUI.IconPacks
{
    public sealed class GeneralV2IconPack : IconPackBase
    {
        public override string DisplayName { get { return "General V2"; } }
        public override string FontFileName { get { return "General_icons2.ttf"; } }
        public override Type IconClass { get { return typeof(GeneralV2Icons); } }
    }
}