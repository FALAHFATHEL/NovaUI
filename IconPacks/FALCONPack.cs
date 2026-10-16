using System;

namespace NovaUI.IconPacks
{
    public sealed class FALCONPack : IconPackBase
    {
        public override string DisplayName { get { return "FALCON V1"; } }
        public override string FontFileName { get { return "FALCON_Icons_Collection_Pack_1.ttf"; } }
        public override Type IconClass { get { return typeof(FALCONICONS); } }
    }
}