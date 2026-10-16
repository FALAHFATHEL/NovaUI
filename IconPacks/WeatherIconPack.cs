using System;

namespace NovaUI.IconPacks
{
    public sealed class WeatherIconPack : IconPackBase
    {
        public override string DisplayName { get { return "Weather"; } }
        public override string FontFileName { get { return "Weather_icons.ttf"; } }
        public override Type IconClass { get { return typeof(WeatherIcons); } }
    }
}