using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    internal class ShipXPApplication : IApplication
    {
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("shipxp", new ShipXPApplication());
        }

        public void Exit()
        {

        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!NetworkManager.Singleton.IsServer)
                terminal.WriteLine("Only host is allowed to run this command!");
            else if (args.Length > 0 && int.TryParse(args[0], out var xp))
            {
                Network.Manager.Send(new GrantShipXP() { XP = xp });
                terminal.WriteLine($"Granted {xp} XP to the ship.");
            }
            else
            {
                terminal.WriteLine("Usage: shipxp [amount]");
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