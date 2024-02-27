using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    internal class StoreApplication : IApplication
    {
        private MobileTerminal Terminal;
        private IScreen CurrentScreen;
        private BoxedScreenStore StoreScreen;
        private CursorMenu CurrentCursorMenu;
        private TextElement CostNode;
        private CursorElement BuyNode;

        public static void Boot()
        {
            MobileTerminal.RegisterApplication("store", new StoreApplication());
            Network.Manager.AddListener<BuyItems>((msg) =>
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    Game.Manager.Terminal.numberOfItemsInDropship += msg.Items.Length;
                    Game.Manager.Terminal.orderedItemsFromTerminal.AddRange(msg.Items);
                }
                for (var i = 0; i < msg.Unlockables.Length; i++)
                {
                    if (global::StartOfRound.Instance.unlockablesList.unlockables.Count > msg.Unlockables[i] && global::StartOfRound.Instance.unlockablesList.unlockables[msg.Unlockables[i]] != null)
                    {
                        if (NetworkManager.Singleton.IsServer)
                            global::StartOfRound.Instance.UnlockShipObject(msg.Unlockables[i]);
                        global::StartOfRound.Instance.unlockablesList.unlockables[msg.Unlockables[i]].hasBeenUnlockedByPlayer = true;
                    }
                    else Plugin.Log.LogWarning("Unlockable " + msg.Unlockables[i] + " is unavailable!");
                }
                Game.Manager.Terminal.groupCredits = msg.NewCredits;
            });
        }

        public void Exit()
        {
        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            if (!Network.Manager.Lobby.Player().Controller.isInHangarShipRoom)
            {
                terminal.WriteLine("You can't buy items outside of the ship.");
                terminal.Exit();
                return;
            }
            int buyIndex = 0;
            Terminal = terminal;

            CurrentCursorMenu = new CursorMenu() { Elements = new List<ITextElement>() };

            CurrentCursorMenu.Elements.Add(new TextElement() { Text = "∎ ITEMS ∎" });

            for (var i = 0; i < Game.Manager.Items.BuyableItems.Count; i++)
            {
                var item = Game.Manager.Items.BuyableItems[i];
                var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                if (config == null || config.Active)
                {
                    CurrentCursorMenu.Elements.Add(new ItemCursorElement(item) { Action = () => { CurrentCursorMenu.SelectedElement = buyIndex; } });
                }
            }

            var firstUpgrade = true;
            for (var i = 0; i < Game.Manager.Items.ShipUpgrades.Count; i++)
            {
                var upgrade = Game.Manager.Items.ShipUpgrades[i];
                if (Game.Manager.Items.UnlockablePrices.ContainsKey(upgrade) && Game.Manager.Items.IsUnlockableBuyable(upgrade))
                {
                    var maxAmount = upgrade.maxNumber - (upgrade.hasBeenUnlockedByPlayer || upgrade.alreadyUnlocked ? 1 : 0);
                    if (maxAmount > 0)
                    {
                        if (firstUpgrade)
                        {
                            CurrentCursorMenu.Elements.Add(new TextElement() { Text = " " });
                            CurrentCursorMenu.Elements.Add(new TextElement() { Text = "∎ SHIP UPGRADES ∎" });
                            firstUpgrade = false;
                        }

                        CurrentCursorMenu.Elements.Add(new UnlockableCursorElement(upgrade, maxAmount) { Action = () => { CurrentCursorMenu.SelectedElement = buyIndex; } });
                    }
                }
            }

            var availableDecor = new List<UnlockableItem>();
            for (var i = 0; i < Game.Manager.Terminal.ShipDecorSelection.Count; i++)
            {
                try
                {
                    availableDecor.Add(global::StartOfRound.Instance.unlockablesList.unlockables[Game.Manager.Terminal.ShipDecorSelection[i].shipUnlockableID]);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while adding ship decor:");
                    Plugin.Log.LogError(e);
                }
            }

            var firstDecoration = true;
            for (var i = 0; i < Game.Manager.Items.Decorations.Count; i++)
            {
                var upgrade = Game.Manager.Items.Decorations[i];
                if (Game.Manager.Items.UnlockablePrices.ContainsKey(upgrade) && availableDecor.Contains(upgrade) && Game.Manager.Items.IsUnlockableBuyable(upgrade))
                {
                    var maxAmount = upgrade.maxNumber - (upgrade.hasBeenUnlockedByPlayer || upgrade.alreadyUnlocked ? 1 : 0);
                    if (maxAmount > 0)
                    {
                        if (firstDecoration)
                        {
                            CurrentCursorMenu.Elements.Add(new TextElement() { Text = " " });
                            CurrentCursorMenu.Elements.Add(new TextElement() { Text = "∎ DECORATIONS ∎" });
                            firstDecoration = false;
                        }
                        CurrentCursorMenu.Elements.Add(new UnlockableCursorElement(upgrade, maxAmount) { Action = () => { CurrentCursorMenu.SelectedElement = buyIndex; } });
                    }
                }
            }

            CurrentCursorMenu.Elements.Add(new TextElement() { Text = " " });

            CurrentCursorMenu.Elements.Add(new CursorElement() { Name = "BACK", Action = () => { Terminal.Clear(); Terminal.Exit(); Terminal.Submit("help"); } });
            buyIndex = CurrentCursorMenu.Elements.Count;
            BuyNode = new CursorElement() { Name = "BUY", Action = () => { Buy(); } };
            CurrentCursorMenu.Elements.Add(BuyNode);


            CurrentCursorMenu.SelectedElement = 1;
            StoreScreen = new BoxedScreenStore() { Title = "COMPANY STORE", TotalCost = 100, Content = new List<AdvancedCompany.Terminal.ITextElement>() { new ScrollBox(15) { Content = CurrentCursorMenu } } };
            CurrentScreen = StoreScreen;
            Terminal.DeactivateInput();
        }

        public void Submit(string text)
        {
        }

        internal void Buy()
        {
            var totalCost = 0;
            var itemsToBuy = new List<int>();
            var unlockablesToBuy = new List<int>();
            for (var i = 0; i < CurrentCursorMenu.Elements.Count; i++)
            {
                if (CurrentCursorMenu.Elements[i] is ItemCursorElement item)
                {
                    var ind = -1;
                    for (var j = 0; j < Game.Manager.Terminal.buyableItemsList.Length; j++)
                    {
                        if (Game.Manager.Terminal.buyableItemsList[j] == item.Item)
                        {
                            ind = j;
                            break;
                        }
                    }
                    if (ind > -1)
                    {
                        totalCost += item.Amount * Game.Manager.Items.GetItemPrice(item.Item, true);
                        for (var c = 0; c < item.Amount; c++)
                            itemsToBuy.Add(ind);
                    }
                }
                if (CurrentCursorMenu.Elements[i] is UnlockableCursorElement unlockable)
                {
                    var ind = global::StartOfRound.Instance.unlockablesList.unlockables.IndexOf(unlockable.Item);
                    if (ind > -1)
                    {
                        if (unlockable.Amount > 0)
                        {
                            totalCost += unlockable.Amount * Game.Manager.Items.GetUnlockablePrice(unlockable.Item);
                            unlockablesToBuy.Add(ind);
                        }
                    }

                }
            }
            StoreScreen.TotalCost = totalCost;

            var credits = Game.Manager.Terminal.groupCredits;
            if (credits >= totalCost)
            {
                Network.Manager.Send(new BuyItems() { Items = itemsToBuy.ToArray(), Unlockables = unlockablesToBuy.ToArray(), NewCredits = credits - totalCost });
                Terminal.Exit();
                Terminal.Clear();
                Terminal.WriteLine("Thanks for your purchase! Your ordered items are on the way!");
            }
        }

        public void Update()
        {
            Terminal.SetText(this.CurrentScreen.GetText(58), false);
            if (CurrentCursorMenu != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                {
                    var currentElement = CurrentCursorMenu.Elements[CurrentCursorMenu.SelectedElement];
                    if (currentElement is ItemCursorElement item)
                        item.Amount = (int)Mathf.Max(0, item.Amount - 1);
                    if (currentElement is UnlockableCursorElement unlockable)
                        unlockable.Amount = (int)Mathf.Max(0, unlockable.Amount - 1);
                }
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                {
                    var currentElement = CurrentCursorMenu.Elements[CurrentCursorMenu.SelectedElement];
                    if (currentElement is ItemCursorElement item)
                        item.Amount = (int)Mathf.Max(0, item.Amount + 1);
                    if (currentElement is UnlockableCursorElement unlockable)
                        unlockable.Amount = (int)Mathf.Min(unlockable.MaxAmount, unlockable.Amount + 1);
                }
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                    CurrentCursorMenu.SelectedElement--;
                if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                    CurrentCursorMenu.SelectedElement++;

                var totalCost = 0;
                for (var i = 0; i < CurrentCursorMenu.Elements.Count; i++)
                {
                    if (CurrentCursorMenu.Elements[i] is ItemCursorElement item)
                        totalCost += item.Amount * Game.Manager.Items.GetItemPrice(item.Item, true);
                    if (CurrentCursorMenu.Elements[i] is UnlockableCursorElement unlockable)
                        totalCost += unlockable.Amount * Game.Manager.Items.GetUnlockablePrice(unlockable.Item);
                }
                StoreScreen.TotalCost = totalCost;

                var credits = Game.Manager.Terminal.groupCredits;
                if (credits < totalCost)
                    BuyNode.Name = "<color=#666666>BUY</color>";
                else
                    BuyNode.Name = "<color=#66FF66>BUY</color>";

                if (Keyboard.current.enterKey.wasPressedThisFrame)
                    CurrentCursorMenu.Execute();
            }
        }
    }
}
