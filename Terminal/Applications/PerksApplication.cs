using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;
using AdvancedCompany.Game;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static Mono.Security.X509.X520;

namespace AdvancedCompany.Terminal.Applications
{
    [Boot.Bootable]
    internal class PerksApplication : IApplication
    {
        private MobileTerminal Terminal;
        private IScreen CurrentScreen;
        private CursorMenu CurrentCursorMenu;
        private static BoxedScreenXP MainMenu;
        private static BoxedScreenXP RespecMenu;
        private static BoxedScreenXP PlayerPerksMenu;
        private static BoxedScreenXP ShipPerksMenu;
        private static BoxedScreenXP BuyPerkMenu;

        private static CursorMenu MainMenuCursorMenu;
        private static CursorMenu RespecCursorMenu;
        private static CursorMenu PlayerPerksCursorMenu;
        private static CursorMenu ShipPerksCursorMenu;
        private static CursorMenu BuyPerkCursorMenu;

        internal bool CanRespecPlayer
        {
            get
            {
                return StartOfRound.Instance.localPlayerController.isInHangarShipRoom;
            }
        }

        internal bool CanRespecShip
        {
            get
            {
                return RoundManager.Instance.currentLevel != null && !RoundManager.Instance.currentLevel.spawnEnemiesAndScrap && !StartOfRound.Instance.inShipPhase;
            }
        }
        public static void Boot()
        {
            MobileTerminal.RegisterApplication("perks", new PerksApplication());
        }

