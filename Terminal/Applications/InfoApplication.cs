using AdvancedCompany.Config;
using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    public class InfoApplication : IApplication
    {
        private MobileTerminal Terminal;
        private IScreen CurrentScreen;
        private CursorMenu CurrentCursorMenu;
        private TextElement Text;
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("info", new InfoApplication());
        }

        private Dictionary<string, string> HelpTexts = new Dictionary<string, string>()
        {
            { "Bulletproof vest", "This bulletproof vest with 100% certified organic kevlar is capable to withstand great amounts of mechanical impact. It will prove itself to be a valuable asset on your journeys through dangerous places. Can withstand all common turret shots and even shotgun shells." },
            { "Cursed items", "These items are of particular interest to AdvancedTech. BUT BEWARE!: These items are extremely cursed and might alter the way you perceive reality in VERY harmful ways." },
            { "Flippers", "Standing in front of a river but you are packed like a mule? These flippers with our proprietary coating (patented by AdvancedTech) helps you to swim even when your bags are filled. Oxygen not included." },
            { "Headset", "Communication is key to the success of your mission. AdvancedTech provides you with the best of the best in communication hardware. The standard key for communication is 'V'." },
            { "Helmet lamp", "Do you often find yourself in dark places? We do too. That's why we have invented this stylish helmet lamp for night dwellers. Put it onto your patented head equipment slot and press 'F' to toggle it on." },
            { "Lightning rod", "Ever wondered what all that scrap you bring back to Lethal Company is used for? Here is your answer. Put it outside of your ship on an elevated place and deploy it by left clicking while holding it. AdvancedTech is not liable for any damages occuring due to misuse." },
            { "Missile launcher", "Nothing is better than a nice evening firework. Fireworks are something which can invoke wonder in a wide range of species including humans. Please don't use fireworks on humans. AdvancedTech is not liable for any damages." },
            { "Rocket boots", "Those pesky jars on the top of the shelf are bugging you too? No more we say! With the latest invention in personal rocket propellant devices by AdvancedTech you will reach not only the top of the shelf but the top of your building as well." },
            { "Tactical helmet", "Looking for that little bit extra? Communications and light? We got you covered! This tactical helmet isn't only a fashion statement but also very useful when exploring big and dark places as well." },
            { "Vision enhancer", "Lights not gonna cut it for you? This vision enhancer wont just make you see in the dark. You will see further in weather conditions like snow or fog and even water steam can't block your vision." },
        };

        public void Open()
        {
            var elements = new List<ITextElement>();
            elements.Add(new CursorElement() { Name = "Bulletproof vest", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Cursed items", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Flippers", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Headset", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Helmet lamp", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Lightning rod", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Missile launcher", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Rocket boots", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Tactical helmet", Action = () => { } });
            elements.Add(new CursorElement() { Name = "Vision enhancer", Action = () => { } });
            elements.Add(new TextElement() { Text = " " });
            elements.Add(new CursorElement() { Name = "CLOSE", Action = () => { Terminal.Clear(); Terminal.Exit(); Terminal.Submit("help"); } });

            var cursorMenu = new CursorMenu()
            {
                Elements = elements
            };
            Text = new TextElement();
            var menu = new HalfBoxedScreen() { 
                Title = "ENCYCLOPEDIA", 
                LeftContent = new List<ITextElement>()
                {
                    cursorMenu
                },
                RightContent = new List<ITextElement>()
                {
                    Text
                }
            };
            SwitchTo(menu, cursorMenu, false);
        }

        public void SwitchTo(HalfBoxedScreen box, CursorMenu menu, bool back = true)
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
            Terminal = terminal;
            Terminal.DeactivateInput();
            Open();
        }

        public void Update()
        {
            if (CurrentCursorMenu != null)
            {
                if (CurrentCursorMenu.Elements[CurrentCursorMenu.SelectedElement] is CursorElement c)
                {
                    if (HelpTexts.ContainsKey(c.Name))
                        Text.Text = HelpTexts[c.Name];
                }
            }
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
