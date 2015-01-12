using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Portal
    {
        public string Name;

        public Point Location;

        public bool HomePortal;
        public string TargetArea;
        public Point TargetLocation;

        public Portal(string Data)
        {
            string[] Arguments = Data.Split('\t');

            if (Arguments.Length == 3)
            {
                Name = Arguments[0];
                Location = new Point(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                HomePortal = true;
            }
            else
            {
                Name = Arguments[0];
                Location = new Point(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                HomePortal = false;
                TargetArea = Arguments[3];
                TargetLocation = new Point(Convert.ToInt32(Arguments[4]), Convert.ToInt32(Arguments[5]));
            }
        }

        public string GetData()
        {
            return Name + "\t" + Location.X + "\t" + Location.Y;
        }
    }
}
