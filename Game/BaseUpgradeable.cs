using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Network;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

namespace AdvancedCompany.Game
{
    internal class BaseUpgradeable : IUpgradeable, ITransferable
    {
        public int XP;
        public Dictionary<string, int> Levels = new();

        public int GetLevel(Perk perk)
        {
            if (Levels.ContainsKey(perk.ID))
            {
                return Levels[perk.ID];
            }
            return 0;
        }

        public void SetLevel(Perk perk, int level)
        {
            if (!Levels.ContainsKey(perk.ID))
                Levels.Add(perk.ID, level);
            else
                Levels[perk.ID] = level;
        }

        public virtual void Reset(bool resetXP)
        {
            Levels.Clear();
            if (resetXP)
                XP = 0;
        }

        public virtual void ReadData(FastBufferReader reader)
        {
            Levels.Clear();

            reader.ReadValueSafe(out int xp);
            XP = xp;

            reader.ReadValueSafe(out int levels);
            for (var i = 0; i < levels; i++)
            {
                reader.ReadValueSafe(out string name, true);
                reader.ReadValueSafe(out int level);
                Levels[name] = level;
            }
        }

        public virtual void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(XP);
            writer.WriteValueSafe(Levels.Count);
            foreach (var kv in Levels)
            {
                writer.WriteValueSafe(kv.Key, true);
                writer.WriteValueSafe(kv.Value);
            }
        }

        public int UsedXP
        {
            get
            {
                var used = 0;

                foreach (var p in Perk.Perks)
                {
                    var pp = p.Value.GetTotalPrice(p.Value.GetLevel(this));
                    used += pp;
                }
                return used;
            }
        }
        public int RemainingXP
        {
            get
            {
                return XP - UsedXP;
            }
        }
    }
}
