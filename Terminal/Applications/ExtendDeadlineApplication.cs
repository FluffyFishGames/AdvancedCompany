using AdvancedCompany.Config;
using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    public class ExtendDeadlineApplication : IApplication
    {
        private MobileTerminal Terminal;
        private IScreen CurrentScreen;
        private CursorMenu CurrentCursorMenu;
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("extend", new ExtendDeadlineApplication());
        }
        public void Open()
        {
            var text = "";
            bool confirm = false;
            var price = AdvancedCompany.Perks.ExtendDeadlinePrice;
            if (Network.Manager.Lobby.CurrentShip.ExtendedDeadline)
                text = "The deadline was already extended during this quota.";
            else
            {
                if (price > Game.Manager.Terminal.groupCredits)
                    text = "Insufficient funds.";
                else
                {
                    confirm = true;
                    text = "Do you really want to extend the deadline?";
                }
            }
            var elements = new List<ITextElement>();
            if (confirm)
            {
                elements.Add(new CursorElement()
                {
                    Name = "CONFIRM",
                    Action = () =>
                    {
                        AdvancedCompany.Perks.ExtendDeadline();
                        Terminal.Clear();
                        Terminal.WriteLine("[ Deadline was extended by one day ]");
                        Terminal.Exit();
                    }
                });
                elements.Add(new CursorElement()
                {
                    Name = "DENY",
                    Action = () =>
                    {
                        Terminal.Clear();
                        Terminal.Exit();
                        Terminal.Submit("help");
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
                        Terminal.Clear();
                        Terminal.Exit(); 
                        Terminal.Submit("help");
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

        public void SwitchTo(BoxedScreen box, CursorMenu menu, bool back = true)
        {
            this.CurrentScreen = box;
            this.CurrentCursorMenu = menu;
            if (!back)
                menu.SelectedElement = 0;
        }

        public static void Close()
        {
        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!ServerConfiguration.Instance.General.EnableExtendDeadline)
            {
                Terminal.WriteLine("This command is not supported.");
                Terminal.Exit();
            }
            else
            {
                Terminal = terminal;
                Terminal.DeactivateInput();
                Open();
            }
        }

        public void Update()
        {
            Terminal.SetText(this.CurrentScreen.GetText(58), true);
            if (CurrentCursorMenu != null)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                    CurrentCursorMenu.SelectedElement--;
                if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                    CurrentCursorMenu.SelectedElement++;
                if (Keyboard.current.enterKey.wasPressedThisFrame)
                    CurrentCursorMenu.Execute();
            }
        }

        public void Exit()
        {
        }

        public void Submit(string text)
        {
        }

    }
}
