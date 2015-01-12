using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed partial class Faction
    {
        private string Keep_Name;
        private Point Keep_Location;
        public int CalltoArms_Honor;

        public Faction(string keepname, Point location)
        {
            Keep_Name = keepname;
            Keep_Location = location;
        }

        public void Summon(Hero Hero)
        {
            if (Hero.Area.FileName == Keep_Name) Hero.Location_Set(Keep_Location);
            else Hero.Area.Heroes_Teleport(Hero);
        }

        public void BroadcastCommand(Connection.Command Command, string Data)
        {
            Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Characters)
                    NextCharacter.Connection.Send(Command, Data);
            }
            finally { Characters_Locker.ExitReadLock(); }
        }
    }
}
