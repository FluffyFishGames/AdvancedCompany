using UnityEngine;

namespace AdvancedCompany.Game
{
    public class ItemData : MonoBehaviour
    {
        // general data
        public int ID;
        public string ItemName;
        public Sprite ItemIcon;
        public float Weight;
        public bool IsConductive;
        public bool IsTwoHanded;
        public bool IsScrap;
        public int MinValue;
        public int MaxValue;
        public int Rarity = 100;
        public bool LimitPlanets;
        public int[] PlanetRarities = new int[Moons.Count];
        public bool UsesBattery;
        public float BatteryUsage;

        // shop data
        public bool IsBuyable;
        public int Price;
        public int MaxDiscount;

        // audio
        public AudioClip GrabSFX;
        public AudioClip DropSFX;
        public AudioClip PocketSFX;

        // animations
        public string HoldAnimation;
        public bool HoldIsTwoHanded;

        // synchronization
        public bool HasSaveData;
        public bool SyncUseFunction;
        public bool SyncInteractLRFunction;
        public bool SyncGrabFunction;
        public bool SyncDiscardFunction;


        public Vector3 EgoHeldPosition;
        public Vector3 EgoHeldRotation;
        public Vector3 HeldPosition;
        public Vector3 HeldRotation;
        public Vector3 GroundRestingRotation;
        public float GroundVerticalOffset;
        public Vector3 HolderRestingRotation;

        public Vector3 NoPosition;
    }
}