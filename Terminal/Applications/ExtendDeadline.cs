using System;
using System.Collections.Generic;
/*
namespace AdvancedCompany.Terminal.Applications
{
    public class ExtendDeadline
    {
        public static void Open()
        {
            var text = "";
            bool confirm = false;
            var price = AdvancedCompany.Perks.ExtendDeadlinePrice;
            if (Network.Manager.Lobby.CurrentShip.ExtendedDeadline)
                text = "The deadline was already extended during this quota.";
            else
            {
                if (price > Patches.Terminal.Instance.groupCredits)
                    text = "Insufficient funds.";
                else
                {
                    confirm = true;
                    text = "Do you really want to extend the deadline?";
                }
            }
            var elements = new List<CursorElement>();
            if (confirm)
            {
                elements.Add(new CursorElement()
                {
                    Name = "CONFIRM",
                    Action = () =>
                    {
                        AdvancedCompany.Perks.ExtendDeadline();
                        Patches.Terminal.ActiveScreen = null;
                        Patches.Terminal.ActiveCursorMenu = null;
                        Patches.Terminal.SetText("[ Deadline was extended by one day ]\n\n\n");
                    }
                });
                elements.Add(new CursorElement()
                {
                    Name = "DENY",
                    Action = () =>
                    {
                        Close();
                    }
                });
            }
            else
            {
                elements.Add(new CursorElement()
                {
                    Name = "BACK",
                    Action = () =>
                    {
                        Close();
                    }
                });
            }
            var cursorMenu = new CursorMenu()
            {
                Elements = elements
            };
            var menu = new BoxedScreen() { Title = "EXTEND DEADLINE", Content = new List<ITextElement>() { new TextElement() { Text = $"To extend the deadline you need {price} credits." }, new TextElement() { Text = " " }, new TextElement() { Text = text }, cursorMenu } };
            SwitchTo(menu, cursorMenu, false);
        }

        public static void SwitchTo(BoxedScreen box, CursorMenu menu, bool back = true)
        {
            if (Patches.Terminal.Instance != null)
            {
                Patches.Terminal.ActiveScreen = box;
                Patches.Terminal.ActiveCursorMenu = menu;
                if (!back)
                    menu.SelectedElement = 0;
            }
        }

        public static void Close()
        {
            if (Patches.Terminal.Instance != null)
            {
                Patches.Terminal.ActiveCursorMenu = null;
                Patches.Terminal.ActiveScreen = null;
                Patches.Terminal.ClosingScreen = true;
            }
        }
    }
}
*/