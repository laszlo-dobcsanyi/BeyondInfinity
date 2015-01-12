using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Splash
    {
        public uint ID;

        public Texture Icon;
        public int Rank;

        public PointF Location;
        public int Diameter;

        public double FullInterval;
        public double Interval;

        public Splash(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[0]);
            Icon = Spell.Icons[Convert.ToUInt32(Arguments[1])];
            Rank = Convert.ToInt32(Arguments[2]);
            Location = new PointF(Convert.ToSingle(Arguments[3]), Convert.ToSingle(Arguments[4]));
            Diameter = Convert.ToInt32(Arguments[5]);
            FullInterval = Convert.ToDouble(Arguments[6]);
            Interval = FullInterval;
        }

        public void Update(double ElapsedTime)
        {
            if (0 < Interval - ElapsedTime) Interval -= ElapsedTime;
            else Interval = 0;
        }

        public void Dispose()
        {
            Icon.Dispose();
        }
    }
}
