using System;
using System.Drawing;

namespace BeyondInfinity
{
    public class Portal
    {
        public string Name;
        public Point Location;

        public Portal(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Name = Arguments[0];
            Location = new Point(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
        }
    }
}
