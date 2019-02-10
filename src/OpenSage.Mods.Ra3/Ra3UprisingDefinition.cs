﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.Ra3
{
    public class Ra3UprisingDefinition : IGameDefinition
    {
        public SageGame Game => SageGame.Ra3Uprising;
        public string DisplayName => "Command & Conquer (tm): Red Alert (tm) 3 Uprising";
        public IGameDefinition BaseGame => null;

        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => @"Launcher\splash.bmp";

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3 Uprising", "Install Dir")
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3 Uprising", "language")
        };

        public IEnumerable<RegistryKeyPath> UserDataLeafName { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\Electronic Arts\Red Alert 3 Uprising", "userdataleafname")
        };

        public string Identifier { get; } = "ra3_uprising";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static Ra3UprisingDefinition Instance { get; } = new Ra3UprisingDefinition();
    }
}
