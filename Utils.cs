using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace AdvancedCompany
{
    [HarmonyPatch]
    public class Utils
    {
        private static List<GameObject> NetworkPrefabs = new List<GameObject>();
        private static bool NetworkManagerStarted = false;
        public static NetworkManager NetworkManager;
        public static void AddNetworkPrefab(GameObject go)
        {
            NetworkPrefabs.Add(go);
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPostfix]
        private static void Start(GameNetworkManager __instance)
        {
            for (var i = 0; i < NetworkPrefabs.Count; i++)
                NetworkManager.Singleton.AddNetworkPrefab(NetworkPrefabs[i]);
        }

    }
}
