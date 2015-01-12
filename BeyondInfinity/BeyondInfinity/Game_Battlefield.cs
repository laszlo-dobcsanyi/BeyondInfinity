using System;

namespace BeyondInfinity
{
    public class Battlefield
    {
        public Flag[] Flags;
        public int Flags_Occupied;
        public int Flags_Unoccupied;

        public Battlefield(int Flags_Number)
        {
            Flags = new Flag[Flags_Number];
        }

        public void CalculateFlags()
        {
            Flags_Occupied = 0;
            Flags_Unoccupied = 0;

            foreach (Flag NextFlag in Flags)
                if (NextFlag.Owner != -1)
                    if (NextFlag.Owner == Game.Character.Faction) Flags_Occupied++;
                    else Flags_Unoccupied++;
        }
    }
}
