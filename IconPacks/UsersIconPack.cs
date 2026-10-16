using System;

namespace NovaUI.IconPacks
{
    public sealed class UsersIconPack : IconPackBase
    {
        public override string DisplayName { get { return "USERS"; } }
        public override string FontFileName { get { return "Users_icons.ttf"; } }
        public override Type IconClass { get { return typeof(UsersIcons); } }
    }
}