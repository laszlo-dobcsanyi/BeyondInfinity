using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Creature :Unit
    {
        public uint ID;

        public Creature(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[0]);
            Name = Arguments[1];
            Faction = Convert.ToUInt32(Arguments[2]);
            Icon = TextureLoader.FromFile(Program.GameForm.Device, @"data\icons\creature_" + Arguments[3] + ".png");
            ItemLevel = (uint)(Convert.ToUInt32(Arguments[4]) * 0.6);

            Energy = Convert.ToDouble(Arguments[5]);
            MaxEnergy = Convert.ToDouble(Arguments[6]);

            Location = new PointF(Convert.ToSingle(Arguments[7]), Convert.ToSingle(Arguments[8]));

            Rotation = Convert.ToInt32(Arguments[9]);
            Speed = Convert.ToDouble(Arguments[10]);
        }
    }
}
