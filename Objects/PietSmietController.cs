using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Objects.Interfaces;
using AdvancedCompany.PostProcessing;
using AdvancedCompany.Unity.UI;
using UnityEngine.Rendering.HighDefinition;
using AdvancedCompany.Config;
using System.Collections;

namespace AdvancedCompany.Objects
{
    [Boot.Bootable]
    [HarmonyPatch]
    [LoadAssets]
    internal class PietSmietController : PhysicsProp, ICursed, IOnUpdate, IMovement
    {
        private float Phase = 0f;
        public bool CurseLifted = false;
        private bool CurseLiftedLastFrame = false;
        private GameNetcodeStuff.PlayerControllerB lastPlayerHeldBy;
        private static GameObject DoomHUDPrefab;
        private DoomHUD DoomHUD;
        private Light Light;

        private AudioSource ActiveAudio;
        private AudioSource InactiveAudio;

        private static DoomEffect Instance;
        private static AdvancedCompany.Lib.HDRP.PostProcessInstance PostProcessingInstance;
        internal static bool DoomRunning = false;
        private static List<EnemyAI> OutsideSpawnedEnemies = new List<EnemyAI>();
        private static List<EnemyAI> InsideSpawnedEnemies = new List<EnemyAI>();
        private static int CurrentInsideSpawnPos = 0;
        private static int CurrentOutsideSpawnPos = 0;
        private static List<EnemyType> SpawnInsideRotation = null;
        private static List<EnemyType> SpawnOutsideRotation = null;

        private static float SpawnTimer = 0f;

        public static void UpdateEffect()
        {
            Instance.intensity.value = DoomRunning ? 1f : 0f;
        }

