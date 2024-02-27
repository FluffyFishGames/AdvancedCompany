using AdvancedCompany.Config;
using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal.Applications
{
    public class HelpApplication : IApplication
    {
        public void Exit()
        {

        }

        private string Headline()
        {
            var r = UnityEngine.Random.Range(0, 10000f);
            if (r > 9999f)
                return "   mankind die   ";
            else if (r > 9998f)
                return "  mankind serve  ";
            else if (r > 9997f)
                return "   mankind obey  ";
            else 
                return "advancing mankind";
        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            terminal.SetText("                      ╔═════════════╗\r\n"+
"╭─────────────────────╢ <color=#ff6666>ADVANCED</color> <color=#ffffff>OS</color> ╟────────────────────╮\r\n"+
"│                     ╚═════════════╝                    │\r\n" +
"│                    "+Headline()+"                   │\r\n" +
"│                                                        │\r\n" +
"│ Welcome Lethal-1,                                      │\r\n" +
"│ Your <color=#ffffff>license</color> is <color=#00ff00>valid</color> for <color=#ffffff>2 years and 11 months</color>        │\r\n" +
"│                                                        │\r\n" +
"│ <color=#ffffff>▶ PERKS</color>                                                │\r\n" +
"│   Let you level your perks on-the-fly                  │\r\n" +
(ServerConfiguration.Instance.General.EnableExtendDeadline ?
"│ <color=#ffffff>▶ EXTEND</color>                                               │\r\n" +
"│   Extend the deadline.                                 │\r\n" : "") +
"│ <color=#ffffff>▶ INFO</color>                                                 │\r\n" +
"│   Opens a manual with further information.             │\r\n" +
"│ <color=#ffffff>▶ STORE</color>                                                │\r\n" +
"│   Open the store.                                      │\r\n" +
"│ <color=#ffffff>▶ HELP</color>                                                 │\r\n" +
"│   Shows this text for guidance.                        │\r\n" +
"╰────────────────────────────────────────────────────────╯\r\n");
            terminal.Exit();
        }

        public void Submit(string text)
        {
        }

        public void Update()
        {

        }
    }
}
