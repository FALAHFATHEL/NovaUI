using System;

namespace NovaUI
{
    public abstract class IconPackBase : IIconPack
    {
        public abstract string DisplayName { get; }
        public abstract string FontFileName { get; }
        public abstract Type IconClass { get; }

        public byte[] FontData
        {
            get { return EmbeddedFontLoader.Load(FontFileName); }
        }
    }
}