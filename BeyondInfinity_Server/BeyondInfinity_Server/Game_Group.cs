using System;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed class Group
    {
        public static Generator IDGenerator = new Generator(Program.CAPACITY);

        public uint ID;
        public uint FactionID;

        public bool InArenaQueue = false;

        public uint Characters_Number = 1;

        public List<Character> Characters = new List<Character>(5);
        public ReaderWriterLockSlim Characters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<Dungeon> Dungeons = new List<Dungeon>();
        public ReaderWriterLockSlim Dungeons_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public Group(Hero Hero)
        {
            ID = IDGenerator.Next();
            FactionID = Hero.FactionID;

            Character Character = Hero as Character;
            if (Character != null)
            {
                Characters.Add(Character);
                Character.Connection.Send(Connection.Command.Character_GroupClear, "!");
                return;
            }

          /*Agent Agent = Hero as Agent;
            if (Agent != null)
            {
                Agents.Add(=
            }*/
        }

        public void Heroes_Add(Hero Hero)
        {
            if (!InArenaQueue)
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
        }

        public void Heroes_Remove(Hero Hero)
        {
            if (InArenaQueue)
            {
                InArenaQueue = false;
                Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Characters)
                        NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Leave, "!");
                }
                finally { Characters_Locker.ExitReadLock(); }
            }

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


        private void Characters_Add(Character Character)
        {
            if (Character.FactionID == FactionID)
            {
                Character.Group.Characters_Remove(Character);
                Character.Group = this;

                SendData(Character);
                BroadcastCommand(Connection.Command.Character_GroupAdd, Character.Name + '\t' + Character.IconID);

                Characters_Locker.EnterWriteLock();
                try
                {
                    Characters_Number++;
                    Characters.Add(Character);
                }
                finally { Characters_Locker.ExitWriteLock(); }
            }
        }

        private void Characters_Remove(Character Character)
        {
            Characters_Locker.EnterWriteLock();
            try
            {
                Characters.Remove(Character);
                Characters_Number--;
            }
            finally { Characters_Locker.ExitWriteLock(); }

            BroadcastCommand(Connection.Command.Character_GroupRemove, Character.Name);

            Character.Group = new Group(Character);
            GameManager.Groups_Add(Character.Group);

            if (Characters.Count == 0)
            {
                GameManager.Groups_Remove(this);

                foreach (Dungeon NextDungeon in Dungeons)
                    GameManager.Dungeons_Remove(NextDungeon);
            }
        }


        private void Agents_Add(Agent Agent)
        {

        }

        private void Agents_Remove(Agent Agent)
        {

        }


        public void SendData(Character Character)
        {
            Character.Connection.Send(Connection.Command.Character_GroupClear, "!");

            Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Characters)
                    Character.Connection.Send(Connection.Command.Character_GroupAdd, NextCharacter.Name + '\t' + NextCharacter.IconID);
            }
            finally { Characters_Locker.ExitReadLock(); }
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
}
