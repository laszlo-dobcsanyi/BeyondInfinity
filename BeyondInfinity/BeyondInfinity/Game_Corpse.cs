using System;
using System.Drawing;

namespace BeyondInfinity
{
    public class Corpse
    {
        public uint ID;
        public Point Location;

        public bool Looting = false;

        public Corpse(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[0]);
            Location = new Point(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
        }
    }
}
