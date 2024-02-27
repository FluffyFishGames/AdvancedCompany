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
    internal class Player : BaseUpgradeable
    {
        internal MobileTerminal MobileTerminal;
        private static Dictionary<int, Player> Players = new();
        private static Mesh LOD1Mesh;
        private static Mesh LOD2Mesh;
        private static Mesh LOD3Mesh;
        private static Mesh WithoutFeetLOD1Mesh;
        private static Mesh WithoutFeetLOD2Mesh;
        private static Mesh WithoutFeetLOD3Mesh;
        private static GameObject HeadMountPrefab;
        internal List<GameObject> EquipmentItems;
        internal GameObject[] EquipmentItemsHead = new GameObject[0];
        internal GameObject[] EquipmentItemsBody = new GameObject[0];
        internal GameObject[] EquipmentItemsFeet = new GameObject[0];
        private SkinnedMeshRenderer LOD1;
        private SkinnedMeshRenderer LOD2;
        private SkinnedMeshRenderer LOD3;
        public GameObject HeadMount;
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
        public IHelmet Helmet;
        public Body Body;
        public Boots Boots;
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
        [Flags]
        public enum BodyLayers : int
        {
            NONE = 0,
            HIDE_FEET = 1
        };
        public enum Bone
        {
            METARIG,
            L_THIGH,
            L_SHIN,
            L_FOOT,
            L_HEEL,
            L_TOE,
            R_THIGH,
            R_SHIN,
            R_FOOT,
            R_HEEL,
            R_TOE,
            ROOT,
            SPINE_0,
            SPINE_1,
            SPINE_2,
            SPINE_3,
            L_SHOULDER,
            L_UPPER_ARM,
            L_LOWER_ARM,
            L_HAND,
            L_FINGER1,
            L_FINGER1_END,
            L_FINGER2,
            L_FINGER2_END,
            L_FINGER3,
            L_FINGER3_END,
            L_FINGER4,
            L_FINGER4_END,
            L_FINGER5,
            L_FINGER5_END,
            R_SHOULDER,
            R_UPPER_ARM,
            R_LOWER_ARM,
            R_HAND,
            R_FINGER1,
            R_FINGER1_END,
            R_FINGER2,
            R_FINGER2_END,
            R_FINGER3,
            R_FINGER3_END,
            R_FINGER4,
            R_FINGER4_END,
            R_FINGER5,
            R_FINGER5_END,
        };
        public enum EgoBone
        {
            L_SHOULDER,
            L_UPPER_ARM,
            L_LOWER_ARM,
            L_HAND,
            L_FINGER1,
            L_FINGER1_END,
            L_FINGER2,
            L_FINGER2_END,
            L_FINGER3,
            L_FINGER3_END,
            L_FINGER4,
            L_FINGER4_END,
            L_FINGER5,
            L_FINGER5_END,
            R_SHOULDER,
            R_UPPER_ARM,
            R_LOWER_ARM,
            R_HAND,
            R_FINGER1,
            R_FINGER1_END,
            R_FINGER2,
            R_FINGER2_END,
            R_FINGER3,
            R_FINGER3_END,
            R_FINGER4,
            R_FINGER4_END,
            R_FINGER5,
            R_FINGER5_END,
        };

        private Dictionary<EgoBone, Transform> EgoBones = new();
        internal static Dictionary<string, EgoBone> EgoBoneNames = new Dictionary<string, EgoBone>()
        {
            { "shoulder.L", EgoBone.L_SHOULDER },
            { "arm.L_upper", EgoBone.L_UPPER_ARM },
            { "arm.L_lower", EgoBone.L_LOWER_ARM },
            { "hand.L", EgoBone.L_HAND },
            { "finger1.L", EgoBone.L_FINGER1 },
            { "finger1.L.001", EgoBone.L_FINGER1_END },
            { "finger2.L", EgoBone.L_FINGER2 },
            { "finger2.L.001", EgoBone.L_FINGER2_END },
            { "finger3.L", EgoBone.L_FINGER3 },
            { "finger3.L.001", EgoBone.L_FINGER3_END },
            { "finger4.L", EgoBone.L_FINGER4 },
            { "finger4.L.001", EgoBone.L_FINGER4_END },
            { "finger5.L", EgoBone.L_FINGER5 },
            { "finger5.L.001", EgoBone.L_FINGER5_END },
            { "shoulder.R", EgoBone.R_SHOULDER },
            { "arm.R_upper", EgoBone.R_UPPER_ARM },
            { "arm.R_lower", EgoBone.R_LOWER_ARM },
            { "hand.R", EgoBone.R_HAND },
            { "finger1.R", EgoBone.R_FINGER1 },
            { "finger1.R.001", EgoBone.R_FINGER1_END },
            { "finger2.R", EgoBone.R_FINGER2 },
            { "finger2.R.001", EgoBone.R_FINGER2_END },
            { "finger3.R", EgoBone.R_FINGER3 },
            { "finger3.R.001", EgoBone.R_FINGER3_END },
            { "finger4.R", EgoBone.R_FINGER4 },
            { "finger4.R.001", EgoBone.R_FINGER4_END },
            { "finger5.R", EgoBone.R_FINGER5 },
            { "finger5.R.001", EgoBone.R_FINGER5_END }
        };

        private Dictionary<Bone, Transform> Bones = new();
        internal static Dictionary<string, Bone> BoneNames = new Dictionary<string, Bone>()
        {
            { "metarig", Bone.METARIG },
            { "spine", Bone.ROOT },
            { "spine.001", Bone.SPINE_0 },
            { "spine.002", Bone.SPINE_1 },
            { "spine.003", Bone.SPINE_2 },
            { "spine.004", Bone.SPINE_3 },
            { "shoulder.L", Bone.L_SHOULDER },
            { "arm.L_upper", Bone.L_UPPER_ARM },
            { "arm.L_lower", Bone.L_LOWER_ARM },
            { "hand.L", Bone.L_HAND },
            { "finger1.L", Bone.L_FINGER1 },
            { "finger1.L.001", Bone.L_FINGER1_END },
            { "finger2.L", Bone.L_FINGER2 },
            { "finger2.L.001", Bone.L_FINGER2_END },
            { "finger3.L", Bone.L_FINGER3 },
            { "finger3.L.001", Bone.L_FINGER3_END },
            { "finger4.L", Bone.L_FINGER4 },
            { "finger4.L.001", Bone.L_FINGER4_END },
            { "finger5.L", Bone.L_FINGER5 },
            { "finger5.L.001", Bone.L_FINGER5_END },
            { "shoulder.R", Bone.R_SHOULDER },
            { "arm.R_upper", Bone.R_UPPER_ARM },
            { "arm.R_lower", Bone.R_LOWER_ARM },
            { "hand.R", Bone.R_HAND },
            { "finger1.R", Bone.R_FINGER1 },
            { "finger1.R.001", Bone.R_FINGER1_END },
            { "finger2.R", Bone.R_FINGER2 },
            { "finger2.R.001", Bone.R_FINGER2_END },
            { "finger3.R", Bone.R_FINGER3 },
            { "finger3.R.001", Bone.R_FINGER3_END },
            { "finger4.R", Bone.R_FINGER4 },
            { "finger4.R.001", Bone.R_FINGER4_END },
            { "finger5.R", Bone.R_FINGER5 },
            { "finger5.R.001", Bone.R_FINGER5_END },
            { "thigh.L", Bone.L_THIGH },
            { "shin.L", Bone.L_SHIN },
            { "foot.L", Bone.L_FOOT },
            { "heel.02.L", Bone.L_HEEL },
            { "toe.L", Bone.L_TOE },
            { "thigh.R", Bone.R_THIGH },
            { "shin.R", Bone.R_SHIN },
            { "foot.R", Bone.R_FOOT },
            { "heel.02.R", Bone.R_HEEL },
            { "toe.R", Bone.R_TOE },
        };

        #region Proxy
        public bool IsLocal { get { return Controller != null && GameNetworkManager.Instance != null && Controller == GameNetworkManager.Instance.localPlayerController; } }
        public bool IsJumping { get { if (Controller == null) return false; return Controller.isJumping; } set { if (Controller != null) Controller.isJumping = value; } }
        public bool IsPlayerSliding { get { if (Controller == null) return false; return Controller.isPlayerSliding; } set { if (Controller != null) Controller.isPlayerSliding = value; } }
        public bool IsOwner { get { if (Controller == null) return false; return Controller.IsOwner; } }
        public Coroutine JumpCoroutine { get { if (Controller == null) return null; return Controller.jumpCoroutine; } set { if (Controller != null) Controller.jumpCoroutine = value; } }
        public IEnumerator PlayerJump() { if (Controller == null) return null; return Controller.PlayerJump(); }
        public float PlayerSlidingTimer { get { if (Controller == null) return 0f; return Controller.playerSlidingTimer; } set { if (Controller != null) Controller.playerSlidingTimer = value; } }
        public bool IsPlayerControlled { get { if (Controller == null) return false; return Controller.isPlayerControlled; } }
        public bool IsServer { get { if (Controller == null) return false; return Controller.IsServer; } }
        public bool IsHostPlayerObject { get { if (Controller == null) return false; return Controller.isHostPlayerObject; } }
        public bool IsTestingPlayer { get { if (Controller == null) return false; return Controller.isTestingPlayer; } }
        public bool IsFallingFromJump { get { if (Controller == null) return false; return Controller.isFallingFromJump; } set { if (Controller != null) Controller.isFallingFromJump = value; } }
        public bool IsFallingNoJump { get { if (Controller == null) return false; return Controller.isFallingNoJump; } set { if (Controller != null) Controller.isFallingNoJump = value; } }
        public bool MovingForward { get { if (Controller == null) return false; return Controller.movingForward; } set { if (Controller != null) Controller.movingForward = value; } }
        public bool IsWalking { get { if (Controller == null) return false; return Controller.isWalking; } set { if (Controller != null) Controller.isWalking = value; } }
        public float SprintMultiplier { get { if (Controller == null) return 0f; return Controller.sprintMultiplier; } set { if (Controller != null) Controller.sprintMultiplier = value; } }
        public Vector3 WalkForce { get { if (Controller == null) return Vector3.zero; return Controller.walkForce; } set { if (Controller != null) Controller.walkForce = value; } }
        public QuickMenuManager QuickMenuManager { get { if (Controller == null) return null; return Controller.quickMenuManager; } }
        public bool InSpecialInteractAnimation { get { if (Controller == null) return false; return Controller.inSpecialInteractAnimation; } set { if (Controller != null) Controller.inSpecialInteractAnimation = value; } }
        public bool IsTypingChat { get { if (Controller == null) return false; return Controller.isTypingChat; } }
        public bool IsUnderwater { get { if (Controller == null) return false; return Controller.isUnderwater; } }
        public int IsMovementHindered { get { if (Controller == null) return 0; return Controller.isMovementHindered; } set { if (Controller != null) Controller.isMovementHindered = value; } }
        public int MovementHinderedPrev { get { if (Controller == null) return 0; return Controller.movementHinderedPrev; } set { if (Controller != null) Controller.movementHinderedPrev = value; } }
        public bool IsCrouching { get { if (Controller == null) return false; return Controller.isCrouching; } }
        public bool IsGrounded { get { if (Controller == null) return false; return Controller.thisController.isGrounded; } }
        public float FallValue { get { if (Controller == null) return 0f; return Controller.fallValue; } set { if (Controller != null) Controller.fallValue = value; } }
        public float FallValueUncapped { get { if (Controller == null) return 0f; return Controller.fallValueUncapped; } set { if (Controller != null) Controller.fallValueUncapped = value; } }
        public float JumpForce { get { if (Controller == null) return 0f; return Controller.jumpForce; } set { if (Controller != null) Controller.jumpForce = value; } }
        #endregion

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

        internal List<Light> CosmeticLights = new List<Light>();
        internal void ShowCosmeticLights()
        {
            foreach (var light in CosmeticLights)
                light.enabled = true;
        }

        internal void HideCosmeticLights()
        {
            foreach (var light in CosmeticLights)
                light.enabled = false;
        }

        public Transform GetBone(Bone bone)
        {
            if (Bones.ContainsKey(bone))
                return Bones[bone];
            return null;
        }

        public Transform GetBone(EgoBone bone)
        {
            if (EgoBones.ContainsKey(bone))
                return EgoBones[bone];
            return null;
        }

        public void BindSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer, Bone root, Bone[] bones)
        {
            skinnedMeshRenderer.rootBone = GetBone(root);
            var boneTransforms = new Transform[bones.Length];
            for (var i = 0; i < bones.Length; i++)
                boneTransforms[i] = GetBone(bones[i]);
            skinnedMeshRenderer.bones = boneTransforms;
            skinnedMeshRenderer.ResetBounds();
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

        protected void FindBones(Transform transform)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == "ScavengerModelArmsOnly")
                    continue;
                if (BoneNames.ContainsKey(child.name))
                    Bones.Add(BoneNames[child.name], child);

                FindBones(child);
            }
        }

        protected void FindEgoBones(Transform transform)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (EgoBoneNames.ContainsKey(child.name))
                    EgoBones.Add(EgoBoneNames[child.name], child);

                FindEgoBones(child);
            }
        }


        public static void Update()
        {
            foreach (var p in Players)
            {
                p.Value.UpdateInstance();
            }
        }

        public void UnequipAll()
        {
            if (Controller == null)
                return;
            if (EquipmentItemsBody != null)
            {
                for (var i = 0; i < EquipmentItemsBody.Length; i++)
                    GameObject.Destroy(EquipmentItemsBody[i]);
            }
            if (EquipmentItemsFeet != null)
            {
                for (var i = 0; i < EquipmentItemsFeet.Length; i++)
                    GameObject.Destroy(EquipmentItemsFeet[i]);
            }
            if (EquipmentItemsHead != null)
            {
                for (var i = 0; i < EquipmentItemsHead.Length; i++)
                    GameObject.Destroy(EquipmentItemsHead[i]);
            }
            if (Helmet != null)
                Helmet.Unequipped(this);
            if (Body != null)
                Body.Unequipped(this);
            if (Boots != null)
                Boots.Unequipped(this);
            Helmet = null;
            Body = null;
            Boots = null;
            EquipmentItemsBody = new GameObject[0];
            EquipmentItemsFeet = new GameObject[0];
            EquipmentItemsHead = new GameObject[0];
            LOD1.sharedMesh = LOD1Mesh;
            LOD2.sharedMesh = LOD2Mesh;
            LOD3.sharedMesh = LOD3Mesh;
        }

        public void Reequip(bool head = true, bool body = true, bool feet = true)
        {
            if (Controller == null)
                return;

            var layers = BodyLayers.NONE;
            if (Helmet != null)
            {
                layers |= Helmet.GetLayers();
            }
            if (Body != null)
            {
                layers |= Body.GetLayers();
            }
            if (Boots != null)
            {
                layers |= Boots.GetLayers();
            }
            if (head)
                ReequipHead();
            if (body)
                ReequipBody();
            if (feet)
                ReequipFeet();

            if ((layers & BodyLayers.HIDE_FEET) == BodyLayers.HIDE_FEET)
            {
                LOD1.sharedMesh = WithoutFeetLOD1Mesh;
                LOD2.sharedMesh = WithoutFeetLOD2Mesh;
                LOD3.sharedMesh = WithoutFeetLOD3Mesh;
            }
            else
            {
                LOD1.sharedMesh = LOD1Mesh;
                LOD2.sharedMesh = LOD2Mesh;
                LOD3.sharedMesh = LOD3Mesh;
            }
        }

        public void ReequipFeet()
        {
            if (Controller == null)
                return;

            if (EquipmentItemsFeet != null)
            {
                for (var i = 0; i < EquipmentItemsFeet.Length; i++)
                    GameObject.Destroy(EquipmentItemsFeet[i]);
            }

            var equipmentItemsFeet = new List<GameObject>();
            if (Boots != null)
            {
                var objs = Boots.CreateWearable(this);
                equipmentItemsFeet.AddRange(objs);
                if (ClientConfiguration.Instance.Compability.HideEquipment)
                    HideRenderers(objs);
            }

            EquipmentItemsFeet = equipmentItemsFeet.ToArray();
            AdvancedCompany.Lib.Equipment.NewFeet(Controller, EquipmentItemsFeet);
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

        public void ReequipBody()
        {
            if (Controller == null)
                return;

            if (EquipmentItemsBody != null)
            {
                for (var i = 0; i < EquipmentItemsBody.Length; i++)
                    GameObject.Destroy(EquipmentItemsBody[i]);
            }

            var equipmentItemsBody = new List<GameObject>();
            if (Body != null)
            {
                var objs = Body.CreateWearable(this);
                equipmentItemsBody.AddRange(objs);
                if (ClientConfiguration.Instance.Compability.HideEquipment)
                    HideRenderers(objs);
            }
            EquipmentItemsBody = equipmentItemsBody.ToArray();
            AdvancedCompany.Lib.Equipment.NewBody(Controller, EquipmentItemsBody);
        }

        public void ReequipHead()
        {
            if (Controller == null)
                return;

            if (EquipmentItemsHead != null)
            {
                for (var i = 0; i < EquipmentItemsHead.Length; i++)
                    GameObject.Destroy(EquipmentItemsHead[i]);
            }

            var equipmentItemsHead = new List<GameObject>();
            if (Helmet != null)
            {
                var objs = Helmet.CreateWearable(this);
                equipmentItemsHead.AddRange(objs);
                if (ClientConfiguration.Instance.Compability.HideEquipment)
                    HideRenderers(objs);
            }
            EquipmentItemsHead = equipmentItemsHead.ToArray();
            AdvancedCompany.Lib.Equipment.NewHead(Controller, EquipmentItemsHead);
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

        public void EquipConfigurationCosmetics()
        {
            SetCosmetics(ClientConfiguration.Instance.Cosmetics.ActivatedCosmetics.ToArray(), false);
        }

        private bool CosmeticsHidden = false;
        public void ShowCosmetics()
        {
            CosmeticsHidden = false;
            foreach (var cosmetic in AppliedCosmetics)
                cosmetic.Value.SetActive(true);
        }

        public void HideCosmetics()
        {
            CosmeticsHidden = true;
            foreach (var cosmetic in AppliedCosmetics)
                cosmetic.Value.SetActive(false);
        }

        public string[] Cosmetics;
        public Dictionary<string, GameObject> AppliedCosmetics = new Dictionary<string, GameObject>();
        public void SetCosmetics(string[] cosmetics, bool sendMessage = true)
        {
            if (ServerConfiguration.Instance.General.EnableCosmetics)
            {
                if (cosmetics == null)
                    cosmetics = new string[0];
                Cosmetics = cosmetics;

                if (Controller != null)
                {
                    if (Lobby.LocalPlayerNum == (int)Controller.playerClientId && sendMessage)
                    {
                        Network.Manager.Send(new CosmeticsSync() { PlayerNum = PlayerNum, Cosmetics = cosmetics });
                    }

                    if (!ClientConfiguration.Instance.Compability.HideCosmetics)
                    {
                        Plugin.Log.LogMessage("Applying cosmetics for " + PlayerNum);
                        Plugin.Log.LogMessage(string.Join(" ", Cosmetics));
                        var remove = new List<string>();
                        var add = new List<string>();
                        foreach (var kv in AppliedCosmetics)
                        {
                            if (!cosmetics.Contains(kv.Key))
                                remove.Add(kv.Key);
                        }
                        foreach (var k in Cosmetics)
                        {
                            if (!AppliedCosmetics.ContainsKey(k))
                                add.Add(k);
                        }
                        for (var i = 0; i < remove.Count; i++)
                        {
                            GameObject.Destroy(AppliedCosmetics[remove[i]]);
                            AppliedCosmetics.Remove(remove[i]);
                        }
                        for (var i = 0; i < add.Count; i++)
                        {
                            AddCosmetic(add[i]);
                        }

                        CosmeticLights.Clear();
                        foreach (var kv in AppliedCosmetics)
                        {
                            var lights = kv.Value.GetComponentsInChildren<Light>();
                            CosmeticLights.AddRange(lights);
                        }
                    }
                }
            }
            else Cosmetics = new string[0];
        }

        private void AddCosmetic(string id)
        {
            if (CosmeticDatabase.AllCosmetics.ContainsKey(id))
            {
                var cosmetic = GameObject.Instantiate(CosmeticDatabase.AllCosmetics[id].gameObject);
                var instance = cosmetic.GetComponent<AdvancedCompany.Cosmetics.CosmeticInstance>();

                Transform bone = null;
                switch (instance.cosmeticType)
                {
                    case AdvancedCompany.Cosmetics.CosmeticType.HAT:
                        bone = GetBone(Bone.SPINE_3);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.CHEST:
                        bone = GetBone(Bone.SPINE_2);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.HIP:
                        bone = GetBone(Bone.ROOT);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_LOWER_ARM:
                        bone = GetBone(Bone.R_LOWER_ARM);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.L_SHIN:
                        bone = GetBone(Bone.L_SHIN);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_SHIN:
                        bone = GetBone(Bone.R_SHIN);
                        break;
                }

                cosmetic.transform.position = bone.position;
                cosmetic.transform.rotation = bone.rotation;
                cosmetic.transform.localScale *= 0.38f;
                cosmetic.transform.parent = bone;
                if ((int) Controller.playerClientId == Lobby.LocalPlayerNum)
                {
                    var renderer = cosmetic.gameObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                        renderer.gameObject.layer = 23;
                    var transforms = cosmetic.gameObject.GetComponentsInChildren<Transform>();
                    for (var i = 0; i < transforms.Length; i++)
                        transforms[i].gameObject.layer = 23;
                }
                AppliedCosmetics.Add(id, cosmetic);
                if (CosmeticsHidden)
                    cosmetic.SetActive(false);
            }
            else
            {
                Plugin.Log.LogWarning("Player has unsupported cosmetic: " + id);
            }
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

        internal Dictionary<string, AnimationClip> OriginalClips = new Dictionary<string, AnimationClip>();
        internal static AnimationClip FindAnimation(string animName)
        {
            foreach (var clip in AnimationPatches.PlayerAnimator.animationClips)
            {
                if (clip.name == animName)
                {
                    return clip;
                }
            }
            if (Lib.Player.Animations.ContainsKey(animName))
                return Lib.Player.Animations[animName];
            return null;
        }

        public void AddOverride(string originalName, string replacementName, bool syncOverride = false)
        {
            var animation = FindAnimation(replacementName);
            if (animation != null)
            {
                AddOverride(originalName, animation);
                if (syncOverride)
                    Network.Manager.Send(new SyncAnimationOverride() { OriginalName = originalName, ReplacementName = replacementName, PlayerNum = PlayerNum });
            }
            else Plugin.Log.LogMessage("Can't find animation " + replacementName);
        }

        public void AddOverride(string originalName, AnimationClip clip)
        {
            if (this.Controller.playerBodyAnimator.runtimeAnimatorController is AnimatorOverrideController @override)
            {
                if (!OriginalClips.ContainsKey(originalName))
                    OriginalClips.Add(originalName, @override[originalName]);
                Plugin.Log.LogMessage("Overriding clip " + originalName + " with " + clip.name);
                @override[originalName] = clip;
            }
            else
            {
                Plugin.Log.LogWarning("Animation controller is not of type AnimatorOverrideController!");
            }
        }

        public void RemoveOverride(string originalName, bool syncOverride = false)
        {
            if (this.Controller.playerBodyAnimator.runtimeAnimatorController is AnimatorOverrideController @override)
            {
                if (OriginalClips.ContainsKey(originalName))
                {
                    Plugin.Log.LogMessage("Restoring animation " + originalName);
                    @override[originalName] = OriginalClips[originalName];
                    if (syncOverride)
                        Network.Manager.Send(new SyncAnimationOverride() { OriginalName = originalName, ReplacementName = "", PlayerNum = PlayerNum });
                }
                else Plugin.Log.LogWarning("Original clip for " + originalName + " was missing.");
            }
            else
            {
                Plugin.Log.LogWarning("Animation controller is not of type AnimatorOverrideController!");
            }
        }

        internal void NetworkOverride (string originalName, string replacementName)
        {
            if (this.Controller.playerBodyAnimator.runtimeAnimatorController is AnimatorOverrideController @override)
            {
                if (replacementName == "")
                {
                    if (OriginalClips.ContainsKey(originalName))
                    {
                        Plugin.Log.LogMessage("Restoring animation " + originalName);
                        @override[originalName] = OriginalClips[originalName];
                    }
                    else Plugin.Log.LogWarning("Original clip for " + originalName + " was missing.");
                }
                else
                {
                    var animation = FindAnimation(replacementName);
                    if (animation != null)
                    {
                        if (!OriginalClips.ContainsKey(originalName))
                            OriginalClips.Add(originalName, @override[originalName]);
                        Plugin.Log.LogMessage("Overriding clip " + originalName + " with " + animation.name);
                        @override[originalName] = animation;
                    }
                }
            }
            else
            {
                Plugin.Log.LogWarning("Animation controller is not of type AnimatorOverrideController!");
            }
        }
    }
}
