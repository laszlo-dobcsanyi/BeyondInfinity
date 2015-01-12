using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Impact
    {
        public uint ID;
        public static Generator IDGenerator = new Generator(Program.CAPACITY * 16);
        public int Rank; 
        public Spell Spell;
        public bool BonusRanked;

        public Impact(int rank, Spell spell,bool bonusranked, double interval)
        {
            ID = IDGenerator.Next();

            Rank = rank;
            Spell = spell;
            BonusRanked = bonusranked;

            Interval = interval;
        }

        public string GetData()
        {
            return ID + "\t" + Spell.Effect_ID+ "\t" + Rank + "\t" + Interval;
        }

        public double Interval;
        public bool Update(double ElapsedTime)
        {
            Interval -= ElapsedTime;

            if (0 < Interval) return false;
            else return true;
        }
    }
}
