using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Game
{
    internal partial class Manager
    {
        internal class Save
        {
            public static void SaveItemsInShip(GrabbableObject[] objs)
            {
                Plugin.Log.LogInfo("Saving ship data...");
                var itemNames = new List<string>();
                for (int i = 0; i < objs.Length && i <= global::StartOfRound.Instance.maxShipItemCapacity; i++)
                {
                    if (StartOfRound.Instance.allItemsList.itemsList.Contains(objs[i].itemProperties) && !objs[i].deactivated && objs[i].itemProperties.spawnPrefab != null && !objs[i].itemUsedUp)
                    {
                        var itemName = objs[i].itemProperties.itemId + "-" + objs[i].itemProperties.itemName + "-" + objs[i].itemProperties.name;
                        Plugin.Log.LogInfo("Saving " + itemName);
                        itemNames.Add(itemName);
                    }
                }
                ES3.Save("shipGrabbableItemNames", itemNames.ToArray(), GameNetworkManager.Instance.currentSaveFileName);
            }

            public static int[] LoadItemsInShip(int[] objs)
            {
                Plugin.Log.LogInfo("Loading savegame data...");
                if (ES3.KeyExists("shipGrabbableItemNames", GameNetworkManager.Instance.currentSaveFileName))
                {
                    Plugin.Log.LogDebug("Found shipGrabbableItemNames");
                    var itemNames = ES3.Load<string[]>("shipGrabbableItemNames", GameNetworkManager.Instance.currentSaveFileName);
                    for (var i = 0; i < itemNames.Length; i++)
                    {
                        var parts = itemNames[i].Split("-");
                        if (parts.Length == 3)
                        {
                            var id = int.Parse(parts[0]);
                            var itemName = parts[1];
                            var name = parts[2];
                            bool found = false;
                            for (var j = 0; j < StartOfRound.Instance.allItemsList.itemsList.Count; j++)
                            {
                                if (StartOfRound.Instance.allItemsList.itemsList[j].itemId == id)
                                {
                                    if (StartOfRound.Instance.allItemsList.itemsList[j].itemName == itemName)
                                    {
                                        Plugin.Log.LogDebug("Found item " + id + "-" + itemName);
                                        found = true;
                                        objs[i] = j;
                                        break;
                                    }
                                    else if (StartOfRound.Instance.allItemsList.itemsList[j].name == name)
                                    {
                                        Plugin.Log.LogDebug("Found item " + id + "-" + name);
                                        found = true;
                                        objs[i] = j;
                                        break;
                                    }
                                    else
                                    {
                                        found = true;
                                        objs[i] = j;
                                    }
                                }
                            }
                            if (!found)
                            {
                                Plugin.Log.LogWarning("Couldn't find item " + itemNames[i] + ". Replacing it with question mark block!");
                                objs[i] = StartOfRound.Instance.allItemsList.itemsList.IndexOf(Game.Manager.ItemProperties[9999]);
                            }
                        }
                    }
                }
                return objs;
            }
        }
    }
}
