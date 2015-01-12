using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Flag
    {
        public uint ID;
        public Point Location;
        public Battlefield Battlefield;

        public int Owner;

        public DateTime LastCap;

        public Flag(uint id, Point location)
        {
            ID = id;
            Location = location;
            Owner = -1;
        }

        public void Cap(Hero Hero)
        {
            GameManager.Flags_Locker.EnterWriteLock();
            try
            {
                if (Owner != -1)
                {
                    if (Battlefield.FlagDefended(this)) return;
                    Battlefield.Fronts[Owner].Bases_Number--;

                    GiveHonor();
                }

                LastCap = DateTime.Now;
                Owner = (int)Hero.FactionID;
                Battlefield.Fronts[Owner].Bases_Number++;
            }
            finally { GameManager.Flags_Locker.ExitWriteLock(); }

            GameManager.World.BroadcastCommand(Connection.Command.CalltoArms_CaptureFlag, ID + "\t" + Owner);
        }

        public void GiveHonor()
        {
            if (Owner != -1)
                GameManager.Factions[Owner].CalltoArms_Honor += (int)((DateTime.Now - LastCap).TotalSeconds * Program.CALLTOARMS_MULTIPLIER);
        }
    }
}
