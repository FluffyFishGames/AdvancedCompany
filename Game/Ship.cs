using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using AdvancedCompany.Network;
using AdvancedCompany.Config;

namespace AdvancedCompany.Game
{
    internal class Ship : BaseUpgradeable
    {
        internal int TotalQuota;
        internal bool ExtendedDeadline;

        public override void ReadData(FastBufferReader reader)
        {
            base.ReadData(reader);
            reader.ReadValueSafe(out TotalQuota);
            reader.ReadValueSafe(out ExtendedDeadline);
        }

        public override void WriteData(FastBufferWriter writer)
        {
            base.WriteData(writer);
            writer.WriteValueSafe(TotalQuota);
            writer.WriteValueSafe(ExtendedDeadline);
        }

        public override void Reset(bool resetXP)
        {
            Levels.Clear();
            if (resetXP)
                XP = ServerConfiguration.Instance.General.StartingShipXP;
        }

        public void ResetSave(string saveFile)
        {
            ES3.Save<int>("ShipXP", ServerConfiguration.Instance.General.StartingShipXP, saveFile);
            ES3.Save<Dictionary<string, int>>("Perks", new Dictionary<string, int>(), saveFile);
            ES3.Save<int>("TotalQuota", 0, saveFile);
            ES3.Save<bool>("ExtendedDeadline", false, saveFile);

            var keys = ES3.GetKeys(saveFile);
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i].StartsWith("PlayerXP_") || keys[i].StartsWith("PlayerPerks_"))
                    ES3.DeleteKey(keys[i], saveFile);
            }
        }

        public static Ship Load(string saveFile)
        {
            var ship = new Ship();
            
                //var saveFile = GameNetworkManager.Instance.currentSaveFileName;

            if (ES3.KeyExists("ShipXP", saveFile))
            {
                ship.XP = ES3.Load<int>("ShipXP", saveFile);
                if (ES3.KeyExists("Perks", saveFile))
                    ship.Levels = ES3.Load<Dictionary<string, int>>("Perks", saveFile);
            }
            else 
                ship.XP = ServerConfiguration.Instance.General.StartingShipXP;
            if (ES3.KeyExists("TotalQuota", saveFile))
                ship.TotalQuota = ES3.Load<int>("TotalQuota", saveFile);
            if (ES3.KeyExists("ExtendedDeadline", saveFile))
                ship.ExtendedDeadline = ES3.Load<bool>("ExtendedDeadline", saveFile);

            return ship;
        }

        public void Save(string saveFile)
        {
            ES3.Save("ShipXP", XP, saveFile);
            ES3.Save("Perks", Levels, saveFile);
            ES3.Save("TotalQuota", TotalQuota, saveFile);
            ES3.Save("ExtendedDeadline", ExtendedDeadline, saveFile);
        }

        public void Log()
        {
            foreach (var kv in Levels)
                Plugin.Log.LogMessage(kv.Key + ": " + kv.Value);
        }
    }
}
