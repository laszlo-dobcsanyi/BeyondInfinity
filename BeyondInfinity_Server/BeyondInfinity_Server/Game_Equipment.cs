using System;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public abstract class Equipment
    {
        public uint Slot;

        public abstract string GetData();

        public static Equipment GetEquipment(Character owner,string Data)
        {
            string[] Arguments = Data.Split(new char[] { '\t' }, 2);
            uint Slot = Convert.ToUInt32(Arguments[0]);

            if (Slot < 8) return new Item(Data);
            else return new Spell(owner,Arguments[1]);

        }
    }
}