        public static void Boot()
        {
            PostProcessingInstance = AdvancedCompany.Lib.HDRP.AddPostProcessing<DoomEffect>(Lib.HDRP.InjectionPoint.AFTER_POSTPROCESS);
            Instance = (DoomEffect)PostProcessingInstance.CurrentInstance;
            PostProcessingInstance.OnInstanceChanged += (old, instance) => {
                if (old != null && old is DoomEffect oldEffect)
                    oldEffect.intensity.value = 0f;
                Instance = (DoomEffect)instance;
                Instance.intensity.value = DoomRunning ? 1f : 0f;
            };
            Network.Manager.AddListener<DoomShotgun>((msg) => {
                var shotgunPosition = msg.ShotgunPosition;
                var shotgunForward = msg.ShotgunForward;

                var localPlayer = global::StartOfRound.Instance.localPlayerController;
                if ((int)localPlayer.playerClientId != msg.PlayerNum)
                {
                    Vector3 vector = localPlayer.playerCollider.ClosestPoint(msg.ShotgunPosition);
                    bool localPlayerHit = false;
                    if (!Physics.Linecast(shotgunPosition, vector, global::StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && Vector3.Angle(shotgunForward, vector - shotgunPosition) < 30f)
                        localPlayerHit = Vector3.Distance(vector, shotgunPosition) < 10f;

                    if (localPlayerHit)
                        localPlayer.KillPlayer((localPlayer.transform.position - msg.ShotgunPosition).normalized * 5f, true, CauseOfDeath.Gunshots);
                }

                var depth = 20f;
                
                if (NetworkManager.Singleton.IsServer)
                { 
                    Collider[] colliders = Physics.OverlapBox(msg.ShotgunPosition + msg.ShotgunForward * (depth / 2f), new Vector3(1.5f, 4f, depth / 2f), Quaternion.LookRotation(msg.ShotgunForward), -1, QueryTriggerInteraction.Collide);
                    foreach (var collider in colliders)
                    {
                        Vector3 vector = collider.ClosestPoint(msg.ShotgunPosition);
                        if (!Physics.Linecast(shotgunPosition, vector, global::StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                        {
                            var enemy = collider.GetComponent<EnemyAI>();
                            if (enemy == null)
                                enemy = collider.GetComponentInParent<EnemyAI>();

                            if (enemy != null && !enemy.isEnemyDead)
                            {
                                KillEnemy(enemy);
                            }
                        }
                    }
                }
            });
        }

        private static bool KillEnemy(EnemyAI enemy)
        {
            if (enemy == null)
                return true;
            if (enemy is SandWormAI)
                return false;
            else if (enemy is BaboonBirdAI || enemy is MaskedPlayerEnemy || enemy is NutcrackerEnemyAI || enemy is SandSpiderAI || enemy is CentipedeAI || enemy is CrawlerAI || enemy is FlowermanAI || enemy is HoarderBugAI || enemy is MouthDogAI)
            {
                enemy.__rpc_exec_stage = __RpcExecStage.Server;
                enemy.KillEnemyClientRpc(false);
            }
            else
            {
                enemy.CancelSpecialAnimationWithPlayer();
                if (!OutsideSpawnedEnemies.Contains(enemy) && !InsideSpawnedEnemies.Contains(enemy))
                    enemy.SubtractFromPowerLevel();
                enemy.KillEnemy(true);
            }
            return true;
        }

        void Awake()
        {
            InactiveAudio = transform.Find("InactiveAudio").GetComponent<AudioSource>();
            ActiveAudio = transform.Find("ActiveAudio").GetComponent<AudioSource>();
            if (!ClientConfiguration.Instance.Compability.DisableMusic)
                InactiveAudio.Play();
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!this.CurseLifted && !this.playerHeldBy.isInHangarShipRoom)
            {
                StartDoom();
            }
        }

        internal void StartDoom()
        {
            if (!DoomRunning)
            {
                DoomRunning = true;

                var player = Game.Player.GetPlayer(this.playerHeldBy);
                player.LockInventory = true;
                HUDManager.Instance.ToggleHUD(false);
                player.Controller.localArmsTransform.parent.parent.gameObject.SetActive(false);
                this.EnableItemMeshes(false);
                if (DoomHUD == null)
                {
                    var go = GameObject.Instantiate(DoomHUDPrefab, HUDManager.Instance.HUDAnimator.gameObject.transform.parent);
                    go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    DoomHUD = go.GetComponent<DoomHUD>();
                    if (Instance != null)
                        Instance.intensity.value = 1f;
                }

                var lightObj = new GameObject("DoomLight");
                lightObj.transform.parent = player.GetBone(Game.Player.Bone.METARIG);
                lightObj.transform.localPosition = new Vector3(0f, 0f, 1f);
                lightObj.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);

                Light = lightObj.AddComponent<Light>();
                Light.color = new Color(0.5f, 0.5f, 0.5f);
                Light.type = LightType.Spot;
                Light.shape = LightShape.Box;
                Light.cullingMask = int.MaxValue - 128;

                var light1Data = lightObj.AddComponent<HDAdditionalLightData>();
                light1Data.SetBoxSpotSize(new Vector2(500f, 500f));
                light1Data.SetIntensity(30f, LightUnit.Lux);
                light1Data.spotLightShape = SpotLightShape.Box;
                //light1Data.affectSpecular = false;
                light1Data.affectsVolumetric = true;
                light1Data.volumetricDimmer = 0f;
                light1Data.range = 1000f;
                light1Data.lightDimmer = 1f;
                light1Data.SetCullingMask(int.MaxValue - 128);
            }
        }

        internal void StopDoom()
        {
            if (DoomRunning)
            {
                var player = Game.Player.GetPlayer(this.lastPlayerHeldBy);

                if (Light != null)
                {
                    GameObject.Destroy(Light.gameObject);
                    Light = null;
                }
                if (DoomHUD != null)
                {
                    GameObject.Destroy(DoomHUD.gameObject);
                    DoomHUD = null;
                }
                player.LockInventory = false; 
                HUDManager.Instance.ToggleHUD(true);
                player.Controller.localArmsTransform.parent.parent.gameObject.SetActive(true);
                this.EnableItemMeshes(true);
                if (Instance != null)
                    Instance.intensity.value = 0f;
                DoomRunning = false;
            }
        }

        public override void DiscardItem()
        {
//            if (!CurseLifted)
//                ItemAudio.Play();
            base.DiscardItem();
            this.StopDoom();
        }
        IEnumerator StopAudio()
        {
            if (!ClientConfiguration.Instance.Compability.DisableMusic)
            {
                InactiveAudio.Stop();
                var startVolume = ActiveAudio.volume;
                while (ActiveAudio != null && ActiveAudio.pitch > 0)
                {
                    ActiveAudio.pitch -= Time.deltaTime / 3f;
                    ActiveAudio.volume = ActiveAudio.pitch * startVolume;
                    yield return new WaitForEndOfFrame();
                }
                ActiveAudio.Stop();
            }
            else yield return new WaitForEndOfFrame();
        }


        public override void Update()
        {
            base.Update();

            var active = playerHeldBy != null;

            if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
                CurseLifted = true;
            if (playerHeldBy == null && lastPlayerHeldBy != null && lastPlayerHeldBy == global::StartOfRound.Instance.localPlayerController)
                StopDoom();

            if (CurseLifted)
            {
                if (!CurseLiftedLastFrame)
                {
                    StartCoroutine(StopAudio());
                    if (NetworkManager.Singleton.IsServer)
                    {
                        foreach (var enemy in InsideSpawnedEnemies)
                            KillEnemy(enemy);
                        foreach (var enemy in OutsideSpawnedEnemies)
                            KillEnemy(enemy);
                    }
                    if (lastPlayerHeldBy == global::StartOfRound.Instance.localPlayerController)
                        StopDoom();
                }
            }
            else
            {
                if (InactiveAudio != null)
                    InactiveAudio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;
                if (ActiveAudio != null)
                    ActiveAudio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;

                if (active)
                {
                    if (InactiveAudio.isPlaying)
                        InactiveAudio.Stop();
                    if (!ActiveAudio.isPlaying)
                        ActiveAudio.Play();
                }
                else
                {
                    if (!InactiveAudio.isPlaying)
                        InactiveAudio.Play();
                    if (ActiveAudio.isPlaying)
                        ActiveAudio.Stop();
                }

                if (NetworkManager.Singleton.IsServer && playerHeldBy != null)
                {
                    if (SpawnInsideRotation == null)
                    {
                        SpawnInsideRotation = new List<EnemyType>()
                        {
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            Manager.Enemies.EnemiesByName["Hoarding bug"].EnemyType,
                            Manager.Enemies.EnemiesByName["Bunker Spider"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            Manager.Enemies.EnemiesByName["Hoarding bug"].EnemyType,
                            Manager.Enemies.EnemiesByName["Bunker Spider"].EnemyType,
                            //Manager.Enemies.EnemiesByName["Masked"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            //Manager.Enemies.EnemiesByName["Nutcracker"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            Manager.Enemies.EnemiesByName["Hoarding bug"].EnemyType,
                            Manager.Enemies.EnemiesByName["Bunker Spider"].EnemyType,
                            //Manager.Enemies.EnemiesByName["Masked"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                            Manager.Enemies.EnemiesByName["Hoarding bug"].EnemyType,
                            //Manager.Enemies.EnemiesByName["Nutcracker"].EnemyType,
                            //Manager.Enemies.EnemiesByName["Nutcracker"].EnemyType,
                            Manager.Enemies.EnemiesByName["Crawler"].EnemyType,
                        };
                        SpawnOutsideRotation = new List<EnemyType>()
                        {
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["Baboon hawk"].EnemyType,
                            Manager.Enemies.EnemiesByName["Baboon hawk"].EnemyType,
                            Manager.Enemies.EnemiesByName["Baboon hawk"].EnemyType,
                            Manager.Enemies.EnemiesByName["Baboon hawk"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                            Manager.Enemies.EnemiesByName["MouthDog"].EnemyType,
                        };
                    }
                    SpawnTimer -= Time.deltaTime;
                    if (SpawnTimer <= 0f)
                    {
                        for (var i = 0; i < InsideSpawnedEnemies.Count; i++)
                        {
                            if (InsideSpawnedEnemies[i] == null || InsideSpawnedEnemies[i].isEnemyDead)
                            {
                                InsideSpawnedEnemies.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }
                        for (var i = 0; i < OutsideSpawnedEnemies.Count; i++)
                        {
                            if (OutsideSpawnedEnemies[i] == null || OutsideSpawnedEnemies[i].isEnemyDead)
                            {
                                OutsideSpawnedEnemies.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }

                        if (OutsideSpawnedEnemies.Count < 15)
                        {
                            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

                            Vector3 position = spawnPoints[global::RoundManager.Instance.AnomalyRandom.Next(0, spawnPoints.Length)].transform.position;
                            position = global::RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 4f);

                            int num3 = 0;
                            bool flag = false;
                            for (int j = 0; j < spawnPoints.Length - 1; j++)
                            {
                                for (int k = 0; k < global::RoundManager.Instance.spawnDenialPoints.Length; k++)
                                {
                                    flag = true;
                                    if (Vector3.Distance(position, global::RoundManager.Instance.spawnDenialPoints[k].transform.position) < 16f)
                                    {
                                        num3 = (num3 + 1) % spawnPoints.Length;
                                        position = spawnPoints[num3].transform.position;
                                        position = global::RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 4f);
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    break;
                                }
                            }
                            GameObject obj = GameObject.Instantiate(SpawnOutsideRotation[CurrentOutsideSpawnPos].enemyPrefab, position, Quaternion.Euler(Vector3.zero));
                            obj.gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                            var enemyAi = obj.GetComponent<EnemyAI>();
                            if (enemyAi == null)
                                enemyAi = obj.GetComponentInChildren<EnemyAI>();
                            RoundManager.Instance.SpawnedEnemies.Add(enemyAi);
                            OutsideSpawnedEnemies.Add(enemyAi);
                            CurrentOutsideSpawnPos++;
                            if (CurrentOutsideSpawnPos >= SpawnOutsideRotation.Count)
                                CurrentOutsideSpawnPos = 0;
                        }
                        if (InsideSpawnedEnemies.Count < 15)
                        {
                            var vent = RoundManager.Instance.allEnemyVents[RoundManager.Instance.AnomalyRandom.Next(0, RoundManager.Instance.allEnemyVents.Length)];
                            var position = vent.floorNode.position;
                            GameObject gameObject = GameObject.Instantiate(SpawnInsideRotation[CurrentInsideSpawnPos].enemyPrefab, position, Quaternion.Euler(new Vector3(0f, 0, 0f)));
                            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                            InsideSpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                            CurrentInsideSpawnPos++;
                            if (CurrentInsideSpawnPos >= SpawnInsideRotation.Count)
                                CurrentInsideSpawnPos = 0;
                        }
                        SpawnTimer = 5f;
                    }
                }
                if (playerHeldBy != null)
                {
                    foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
                    {
                        if (enemy.isOutside == !playerHeldBy.isInsideFactory && enemy.IsOwner)
                        {
                            enemy.StopSearch(enemy.currentSearch);
                            enemy.movingTowardsTargetPlayer = true;
                            enemy.targetPlayer = playerHeldBy;
                        }
                    }
                }
            }
            CurseLiftedLastFrame = CurseLifted;
            lastPlayerHeldBy = playerHeldBy;
        }

        public static void LoadAssets(AssetBundle assets)
        {
            DoomHUDPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/DoomHUD.prefab");
        }

        [HarmonyPatch(typeof(ForestGiantAI), "IVisibleThreat.GetThreatVelocity")]
        [HarmonyPrefix]
        public static bool ForestGiantGetThreatVelocity(ForestGiantAI __instance)
        {
            if (__instance.agent == null)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(MouthDogAI), "IVisibleThreat.GetThreatVelocity")]
        [HarmonyPrefix]
        public static bool MouthDogGetThreatVelocity(MouthDogAI __instance)
        {
            if (__instance.agent == null)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(ForestGiantAI), "GrabPlayerServerRpc")]
        [HarmonyPrefix]
        public static bool GrabPlayerServerRpc(ForestGiantAI __instance, int playerId)
        {
            if ((int)global::StartOfRound.Instance.localPlayerController.playerClientId == playerId)
            {
                if (global::StartOfRound.Instance.localPlayerController.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
                {
                    controller.Damage(15);
                    if (controller.Health <= 0)
                        return true;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(MouthDogAI), "OnCollideWithPlayer")]
        [HarmonyPrefix]
        public static bool OnCollideWithPlayer(MouthDogAI __instance, Collider other)
        {
            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation);
            if (playerControllerB != null && __instance.currentBehaviourStateIndex == 3 && playerControllerB.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
            {
                if (playerControllerB == global::StartOfRound.Instance.localPlayerController)
                {
                    controller.Damage(15);
                    if (controller.Health <= 0)
                        return true;
                }
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(InteractTrigger), "CancelLadderAnimation")]
        [HarmonyPostfix]
        public static void CancelLadderAnimation(InteractTrigger __instance)
        {
            if (__instance.playerScriptInSpecialAnimation.currentlyHeldObjectServer != null && __instance.playerScriptInSpecialAnimation.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
                controller.EnableItemMeshes(false);
        }


        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "BeginGrabObject")]
        [HarmonyPrefix]
        public static bool BeginGrabObject(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer != null && __instance.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
                return false;
            return true;
        }

        internal void Damage(int damage)
        {
            if (DamagePhase > 0f) return;
            DoomHUD.HeadPhase = DoomHUD.SpritePhase.DAMAGE;
            DamagePhase = .7f;
            LeftPhase = 0f;
            RightPhase = 0f;
            ManiacPhase = 0f;

            if (Armor > 0)
            {
                Armor -= damage;
                if (Armor < 0)
                {
                    damage = -Armor;
                    Armor = 0;
                }
                else damage = 0;
            }
            if (damage > 0 && Health > 0)
            {
                Health -= damage;
                if (Health < 0)
                    Health = 0;
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Crouch")]
        [HarmonyPrefix]
        public static bool Crouch(GameNetcodeStuff.PlayerControllerB __instance, bool crouch)
        {
            if (__instance.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
            {
                if (__instance == global::StartOfRound.Instance.localPlayerController)
                {
                    if (crouch)
                        return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DamagePlayer")]
        [HarmonyPrefix]
        public static bool DamagePlayer(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
            {
                if (__instance == global::StartOfRound.Instance.localPlayerController)
                {
                    controller.Damage(5);
                    if (controller.Health <= 0)
                    {
                        __instance.health = 0;
                        __instance.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown);
                        return false;
                    }
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DiscardHeldObject")]
        [HarmonyPrefix]
        public static bool DiscardHeldObject(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer is PietSmietController controller)
            {
                if (!controller.CurseLifted)
                    return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "CalculateNormalLookingInput")]
        [HarmonyPostfix]
        public static void CalculateNormalLookingInput(GameNetcodeStuff.PlayerControllerB __instance, ref Vector2 inputVector)
        {
            if (__instance.currentlyHeldObjectServer is PietSmietController controller)
            {
                __instance.cameraUp = Mathf.Clamp(__instance.cameraUp, -20f, 20f);
            }
        }

        public override int GetItemDataToSave()
        {
            return CurseLifted ? 1 : 0;
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            CurseLifted = saveData == 1;
            CurseLiftedLastFrame = CurseLifted;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!CurseLifted && DoomRunning)
            {
                if (DoomHUD.Shoot())
                {
                    Network.Manager.Send(new DoomShotgun() { PlayerNum = (int)playerHeldBy.playerClientId, ShotgunPosition = playerHeldBy.gameplayCamera.transform.position, ShotgunForward = playerHeldBy.gameplayCamera.transform.forward });
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        DoomHUD.HeadPhase = DoomHUD.SpritePhase.MANIAC;
                        ManiacPhase = 0.5f;
                        LeftPhase = 0f;
                        RightPhase = 0f;
                        DamagePhase = 0f;
                    }
                    
                }
            }
        }

        private float WalkPhase = 0f;
        private float DamagePhase = 0f;
        private float ManiacPhase = 0f;
        private float LeftPhase = 0f;
        private float RightPhase = 0f;
        internal int Health = 100;
        internal int Armor = 100;

        public void OnUpdate(Game.Player player)
        {
            if (!CurseLifted && DoomRunning)
            {
                player.Controller.health = 100;
                player.Controller.sprintMeter = 1f;

                DoomHUD.HP = Health;
                DoomHUD.Armor = Armor;

                if (DamagePhase > 0f)
                    DamagePhase -= Time.deltaTime;
                if (ManiacPhase > 0f)
                    ManiacPhase -= Time.deltaTime;
                if (LeftPhase > 0f)
                {
                    LeftPhase -= Time.deltaTime;
                    if (LeftPhase <= 0f)
                    {
                        RightPhase = 0.5f;
                        DoomHUD.HeadPhase = DoomHUD.SpritePhase.RIGHT;
                    }
                }
                if (RightPhase > 0f)
                {
                    RightPhase -= Time.deltaTime;
                }
                if (ManiacPhase <= 0f && LeftPhase <= 0f && RightPhase <= 0f && DamagePhase <= 0f)
                {
                    if (DoomHUD.HeadPhase != DoomHUD.SpritePhase.STRAIGHT)
                        DoomHUD.HeadPhase = DoomHUD.SpritePhase.STRAIGHT;
                    if (WalkPhase > 0f && UnityEngine.Random.value > 0.998f)
                    {
                        LeftPhase = 0.5f;
                        DoomHUD.HeadPhase = DoomHUD.SpritePhase.LEFT;
                    }
                }
                if (player.Controller.moveInputVector.sqrMagnitude > 0.1f)
                {
                    DoomHUD.ShotgunAnimator.SetBool("Walking", true);
                    WalkPhase += Time.deltaTime * 5f;
                    while (WalkPhase > Mathf.PI)
                        WalkPhase -= Mathf.PI;
                }
                else
                {
                    WalkPhase = Mathf.Lerp(WalkPhase, WalkPhase > Mathf.PI / 2f ? Mathf.PI : 0f, Time.deltaTime * 5f);
                    DoomHUD.ShotgunAnimator.SetBool("Walking", false);
                }
                if (player.Controller.isCrouching)
                    player.Controller.Crouch(false);
                player.Controller.gameplayCamera.transform.localPosition = new Vector3(0f, Mathf.Sin(WalkPhase) * -0.4f, 0f);
            }
            else
                player.Controller.gameplayCamera.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        public void UpdateEffects(Game.Player player)
        {
            if (!CurseLifted)
            {
                player.SetMultiplierOverride("WeightSpeed", 0f);
                player.SetMultiplierOverride("Weight", 0f);
                player.SetMultiplierOverride("SprintSpeed", 1f);
                player.SetMultiplierOverride("FallDamage", 10f);
                player.SetMultiplierOverride("JumpHeight", 1f);
                player.SetMultiplierOverride("SprintStamina", 100f);
                player.SetMultiplierOverride("JumpStamina", 100f);
            }
        }

        public bool Movement(Game.Player player)
        {
            if (!CurseLifted)
            {
            }
            return false;
        }
    }
}
