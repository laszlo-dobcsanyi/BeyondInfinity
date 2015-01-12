using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Missile
    {
        public Area Area;
        public Region Region;

        public uint ID;
        public uint EffectID;

        public int Rank;
        public Spell Spell;
        public bool BonusRanked;

        public PointF Location;
        private double Rotation;
        private double Speed;
        private double Range;

        private PointF StartLocation;

        public Missile(Area area,uint effectid, int rank, Spell spell,bool bonusranked, PointF location, double rotation, double speed, double range)
        {
            Area = area;

            EffectID = effectid;

            Rank = rank;
            Spell = spell;
            BonusRanked = bonusranked;

            Location = location;
            Rotation = rotation;
            Speed = speed;
            Range = range;

            StartLocation = location;
        }

        public string GetData()
        {
            return ID + "\t" + EffectID + "\t" + Rank + "\t" + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + Speed;
        }

        public int Update(double ElapsedTime)
        {
            PointF NextLocation = new PointF(Location.X + (float)(ElapsedTime / 1000 * Speed * Math.Cos((double)Rotation / 180 * Math.PI)),
                Location.Y - (float)(ElapsedTime / 1000 * Speed * Math.Sin((double)Rotation / 180 * Math.PI)));

            if (Area.IsValidAirLocation(NextLocation))
            {
                Location = NextLocation;
                if (GetDistance(StartLocation, Location) < Range)
                    if (!Region.Collide(new RectangleF((Region.Index.X - 1) * Area.Regions_Size, (Region.Index.Y - 1) * Area.Regions_Size, 2 * Area.Regions_Size, 2 * Area.Regions_Size), Location)) return 1;
                    else return 0;
            }
            return 2;
        }

        private double GetDistance(PointF First, PointF Second)
        {
            return Math.Sqrt(Math.Pow(Second.X - First.X, 2) + Math.Pow(Second.Y - First.Y, 2));
        }
    }
}
