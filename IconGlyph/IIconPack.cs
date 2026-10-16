using System;

namespace NovaUI
{
    public interface IIconPack
    {
        string DisplayName { get; }
        byte[] FontData { get; }
        Type IconClass { get; }
    }
}