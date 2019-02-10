﻿using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Gui;

namespace OpenSage.Mods.Cnc4
{
    public class Cnc4Definition : IGameDefinition
    {
        public SageGame Game => SageGame.Cnc4;
        public string DisplayName => "Command & Conquer (tm) 4: Tiberian Twilight";
        public IGameDefinition BaseGame => null;

        public bool LauncherImagePrefixLang => false;
        public string LauncherImagePath => null;

        public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command Conquer 4 Tiberian Twilight", "Install Dir"), // Origin
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\command and conquer 4", "install dir") // Steam
        };

        public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command Conquer 4 Tiberian Twilight", "language"), // Origin
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\command and conquer 4", "language") // Steam
        };

        public IEnumerable<RegistryKeyPath> UserDataLeafName { get; } = new[]
        {
            new RegistryKeyPath(@"SOFTWARE\EA Games\Command Conquer 4 Tiberian Twilight", "userdataleafname"), // Origin
            new RegistryKeyPath(@"SOFTWARE\Electronic Arts\command and conquer 4", "userdataleafname") // Steam
        };

        public string Identifier { get; } = "cnc4";

        public IMainMenuSource MainMenu { get; }
        public IControlBarSource ControlBar { get; }

        public static Cnc4Definition Instance { get; } = new Cnc4Definition();
    }
}
