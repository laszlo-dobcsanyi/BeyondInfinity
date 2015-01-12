using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Missile
    {
        public uint ID;

        public Texture Icon;
        public int Rank;

        public PointF Location;
        public double Rotation;
        public double Speed;

        public Missile(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[0]);

            Icon = Spell.Icons[Convert.ToUInt32(Arguments[1])];
            Rank = Convert.ToInt32(Arguments[2]);

            Location = new PointF(Convert.ToSingle(Arguments[3]), Convert.ToSingle(Arguments[4]));
            Rotation = Convert.ToDouble(Arguments[5]);
            Speed = Convert.ToDouble(Arguments[6]);
        }

        public void Update(double ElapsedTime)
        {
            Location.X += (float)(ElapsedTime / 1000 * Speed * Math.Cos((double)Rotation / 180 * Math.PI));
            Location.Y -= (float)(ElapsedTime / 1000 * Speed * Math.Sin((double)Rotation / 180 * Math.PI));
        }

        public void Dispose()
        {
            Icon.Dispose();
        }
    }
}
