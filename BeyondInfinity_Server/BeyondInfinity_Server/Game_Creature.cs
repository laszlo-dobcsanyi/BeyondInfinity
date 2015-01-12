using System;
using System.IO;

namespace BeyondInfinity_Server
{
    public sealed class Creature : Unit
    {
        public Mind Mind;

        public Creature(Area area, int x, int y, int Number)
        {
            Area = area;
            ID = Area.Creatures_Generator.Next();

            Name = CreaturesData[Number].Name;
            IconID = CreaturesData[Number].IconID;
            FactionID = CreaturesData[Number].FactionID;
            Energy = CreaturesData[Number].Energy * Area.Creatures_PowerMultiplier;
            MaxEnergy = CreaturesData[Number].MaxEnergy * Area.Creatures_PowerMultiplier;
            Spells = new Spell[CreaturesData[Number].Spells.Length];
            for (int Current = 0; Current < Spells.Length; Current++)
                Spells[Current] = new Spell(this, CreaturesData[Number].Spells[Current]);

            Global_Accuracy += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_ClearcastChance += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Haste += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Power += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Resistance += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;

            Rotation = 0;
            Speed = 50;
            Moving = false;
            Location = new System.Drawing.PointF(x, y);

            Schedule = new WayPoint[3];
            for (int Current = 0; Current < 3; Current++)
                Schedule[Current] = new WayPoint(Area.FileName, x + Random.Next(-256 + 32, 256 - 32), y + Random.Next(-256 + 32, 256 - 32));

            Mind = new Mind(this, true);
        }

        public Creature(Area area, int x, int y)
        {
            Area = area;
            ID = Area.Creatures_Generator.Next();
            int Number = Random.Next(9);

            Name = CreaturesData[Number].Name;
            IconID = CreaturesData[Number].IconID;
            FactionID = CreaturesData[Number].FactionID;
            Energy = CreaturesData[Number].Energy * Area.Creatures_PowerMultiplier;
            MaxEnergy = CreaturesData[Number].MaxEnergy * Area.Creatures_PowerMultiplier;
            Spells = new Spell[CreaturesData[Number].Spells.Length];
            for (int Current = 0; Current < Spells.Length; Current++)
                Spells[Current] = new Spell(this,CreaturesData[Number].Spells[Current]);

            Global_Accuracy += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_ClearcastChance += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Haste += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Power += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;
            Global_Resistance += Area.Creatures_PowerMultiplier * Area.Creatures_PowerBonus;

            Rotation = 0;
            Speed = 50;
            Moving = false;
            Location = new System.Drawing.PointF(x, y);

            Schedule = new WayPoint[3];
            for (int Current = 0; Current < 3; Current++)
                Schedule[Current] = new WayPoint(Area.FileName, x + Random.Next(-256 + 32, 256 - 32), y + Random.Next(-256 + 32, 256 - 32));

            Mind = new Mind(this, true);
        }

        public override void Status_Death(Unit Killer)
        {
            if (Killer != null) Area.Corpses_Add(new Corpse(this, Killer));
            Area.Creatures_Remove(this);
        }


        private int PreviousWayPoint = -1;
        private WayPoint[] Schedule = new WayPoint[3]
        {
            new WayPoint("hole\t2570\t1400"),
            new WayPoint("hole\t2420\t1720"),
            new WayPoint("hole\t2790\t1740")
        };
        public override WayPoint Schedule_Next()
        {
            int CurrentWayPoint;
            do
            {
                CurrentWayPoint = Random.Next(Schedule.Length);
            } while (CurrentWayPoint == PreviousWayPoint);

            PreviousWayPoint = CurrentWayPoint;
            return Schedule[CurrentWayPoint];
        }

