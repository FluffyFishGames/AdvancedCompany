using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Player
    {
        internal static Dictionary<string, AnimationClip> Animations = new Dictionary<string, AnimationClip>();
        public static void AddAnimation(AnimationClip clip)
        {
            if (!Animations.ContainsKey(clip.name))
            {
                Animations.Add(clip.name, clip);
            }
        }

        public static void SetAnimationOverride(PlayerControllerB player, string originalName, string newName, bool sync = true)
        {
            var p = Game.Player.GetPlayer(player);
            p.AddOverride(originalName, newName, sync);
        }

        public static void RemoveAnimationOverride(PlayerControllerB player, string originalName, bool sync = true)
        {
            AssetBundle n;
            var p = Game.Player.GetPlayer(player);
            p.RemoveOverride(originalName, sync);
        }
    }
}
