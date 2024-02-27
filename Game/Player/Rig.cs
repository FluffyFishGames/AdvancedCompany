using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal partial class Player
    {
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


    }
}
