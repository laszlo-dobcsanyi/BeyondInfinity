using System;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public delegate void SpellEffect(Unit Caster, Unit Target, int Rank, double Multiplier);

    public sealed partial class Spell : Equipment
    {
        public Unit Caster;
        public uint BookSlot;

        public double[] Parameters = new double[3];

        public uint Effect_ID;
        public double[] Effect_Parameters = new double[3];

        public SpellEffect SpellEffect;

        public float Cooldown;
        private double TriggerMultiplier;
        private int RandomBonusRank;

        public Spell(Unit caster, string Data)
        {
            Caster = caster;
            Slot = 8;

            string[] Arguments = Data.Split('\t');

            for (int Current = 0; Current < 3; Current++)
                Parameters[Current] = Convert.ToDouble(Arguments[0 + Current]);
            Effect_ID = Convert.ToUInt32(Arguments[3]);
            SpellEffect = GetEffect(Effect_ID);
            for (int Current = 0; Current < 3; Current++)
                Effect_Parameters[Current] = Convert.ToDouble(Arguments[4 + Current]);

            BookSlot = 35 < Effect_ID ? Effect_ID / 6 - 2 : Effect_ID / 6;
            Cooldown = 0;

            RandomBonusRank = Random.Next(6);
        }

        public Spell(Unit caster, int Power, int Range)
        {
            Caster = caster;
            Slot = 8;

            for (int Current = 0; Current < 3; Current++)
                Parameters[Current] = Power + Random.NextDouble() * Range;
            Effect_ID = (uint)(Random.Next(4 * 6) + Random.Next(1) * (Random.Next(2 * 6) + 2 * 6));
            SpellEffect = GetEffect(Effect_ID);
            for (int Current = 0; Current < 3; Current++)
                Effect_Parameters[Current] = Power + Random.NextDouble() * Range;

            BookSlot = 35 < Effect_ID ? Effect_ID / 6 - 2 : Effect_ID / 6;
            Cooldown = 0;

            RandomBonusRank = Random.Next(6);
        }

        public static Random Random = new Random();
        public void Trigger(int Rank, double Rotation)
        {
            if (Cooldown == 0)
            {
                TriggerMultiplier = Program.MULTIPLIER_MISSILE;

                bool BonusRanked = false;
                if (Rank == RandomBonusRank)
                {
                    RandomBonusRank = Random.Next(6);
                    BonusRanked = true;
                }

                if (EnergyCost[Effect_ID / 6, Effect_ID % 6] != 0) Caster.Energy_Damage(null, EnergyChangeType.SpellCost, EnergyCost[Effect_ID / 6, Effect_ID % 6]);
                Cooldown_Set(Rank);

                Rotation = (Rotation + (Random.NextDouble() * (600 - Parameters[1]) - (600 - Parameters[1]) / 2) / 20) % 360;
                Caster.Area.Missiles_Add(new Missile(Caster.Area, Effect_ID, Rank, this, BonusRanked, Caster.Location, Rotation, 250 + Parameters[2] / 2, 150 + Parameters[0] / 2));
            }
        }

        public void Trigger(int Rank, Unit Target)
        {
            if (Cooldown == 0)
            {
                TriggerMultiplier = Program.MULTIPLIER_TARGET;

                bool BonusRanked = false;
                if (Rank == RandomBonusRank)
                {
                    RandomBonusRank = Random.Next(6);
                    BonusRanked = true;
                }

                if (EnergyCost[Effect_ID / 6, Effect_ID % 6] != 0) Caster.Energy_Damage(null, EnergyChangeType.SpellCost, EnergyCost[Effect_ID / 6, Effect_ID % 6]);
                Cooldown_Set(Rank);

                Character Character = Caster as Character;
                if (250 - (Parameters[1] / 2) <= Random.Next(1000))
                {
                    if (Math.Sqrt(Math.Pow(Caster.Location.X - Target.Location.X, 2) + Math.Pow(Caster.Location.Y - Target.Location.Y, 2)) < 200 + Parameters[0] / 2)
                        Target.Impacts_Add(new Impact(Rank, this, BonusRanked, (1000 - Parameters[2]) * Intervals[Effect_ID]));
                    else if (Character != null)
                        Character.Connection.Send(Connection.Command.Message, "0");
                }
                else
                {
                    if (Character != null)
                        Character.Connection.Send(Connection.Command.Message, "2");
                }
            }
        }

        public void Trigger(int Rank, PointF Location)
        {
            if (Cooldown == 0)
            {
                TriggerMultiplier = Program.MULTIPLIER_AREA;

                bool BonusRanked = false;
                if (Rank == RandomBonusRank)
                {
                    RandomBonusRank = Random.Next(6);
                    BonusRanked = true;
                }

                if (EnergyCost[Effect_ID / 6, Effect_ID % 6] != 0) Caster.Energy_Damage(null, EnergyChangeType.SpellCost, EnergyCost[Effect_ID / 6, Effect_ID % 6]);
                Cooldown_Set(Rank);

                if (Math.Sqrt(Math.Pow(Caster.Location.X - Location.X, 2) + Math.Pow(Caster.Location.Y - Location.Y, 2)) < 150 + Parameters[0] / 2)
                {
                    Location.X += (float)((128 - Parameters[1] / 3) / 2 - Random.Next((int)(128 - Parameters[1] / 3)));
                    Location.Y += (float)((128 - Parameters[1] / 3) / 2 - Random.Next((int)(128 - Parameters[1] / 3)));
                    Caster.Area.Splashes_Add(new Splash(Caster.Area, Effect_ID, Rank, this, BonusRanked, Location, Parameters[0], Parameters[2] * 10));
                }
                else
                {
                    Character Character = Caster as Character;
                    if (Character != null)
                        Character.Connection.Send(Connection.Command.Message, "0");
                }
            }
        }

        public void Effect(Unit Caster, Unit Target, int Rank, bool BonusRanked)
        {
            //Clearcast
            double Multiplier = (TriggerMultiplier + (BonusRanked ? Program.BONUS_RANDOMRANK : 0f));
            if (Random.NextDouble() < ((double)Caster.Global_ClearcastChance / 1000))
            {
                Multiplier += 2;
                Character Character = Caster as Character;
                if (Character != null)
                    Character.Connection.Send(Connection.Command.Message, "1");
            }
            else Multiplier += 1;

            if (Target.Status_Reflection <= 0) SpellEffect(Caster, Target, Rank, Multiplier);
            else
                if (Support[Effect_ID / 6, Effect_ID % 6])
                    SpellEffect(Caster, Target, Rank, Multiplier);
                else
                    SpellEffect(Target, Caster, Rank, Multiplier);
        }

        public void Update(double ElapsedTime)
        {
            ElapsedTime /= 1000;

            if (0 < Cooldown - ElapsedTime) Cooldown -= (float)ElapsedTime;
            else Cooldown = 0;
        }

        public override string GetData()
        {
            return Slot + "\t" + GetSaveData() + "\t" + Cooldown + "\t" + RandomBonusRank;
        }

        public string GetSaveData()
        {
            return
                  Parameters[0] + "\t"
                + Parameters[1] + "\t"
                + Parameters[2] + "\t"

                + Effect_ID + "\t"
                + Effect_Parameters[0] + "\t"
                + Effect_Parameters[1] + "\t"
                + Effect_Parameters[2];
        }

        public void Cooldown_Set(int Rank)
        {
            Cooldown = Cooldowns[Effect_ID, Rank] * (2 - ((float)Caster.Global_Haste / 1000));

            Character Character = Caster as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_SpellCooldown, BookSlot + "\t" + Cooldown + "\t" + RandomBonusRank);
        }
    }
}
