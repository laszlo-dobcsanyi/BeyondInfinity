using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Splash
    {
        public Area Area;
        public Region Region;

        public uint ID;
        public uint EffectID;

        public int Rank;
        public Spell Spell;
        public bool BonusRanked;

        public PointF Location;
        public int Diameter;

        public Splash(Area area, uint effectid, int rank, Spell spell, bool bonusranked, PointF location, double diameter, double interval)
        {
            Area = area;
            EffectID = effectid;

            Rank = rank;
            Spell = spell;
            BonusRanked = bonusranked;

            Location = location;
            Diameter = (int)diameter;
            Interval = interval;
        }

        public string GetData()
        {
            return ID + "\t" + EffectID + "\t" + Rank + "\t" + Location.X + "\t" + Location.Y + "\t" + Diameter + "\t" + Interval;
        }

        public double Interval;
        public bool Update(double ElapsedTime)
        {
            Interval -= ElapsedTime;

            if (0 < Interval) return false;
            else
            {
                Trigger();
                return true;
            }
        }

        private void Trigger()
        {
            Console.WriteLine(">Triggering..");
            for (int Column = -1; Column < 2; Column++)
                for (int Row = -1; Row < 2; Row++)
                    if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= Area.MapSize.Width * Area.Regions_Multiplier))
                        if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= Area.MapSize.Height * Area.Regions_Multiplier))
                        {
                            foreach (Character NextCharacter in Area.Regions[Region.Index.X + Row, Region.Index.Y + Column].Characters)
                                if (Math.Sqrt(Math.Pow(NextCharacter.Location.X - Location.X, 2) + Math.Pow(NextCharacter.Location.Y - Location.Y, 2)) < Diameter / 2)
                                    Spell.Effect(Spell.Caster, NextCharacter, Rank, BonusRanked);

                            foreach (Agent NextAgent in Area.Regions[Region.Index.X + Row, Region.Index.Y + Column].Agents)
                                if (Math.Sqrt(Math.Pow(NextAgent.Location.X - Location.X, 2) + Math.Pow(NextAgent.Location.Y - Location.Y, 2)) < Diameter / 2)
                                    Spell.Effect(Spell.Caster, NextAgent, Rank, BonusRanked);

                            foreach (Creature NextCreature in Area.Regions[Region.Index.X + Row, Region.Index.Y + Column].Creatures)
                                if (Math.Sqrt(Math.Pow(NextCreature.Location.X - Location.X, 2) + Math.Pow(NextCreature.Location.Y - Location.Y, 2)) < Diameter / 2)
                                    Spell.Effect(Spell.Caster, NextCreature, Rank, BonusRanked);

                            foreach (Person NextPerson in Area.Regions[Region.Index.X + Row, Region.Index.Y + Column].Persons)
                                if (Math.Sqrt(Math.Pow(NextPerson.Location.X - Location.X, 2) + Math.Pow(NextPerson.Location.Y - Location.Y, 2)) < Diameter / 2)
                                    Spell.Effect(Spell.Caster, NextPerson, Rank, BonusRanked);
                        }
        }
    }
}
