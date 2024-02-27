using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    public class DebugApplication : IApplication
    {

        public static void Boot()
        {
            var app = new DebugApplication();
            MobileTerminal.RegisterApplication("debug", app);
        }

        public void Exit()
        {

        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (args.Length > 0 && args[0] == "hack")
            {
                var text = @"1db3 d306 1bbe 4424 4d39 cb8d 4acb 15a3
d731 46f6 5c24 3cd2 f8dc 58d1 202e e524
d514 6469 65b1 eed3 204d 73e2 82eb 81c0
cd89 e7ba 515a 7365 7276 656b db64 68df
65cd 8a5d 7a4c d01e 05e6 9810 980e aaf0
c31d 62c9 87c0 55c2 59b1 b3d4 c283 b8be
a59a 2461 9b24 9b42 1fb7 6f62 6579 5f63
eadd b66b 66eb 06e9 1e33 18be 4786 2158
5837 30dd 2ca4 dbbc 2db1 7fa5 b571 0594
c01c a935 bbfb e0eb 38fd 8517 2ccf 811b
223d 9264 6965 4b16 fca3 8bcb 5ee7 f70e
1e94 5e60 d0f6 ea2a 8bd3 ed90 6469 6565";
                terminal.SetText(text, true);
            }
            else
            {
                if (args.Length > 0 && args[0] == "quota")
                    TimeOfDay.Instance.quotaFulfilled += 100000;
                var text = "╢ PLAYER STATUS ╟\n";
                text += "Local client ID: " + Lobby.LocalPlayerNum;
                var player = Network.Manager.Lobby.Player();
                text += player == null ? "Local player not found in lobby?\n" : "Local player found in lobby!\n";
                if (player != null)
                {
                    text += "Client ID:" + player.PlayerNum + "\n";
                }
                var localPlayer = Game.Player.GetPlayer(global::StartOfRound.Instance.localPlayerController);
                text += localPlayer == null ? "Local player not found in players?\n" : "Local player found in players!\n";
                if (localPlayer != null)
                {
                    text += "Client ID:" + localPlayer.PlayerNum + "\n";
                }
                text += "\n";
                text += "╢ LOBBY STATUS ╟\n";
                text += "Game has started: " + global::GameNetworkManager.Instance.gameHasStarted + "\n";
                text += "Is in ship phase: " + global::StartOfRound.Instance.inShipPhase + "\n";
                text += "\n";
                text += "Clients: " + Network.Manager.Lobby.ConnectedPlayers.Count + "\n";
                foreach (var kv in Network.Manager.Lobby.ConnectedPlayers)
                {
                    var levels = new List<string>();
                    foreach (var kv2 in kv.Value.Levels)
                        levels.Add(kv2.Key + "=" + kv2.Value);
                    text += "Client #" + kv.Key + ": id=" + kv.Value.PlayerNum + ";isLate=" + Network.Manager.Lobby.LateJoiners.Contains((int)kv.Key) + ";isDead=" + kv.Value.Controller.isPlayerDead + ";cosmetic=" + string.Join(",", kv.Value.Cosmetics) + ";xp=" + kv.Value.XP + ";" + string.Join(";", levels) + "\n";
                }
                terminal.SetText(text, true);
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
