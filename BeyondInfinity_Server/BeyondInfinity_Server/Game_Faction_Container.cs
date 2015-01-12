using System;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Faction
    {
        public List<Character> Characters = new List<Character>();
        public ReaderWriterLockSlim Characters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<Agent> Agents = new List<Agent>();
        public ReaderWriterLockSlim Agents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
            BroadcastCommand(Connection.Command.Chat, "a:" + Character.Name + " joined.\n");
            Characters_Locker.EnterWriteLock();
            try
            {
                GameManager.Factions_Locker.EnterWriteLock();
                try
                {
                    Characters.Add(Character);
                }
                finally { GameManager.Factions_Locker.ExitWriteLock(); }
            }
            finally { Characters_Locker.ExitWriteLock(); }
        }

        public Character Characters_Get(string Name)
        {
            Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Characters)
                    if (NextCharacter.Name == Name) return NextCharacter;

                return null;
            }
            finally { Characters_Locker.ExitReadLock(); }
        }

        public void Characters_Remove(Character Character)
        {
            Characters_Locker.EnterWriteLock();
            try
            {
                GameManager.Factions_Locker.EnterWriteLock();
                try
                {
                    Characters.Remove(Character);
                }
                finally { GameManager.Factions_Locker.ExitWriteLock(); }
            }
            finally { Characters_Locker.ExitWriteLock(); }

            BroadcastCommand(Connection.Command.Chat, "a:" + Character.Name + " left.\n");
        }


        public void Agents_Add(Agent Agent)
        {
            Agents_Locker.EnterWriteLock();
            try
            {
                GameManager.Factions_Locker.EnterWriteLock();
                try
                {
                    Agents.Add(Agent);
                }
                finally { GameManager.Factions_Locker.ExitWriteLock(); }
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }

        public void Agents_Remove(Agent Agent)
        {
            Characters_Locker.EnterWriteLock();
            try
            {
                GameManager.Factions_Locker.EnterWriteLock();
                try
                {
                    Agents.Remove(Agent);
                }
                finally { GameManager.Factions_Locker.ExitWriteLock(); }
            }
            finally { Characters_Locker.ExitWriteLock(); }
        }
    }
}
