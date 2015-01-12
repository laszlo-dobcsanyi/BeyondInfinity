using System;
using System.IO;

namespace BeyondInfinity_Server
{
    public partial class Person : Unit
    {
        public Mind Mind;
        public uint DialogID;

        public Person(Area area, string Data)
        {
            Area = area;
            ID = Area.Creatures_Generator.Next();

            string[] Arguments = Data.Split('\t');
            int Number = Convert.ToInt32(Arguments[0]);

            Name = PersonsData[Number].Name;
            IconID = PersonsData[Number].IconID;
            FactionID = PersonsData[Number].FactionID;
            Energy = PersonsData[Number].Energy;
            MaxEnergy = PersonsData[Number].MaxEnergy;
            Spells = new Spell[PersonsData[Number].Spells.Length];
            for (int Current = 0; Current < Spells.Length; Current++)
                Spells[Current] = new Spell(this, PersonsData[Number].Spells[Current]);

            Rotation = 0;
            Speed = 50;
            Moving = false;
            Location = new System.Drawing.PointF(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));

            Mind = new Mind(this, false);
        }

        public override WayPoint Schedule_Next()
        {
            return null;
        }

        public override void Status_Death(Unit Killer)
        {
            if (Killer != null) Area.Corpses_Add(new Corpse(this, Killer));
            Area.Persons_Remove(this);
        }

        #region Broadcast
        public override void Broadcast_Enter()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Add, ID + "\t" + Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
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
            Region.BroadcastCommand(Connection.Command.Person_Add, ID + "\t" + Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Region.BroadcastCommand(Connection.Command.Person_Impacts_Add, ID + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Region.BroadcastCommand(Connection.Command.Person_Marks_Add, ID + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Enter(Connection Connection)
        {
            Connection.Send(Connection.Command.Person_Add, ID + "\t" + Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Connection.Send(Connection.Command.Person_Impacts_Add, ID + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Connection.Send(Connection.Command.Person_Marks_Add, ID + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Leave()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Remove, ID.ToString());
        }

        public override void Broadcast_Energy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_SetEnergyForced, ID + "\t" + (int)Energy);
        }

        public override void Broadcast_EnergyModify(Unit Caster)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_SetEnergy, ID + "\t" + (int)Energy + "\t" + Caster.Name);
        }

        public override void Broadcast_MaxEnergy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_SetMaxEnergy, ID + "\t" + (int)MaxEnergy);
        }

        public override void Broadcast_Location()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_SetPosition, ID + "\t" + (int)Location.X + "\t" + (int)Location.Y + "\t" + Rotation + "\t" + (Status_Rooted <= 0 ? Moving == false ? 0 : Speed : 0));
        }

        public override void Broadcast_ItemLevel()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_SetItemLevel, ID + "\t" + ItemLevel);
        }

        public override void Broadcast_MarksAdd(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Marks_Add, ID + "\t" + Mark.GetData());
        }

        public override void Broadcast_MarkStack(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Marks_SetStack, ID + "\t" + Mark.Effect_ID + "\t" + Mark.Stack);
        }

        public override void Broadcast_MarkDuration(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Marks_SetDuration, ID + "\t" + Mark.Effect_ID + "\t" + Mark.Periods[Mark.Effect_ID / 6, Mark.Effect_ID % 6] * Mark.Intervals[Mark.Effect_ID / 6, Mark.Effect_ID % 6, Mark.Stack - 1] * 1000);
        }

        public override void Broadcast_MarksRemove(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Marks_Remove, ID + "\t" + Mark.ID);
        }

        public override void Broadcast_ImpactsAdd(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Impacts_Add, ID + "\t" + Impact.GetData());
        }

        public override void Broadcast_ImpactsRemove(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Impacts_Remove, ID + "\t" + Impact.ID);
        }

        public override void Broadcast_Clear()
        {
            Area.BroadcastCommand(Region, Connection.Command.Person_Clear, ID.ToString());
        }
        #endregion

        private static PersonData[] PersonsData;
        public static void LoadPersonsData()
        {
            PersonsData = new PersonData[Program.PERSONS_NAMESNUMBER];
            for (int Current = 0; Current < Program.PERSONS_NAMESNUMBER; Current++)
                PersonsData[Current] = new PersonData(Program.PERSONS_NAMES[Current]);
        }

        private sealed class PersonData
        {
            public string Name;
            public uint IconID;
            public uint DialogID;
            public uint FactionID;
            public int Energy;
            public int MaxEnergy;
            public string[] Spells;

            public PersonData(string name)
            {
                try
                {
                    StreamReader CreatureFile = new StreamReader(@"data\persons\" + name + ".data");

                    Name = CreatureFile.ReadLine();
                    IconID = Convert.ToUInt32(CreatureFile.ReadLine());
                    DialogID = Convert.ToUInt32(CreatureFile.ReadLine());
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
                    Console.WriteLine("\t ! Error while loading Person Data ({0}):\n{1}", name, E.Message);
                }
            }
        }
    }
}
