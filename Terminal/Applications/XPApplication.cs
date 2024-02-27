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
    internal class XPApplication : IApplication
    {
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("xp", new XPApplication());
        }

        public void Exit()
        {

        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!NetworkManager.Singleton.IsServer)
                terminal.WriteLine("Only host is allowed to run this command!");
            else if (args.Length > 1 && int.TryParse(args[1], out var xp))
            {
                Game.Player player = null;
                player = Network.Manager.Lobby.Player(args[0]);
                if (player == null && int.TryParse(args[0], out var playerNum))
                    player = Network.Manager.Lobby.Player(playerNum);
                if (player == null)
                {
                    terminal.WriteLine($"Player {args[0]} couldn't be found!");
                }
                else
                {
                    Network.Manager.Send(new GrantPlayerXP() { All = false, PlayerNum = player.PlayerNum, XP = xp });
                    terminal.WriteLine($"Granted {xp} XP to {player.Controller.playerUsername}.");
                }
            }
            else
            {
                terminal.WriteLine("Usage: xp [playerNum|playerName] [amount]");
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