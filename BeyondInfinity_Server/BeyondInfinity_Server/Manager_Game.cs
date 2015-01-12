using System;
using System.Timers;
using System.Drawing;
using System.Threading;

namespace BeyondInfinity_Server
{
    public static partial class GameManager
    {
        public static void Initialize()
        {
            Factions[0] = new Faction("underbog0", new System.Drawing.Point(3350, 1300));
            Factions[1] = new Faction("underbog1", new System.Drawing.Point(3350, 1300));
            Factions[2] = new Faction("underbog2", new System.Drawing.Point(3350, 1300));

            World = new World();

            Keeps[0] = new Keep(0);
            Keeps[1] = new Keep(1);
            Keeps[2] = new Keep(2);

            Battlefields[0] = new Battlefield(0, 0, 1);
            Battlefields[1] = new Battlefield(1, 1, 2);
            Battlefields[2] = new Battlefield(2, 0, 2);
            Battlefields[3] = new Battlefield(3);

            Battlefields[0].Flags = new Flag[5];
            Battlefields[0].Flags[0] = Flags[0]; Flags[0].Battlefield = Battlefields[0];
            Battlefields[0].Flags[1] = Flags[1]; Flags[1].Battlefield = Battlefields[0];
            Battlefields[0].Flags[2] = Flags[2]; Flags[2].Battlefield = Battlefields[0];
            Battlefields[0].Flags[3] = Flags[3]; Flags[3].Battlefield = Battlefields[0];
            Battlefields[0].Flags[4] = Flags[4]; Flags[4].Battlefield = Battlefields[0];

            Battlefields[1].Flags = new Flag[5];
            Battlefields[1].Flags[0] = Flags[5]; Flags[5].Battlefield = Battlefields[1];
            Battlefields[1].Flags[1] = Flags[6]; Flags[6].Battlefield = Battlefields[1];
            Battlefields[1].Flags[2] = Flags[7]; Flags[7].Battlefield = Battlefields[1];
            Battlefields[1].Flags[3] = Flags[8]; Flags[8].Battlefield = Battlefields[1];
            Battlefields[1].Flags[4] = Flags[9]; Flags[9].Battlefield = Battlefields[1];

            Battlefields[2].Flags = new Flag[5];
            Battlefields[2].Flags[0] = Flags[10]; Flags[10].Battlefield = Battlefields[2];
            Battlefields[2].Flags[1] = Flags[11]; Flags[11].Battlefield = Battlefields[2];
            Battlefields[2].Flags[2] = Flags[12]; Flags[12].Battlefield = Battlefields[2];
            Battlefields[2].Flags[3] = Flags[13]; Flags[13].Battlefield = Battlefields[2];
            Battlefields[2].Flags[4] = Flags[14]; Flags[14].Battlefield = Battlefields[2];

            Battlefields[3].Flags = new Flag[7];
            Battlefields[3].Flags[0] = Flags[15]; Flags[15].Battlefield = Battlefields[3];
            Battlefields[3].Flags[1] = Flags[16]; Flags[16].Battlefield = Battlefields[3];
            Battlefields[3].Flags[2] = Flags[17]; Flags[17].Battlefield = Battlefields[3];
            Battlefields[3].Flags[3] = Flags[18]; Flags[18].Battlefield = Battlefields[3];
            Battlefields[3].Flags[4] = Flags[19]; Flags[19].Battlefield = Battlefields[3];
            Battlefields[3].Flags[5] = Flags[20]; Flags[20].Battlefield = Battlefields[3];
            Battlefields[3].Flags[6] = Flags[21]; Flags[21].Battlefield = Battlefields[3];

            CalltoArms_StartTimer.Elapsed += CalltoArms_Start;
            CalltoArms_EndTimer.Elapsed += CalltoArms_End;
            CalltoArms_StartTimer.Start();

            ArenaGroups_Locker[0] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ArenaGroups_Locker[1] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ArenaGroups_Locker[2] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ArenaGroups_Locker[3] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ArenaGroups_Locker[4] = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public static void TeleportHero(Hero Hero)
        {
            Hero.Moving = false;

                Hero.Location = new PointF(3350, 1300);
                Keeps[Hero.FactionID].Heroes_Add(Hero);
        }

        public static void CreateHero(Hero Hero, string TargetArea, Point TargetLocation)
        {
            Hero.Moving = false;
            Hero.Location = TargetLocation;

            if (World.FileName == TargetArea)
            {
                World.Heroes_Add(Hero);
                return;
            }

            if (Keeps[Hero.FactionID].FileName == TargetArea)
            {
                Keeps[Hero.FactionID].Heroes_Add(Hero);
                return;
            }

            Hero.Group.Dungeons_Locker.EnterReadLock();
            try
            {
                foreach (Dungeon NextDungeon in Hero.Group.Dungeons)
                    if (NextDungeon.FileName == TargetArea)
                    {
                        NextDungeon.Heroes_Add(Hero);
                        return;
                    }
            }
            finally { Hero.Group.Dungeons_Locker.ExitReadLock(); }

            if (TargetArea == "hole") Dungeons_Add(new Dungeon(Hero));
            else Dungeons_Add(new Dungeon(TargetArea, Hero));
        }


        public static bool CallToArms = false;
        public static DateTime EventStartTime = DateTime.Now;

        public static System.Timers.Timer CalltoArms_StartTimer = new System.Timers.Timer(Program.CALLTOARMS_START);
        public static void CalltoArms_Start(object Sender, ElapsedEventArgs Event)
        {
            Console.WriteLine(" ->Call to Arms..");

            CalltoArms_StartTimer.Stop();
            CalltoArms_EndTimer.Start();

            Flags_Locker.EnterWriteLock();
            try
            {
                foreach (Flag NextFlag in Flags)
                    NextFlag.LastCap = DateTime.Now;
            }
            finally { Flags_Locker.ExitWriteLock(); }

            foreach (Faction NextFaction in Factions)
                NextFaction.CalltoArms_Honor = 0;

            CallToArms = true;
            EventStartTime = DateTime.Now;

            foreach (Faction NextFaction in Factions)
            {
                NextFaction.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in NextFaction.Characters)
                        NextCharacter.Connection.Send(Connection.Command.CalltoArms_True, "!");
                }
                finally { NextFaction.Characters_Locker.ExitReadLock(); }
            }