        public void Init()
        {
            MainMenuCursorMenu = new CursorMenu()
            {
                Elements = new List<ITextElement>()
                {
                    new CursorElement() { Name = "PLAYER", Description = "Upgrade your character.", Action = () => { SwitchTo(PlayerPerksMenu, PlayerPerksCursorMenu, false); } },
                    new CursorElement() { Name = "SHIP", Description = "Upgrade your ship.", Action = () => { SwitchTo(ShipPerksMenu, ShipPerksCursorMenu, false); } },
                    new CursorElement() { Name = "RESPEC", Description = "Reset your points.", Action = () => { SwitchTo(RespecMenu, RespecCursorMenu, false); } },
                    new CursorElement() { Name = "CLOSE", Description = null, Action = () => { Terminal.Clear(); Terminal.Exit(); Terminal.Submit("help"); } },
                }
            };
            MainMenu = new BoxedScreenXP() { Title = "PERKS", Content = new List<ITextElement>() { new TextElement() { Text = "Please select an action:" }, new TextElement() { Text = " " }, MainMenuCursorMenu } };

            var playerPerks = new List<ITextElement>();
            foreach (var p in Perk.PerksByType(Perk.Type.PLAYER))
                if (p.IsActive)
                    playerPerks.Add(new PerkCursorElement(p) { Action = () => { BuyPerk(p, () => { SwitchTo(PlayerPerksMenu, PlayerPerksCursorMenu, true); }); } });
            playerPerks.Add(new CursorElement() { Name = "BACK", Action = () => { SwitchTo(MainMenu, MainMenuCursorMenu, true); } });

            PlayerPerksCursorMenu = new CursorMenu()
            {
                Elements = playerPerks
            };

            PlayerPerksMenu = new BoxedScreenXP() { Title = "PLAYER PERKS", Content = new List<ITextElement>() { new TextElement() { Text = "Please select a perk to level:" }, new TextElement() { Text = " " }, PlayerPerksCursorMenu } };

            var shipPerks = new List<ITextElement>();
            foreach (var p in Perk.PerksByType(Perk.Type.SHIP))
                if (p.IsActive)
                    shipPerks.Add(new PerkCursorElement(p) { Action = () => { BuyPerk(p, () => { SwitchTo(ShipPerksMenu, ShipPerksCursorMenu, true); }); } });
            shipPerks.Add(new CursorElement() { Name = "BACK", Action = () => { SwitchTo(MainMenu, MainMenuCursorMenu, true); } });

            ShipPerksCursorMenu = new CursorMenu()
            {
                Elements = shipPerks
            };

            ShipPerksMenu = new BoxedScreenXP() { Title = "SHIP PERKS", Content = new List<ITextElement>() { new TextElement() { Text = "Please select a perk to level:" }, new TextElement() { Text = " " }, ShipPerksCursorMenu } };

            var respecElements = new List<ITextElement>();
            respecElements.Add(new CursorElement()
            {
                Name = "RESPEC PLAYER",
                Description = "Respec your player.",
                Action = () =>
                {
                    if (!CanRespecPlayer)
                    {
                        var cursorMenu = new CursorMenu()
                        {
                            Elements = new List<ITextElement>()
                            {
                                new CursorElement() { Name = "BACK", Action = () => {  SwitchTo(RespecMenu, RespecCursorMenu, true); } }
                            }
                        };
                        var menu = new BoxedScreenXP() { Title = "RESPEC PLAYER", Content = new List<ITextElement>() { new TextElement() { Text = "You can only respec inside your ship." }, new TextElement() { Text = " " }, cursorMenu } };
                        SwitchTo(menu, cursorMenu, false);
                    }
                    else
                    {
                        Confirm("RESPEC PLAYER", "Do you really want to respec your player?",
                            () =>
                            {
                                Network.Manager.Send(new Network.Messages.Respec() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, Type = Perk.Type.PLAYER, Reset = false });
                                SwitchTo(RespecMenu, RespecCursorMenu, true);
                            },
                            () =>
                            {
                                SwitchTo(RespecMenu, RespecCursorMenu, true);
                            });
                    }
                }
            });
            respecElements.Add(new CursorElement()
            {
                Name = "RESET PLAYER",
                Description = "Reset the progress of your player.",
                Action = () =>
                {
                    Confirm("RESET PLAYER", "Do you really want to RESET your player?",
                        () =>
                        {
                            Network.Manager.Send(new Network.Messages.Respec() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, Type = Perk.Type.PLAYER, Reset = true });
                            SwitchTo(RespecMenu, RespecCursorMenu, true);
                        },
                        () =>
                        {
                            SwitchTo(RespecMenu, RespecCursorMenu, true);
                        });
                }
            });
            if (NetworkManager.Singleton.IsServer)
            {
                respecElements.Add(new CursorElement()
                {
                    Name = "RESPEC SHIP",
                    Description = "Respec your ship.",
                    Action = () =>
                    {
                        if (!CanRespecShip)
                        {
                            var cursorMenu = new CursorMenu()
                            {
                                Elements = new List<ITextElement>()
                            {
                                new CursorElement() { Name = "BACK", Action = () => { SwitchTo(RespecMenu, RespecCursorMenu, true); } }
                            }
                            };
                            var menu = new BoxedScreenXP() { Title = "RESPEC SHIP", Content = new List<ITextElement>() { new TextElement() { Text = "You can only respec the ship at the company building." }, new TextElement() { Text = " " }, cursorMenu } };
                            SwitchTo(menu, cursorMenu, false);
                        }
                        else
                        {
                            Confirm("RESPEC SHIP", "Do you really want to respec your ship?",
                                () =>
                                {
                                    Network.Manager.Send(new Network.Messages.Respec() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, Type = Perk.Type.SHIP, Reset = false });
                                    SwitchTo(RespecMenu, RespecCursorMenu, true);
                                },
                                () =>
                                {
                                    SwitchTo(RespecMenu, RespecCursorMenu, true);
                                });
                        }
                    }
                });
                respecElements.Add(new CursorElement()
                {
                    Name = "RESET SHIP",
                    Description = "Reset the progress of your ship.",
                    Action = () =>
                    {
                        Confirm("RESET SHIP", "Do you really want to RESET your ship?",
                        () =>
                        {
                            Network.Manager.Send(new Network.Messages.Respec() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, Type = Perk.Type.SHIP, Reset = true });
                            SwitchTo(RespecMenu, RespecCursorMenu, true);
                        },
                        () =>
                        {
                            SwitchTo(RespecMenu, RespecCursorMenu, true);
                        });
                    }
                });
            }
            respecElements.Add(new CursorElement() { Name = "BACK", Description = null, Action = () => { SwitchTo(MainMenu, MainMenuCursorMenu, true); } });
            RespecCursorMenu = new CursorMenu()
            {
                Elements = respecElements
            };

            RespecMenu = new BoxedScreenXP() { Title = "RESPEC", Content = new List<ITextElement>() { new TextElement() { Text = "Please select an action:" }, new TextElement() { Text = " " }, RespecCursorMenu } };

        }

        public void Confirm(string title, string text, Action confirmAction, Action declineAction)
        {
            var cursorMenu = new CursorMenu()
            {
                Elements = new List<ITextElement>()
                {
                    new CursorElement() { Name = "CONFIRM", Action = () => { confirmAction(); } },
                    new CursorElement() { Name = "DECLINE", Action = () => { declineAction(); } },
                }
            };

            var menu = new BoxedScreenXP() { Title = title, Content = new List<ITextElement>() { new TextElement() { Text = text }, new TextElement() { Text = " " }, cursorMenu } };
            SwitchTo(menu, cursorMenu, false);
        }

        public void BuyPerk(Perk perk, Action backAction)
        {
            var cost = 0;
            var title = "";
            var maxLevel = false;
            var canAfford = false;
            var level = 0;
            if (perk.PerkType == Perk.Type.PLAYER)
            {
                title = "PLAYER PERKS";
                var player = Network.Manager.Lobby.Player();
                level = perk.GetLevel(player);
                if (level < perk.Levels)
                {
                    level++;
                    cost = perk.GetNextPrice(player);
                    canAfford = player.RemainingXP >= cost;
                }
                else maxLevel = true;
            }
            if (perk.PerkType == Perk.Type.SHIP)
            {
                title = "SHIP PERKS";
                var ship = Network.Manager.Lobby.CurrentShip;
                level = perk.GetLevel(ship);
                if (level < perk.Levels)
                {
                    level++;
                    cost = perk.GetNextPrice(ship);
                    canAfford = ship.RemainingXP >= cost;
                }
                else maxLevel = true;
            }

            if (maxLevel)
            {
                var cursorMenu = new CursorMenu()
                {
                    Elements = new List<ITextElement>()
                    {
                        new CursorElement() { Name = "BACK", Action = () => { backAction(); } }
                    }
                };
                var menu = new BoxedScreenXP() { Title = title, Content = new List<ITextElement>() { new TextElement() { Text = perk.DisplayDescription }, new TextElement() { Text = " " }, new TextElement() { Text = "This perk is already maxed out!" }, new TextElement() { Text = " " }, cursorMenu } };
                SwitchTo(menu, cursorMenu, false);
            }
            else if (!canAfford)
            {
                var cursorMenu = new CursorMenu()
                {
                    Elements = new List<ITextElement>()
                    {
                        new CursorElement() { Name = "BACK", Action = () => { backAction(); } }
                    }
                };
                var menu = new BoxedScreenXP() { Title = title, Content = new List<ITextElement>() { new TextElement() { Text = perk.DisplayDescription }, new TextElement() { Text = " " }, new TextElement() { Text = "You can't afford this perk. You need " + cost + "XP!" }, new TextElement() { Text = " " }, cursorMenu } };
                SwitchTo(menu, cursorMenu, false);
            }
            else
            {
                var cursorMenu = new CursorMenu()
                {
                    Elements = new List<ITextElement>()
                    {
                        new CursorElement() { Name = "CONFIRM", Action = () => {
                            Network.Manager.Send(new Network.Messages.ChangePerk() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, ID = perk.ID, Level = level});
                            backAction();
                        } },
                        new CursorElement() { Name = "DECLINE", Action = () => { backAction(); } }
                    }
                };
                var menu = new BoxedScreenXP() { Title = title, Content = new List<ITextElement>() { new TextElement() { Text = perk.DisplayDescription }, new TextElement() { Text = " " }, new TextElement() { Text = "Do you want to buy this perk for " + cost + "XP?" }, new TextElement() { Text = " " }, cursorMenu } };
                SwitchTo(menu, cursorMenu, false);
            }
        }

        public void SwitchTo(BoxedScreenXP box, CursorMenu menu, bool back = true)
        {
            this.CurrentScreen = box;
            this.CurrentCursorMenu = menu;
            if (!back)
                menu.SelectedElement = 0;
        }

        public void Main(MobileTerminal terminal, string[] args)
        {
            Init();
            Terminal = terminal;
            Terminal.DeactivateInput();
            SwitchTo(MainMenu, MainMenuCursorMenu, false);
        }

        public void Exit()
        {
        }

        public void Update()
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

        public void Submit(string text)
        {
        }
    }
}
