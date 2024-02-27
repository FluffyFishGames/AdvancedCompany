using AdvancedCompany.Config;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Objects
{
    internal class MissileLauncher : GrabbableObject
    {
        public int Ammo = 3;
        public Transform Rocket1;
        public Transform Rocket2;
        public Transform Rocket3;

        void OnEnable()
        {
            Rocket1 = transform.Find("Rocket1");
            Rocket2 = transform.Find("Rocket2");
            Rocket3 = transform.Find("Rocket3");
            UpdateAmmo();
        }

        public void UpdateAmmo()
        {
            Rocket1.gameObject.SetActive(Ammo > 0);
            Rocket2.gameObject.SetActive(Ammo > 1);
            Rocket3.gameObject.SetActive(Ammo > 2);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (Ammo > 0)
            {
                Ammo--;
                UpdateAmmo();

                if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                    Rocket.Spawn(Rocket1.position, Rocket1.rotation);
            }
        }

        public override void LoadItemSaveData(int saveData)
        {
            Ammo = saveData;
            UpdateAmmo();
        }

        public override int GetItemDataToSave()
        {
            return Ammo;
        }

        public override void PocketItem()
        {
            if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
            {
                var player = Game.Player.GetPlayer(playerHeldBy);
                if (player != null)
                    player.RemoveOverride("HoldOneHandedItem", true);
            }
            base.PocketItem();
        }

        public override void DiscardItem()
        {
            if (!this.isPocketed && (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability))
            {
                var player = Game.Player.GetPlayer(playerHeldBy);
                if (player != null)
                    player.RemoveOverride("HoldOneHandedItem", true);
            }
            base.DiscardItem();
        }
        public override void EquipItem()
        {
            if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
            {
                var player = Game.Player.GetPlayer(playerHeldBy);
                if (player != null)
                    player.AddOverride("HoldOneHandedItem", "HoldRocketLauncher", true);
            }
            base.EquipItem();
        }
    }
}
