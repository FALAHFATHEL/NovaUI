using System;

namespace NovaUI.IconPacks
{
    public sealed class GeneralV1IconPack : IconPackBase
    {
        public override string DisplayName { get { return "General V1"; } }
        public override string FontFileName { get { return "General-Purpose.ttf"; } }
        public override Type IconClass { get { return typeof(GeneralV1Icons); } }
    }
}