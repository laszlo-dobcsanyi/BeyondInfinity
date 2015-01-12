using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed class Front
    {
        public uint Faction;

        public List<Character> Characters = new List<Character>();
        public ReaderWriterLockSlim Characters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<Agent> Agents = new List<Agent>();
        public ReaderWriterLockSlim Agents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public int Bases_Number = 0;

        private Battlefield Battlefield;

        public Front(uint faction, Battlefield battlefield)
        {
            Faction = faction;
            Battlefield = battlefield;
        }


        public void Heroes_Add(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
            {
                Characters_Add(Character);
                return;
            }

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Add(Agent);
        }

        public void Heroes_Remove(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
            {
                Characters_Remove(Character);
                return;
            }

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Remove(Agent);
        }


        public void Characters_Add(Character Character)
        {
            Character.Battlefield = Battlefield;
            Character.Connection.Send(Connection.Command.Character_SetBattlefield, Battlefield.ID.ToString());

            Characters_Locker.EnterWriteLock();
            try
            {
                Characters.Add(Character);
            }
            finally { Characters_Locker.ExitWriteLock(); }
        }

        public void Characters_Remove(Character Character)
        {
            Character.Battlefield = null;
            Character.Connection.Send(Connection.Command.Character_SetBattlefield, "-1");

            Characters_Locker.EnterWriteLock();
            try
            {
                Characters.Remove(Character);
            }
            finally { Characters_Locker.ExitWriteLock(); }
        }

        public void Characters_Clear()
        {
            Characters_Locker.EnterWriteLock();
            try
            {
                Characters.Clear();
            }
            finally { Characters_Locker.ExitWriteLock(); }
        }


        public void Agents_Add(Agent Agent)
        {
            Agent.Battlefield = Battlefield;

            Agents_Locker.EnterWriteLock();
            try
            {
                Agents.Add(Agent);
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }

        public void Agents_Remove(Agent Agent)
        {
            Agents_Locker.EnterWriteLock();
            try
            {
                Agents.Clear();
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }

        public void Agents_Clear()
        {
            Agents_Locker.EnterWriteLock();
            try
            {
                Agents.Clear();
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }


        public void BroadcastCommand(Connection.Command Command, string Data)
        {
            Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Characters)
                    NextCharacter.Connection.Send(Command, Data);
            }
            finally { Characters_Locker.ExitReadLock(); }
        }
    }

    public sealed class Battlefield
    {
        public uint ID;
        public int ArmySize = 0;
        public Front[] Fronts = new Front[3];

        public Flag[] Flags;
        public ReaderWriterLockSlim Flags_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public Battlefield(uint id)
        {
            ID = id;

            Fronts[0] = new Front(0, this);
            Fronts[1] = new Front(1, this);
            Fronts[2] = new Front(2, this);
        }

        public Battlefield(uint id, uint Faction0, uint Faction1)
        {
            ID = id;

            Fronts[Faction0] = new Front(Faction0, this);
            Fronts[Faction1] = new Front(Faction1, this);
        }

        public bool FlagDefended(Flag Flag)
        {
            if (Fronts[Flag.Owner] != null)
            {
                Fronts[Flag.Owner].Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Fronts[Flag.Owner].Characters)
                        if (Math.Sqrt(Math.Pow(Flag.Location.X - NextCharacter.Location.X, 2) + Math.Pow(Flag.Location.Y - NextCharacter.Location.Y, 2)) < 100)
                            return true;
                }
                finally { Fronts[Flag.Owner].Characters_Locker.ExitReadLock(); }

                Fronts[Flag.Owner].Agents_Locker.EnterReadLock();
                try
                {
                    foreach (Agent NextAgent in Fronts[Flag.Owner].Agents)
                        if (Math.Sqrt(Math.Pow(Flag.Location.X - NextAgent.Location.X, 2) + Math.Pow(Flag.Location.Y - NextAgent.Location.Y, 2)) < 100)
                            return true;
                }
                finally { Fronts[Flag.Owner].Agents_Locker.ExitReadLock(); }
            }
            return false;
        }

        public int GetFlagID(PointF Location)
        {
            for (int Current = 0; Current < Flags.Length; Current++)
                if (Math.Sqrt(Math.Pow(Location.X - Flags[Current].Location.X, 2) + Math.Pow(Location.Y - Flags[Current].Location.Y, 2)) < 32) return Current;

            return -1;
        }

        public void BroadcastCommand(uint Faction, Connection.Command Command, string Data)
        {
            if (Fronts[Faction] != null)
                Fronts[Faction].BroadcastCommand(Command, Data);
        }

        public void Shutdown()
        {
            foreach (Front NextFront in Fronts)
                if (NextFront != null)
                {
                    NextFront.Characters_Clear();
                    NextFront.Agents_Clear();
                }

            Flags_Locker.EnterReadLock();
            try
            {
                foreach (Flag NextFlag in Flags)
                    NextFlag.GiveHonor();
            }
            finally { Flags_Locker.ExitReadLock(); }
        }
    }
}