        #region Broadcast
        public override void Broadcast_Enter()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Add, ID + "\t" + Name + "\t"+ FactionID +"\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy +  "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Broadcast_ImpactsAdd(NextImpact);
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Broadcast_MarksAdd(NextMark);
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Enter(Region Region)
        {
            Region.BroadcastCommand(Connection.Command.Creature_Add, ID + "\t" + Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Region.BroadcastCommand(Connection.Command.Creature_Impacts_Add, ID + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Region.BroadcastCommand(Connection.Command.Creature_Marks_Add, ID + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Enter(Connection Connection)
        {
            Connection.Send(Connection.Command.Creature_Add, ID + "\t" + Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Connection.Send(Connection.Command.Creature_Impacts_Add, ID + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Connection.Send(Connection.Command.Creature_Marks_Add, ID + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Leave()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Remove, ID.ToString());
        }

        public override void Broadcast_Energy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_SetEnergyForced, ID + "\t" + (int)Energy);
        }

        public override void Broadcast_EnergyModify(Unit Caster)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_SetEnergy, ID + "\t" + (int)Energy + "\t" + Caster.Name);
        }

        public override void Broadcast_MaxEnergy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_SetMaxEnergy, ID + "\t" + (int)MaxEnergy);
        }

        public override void Broadcast_Location()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_SetPosition, ID + "\t" + (int)Location.X + "\t" + (int)Location.Y + "\t" + Rotation + "\t" + (Status_Rooted <= 0 ? Moving == false ? 0 : Speed : 0));
        }

        public override void Broadcast_ItemLevel()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_SetItemLevel, ID + "\t" + ItemLevel);
        }

        public override void Broadcast_MarksAdd(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Marks_Add, ID + "\t" + Mark.GetData());
        }

        public override void Broadcast_MarkStack(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Marks_SetStack, ID + "\t" + Mark.Effect_ID + "\t" + Mark.Stack);
        }

        public override void Broadcast_MarkDuration(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Marks_SetDuration, ID + "\t" + Mark.Effect_ID + "\t" + Mark.Periods[Mark.Effect_ID / 6, Mark.Effect_ID % 6] * Mark.Intervals[Mark.Effect_ID / 6, Mark.Effect_ID % 6, Mark.Stack - 1] * 1000);
        }

        public override void Broadcast_MarksRemove(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Marks_Remove, ID + "\t" + Mark.ID);
        }

        public override void Broadcast_ImpactsAdd(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Impacts_Add, ID + "\t" + Impact.GetData());
        }

        public override void Broadcast_ImpactsRemove(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Impacts_Remove, ID + "\t" + Impact.ID);
        }

        public override void Broadcast_Clear()
        {
            Area.BroadcastCommand(Region, Connection.Command.Creature_Clear, ID.ToString());
        }
        #endregion

        private static CreatureData[] CreaturesData;
        public static void LoadCreaturesData()
        {
            CreaturesData = new CreatureData[Program.CREATURES_NAMESNUMBER];
            for (int Current = 0; Current < Program.CREATURES_NAMESNUMBER; Current++)
                CreaturesData[Current] = new CreatureData(Program.CREATURES_NAMES[Current]);
        }

        private sealed class CreatureData
        {
            public string Name;
            public uint IconID;
            public uint FactionID;
            public int Energy;
            public int MaxEnergy;
            public string[] Spells;

            public CreatureData(string name)
            {
                try
                {
                    StreamReader CreatureFile = new StreamReader(@"data\creatures\" + name + ".data");

                    Name = CreatureFile.ReadLine();
                    IconID = Convert.ToUInt32(CreatureFile.ReadLine());
                    FactionID = Convert.ToUInt32(CreatureFile.ReadLine());
                    Energy = Convert.ToInt32(CreatureFile.ReadLine());
                    MaxEnergy = Convert.ToInt32(CreatureFile.ReadLine());

                    Spells = new string[Convert.ToInt32(CreatureFile.ReadLine())];
                    for (uint Current = 0; Current < Spells.Length; Current++)
                        Spells[Current] = CreatureFile.ReadLine();

                    CreatureFile.Close();
                }
                catch (Exception E)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t ! Error while loading Creature Data ({0}):\n{1}", name, E.Message);
                }
            }
        }
    }
}
