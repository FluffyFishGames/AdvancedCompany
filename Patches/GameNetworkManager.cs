using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using Unity.Netcode;
using AdvancedCompany.Network;
using AdvancedCompany.Game;
using AdvancedCompany.Config;
using AdvancedCompany.Network.Messages;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class GameNetworkManager
    {
        public static string KickReason = null;

        [HarmonyPatch(typeof(global::GameNetworkManager), "Singleton_OnClientConnectedCallback")]
        [HarmonyPrefix]
        private static void Singleton_OnClientConnectedCallback(global::GameNetworkManager __instance, ulong clientId)
        {
            Network.Manager.NetworkManager_OnClientConnectedCallback(clientId);
        }
        [HarmonyPatch(typeof(global::GameNetworkManager), "Start")]
        [HarmonyPostfix]
        private static void Start(global::GameNetworkManager __instance)
        {
            Network.Manager.Initialize();
            Network.Manager.OnKicked += (reason) =>
            {
                KickReason = reason;
            };
        }


        [HarmonyPatch(typeof(global::GameNetworkManager), "SaveItemsInShip")]
        [HarmonyPostfix]
        public static void SaveItemsInShip(GameNetworkManager __instance)
        {

        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "Disconnect")]
        [HarmonyPostfix]
        private static void Disconnect(global::GameNetworkManager __instance)
        {
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "SaveGame")]
        [HarmonyPrefix]
        private static void Save(global::GameNetworkManager __instance)
        {
            try
            {
                Network.Manager.Lobby.Player().SaveLocal();
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                    Network.Manager.Lobby.CurrentShip.Save(__instance.currentSaveFileName);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "ResetUnlockablesListValues")]
        [HarmonyPrefix]
        private static bool ResetClone(global::GameNetworkManager __instance)
        {
            if (!(global::StartOfRound.Instance != null))
            {
                return true;
            }
            List<UnlockableItem> unlockables = global::StartOfRound.Instance.unlockablesList.unlockables;
            for (int i = 0; i < unlockables.Count; i++)
            {
                if (!ServerConfiguration.Instance.General.SaveSuitsAfterDeath || unlockables[i].unlockableType != 0)
                    unlockables[i].hasBeenUnlockedByPlayer = false;
                if (unlockables[i].unlockableType == 1)
                {
                    unlockables[i].placedPosition = UnityEngine.Vector3.zero;
                    unlockables[i].placedRotation = UnityEngine.Vector3.zero;
                    unlockables[i].hasBeenMoved = false;
                    unlockables[i].inStorage = false;
                }
            }
            return false;
        }
    }
}
