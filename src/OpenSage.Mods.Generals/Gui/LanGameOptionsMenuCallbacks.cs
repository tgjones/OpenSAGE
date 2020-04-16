﻿using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    class LanGameOptionsMenuCallbacks
    {
        private const string TextEntryChatPrefix = "LanGameOptionsMenu.wnd:TextEntryChat";

        public static GameOptionsUtil GameOptions { get; private set; }

        public static void LanGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            if (!GameOptions.HandleSystem(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "LanGameOptionsMenu.wnd:ButtonBack":
                                context.Game.LobbyManager.Stop();
                                context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                                // TODO: Go back to Single Player sub-menu
                                break;
                        }
                        break;
                }
            }
        }

        public static void LanGameOptionsMenuInit(Window window, Game game)
        {
            GameOptions = new GameOptionsUtil(window, game, "Lan");

            // Clear chat field
            var textChat = (TextBox) window.Controls.FindControl(TextEntryChatPrefix);
            textChat.Text = string.Empty;
        }
    }
}
