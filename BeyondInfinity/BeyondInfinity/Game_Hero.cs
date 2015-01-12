using System;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Hero : Unit
    {
        public bool InGroup = false;
        public static Texture[] Icons = new Texture[72];

        public Hero(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Name = Arguments[0];
            Faction = Convert.ToUInt32(Arguments[1]);
            Icon = Icons[Faction * 24 + Convert.ToUInt32(Arguments[2])];  //TextureLoader.FromFile(Program.GameForm.Device, @"data\icons\hero" + Faction + "_" + Arguments[2] + ".png");
            ItemLevel = (uint)(Convert.ToUInt32(Arguments[3]) * 0.6);

            Energy = Convert.ToDouble(Arguments[4]);
            MaxEnergy = Convert.ToDouble(Arguments[5]);

            Location = new PointF(Convert.ToSingle(Arguments[6]), Convert.ToSingle(Arguments[7]));

            Rotation = Convert.ToInt32(Arguments[8]);
            Speed = Convert.ToDouble(Arguments[9]);
        }
    } 
}
