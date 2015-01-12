using System;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Impact
    {
        public uint ID;

        public string Name;
        public Texture Icon;

        public uint EffectID;
        public int Rank;

        public double FullDuration;
        public double Duration;

        public Impact(string Data)
        {
            string[] Arguments = Data.Split('\t');

            ID = Convert.ToUInt32(Arguments[1]);
            EffectID = Convert.ToUInt32(Arguments[2]);

            Name = Spell.Names[EffectID];
            Icon = Spell.Icons[EffectID];
            Rank = Convert.ToInt32(Arguments[3]);
            Duration = Convert.ToDouble(Arguments[4]);
            FullDuration = Convert.ToDouble(Arguments[4]);
        }

        public void Update(double ElapsedTime)
        {
            if (0 < Duration - ElapsedTime) Duration -= ElapsedTime;
            else Duration = 0;
        }
    }
}
