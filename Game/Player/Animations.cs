using AdvancedCompany.Network.Messages;
using AdvancedCompany.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal partial class Player
    {
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

        internal void NetworkOverride(string originalName, string replacementName)
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
