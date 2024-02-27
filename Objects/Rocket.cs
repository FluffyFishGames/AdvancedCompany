using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.VFX;
using UnityEngine;
using System.Collections;
using AdvancedCompany.Network.Messages;
using Unity.Netcode;
using UnityEngine.Rendering.HighDefinition;
using Unity.Mathematics;
using HarmonyLib;
using System.Reflection;
using AdvancedCompany.Patches;
using System.Linq;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    [HarmonyPatch]
    internal class Rocket : MonoBehaviour
    {
        public static GameObject RocketAsset;
        public long ID;
        public long TempID;
        private List<(Vector3, Quaternion)> RocketPath;
        private ParticleSystem Smoke;
        private ParticleSystem Fireworks;
        private AudioSource Audio;
        private AudioSource FireworkAudio;
        private HDAdditionalLightData FireworkLightData;
        private Light FireworkLight;
        private GameObject Model;
        private float FlyPhase = 0f;
        private bool Exploded = false;
        private Color Color;
        public float FlyTime = 2.5f;
        private bool ClientSide = false;
        private static long CurrentTempID = 1;
        private static long CurrentID = 1;
        private static Dictionary<long, Rocket> Rockets = new Dictionary<long, Rocket>();
        private static Dictionary<long, Rocket> TempRockets = new Dictionary<long, Rocket>();

        static Rocket()
        {
            Network.Manager.AddListener<SpawnRocketFromClient>((msg) =>
            {
                SpawnOnAllClients(msg.PlayerNum, msg.TempID, msg.Position, msg.Rotation, msg.TurbulenceTime, msg.TurbulenceSpeed, msg.TurbulenceStrength, msg.FlyTime, msg.Color, msg.Time, !NetworkManager.Singleton.IsServer);
            });
            Network.Manager.AddListener<SpawnRocket>((msg) =>
            {
                SpawnNetwork(msg.PlayerNum, msg.TempID, msg.ID, msg.Position, msg.Rotation, msg.TurbulenceTime, msg.TurbulenceSpeed, msg.TurbulenceStrength, msg.FlyTime, msg.Color, msg.Time, !NetworkManager.Singleton.IsServer);
            });
            Network.Manager.AddListener<RocketExplode>((msg) =>
            {
                Explode(msg.ID, msg.Position);
            });
        }

        public static void LoadAssets(AssetBundle assets)
        {
            RocketAsset = assets.LoadAsset<GameObject>("Assets/Prefabs/Objects/Rocket.prefab");
        }
        // turbulence time: 10 - 100; turbulence speed: 0.01 - 0.05; turbulenceStrength = 0.5 - 10, flyTime = 2.0 - 3.0

        public static void Spawn(Vector3 position, Quaternion rotation)
        {
            SpawnLocal(CurrentTempID++, position, rotation, UnityEngine.Random.Range(10f, 100f), UnityEngine.Random.Range(0.01f, 0.1f), UnityEngine.Random.Range(0.5f, 10f), UnityEngine.Random.Range(2f, 3f), NetworkManager.Singleton.LocalTime.Time, Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 1f, 1f), !NetworkManager.Singleton.IsServer);
        }

        private static void SpawnNetwork(int playerNum, long tempID, long ID, Vector3 position, Quaternion rotation, float turbulenceTime, float turbulenceSpeed, float turbulenceStrength, float flyTime, Color color, double time, bool clientSide = false)
        {
            if (playerNum == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                if (TempRockets.ContainsKey(tempID))
                {
                    var rocket = TempRockets[tempID];
                    rocket.ID = ID;
                    Rockets.Add(ID, rocket);
                    TempRockets.Remove(tempID);
                }
            }
            else if (!Rockets.ContainsKey(ID))
            {
                var go = GameObject.Instantiate(RocketAsset, position, rotation);
                var rocket = go.AddComponent<Rocket>();
                rocket.ID = ID;
                rocket.CalculatePath(position, rotation, turbulenceTime, turbulenceSpeed, turbulenceStrength, flyTime, color, time, true);
                Rockets.Add(ID, rocket);
            }
        }

        private static void SpawnOnAllClients(int playerNum, long tempID, Vector3 position, Quaternion rotation, float turbulenceTime, float turbulenceSpeed, float turbulenceStrength, float flyTime, Color color, double time, bool clientSide = false)
        {
            var id = CurrentID++;
            var go = GameObject.Instantiate(RocketAsset, position, rotation);
            var rocket = go.AddComponent<Rocket>();
            rocket.TempID = tempID;
            rocket.ID = id;
            rocket.CalculatePath(position, rotation, turbulenceTime, turbulenceSpeed, turbulenceStrength, flyTime, color, time, false);
            Rockets.Add(id, rocket);

            Network.Manager.Send(new SpawnRocket() { PlayerNum = playerNum, TempID = tempID, ID = id, Position = position, Rotation = rotation, TurbulenceTime = turbulenceTime, TurbulenceSpeed = turbulenceSpeed, TurbulenceStrength = turbulenceStrength, FlyTime = flyTime, Color = color, Time = time });
        }

        public static void SpawnLocal(long tempID, Vector3 position, Quaternion rotation, float turbulenceTime, float turbulenceSpeed, float turbulenceStrength, float flyTime, double time, Color color, bool clientSide = false)
        {
            if (clientSide)
            {
                var go = GameObject.Instantiate(RocketAsset, position, rotation);
                var rocket = go.AddComponent<Rocket>();
                rocket.TempID = tempID;
                rocket.CalculatePath(position, rotation, turbulenceTime, turbulenceSpeed, turbulenceStrength, flyTime, color, time, true);
                TempRockets.Add(tempID, rocket);

                Network.Manager.Send(new SpawnRocketFromClient() { PlayerNum = (int) GameNetworkManager.Instance.localPlayerController.playerClientId, TempID = tempID, Position = position, Rotation = rotation, TurbulenceTime = turbulenceTime, TurbulenceSpeed = turbulenceSpeed, TurbulenceStrength = turbulenceStrength, FlyTime = flyTime, Color = color, Time = time });
            }
            else
            {
                SpawnOnAllClients((int) GameNetworkManager.Instance.localPlayerController.playerClientId, tempID, position, rotation, turbulenceTime, turbulenceSpeed, turbulenceStrength, flyTime, color, time, false);
            }
        }

        public static void Explode(long ID, Vector3 position)
        {
            if (Rockets.ContainsKey(ID))
            {
                Rockets[ID].Explode(position);
                Rockets.Remove(ID);
            }
        }

        public void OnDestroy()
        {
            Rockets.Remove(this.ID);
        }

        public void CalculatePath(Vector3 position, Quaternion rotation, float turbulenceTime, float turbulenceSpeed, float turbulenceStrength, float flyTime, Color color, double time, bool clientSide = false)
        {
            Color = color;
            FlyTime = flyTime;
            ClientSide = clientSide;
            var shootPower = 90f;
            var powerReduce = 0.2f;
            var turbulenceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
            var rocketHeight = 1f;
            var maxAngleRot = 1f;
            var turbulenceAngle = 0f;
            var path = new List<Vector3>() { position };
            var currentRotation = rotation;
            var iterations = 0;
            var lastPosition = path[0];
            var stepSize = 4f;
            var power = shootPower;
            while (iterations < 100)
            {
                var phase = Mathf.Clamp01((float)iterations / turbulenceTime);
                var turbulence = turbulenceCurve.Evaluate(phase) * turbulenceStrength * stepSize;
                turbulenceAngle += Mathf.PI * turbulenceSpeed * stepSize;
                var newPosition = lastPosition + currentRotation * new Vector3(0f, stepSize * (power / 100f), 0f) + new Vector3(0f, -stepSize * ((100f - power) / 100f), 0f);
                var diff = (newPosition - lastPosition);
                var rot = diff == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(diff.normalized);
                var r = rot * new Vector3(1f, 0f, 0f) * (Mathf.Cos(turbulenceAngle) * turbulence);
                var f = rot * new Vector3(0f, 1f, 0f) * (Mathf.Sin(turbulenceAngle) * turbulence);
                path.Add(newPosition + r + f);
                lastPosition = newPosition;
                iterations++;
                power -= powerReduce;
            }

            iterations = 0;
            lastPosition = position;
            currentRotation = Quaternion.LookRotation((position + rotation * new Vector3(0f, 1f, 0f)) - position);
            float velocity = 0.1f;
            RocketPath = new List<(Vector3, Quaternion)>() { (position, currentRotation) };
            while (iterations < 250)
            {
                if (velocity < 1f) velocity += 0.01f;

                var next = lastPosition + currentRotation * new Vector3(0f, 0f, velocity);
                var rocketHead = lastPosition + currentRotation * new Vector3(0f, 0f, rocketHeight);
                var target = rocketHead;
                var closestDistance = 99999f;
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var closestPoint = ClosestPoint(path[i], path[i + 1], rocketHead);
                    if (closestPoint != null)
                    {
                        var dist = (closestPoint.Value - rocketHead).sqrMagnitude;
                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            target = closestPoint.Value;
                        }
                    }
                }
                var diff = target - lastPosition;
                var wantedRot = diff == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(diff);
                currentRotation = Quaternion.RotateTowards(currentRotation, wantedRot, maxAngleRot);
                lastPosition = next;
                RocketPath.Add((next, currentRotation));
                iterations++;
            }
            FlyPhase = (float) (NetworkManager.Singleton.LocalTime.Time - time);
        }
        private static Vector3? ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
        {
            var d = Vector3.Distance(a, b);
            var dir = (b - a).normalized;
            var t = Vector3.Dot(dir, p - a);
            if (t < 0f || t > d)
                return null;

            return a + dir * Mathf.Clamp(t, 0f, d);
        }

        void Start()
        {
            Smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
            Fireworks = transform.Find("Firework").GetComponent<ParticleSystem>();
            FireworkLight = Fireworks.GetComponent<Light>();
            FireworkLightData = Fireworks.GetComponent<HDAdditionalLightData>();
            Model = transform.Find("Model").gameObject;
            Audio = GetComponent<AudioSource>();
            FireworkAudio = Fireworks.GetComponent<AudioSource>();
            Smoke.Play();
            Audio.Play();
        }

        IEnumerator DelayedDestroy()
        {
            yield return new WaitForSeconds(5f);
            GameObject.DestroyImmediate(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger && !ClientSide && !Exploded)
            {
                Exploded = true;
                Network.Manager.Send(new RocketExplode() { ID = this.ID, Position = transform.position });
            }
        }

        private static Collider[] Colliders = new Collider[100];
        private static MethodInfo GiantReactToNoise = typeof(ForestGiantAI).GetMethod("ReactToNoise", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        private static MethodInfo DropPlayerBody = typeof(ForestGiantAI).GetMethod("StopKillAnimation", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        private static FieldInfo MouthDogAIHearNoiseCooldown = typeof(MouthDogAI).GetField("hearNoiseCooldown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        private static FieldInfo MouthDogAITimer = typeof(MouthDogAI).GetField("AITimer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        private static Dictionary<global::EnemyAI, float> IgnoringPlayers = new Dictionary<EnemyAI, float>();

        [HarmonyPatch(typeof(global::EnemyAI), "GetAllPlayersInLineOfSight")]
        [HarmonyPrefix]
        public static bool LineOfSight(global::EnemyAI __instance, GameNetcodeStuff.PlayerControllerB[] __result)
        {
            if (IgnoringPlayers.ContainsKey(__instance))
            {
                __result = new GameNetcodeStuff.PlayerControllerB[0];
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::EnemyAI), "GetClosestPlayer")]
        [HarmonyPrefix]
        public static bool ClosestPlayer(global::EnemyAI __instance, GameNetcodeStuff.PlayerControllerB __result)
        {
            if (IgnoringPlayers.ContainsKey(__instance))
            {
                __result = null;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::RoundManager), "Update")]
        [HarmonyPrefix]
        public static void UpdateEnemyTimers()
        {
            var keys = IgnoringPlayers.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                IgnoringPlayers[keys[i]] -= Time.deltaTime;
                if (IgnoringPlayers[keys[i]] < 0f)
                    IgnoringPlayers.Remove(keys[i]);
                else
                {
                    if (keys[i] is ForestGiantAI giant)
                    {
                        giant.chasingPlayer = null;
                        
                    }
                }
            }
        }

        [HarmonyPatch(typeof(global::ForestGiantAI), "OnCollideWithPlayer")]
        [HarmonyPrefix]
        public static bool IgnoreCollision(global::ForestGiantAI __instance)
        {
            if (IgnoringPlayers.ContainsKey(__instance))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::ForestGiantAI), "BeginChasingNewPlayerClientRpc")]
        [HarmonyPrefix]
        public static bool IgnoreChase(global::ForestGiantAI __instance)
        {
            if (IgnoringPlayers.ContainsKey(__instance))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::ForestGiantAI), "BeginEatPlayer")]
        [HarmonyPrefix]
        public static bool IgnoreEat(global::ForestGiantAI __instance)
        {
            if (IgnoringPlayers.ContainsKey(__instance))
            {
                return false;
            }
            return true;
        }
        public static void IgnorePlayers(EnemyAI enemy, float time)
        {
            IgnoringPlayers[enemy] = time;
        }

        public void Explode(Vector3 position)
        {
            try
            {
                transform.position = position;
                Smoke.Stop();
                Audio.Stop();
                Model.SetActive(false);

                Color.RGBToHSV(Color, out var h1, out var s1, out var v1);
                Color.RGBToHSV(Color, out var h2, out var s2, out var v2);
                var randomColor1 = Color.HSVToRGB(h1 - 0.1f, s1, v1);
                var randomColor2 = Color.HSVToRGB(h2 + 0.1f, s1, v1);

                Exploded = true;
                FireworkLight.enabled = true;
                FireworkLightData.lightUnit = LightUnit.Candela;
                FireworkLightData.range = 12f;
                FireworkLightData.color = Color;
                FireworkLightData.enableSpotReflector = true;
                FireworkLightData.volumetricDimmer = 10f;
                FireworkLightData.lightDimmer = 10f;
                FireworkLightData.interactsWithSky = true;
                FireworkLightData.intensity = 800000f;

                var distance = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, transform.position);
                var scale = Mathf.Clamp(Mathf.Pow(distance / 200f, 0.5f), 0.1f, 1.5f);
                var m = Fireworks.main;
                m.startColor = new ParticleSystem.MinMaxGradient(randomColor1, randomColor2);
                m.startSize = new ParticleSystem.MinMaxCurve(m.startSize.constantMin * scale, m.startSize.constantMax * scale);
                var c = Fireworks.collision;
                c.radiusScale = scale;
                var s = Fireworks.subEmitters;
                for (var i = 0; i < s.subEmittersCount; i++)
                {
                    var e = s.GetSubEmitterSystem(i);
                    var em = e.main;
                    em.startSize = new ParticleSystem.MinMaxCurve(em.startSize.constantMin * scale, em.startSize.constantMax * scale);
                }

                Collider[] colliders = new Collider[100];
                int num = Physics.OverlapSphereNonAlloc(transform.position, 500f, Colliders, 8912896);
                var pos = transform.position;
                if (Physics.Raycast(pos, Vector3.down, out var hitInfo, 200f, 268437760, QueryTriggerInteraction.Ignore))
                    pos = hitInfo.point;
                for (int i = 0; i < num; i++)
                {
                    if (Colliders[i].transform.TryGetComponent<MouthDogAI>(out var dog))
                    {
                        if (dog.isEnemyDead)
                            continue;
                        dog.destination = pos;
                        dog.moveTowardsDestination = true;
                        dog.movingTowardsTargetPlayer = false;
                        dog.suspicionLevel = 9;
                        MouthDogAITimer.SetValue(dog, 10f);
                        MouthDogAIHearNoiseCooldown.SetValue(dog, 10f);
                        dog.SwitchToBehaviourState(1);
                        IgnorePlayers(dog, 7f);
//                        noiseListener.DetectNoise(pos, 10f, 0, 9999);   
                    }
                    else if (Colliders[i].transform.TryGetComponent<ForestGiantAI>(out var forestGiant))
                    {
                        forestGiant.SwitchToBehaviourState(0);
                        forestGiant.destination = pos;
                        forestGiant.moveTowardsDestination = true;
                        forestGiant.movingTowardsTargetPlayer = false;
                        forestGiant.timeSpentStaring = 10f;
                        forestGiant.chasingPlayer = null;
                        forestGiant.investigatePosition = RoundManager.Instance.GetNavMeshPosition(pos);
                        forestGiant.investigating = true;
                        
                        GiantReactToNoise.Invoke(forestGiant, new object[] { -4f, pos });
                        DropPlayerBody.Invoke(forestGiant, new object[] { });
                        IgnorePlayers(forestGiant, 5f);
                    }
                    else if (Colliders[i].transform.TryGetComponent<INoiseListener>(out var noiseListener))
                        noiseListener.DetectNoise(pos, 10f, 0, 9999);
                }
                num = Physics.OverlapSphereNonAlloc(transform.position, 3f, Colliders, 1051400);
                for (var i = 0; i < num; i++)
                {
                    if (Colliders[i].transform.CompareTag("Player"))
                    {
                        var controller = Colliders[i].GetComponent<GameNetcodeStuff.PlayerControllerB>();
                        if (controller == GameNetworkManager.Instance.localPlayerController)
                        {
                            var dist = Vector3.Distance(Colliders[i].transform.position, transform.position);
                            controller.DamagePlayer(((int)dist) * 5, false, true, CauseOfDeath.Blast);
                        }
                    }
                }
                RoundManager.Instance.PlayAudibleNoise(transform.position, 1000f, 1000f, 0, false, 9999);
                Fireworks.Play();
                FireworkAudio.Play();
                StartCoroutine(DelayedDestroy());
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Error while exploding rocket!");
                Plugin.Log.LogError(ex);
            }
        }

        private float lightPhase = 0f;
        void Update()
        {
            if (FireworkLight.enabled)
            {
                lightPhase += Time.deltaTime / 2f;
                FireworkLightData.range = Mathf.SmoothStep(25f, 0f, Mathf.Clamp01(lightPhase / 0.7f - 0.3f));
                FireworkLightData.shapeRadius = Mathf.SmoothStep(30f, 150f, Mathf.Clamp01(lightPhase * 2f));
                FireworkLightData.intensity = Mathf.SmoothStep(800000f, 100000f, Mathf.Clamp01(lightPhase / 0.5f - 0.5f));
                if (FireworkLightData.range <= 0.01f)
                    FireworkLight.enabled = false;
            }
            if (Exploded || (ClientSide && FlyPhase >= FlyTime)) return;
            Quaternion q = Quaternion.Euler(90f, 0f, 0f);
            FlyPhase += Time.deltaTime;
            if (FlyPhase >= FlyTime)
            {
                if (!ClientSide)
                {
                    Exploded = true;
                    Network.Manager.Send(new RocketExplode() { ID = this.ID, Position = transform.position });
                }
            }
            else
            {
                var p = FlyPhase / FlyTime;
                var start = (int)((RocketPath.Count - 2) * p);
                var phase = p * (RocketPath.Count - 2) - start;
                transform.position = RocketPath[start].Item1 + (RocketPath[start + 1].Item1 - RocketPath[start].Item1) * phase;
                transform.rotation = Quaternion.Lerp(RocketPath[start].Item2, RocketPath[start + 1].Item2, phase) * q;// - RocketPath[start].Item1) * phase;
            }
        }
    }

}
