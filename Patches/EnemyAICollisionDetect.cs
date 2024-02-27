using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class EnemyAICollisionDetect
    {
        [HarmonyPatch(typeof(global::EnemyAICollisionDetect), "IHittable.Hit")]
        [HarmonyPrefix]
        private static void Hit(ref int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX)
        {
            if (global::GameNetworkManager.Instance.localPlayerController == playerWhoHit)
            {
                if (UnityEngine.Random.value < Perks.GetMultiplier("DealDamage"))
                    force += 100;
            }
        }

    }
}
