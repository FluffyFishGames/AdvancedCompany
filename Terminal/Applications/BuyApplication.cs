using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    internal class BuyApplication : IApplication, IFallbackApplication
    {
        private MobileTerminal Terminal;
        private IScreen CurrentScreen;
        private CursorMenu CurrentCursorMenu;

        public static void Boot()
        {
            var app = new BuyApplication();
            MobileTerminal.RegisterApplication("buy", app);
        }
        public void Exit()
        {
        }

        internal void Buy(UnlockableItem item, int amount)
        {
            var itemsToBuy = new List<int>();
            var unlockablesToBuy = new List<int>();
            var totalCost = 0;
            var ind = global::StartOfRound.Instance.unlockablesList.unlockables.IndexOf(item);
            if (ind > -1)
            {
                totalCost = amount * Game.Manager.Items.GetUnlockablePrice(item);
                for (var c = 0; c < amount; c++)
                    unlockablesToBuy.Add(ind);

                var credits = Game.Manager.Terminal.groupCredits;
                if (credits >= totalCost)
                {
                    Network.Manager.Send(new BuyItems() { Items = itemsToBuy.ToArray(), Unlockables = unlockablesToBuy.ToArray(), NewCredits = credits - totalCost });
                    Terminal.Exit();
                    Terminal.Clear();
                    Terminal.WriteLine("Thanks for your purchase! Your ordered items are on the way!");
                }
            }
        }
        
        internal void Buy(Item item, int amount)
        {
            var itemsToBuy = new List<int>();
            var unlockablesToBuy = new List<int>();
            var totalCost = 0;
            var ind = -1;
            for (var j = 0; j < Game.Manager.Terminal.buyableItemsList.Length; j++)
            {
                if (Game.Manager.Terminal.buyableItemsList[j] == item)
                {
                    ind = j;
                    break;
                }
            }
            if (ind > -1)
            {
                totalCost = amount * Game.Manager.Items.GetItemPrice(item, true);
                for (var c = 0; c < amount; c++)
                    itemsToBuy.Add(ind);

                var credits = Game.Manager.Terminal.groupCredits;
                if (credits >= totalCost)
                {
                    Network.Manager.Send(new BuyItems() { Items = itemsToBuy.ToArray(), Unlockables = unlockablesToBuy.ToArray(), NewCredits = credits - totalCost });
                    Terminal.Exit();
                    Terminal.Clear();
                    Terminal.WriteLine("Thanks for your purchase! Your ordered items are on the way!");
                }
            }
        }


        public void Fallback(MobileTerminal terminal, string input)
        {
            var parts = input.Split(" ");
            var name = "";
            int amount = 1;
            for (var i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out var a))
                    amount = a;
                else
                    name += parts[i];
            }
            Main(terminal, new string[] { name, amount + "" });
        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!Network.Manager.Lobby.Player().Controller.isInHangarShipRoom)
            {
                terminal.WriteLine("You can't buy items outside of the ship.");
                terminal.Exit();
                return;
            }

            Terminal = terminal;
            var amount = 1;
            var name = "";
            for (var i = 0; i < args.Length; i++)
            {
                if (int.TryParse(args[i], out var a))
                    amount = a;
                else
                    name += args[i];
            }
            var found = FindLowest(name);
            if (found.Item2 != null)
            {
                if (found.Item2 is Item item)
                {
                    var totalCost = Game.Manager.Items.GetItemPrice(item) * amount;
                    if (totalCost <= Game.Manager.Terminal.groupCredits)
                    {
                        this.CurrentCursorMenu = new CursorMenu()
                        {
                            Elements = new List<ITextElement>() {
                            new CursorElement() { Name = "CONFIRM", Action = () => { Buy(item, amount); } },
                            new CursorElement() { Name = "DENY", Action = () => { Terminal.Clear(); Terminal.Exit(); } }
                        }
                        };
                        this.CurrentScreen = new BoxedScreenStore()
                        {
                            Title = "BUY",
                            TotalCost = totalCost,
                            Content = new List<ITextElement>() {
                                new TextElement() { Text = $"Do you want to buy {amount}x {item.itemName} for a total cost of {totalCost} credits?" },
                                this.CurrentCursorMenu
                            }
                        };
                    }
                    else
                    {
                        this.CurrentCursorMenu = new CursorMenu()
                        {
                            Elements = new List<ITextElement>() {
                                new CursorElement() { Name = "BACK", Action = () => { Terminal.Clear(); Terminal.Exit(); } }
                            }
                        };
                        this.CurrentScreen = new BoxedScreenStore()
                        {
                            Title = "BUY",
                            TotalCost = totalCost,
                            Content = new List<ITextElement>() {
                                new TextElement() { Text = $"You can't afford {amount}x {item.itemName}. You need at least {totalCost} credits!" },
                                this.CurrentCursorMenu
                            }
                        };
                    }
                    Terminal.DeactivateInput();
                }
                else if (found.Item2 is UnlockableItem unlockableItem)
                {
                    var maxAmount = unlockableItem.maxNumber - (unlockableItem.alreadyUnlocked || unlockableItem.hasBeenUnlockedByPlayer ? 1 : 0);
                    if (maxAmount > 1)
                    {
                        if (amount > maxAmount)
                            amount = maxAmount;
                        var totalCost = Game.Manager.Items.GetUnlockablePrice(unlockableItem) * amount;
                        if (totalCost <= Game.Manager.Terminal.groupCredits)
                        {
                            this.CurrentCursorMenu = new CursorMenu()
                            {
                                Elements = new List<ITextElement>() {
                                new CursorElement() { Name = "CONFIRM", Action = () => { Buy(unlockableItem, amount); } },
                                new CursorElement() { Name = "DENY", Action = () => { Terminal.Clear(); Terminal.Exit(); } }
                            }
                            };
                            this.CurrentScreen = new BoxedScreenStore()
                            {
                                Title = "BUY",
                                TotalCost = totalCost,
                                Content = new List<ITextElement>() {
                                    new TextElement() { Text = $"Do you want to buy {amount}x {unlockableItem.unlockableName} for a total cost of {totalCost} credits?" },
                                    this.CurrentCursorMenu
                                }
                            };
                        }
                        else
                        {
                            this.CurrentCursorMenu = new CursorMenu()
                            {
                                Elements = new List<ITextElement>() {
                                    new CursorElement() { Name = "BACK", Action = () => { Terminal.Clear(); Terminal.Exit(); } }
                                }
                            };
                            this.CurrentScreen = new BoxedScreenStore()
                            {
                                Title = "BUY",
                                TotalCost = totalCost,
                                Content = new List<ITextElement>() {
                                    new TextElement() { Text = $"You can't afford {amount}x {unlockableItem.unlockableName}. You need at least {totalCost} credits!" },
                                    this.CurrentCursorMenu
                                }
                            };
                        }
                        Terminal.DeactivateInput();
                    }
                    else
                    {
                        terminal.WriteLine(unlockableItem.unlockableName + " was already bought!");
                        terminal.Exit();
                    }
                }
            }
            else
            {
                terminal.WriteLine("Couldn't find item.");
                terminal.Exit();
            }
        }

        public void Submit(string text)
        {
        }

        internal (float, object) FindLowest(string input)
        {
            input = input.Replace(" ", "").Replace("-", "").ToLowerInvariant().Trim();

            List<(string, object)> checks = new();
            for (var i = 0; i < Game.Manager.Items.BuyableItems.Count; i++)
            {
                var item = Game.Manager.Items.BuyableItems[i];
                var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                var active = config == null || config.Active;
                if (active)
                {
                    var itemName = item.itemName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
                    checks.Add((itemName, item));
                }
            }
            for (var i = 0; i < Game.Manager.Items.ShipUpgrades.Count; i++)
            {
                var item = Game.Manager.Items.ShipUpgrades[i];
                var itemName = item.unlockableName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
                checks.Add((itemName, item));
            }
            for (var i = 0; i < Game.Manager.Items.Decorations.Count; i++)
            {
                var item = Game.Manager.Items.Decorations[i];
                var itemName = item.unlockableName.Replace(" ", "").Replace("-", "").ToLowerInvariant();
                checks.Add((itemName, item));
            }

            var lowestDistance = 1000f;
            object lowest = null;
            foreach (var c in checks)
            {
                var distance = Distance(input, c.Item1);
                if (input.Length >= 3 && c.Item1.StartsWith(input))
                    distance = 1;
                if (c.Item1 == input)
                    distance = 0;
                if (distance < 10)
                {
                    if (distance < lowestDistance || lowest == null)
                    {
                        lowestDistance = distance;
                        lowest = c.Item2;
                    }
                }
            }
            return (lowestDistance, lowest);
        }

        public float Certainty(string input)
        {
            if (!Network.Manager.Lobby.Player().Controller.isInHangarShipRoom)
                return 0f;
            var parts = input.Split(" ");
            var name = "";
            for (var i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out var amount))
                    name += parts[i];
            }
            var found = FindLowest(name);
            if (found.Item2 == null)
                return 0f;
            return Mathf.Clamp01(1f - (found.Item1 / 10f));
        }

        public static int Distance(string source1, string source2) //O(n*m)
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = System.Math.Min(
                        System.Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }

        public void Update()
        {
            if (this.CurrentScreen != null)
            {
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
        }
    }
}
/*
            private static TerminalNode FindBuyItem(global::Terminal __instance, string name)
            {
                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                bool foundAmount = false;
                for (var i = 0; i < parts.Count; i++)
                {
                    if (int.TryParse(parts[i], out var num))
                    {
                        __instance.playerDefinedAmount = num;
                        foundAmount = true;
                        parts.RemoveAt(i);
                        i--;
                    }
                }
                if (!foundAmount)
                    __instance.playerDefinedAmount = 1;

                name = Instance.RemovePunctuation(string.Join("", parts)).Replace(" ", "").ToLowerInvariant();
                Dictionary<TerminalNode, int> distances = new Dictionary<TerminalNode, int>();
                foreach (var kv in KeywordsByItemName)
                {
                    if (!distances.ContainsKey(kv.Value))
                    {
                        var distance = ItemDistance(name, kv.Key);
                        if (distance != -1)
                            distances.Add(kv.Value, distance);
                    }
                }
                foreach (var kv in KeywordsByUnlockableName)
                {
                    if (!distances.ContainsKey(kv.Value))
                    {
                        var distance = ItemDistance(name, kv.Key);
                        if (distance != -1)
                            distances.Add(kv.Value, distance);
                    }
                }
                foreach (var kv in KeywordsByDecorName)
                {
                    if (!distances.ContainsKey(kv.Value))
                    {
                        var distance = ItemDistance(name, kv.Key);
                        if (distance != -1)
                            distances.Add(kv.Value, distance);
                    }
                }
                var lowestDistance = 999;
                TerminalNode node = null;
                foreach (var d in distances)
                {
                    if (d.Value < lowestDistance && d.Value < 10)
                    {
                        node = d.Key;
                        lowestDistance = d.Value;
                    }
                }
                return node;
            }

            private static int ItemDistance(string name, string itemName)
            {
                itemName = Instance.RemovePunctuation(itemName).Replace(" ", "").ToLowerInvariant();
                if (itemName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    Plugin.Log.LogMessage("Starts with. Distance: " + (itemName.Length - name.Length));
                    return 4;
                }
                else
                {
                    var distance = Distance(name, itemName);
                    if (distance <= name.Length - 2)
                    {
                        Plugin.Log.LogMessage(name + " " + itemName + " Distance: " + (distance + 5));
                        return distance + 5; // prefer startwith
                    }
                }
                return -1;
            }
        }

        public static int Distance(string source1, string source2) //O(n*m)
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
        /**
         
                
                new Command() { CommandText = "credits", Description = "CHEAT - Gives you credits", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    if (@params.Length > 0 && int.TryParse(@params[0], out int credits))
                    {
                        Instance.groupCredits += credits;
                        Instance.SyncGroupCreditsServerRpc(Instance.groupCredits, Instance.numberOfItemsInDropship);
                        SetText($"[ Granted {credits} credits ]\n\n\n");
                    }
                    else
                    {
                        SetText($"Invalid arguments\n\nUsage: credits [number]\n\n\n");
                    }
                } };

                new Command() { CommandText = "xp", Description = "CHEAT - Gives XP", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    if (@params.Length > 1 && int.TryParse(@params[1], out int xp))
                    {
                        Game.Player player = null;
                        player = Network.Manager.Lobby.Player(@params[0]);
                        if (player == null && ulong.TryParse(@params[0], out var clientID))
                            player = Network.Manager.Lobby.Player(clientID);
                        if (player == null)
                        {
                            SetText($"[ Player {@params[0]} couldn't be found ]\n\n\n");
                        }
                        else
                        {
                            Network.Manager.Send(new GrantPlayerXP() { All = false, ClientID = player.ClientID, XP = xp });
                            SetText($"[ Granted {xp} XP to {@params[0]} ]\n\n\n");
                        }
                    }
                    else
                    {
                        SetText($"Invalid arguments\n\nUsage: xp [playerName] [number]\n\n\n");
                    }
                } };

                new Command() { CommandText = "shipxp", Description = "CHEAT - Gives the ship XP", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    if (@params.Length > 0 && int.TryParse(@params[0], out int xp))
                    {
                        Network.Manager.Send(new GrantShipXP() { XP = xp });
                        SetText($"[ Granted {xp} XP to the ship ]\n\n\n");
                    }
                    else
                    {
                        SetText($"Invalid arguments\n\nUsage: shipxp [number]\n\n\n");
                    }
                } };
                
                new Command() { CommandText = "totalquota", Description = "Shows you current total quota of this run", Action = (@params) => {
                    SetText($"[ Your current total quota is {Network.Manager.Lobby.CurrentShip.TotalQuota} ]\n\n\n");
                } };
                
                
                new Command() { CommandText = "profit", Description = "CHEAT - Increases the current profit by 1000", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    global::TimeOfDay.Instance.quotaFulfilled += 1000;
                    SetText($"[ Quota increased by 1000 ]\n\n\n");
                } };
                
                new Command() { CommandText = "reduce", Description = "CHEAT - Reduces deadline to 0", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    global::TimeOfDay.Instance.timeUntilDeadline = global::TimeOfDay.Instance.globalTime;
                    SetText($"[ Deadline reduced to 0 ]\n\n\n");
                } };

                new Command() { CommandText = "stoptime", Description = "CHEAT - Stops the global timer", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    Debug = true;
                    SetText($"[ Stopped the global timer ]\n\n\n");
                } };

                new Command() { CommandText = "spawn", Description = "CHEAT - Spawns an item", IsCheat = true, ServerOnly = true, Action = (@params) => {
                    if (@params.Length == 0)
                    {
                        var availableScrap = "";
                        var first = true;
                        foreach (var kv in GameAssets.Scrap)
                        {
                            availableScrap += (!first ? ", " : "") + kv.Key;
                            first = false;
                        }
                        SetText(new TextElement() { Text = "Available scrap:\n\n" + availableScrap + "\n\n\n" });
                    }
                    else
                    {
                        int price = 666;
                        bool ignoreLast = false;
                        if (int.TryParse(@params[@params.Length - 1], out price))
                        {
                            ignoreLast = true;
                        }
                        
                        var id = String.Join(' ', @params, 0, @params.Length + (ignoreLast ? -1 : 0));
                        
                        if (GameAssets.Scrap.ContainsKey(id))
                        {
                            var obj = (GameObject)GameObject.Instantiate(GameAssets.Scrap[id].spawnPrefab, global::GameNetworkManager.Instance.localPlayerController.transform.position, Quaternion.identity);
                            var gObj = obj.GetComponent<global::GrabbableObject>();
                            gObj.scrapValue = price;
                            obj.GetComponent<NetworkObject>().Spawn();

                            SetText(new TextElement() { Text = "Spawned " + id + "!\n\n\n" });
                        }
                        else
                        {
                            SetText(new TextElement() { Text = id + " not found!\n\n\n" });
                        }
                    }
                } };
         
    }
}
*/