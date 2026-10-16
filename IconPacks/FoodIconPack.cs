using System;

namespace NovaUI.IconPacks
{
    public sealed class FoodIconPack : IconPackBase
    {
        public override string DisplayName { get { return "FOODs"; } }
        public override string FontFileName { get { return "Food_icons.ttf"; } }
        public override Type IconClass { get { return typeof(FoodIcons); } }
    }
}