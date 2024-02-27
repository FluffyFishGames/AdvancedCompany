using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Equipment
    {
        public enum Slot
        {
            HEAD,
            BODY,
            FEET
        }
        public delegate void EquipmentListener(GameNetcodeStuff.PlayerControllerB controller, Slot slot, GameObject[] objs);
        public static event EquipmentListener Listener;
        
        public static GameObject[] GetSpawnedEquipment(GameNetcodeStuff.PlayerControllerB controller, Slot slot)
        {
            var player = Game.Player.GetPlayer((int)controller.playerClientId);
            if (player == null) return new GameObject[0];
            if (slot == Slot.HEAD)
                return player.EquipmentItemsHead.ToArray();
            else if (slot == Slot.BODY)
                return player.EquipmentItemsBody.ToArray();
            else if (slot == Slot.FEET)
                return player.EquipmentItemsFeet.ToArray();
            return new GameObject[0];
        }

        internal static void NewFeet(GameNetcodeStuff.PlayerControllerB player, GameObject[] feet)
        {
            if (Listener != null)
                Listener.Invoke(player, Slot.FEET, feet);
        }

        internal static void NewBody(GameNetcodeStuff.PlayerControllerB player, GameObject[] body)
        {
            if (Listener != null)
                Listener.Invoke(player, Slot.BODY, body);
        }

        internal static void NewHead(GameNetcodeStuff.PlayerControllerB player, GameObject[] head)
        {
            if (Listener != null)
                Listener.Invoke(player, Slot.HEAD, head);
        }
    }
}
