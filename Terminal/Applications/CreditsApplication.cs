using AdvancedCompany.Config;
using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    internal class CreditsApplication : IApplication
    {
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("credits", new CreditsApplication());
        }

        public void Exit()
        {

        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!NetworkManager.Singleton.IsServer)
                terminal.WriteLine("Only host is allowed to run this command!");
            else if (args.Length > 0 && int.TryParse(args[0], out var credits))
            {
                Game.Manager.Terminal.groupCredits += credits;
                Game.Manager.Terminal.SyncGroupCreditsServerRpc(Game.Manager.Terminal.groupCredits, Game.Manager.Terminal.numberOfItemsInDropship);
                terminal.WriteLine("You've been given " + credits + " credits!");
            }
            else
            {
                terminal.WriteLine("Usage: credits [amount]");
            }
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