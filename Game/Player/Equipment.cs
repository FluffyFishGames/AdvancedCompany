using AdvancedCompany.Config;
using AdvancedCompany.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal partial class Player
    {
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
        public IHelmet Helmet;
        public Body Body;
        public Boots Boots;

        [Flags]
        public enum BodyLayers : int
        {
            NONE = 0,
            HIDE_FEET = 1
        };


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

    }
}
