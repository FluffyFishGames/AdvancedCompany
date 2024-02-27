using HarmonyLib;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class NoLobbyPatches
    {
        [HarmonyPatch(typeof(global::StartOfRound), "Start")]
        [HarmonyPostfix]
        static void AddPlayers()
        {
            for (var i = 0; i < global::StartOfRound.Instance.allPlayerObjects.Length; i++)
            {
                Game.Player.GetPlayer(global::StartOfRound.Instance.allPlayerScripts[i]);
            }
        }
    }
}
