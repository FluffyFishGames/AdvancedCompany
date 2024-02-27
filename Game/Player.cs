using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using AdvancedCompany.Objects;
using AdvancedCompany.Objects.Interfaces;
using System.Collections;
using AdvancedCompany.Config;
using AdvancedCompany.Network;
using System.IO;
using AdvancedCompany.Network.Messages;
using System.Linq;
using HarmonyLib;
using static Mono.Security.X509.X520;
using TMPro;
using AdvancedCompany.Patches;
using Newtonsoft.Json.Linq;
using UnityEngine.InputSystem.XR;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine.Rendering;
using AdvancedCompany.Lib;

namespace AdvancedCompany.Game
{
    [LoadAssets]
    [HarmonyPatch]
    [Boot.Bootable]
    internal partial class Player : BaseUpgradeable
    {
        internal MobileTerminal MobileTerminal;
        private static Dictionary<int, Player> Players = new();
        public TransformAndName RadarTarget;

        public ulong SteamID;
        public GameNetcodeStuff.PlayerControllerB Controller;
        public bool JoinedLate;
        private ulong _ClientID;
        public ulong ClientID
        {
            get
            {
                return _ClientID;
            }
            set
            {
                _ClientID = value;
            }
        }
        private int _PlayerNum;
        public int PlayerNum
        {
            get
            {
                return _PlayerNum;
            }
            set
            {
                _PlayerNum = value;
            }
        }
        private string _Username = "";
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value;
            }
        }
        public bool Initialized = false;
        public bool LockInventory;

        public static void Reset()
        {
            Players = new();
        }
        public enum SpeedType
        {
            Normal = 0,
            SprintCrouching = 1,
            Crouching = 2,
            Falling = 3
        };

        public static void Boot()
        {
            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
            Network.Manager.AddListener<CosmeticsSync>((msg) =>
            {
                if ((int) GameNetworkManager.Instance.localPlayerController.playerClientId != msg.PlayerNum)
                {
                    var player = GetPlayer(msg.PlayerNum);
                    if (player != null)
                        player.SetCosmetics(msg.Cosmetics, false);
                }
            });

            Network.Manager.AddListener<SyncAnimationOverride>((msg) =>
            {
                if ((int) GameNetworkManager.Instance.localPlayerController.playerClientId != msg.PlayerNum)
                {
                    var player = GetPlayer(msg.PlayerNum);
                    if (player != null)
                    {
                        player.NetworkOverride(msg.OriginalName, msg.ReplacementName);
                    }
                }
            });
        }

        private static void RenderPipelineManager_endCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            /*if (StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null)
            {
                var player = GetPlayer(StartOfRound.Instance.localPlayerController);
                if (player.Controller != null && arg2 == player.Controller.gameplayCamera)
                    player.ShowCosmeticLights();
            }*/
        }

        private static void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            if (StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null)
            {
                var player = GetPlayer(StartOfRound.Instance.localPlayerController);
                player.ShowCosmeticLights();
                if (player.Controller != null && arg2 == player.Controller.gameplayCamera)
                    player.HideCosmeticLights();
            }
        }

        public static void LoadAssets(AssetBundle assets)
        {
            WithoutFeetLOD1Mesh = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/WFLOD1.prefab").GetComponent<SkinnedMeshRenderer>().sharedMesh;
            WithoutFeetLOD2Mesh = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/WFLOD2.prefab").GetComponent<SkinnedMeshRenderer>().sharedMesh;
            WithoutFeetLOD3Mesh = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/WFLOD3.prefab").GetComponent<SkinnedMeshRenderer>().sharedMesh;
            HeadMountPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/HeadMount.prefab");
        }

        public static Player GetPlayer(int playerNum)
        {
            if (Players.TryGetValue(playerNum, out var p))
                return p;
            else
            {
                Players.Add(playerNum, new Player(playerNum));
                return Players[playerNum];
            }
        }

        public static Player GetPlayer(GameNetcodeStuff.PlayerControllerB controller)
        {
            var id = (int)controller.playerClientId;
            if (!Players.ContainsKey(id))
                Players.Add(id, new Player(controller));
            if (Players[id].Controller == null)
                Players[id].SetController(controller);
            return Players[id];
        }

        internal Player()
        {
        }

        protected Player(int clientID)
        {
            PlayerNum = clientID;
        }

        protected Player(GameNetcodeStuff.PlayerControllerB controller)
        {
            SetController(controller);
        }

        protected void SetController(GameNetcodeStuff.PlayerControllerB controller)
        {
            if (controller == null)
                return;
            PlayerNum = (int) controller.playerClientId;
            Controller = controller;
            var scavengerModel = Controller.transform.Find("ScavengerModel");
            if (scavengerModel == null)
                return;
            LOD1 = scavengerModel.Find("LOD1").GetComponent<SkinnedMeshRenderer>();
            LOD2 = scavengerModel.Find("LOD2").GetComponent<SkinnedMeshRenderer>();
            LOD3 = scavengerModel.Find("LOD3").GetComponent<SkinnedMeshRenderer>();
            if (LOD1Mesh == null) LOD1Mesh = LOD1.sharedMesh;
            if (LOD2Mesh == null) LOD2Mesh = LOD2.sharedMesh;
            if (LOD3Mesh == null) LOD3Mesh = LOD3.sharedMesh;
            var metarig = scavengerModel.Find("metarig");
            Bones.Add(Bone.METARIG, metarig);
            FindBones(metarig);
            var scavengerModelArmsOnly = metarig.Find("ScavengerModelArmsOnly");
            FindEgoBones(scavengerModelArmsOnly);
            SetCosmetics(Cosmetics, false);
            if (MobileTerminal == null)
                MobileTerminal = new MobileTerminal(this);
        }

        public static void Update()
        {
            foreach (var p in Players)
            {
                p.Value.UpdateInstance();
            }
        }

        private void HideRenderers(GameObject[] objs)
        {
            foreach (var obj in objs)
            {
                var m = obj.GetComponent<MeshRenderer>();
                if (m != null) m.enabled = false;
                var s = obj.GetComponent<SkinnedMeshRenderer>();
                if (s != null) s.enabled = false;

                var mm = obj.GetComponentsInChildren<MeshRenderer>();
                var ss = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var mmm in mm)
                    mmm.enabled = false;
                foreach (var sss in ss)
                    sss.enabled = false;
            }
        }

        public void UpdateInstance()
        {
            if (Controller == null)
            {
                if (global::StartOfRound.Instance != null)
                {
                    for (var i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
                    {
                        var controller = global::StartOfRound.Instance.allPlayerScripts[i];
                        if ((int) controller.playerClientId == PlayerNum)
                        {
                            if (i != (int) PlayerNum)
                                Plugin.Log.LogWarning("Wrong player script " + i + " had playerClientId " + PlayerNum + ". This might result in desyncs. Some other mod might have caused this.");
                            SetController(controller);
                        }
                    }
                }
            }
            if (Controller != null)
            {
                if (Controller.playerUsername != Username)
                {
                    if (SteamID > 0)
                    {
                        QuickMenuManager.playerListSlots[Controller.playerClientId].playerSteamId = SteamID;
                    }
                    QuickMenuManager.playerListSlots[Controller.playerClientId].usernameHeader.text = Username;
                    Controller.playerUsername = Username;
                    Controller.usernameBillboardText.text = Username;
                    RadarTarget.name = Username ?? "";
                }
                
                if (Controller.isPlayerControlled || Controller.isPlayerDead)
                {
                    if (GameNetworkManager.Instance.localPlayerController != null)
                    {
                        if (!ServerConfiguration.Instance.General.DeactivateHotbar)
                        {
                            if (HeadMount == null)
                            {
                                HeadMount = GameObject.Instantiate(HeadMountPrefab, GetBone(Bone.SPINE_3));
                                HeadMount.transform.localRotation = Quaternion.Euler(new Vector3(88.47f, 0f, 0f));
                                HeadMount.transform.localPosition = new Vector3(0f, 0.3336f, 0.1184f);
                                HeadMount.transform.localScale = new Vector3(0.8915f, 0.8915f, 0.8915f);
                                if (Controller == GameNetworkManager.Instance.localPlayerController)
                                {
                                    var renderer = HeadMount.GetComponent<Renderer>();
                                    renderer.gameObject.layer = 23;
                                }
                                if (ClientConfiguration.Instance.Compability.HideEquipment)
                                    HeadMount.GetComponent<MeshRenderer>().enabled = false;
                            }
                            if (HeadMount != null && InventoryPatches.HeadSlotAvailable != HeadMount.activeSelf)
                                HeadMount.SetActive(InventoryPatches.HeadSlotAvailable);
                            var head = Controller.ItemSlots[10];
                            var body = Controller.ItemSlots[11];
                            var boots = Controller.ItemSlots[12];
                            bool reequipHead = false;
                            bool reequipBody = false;
                            bool reequipFeet = false;
                            if (head == null && Helmet != null)
                            {
                                Helmet.Unequipped(this);
                                Helmet = null;
                                reequipHead = true;
                            }
                            else if (head != null && head is Objects.IHelmet h && Helmet != h)
                            {
                                if (Helmet != null)
                                    Helmet.Unequipped(this);
                                Helmet = h;
                                reequipHead = true;
                                Helmet.Equipped(this);
                            }

                            if (body == null && Body != null)
                            {
                                Body.Unequipped(this);
                                Body = null;
                                reequipBody = true;
                            }
                            else if (body != null && body is Objects.Body bo && Body != bo)
                            {
                                if (Body != null)
                                    Body.Unequipped(this);
                                Body = bo;
                                reequipBody = true;
                                Body.Equipped(this);
                            }

                            if (boots == null && Boots != null)
                            {
                                Boots.Unequipped(this);
                                Boots = null;
                                reequipFeet = true;
                            }
                            else if (boots != null && boots is Objects.Boots b && Boots != b)
                            {
                                if (Boots != null)
                                    Boots.Unequipped(this);
                                Boots = b;
                                reequipFeet = true;
                                Boots.Equipped(this);
                            }

                            if (reequipHead || reequipBody || reequipFeet)
                            {
                                this.Reequip(reequipHead, reequipBody, reequipFeet);
                            }
                        }
                        if (IsLocal)
                        {
                            MultiplierOverrides.Clear();
                            if (Boots is ICursed cursedBoots)
                                cursedBoots.UpdateEffects(this);
                            if (Helmet is ICursed cursedHead)
                                cursedHead.UpdateEffects(this);
                            if (Body is ICursed cursedBody)
                                cursedBody.UpdateEffects(this);
                            if (Controller.currentlyHeldObjectServer is ICursed cursedItem)
                                cursedItem.UpdateEffects(this);

                            var usingComms = false;
                            if (Helmet is IEquipmentCommunication commsEquipments1)
                                usingComms = true;
                            if (Boots is IEquipmentCommunication commsEquipments2)
                                usingComms = true;
                            if (Body is IEquipmentCommunication commsEquipments3)
                                usingComms = true;

                            if (usingComms)
                                Controller.holdingWalkieTalkie = true;
                        }
                    }
                }
            }
        }

        public void SetMultiplierOverride(string perk, float val)
        {
            MultiplierOverrides[perk] = val;
        }

        internal Dictionary<string, float> MultiplierOverrides = new Dictionary<string, float>();

        protected void InstancePerformJump()
        {
            if (Boots is IPerformJump j1)
                j1.PerformJump(this);
            if (Helmet is IPerformJump j2)
                j2.PerformJump(this);
            if (Body is IPerformJump j3)
                j3.PerformJump(this);
            if (Controller.currentlyHeldObjectServer is IPerformJump j4)
                j4.PerformJump(this);
        }

        protected void InstanceOnUpdate()
        {
            if (Boots is IOnUpdate j1)
                j1.OnUpdate(this);
            if (Helmet is IOnUpdate j2)
                j2.OnUpdate(this);
            if (Body is IOnUpdate j3)
                j3.OnUpdate(this);
            if (Controller.currentlyHeldObjectServer is IOnUpdate j4)
                j4.OnUpdate(this);
        }

        protected bool InstanceMovement()
        {
            if (Boots is IMovement j1 && j1.Movement(this))
                return true;
            if (Helmet is IMovement j2 && j2.Movement(this))
                return true;
            if (Body is IMovement j3 && j3.Movement(this))
                return true;
            if (Controller.currentlyHeldObjectServer is IMovement j4 && j4.Movement(this))
                return true;
            return false;
        }

        public static void PerformJump(GameNetcodeStuff.PlayerControllerB player)
        {
            GetPlayer(player).InstancePerformJump();
        }

        public static void OnUpdate(GameNetcodeStuff.PlayerControllerB player)
        {
            GetPlayer(player).InstanceOnUpdate();
        }

        public static bool Movement(GameNetcodeStuff.PlayerControllerB player)
        {
            return GetPlayer(player).InstanceMovement();
        }

        public override void Reset(bool resetXP)
        {
            base.Reset(resetXP);
            if (resetXP)
                this.XP = ServerConfiguration.Instance.General.SaveProgress ? 500 : ServerConfiguration.Instance.General.StartingXP;
        }

        public void CopyFrom(Player player)
        {
            XP = player.XP;
            Levels = player.Levels;
            _Username = player._Username;
            SteamID = player.SteamID;
            JoinedLate = player.JoinedLate;
            ClientID = player.ClientID;
            Cosmetics = player.Cosmetics?.ToArray() ?? new string[] { };
        }

        public override void ReadData(FastBufferReader reader)
        {
            base.ReadData(reader);
            reader.ReadValueSafe(out SteamID);
            reader.ReadValueSafe(out _ClientID);
            reader.ReadValueSafe(out _PlayerNum);
            reader.ReadValueSafe(out JoinedLate);
            reader.ReadValueSafe(out _Username);
            reader.ReadValueSafe(out int cosmeticsLength);
            Cosmetics = new string[cosmeticsLength];
            for (var i = 0; i < cosmeticsLength; i++)
            {
                reader.ReadValueSafe(out string cosmetic, true);
                Cosmetics[i] = cosmetic;
            }
        }

        public override void WriteData(FastBufferWriter writer)
        {
            base.WriteData(writer);
            writer.WriteValueSafe(SteamID);
            writer.WriteValueSafe(_ClientID);
            writer.WriteValueSafe(_PlayerNum);
            writer.WriteValueSafe(JoinedLate);
            writer.WriteValueSafe(_Username);
            if (Cosmetics != null)
            {
                writer.WriteValueSafe(Cosmetics.Length);
                for (var i = 0; i < Cosmetics.Length; i++)
                    writer.WriteValueSafe(Cosmetics[i], true);
            }
            else writer.WriteValueSafe((int)0);
        }

        private string GetLocalSaveFile(bool backup = false)
        {
            if (ClientConfiguration.Instance.File.SaveInProfile)
            {
                var directory = Path.Combine(BepInEx.Paths.ConfigPath, "../saves/advancedcompany");
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                return Path.Combine(BepInEx.Paths.ConfigPath, "../saves/advancedcompany/savefile" + (backup ? ".bkp" : ""));
            }
            return Path.Combine(Path.GetFullPath(Environment.ExpandEnvironmentVariables("%appdata%/../LocalLow/ZeekerssRBLX/Lethal Company")), "AdvancedCompany" + (backup ? ".bkp" : ""));
        }
        private string NoPunctuation(string input)
        {
            return new string(input.Where((char c) => char.IsLetter(c)).ToArray());
        }

        public void LoadLocal()
        {
            var saveFile = GetLocalSaveFile();
            try
            {
                if (!GameNetworkManager.Instance.disableSteam)
                {
                    SteamID = Steamworks.SteamClient.SteamId.Value;
                    string username = NoPunctuation(new Friend(SteamID).Name);
                    username = Regex.Replace(username, "[^\\w\\._]", "");
                    Username = username;
                    Plugin.Log.LogMessage("Found steam ID: " + SteamID);
                    Plugin.Log.LogMessage("Found username: " + Username);
                }
                else
                {
                    SteamID = 0;
                    Username = "Player";
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while retrieving steam ID:");
                Plugin.Log.LogError(e);
                SteamID = 0;
            }
            try
            {
                Cosmetics = ClientConfiguration.Instance.Cosmetics.ActivatedCosmetics.ToArray();
                if (System.IO.File.Exists(saveFile))
                {
                    JObject data = JObject.Parse(System.IO.File.ReadAllText(saveFile));
                    data.TryGetInt("xp", out XP);
                    if (data.ContainsKey("perks") && data["perks"] is JObject perksData)
                    {
                        foreach (var kv in perksData)
                        {
                            if (int.TryParse(kv.Value.ToString(), out var l))
                                Levels.Add(kv.Key, l);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Error while loading savegame:");
                Plugin.Log.LogWarning(e);
                Plugin.Log.LogMessage("Trying to load backup...");

                saveFile = GetLocalSaveFile(true);
                try
                {
                    if (System.IO.File.Exists(saveFile))
                    {
                        JObject data = JObject.Parse(System.IO.File.ReadAllText(saveFile));
                        data.TryGetInt("xp", out XP);
                        if (data.ContainsKey("perks") && data["perks"] is JObject perksData)
                        {
                            foreach (var kv in perksData)
                            {
                                if (int.TryParse(kv.Value.ToString(), out var l))
                                    Levels.Add(kv.Key, l);
                            }
                        }
                    }
                    Plugin.Log.LogMessage("Rewrite savegame from backup...");
                    SaveLocal(false);
                }
                catch (Exception e2)
                {
                    Plugin.Log.LogError("Error while loading savegame backup:");
                    Plugin.Log.LogError(e2);
                }
            }
            if (XP < 500)
                XP = 500;

        }

        public void SaveLocal(bool saveBackup = true)
        {
            if (ServerConfiguration.Instance.General.SaveProgress)
            {
                var saveFile = GetLocalSaveFile();
                var backupPath = GetLocalSaveFile(true);

                if (saveBackup)
                {
                    try
                    {
                        if (System.IO.File.Exists(saveFile))
                            System.IO.File.Copy(saveFile, backupPath, true);
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogWarning("Error while creating backup:");
                        Plugin.Log.LogWarning(e);
                    }
                }

                Plugin.Log.LogInfo("Saving player data to client save file " + saveFile + ".");
                try
                {
                    JObject data = new JObject();
                    data["xp"] = XP;
                    var perks = new JObject();
                    foreach (var kv in Levels)
                        perks[kv.Key] = kv.Value;
                    data["perks"] = perks;
                    System.IO.File.WriteAllText(saveFile, data.ToString());
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning("Error while saving:");
                    Plugin.Log.LogWarning(e);
                }
            }
        }
        public void SaveServer(string saveFile)
        {
            if (!ServerConfiguration.Instance.General.SaveProgress)
            {
                var username = Controller.playerUsername;
                if (SteamID > 0)
                    username = SteamID + "";
                Plugin.Log.LogInfo("Saving player data for username " + username + " to server save file " + saveFile + ".");
                ES3.Save("PlayerXP_" + username, XP, saveFile);
                ES3.Save("PlayerPerks_" + username, Levels, saveFile);
            }
        }

        public void LoadServer(string saveFile)
        {
            if (!ServerConfiguration.Instance.General.SaveProgress)
            {
                var username = Controller == null ? "Player #0" : Controller.playerUsername;
                if (SteamID > 0)
                    username = SteamID + "";
                Plugin.Log.LogInfo("Loading player data for " + username + " from server save file " + saveFile + ".");
                if (Controller == null || ServerConfiguration.Instance.General.IndividualXP)
                {
                    if (ES3.KeyExists("PlayerXP_" + username, saveFile))
                        XP = ES3.Load<int>("PlayerXP_" + username, saveFile);
                    else XP = ServerConfiguration.Instance.General.StartingXP;
                }
                else
                {
                    XP = Network.Manager.Lobby.ConnectedPlayers[0].XP;
                }
                if (ES3.KeyExists("PlayerPerks_" + username, saveFile))
                    Levels = ES3.Load<Dictionary<string, int>>("PlayerPerks_" + username, saveFile);
                else Levels = new Dictionary<string, int>();
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        public static void Init(GameNetcodeStuff.PlayerControllerB __instance)
        {
            try
            {
                PostProcessing.VisionEnhancerEffect.Initialize();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}