            MasterAssign();
        }

        public static System.Timers.Timer CalltoArms_EndTimer = new System.Timers.Timer(Program.CALLTOARMS_END);
        public static void CalltoArms_End(object Sender, ElapsedEventArgs Event)
        {
            CallToArms = false;
            EventStartTime = DateTime.Now;

            CalltoArms_EndTimer.Stop();
            CalltoArms_StartTimer.Start();

            foreach (Battlefield NextBattlefield in Battlefields)
                NextBattlefield.Shutdown();

            foreach (Faction NextFaction in Factions)
            {
                Console.WriteLine(" = Reputation for Next Faction: {0}!", NextFaction.CalltoArms_Honor);
                NextFaction.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in NextFaction.Characters)
                    {
                        NextCharacter.Connection.Send(Connection.Command.CalltoArms_False, ".");

                        NextCharacter.Reputation_Modify(NextFaction.CalltoArms_Honor);

                        NextCharacter.Save();
                    }
                }
                finally { NextFaction.Characters_Locker.ExitReadLock(); }

                /*NextFaction.Agents_Locker.EnterReadLock();
                  try
                  {
                      foreach (Agent NextAgent in NextFaction.Agents)
                          NextAgent.Reputation_Modify(NextFaction.CalltoArms_Honor);
                  }
                  finally { NextFaction.Agents_Locker.ExitReadLock(); }*/
            }

            Console.WriteLine("<- Call to Arms Ended!");
        }

        public static void AssignHero(Hero Hero)
        {
            CalculateArmies();

            int Difference = int.MinValue;
            Front BestFront = null;
            for (int Current = 0; Current < 4; Current++)
                if (Battlefields[Current].Fronts[Hero.FactionID] != null)
                {
                    int Diff = Battlefields[Current].ArmySize -
                        (Battlefields[Current].Fronts[Hero.FactionID].Characters.Count + Battlefields[Current].Fronts[Hero.FactionID].Agents.Count);

                    if (Difference < Diff)
                    {
                        Difference = Diff;
                        BestFront = Battlefields[Current].Fronts[Hero.FactionID];
                    }
                }

            BestFront.Heroes_Add(Hero);

            Agent Agent = Hero as Agent;
            if (Agent != null) Agent.Mind.NewPath();
        }

        public static void MasterAssign()
        {
            World.UpdateLocker.EnterReadLock();
            try
            {
                int[] Faction_Numbers = new int[3];

                for (int Column = 0; Column <= World.MapSize.Height * World.Regions_Multiplier; Column++)
                    for (int Row = 0; Row <= World.MapSize.Width * World.Regions_Multiplier; Row++)
                    {
                        foreach (Agent NextAgent in World.Regions[Row, Column].Agents)
                            Faction_Numbers[NextAgent.FactionID]++;

                        foreach (Character NextCharacter in World.Regions[Row, Column].Characters)
                            Faction_Numbers[NextCharacter.FactionID]++;
                    }

                int MinMax = int.MinValue;
                int Difference = Faction_Numbers[1] - Faction_Numbers[2];
                for (int Current = Difference < 0 ? -Difference : 0; Current < (Faction_Numbers[0] - Difference) / 2 + 1; Current++)
                {
                    int x2 = Current;
                    int x0 = Current + Difference;
                    int x3 = Faction_Numbers[0] - x2 - x0;
                    int x1 = Faction_Numbers[2] - x2 - x3;
                    int minmax = Math.Min(Math.Min(Math.Min(x0, x1), x2), x3);

                    if (MinMax <= minmax)
                    {
                        MinMax = minmax;

                        Battlefields[0].ArmySize = x0 < 0 ? 0 : x0;
                        Battlefields[1].ArmySize = x1 < 0 ? 0 : x1;
                        Battlefields[2].ArmySize = x2 < 0 ? 0 : x2;
                        Battlefields[3].ArmySize = x3 < 0 ? 0 : x3;
                    }
                }

                int[] CurrentFields = new int[3];
                for (int Column = 0; Column <= World.MapSize.Height * World.Regions_Multiplier; Column++)
                    for (int Row = 0; Row <= World.MapSize.Width * World.Regions_Multiplier; Row++)
                    {
                        foreach (Agent NextAgent in World.Regions[Row, Column].Agents)
                        {
                            if (Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID] == null) CurrentFields[NextAgent.FactionID]++;
                            if (Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID].Characters.Count + Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID].Agents.Count
                                <= Battlefields[CurrentFields[NextAgent.FactionID]].ArmySize) Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID].Agents_Add(NextAgent);
                            else
                            {
                                CurrentFields[NextAgent.FactionID]++;
                                if (Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID] == null) CurrentFields[NextAgent.FactionID]++;
                                Battlefields[CurrentFields[NextAgent.FactionID]].Fronts[NextAgent.FactionID].Agents_Add(NextAgent);

                                //NextAgent.Mind.NewPath();
                            }
                        }

                        foreach (Character NextCharacter in World.Regions[Row, Column].Characters)
                        {
                            if (Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID] == null) CurrentFields[NextCharacter.FactionID]++;
                            if (Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID].Characters.Count + Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID].Agents.Count
                                <= Battlefields[CurrentFields[NextCharacter.FactionID]].ArmySize) Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID].Characters_Add(NextCharacter);
                            else
                            {
                                CurrentFields[NextCharacter.FactionID]++;
                                if (Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID] == null) CurrentFields[NextCharacter.FactionID]++;
                                Battlefields[CurrentFields[NextCharacter.FactionID]].Fronts[NextCharacter.FactionID].Characters_Add(NextCharacter);
                            }
                        }
                    }
            }
            finally { World.UpdateLocker.ExitReadLock(); }
        }

        private static void CalculateArmies()
        {
            World.UpdateLocker.EnterReadLock();
            try
            {
                int[] Faction_Numbers = new int[3];

                for (int Column = 0; Column <= World.MapSize.Height * World.Regions_Multiplier; Column++)
                    for (int Row = 0; Row <= World.MapSize.Width * World.Regions_Multiplier; Row++)
                    {
                        foreach (Agent NextAgent in World.Regions[Row, Column].Agents)
                            Faction_Numbers[NextAgent.FactionID]++;

                        foreach (Character NextCharacter in World.Regions[Row, Column].Characters)
                            Faction_Numbers[NextCharacter.FactionID]++;
                    }

                int MinMax = int.MinValue;
                int Difference = Faction_Numbers[1] - Faction_Numbers[2];
                for (int Current = Difference < 0 ? -Difference : 0; Current < (Faction_Numbers[0] - Difference) / 2 + 1; Current++)
                {
                    int x2 = Current;
                    int x0 = Current + Difference;
                    int x3 = Faction_Numbers[0] - x2 - x0;
                    int x1 = Faction_Numbers[2] - x2 - x3;
                    int minmax = Math.Min(Math.Min(Math.Min(x0, x1), x2), x3);

                    if (MinMax <= minmax)
                    {
                        MinMax = minmax;

                        Battlefields[0].ArmySize = x0 < 0 ? 0 : x0;
                        Battlefields[1].ArmySize = x1 < 0 ? 0 : x1;
                        Battlefields[2].ArmySize = x2 < 0 ? 0 : x2;
                        Battlefields[3].ArmySize = x3 < 0 ? 0 : x3;
                    }
                }
            }
            finally { World.UpdateLocker.ExitReadLock(); }

            //Console.WriteLine("   Armies:\t{0}\t{1}\t{2}\t{3}\t({4})", Battlefields[0].ArmySize, Battlefields[1].ArmySize, Battlefields[2].ArmySize, Battlefields[3].ArmySize, MinMax);
        }

        public static string GetFlagData()
        {
            string FlagData = Flags[0].Owner.ToString();
            Flags_Locker.EnterReadLock();
            try
            {
                for (int Current = 1; Current < 22; Current++)
                    FlagData += "\t" + Flags[Current].Owner;
                return FlagData;
            }
            finally { Flags_Locker.ExitReadLock(); }
        }


        private static Group[] ArenaGroups = new Group[5];
        private static ReaderWriterLockSlim[] ArenaGroups_Locker = new ReaderWriterLockSlim[5];
        public static void Arena_Join(Group Group)
        {
            if (!Group.InArenaQueue)
                ThreadPool.QueueUserWorkItem(new WaitCallback(Join), Group);
        }

        private static void Join(object group)
        {
            Group Group = (Group)group;
            Group.InArenaQueue = true;

            Group.Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Group.Characters)
                    NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Join, "!");

                ArenaGroups_Locker[Group.Characters_Number - 1].EnterWriteLock();
                try
                {

                    if (ArenaGroups[Group.Characters_Number - 1] == null)
                    {
                        ArenaGroups[Group.Characters_Number - 1] = Group;
                    }
                    else
                        if (Group != ArenaGroups[Group.Characters_Number - 1])
                        {
                            ArenaGroups[Group.Characters_Number - 1].Characters_Locker.EnterReadLock();

                            if (ArenaGroups[Group.Characters_Number - 1].Characters_Number == Group.Characters_Number)
                            {
                                Arenas_Add(new Arena(ArenaGroups[Group.Characters_Number - 1], Group));

                                ArenaGroups[Group.Characters_Number - 1].Characters_Locker.ExitReadLock();
                                ArenaGroups[Group.Characters_Number - 1] = null;
                            }
                            else ArenaGroups[Group.Characters_Number - 1] = Group;
                        }
                }
                finally { ArenaGroups_Locker[Group.Characters_Number - 1].ExitWriteLock(); }
            }
            finally { Group.Characters_Locker.ExitReadLock(); }
        }
    }
}
