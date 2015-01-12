using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public partial class Area
    {
        public Size MapSize;
        public int[,] MapGrid;
        public int[,] MapTiles;

        public int Regions_Size;
        public int Regions_Multiplier;
        public Size Regions_Neighbourhood;
        public Region[,] Regions;

        public Generator Persons_Generator = new Generator(32);
        public Generator Creatures_Generator = new Generator(64);
        public Generator Missiles_Generator = new Generator(Program.CAPACITY);
        public Generator Splashes_Generator = new Generator(Program.CAPACITY);

        public ReaderWriterLockSlim UpdateLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint AddingCharacters_Count = 0;
        protected List<Character> AddingCharacters = new List<Character>();
        protected ReaderWriterLockSlim AddingCharacters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint RemovingCharacters_Count = 0;
        protected List<Character> RemovingCharacters = new List<Character>();
        protected ReaderWriterLockSlim RemovingCharacters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint TeleportingCharacters_Count = 0;
        protected List<Character> TeleportingCharacters = new List<Character>();
        protected ReaderWriterLockSlim TeleportingCharacters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint LeavingCharacters_Count = 0;
        protected List<Character> LeavingCharacters = new List<Character>();
        protected List<string> LeavingCharacters_TargetArea = new List<string>();
        protected List<Point> LeavingCharacters_TargetLocation = new List<Point>();
        protected ReaderWriterLockSlim LeavingCharacters_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint AddingAgents_Count = 0;
        protected List<Agent> AddingAgents = new List<Agent>();
        protected ReaderWriterLockSlim AddingAgents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint RemovingAgents_Count = 0;
        protected List<Agent> RemovingAgents = new List<Agent>();
        protected ReaderWriterLockSlim RemovingAgents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint TeleportingAgents_Count = 0;
        protected List<Agent> TeleportingAgents = new List<Agent>();
        protected ReaderWriterLockSlim TeleportingAgents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint LeavingAgents_Count = 0;
        protected List<Agent> LeavingAgents = new List<Agent>();
        protected List<string> LeavingAgents_TargetArea = new List<string>();
        protected List<Point> LeavingAgents_TargetLocation = new List<Point>();
        protected ReaderWriterLockSlim LeavingAgents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint AddingMissiles_Count = 0;
        protected List<Missile> AddingMissiles = new List<Missile>();
        protected ReaderWriterLockSlim AddingMissiles_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint RemovingMissiles_Count = 0;
        protected List<Missile> RemovingMissiles = new List<Missile>();
        protected ReaderWriterLockSlim RemovingMissiles_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint AddingSplashes_Count = 0;
        protected List<Splash> AddingSplashes = new List<Splash>();
        protected ReaderWriterLockSlim AddingSplashes_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected uint RemovingSplashes_Count = 0;
        protected List<Splash> RemovingSplashes = new List<Splash>();
        protected ReaderWriterLockSlim RemovingSplashes_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public uint Heroes_Number = 0;
        public uint Creatures_Number = 0;

        public virtual void Heroes_Add(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
                Characters_Add(Character);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Add(Agent);
        }

        public Hero Heroes_Get(Region Region, string Name)
        {
            Character Character = Characters_Get(Region, Name);
            if (Character != null) return Character;

            Agent Agent = Agents_Get(Region, Name);
            if (Agent != null) return Agent;

            return null;
        }

        public virtual void Heroes_Remove(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
                Characters_Remove(Character);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Remove(Agent);
        }

        public void Heroes_Teleport(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
                Characters_Teleport(Character);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Teleport(Agent);
        }

        public void Heroes_Leave(Hero Hero, string TargetArea, Point TargetLocation)
        {
            Character Character = Hero as Character;
            if (Character != null)
                Characters_Leave(Character, TargetArea, TargetLocation);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Leave(Agent, TargetArea, TargetLocation);
        }


        private void Process_AddingCharacters()
        {
            if (AddingCharacters_Count != 0)
            {
                AddingCharacters_Locker.EnterWriteLock();
                try
                {
                    foreach (Character NextCharacter in AddingCharacters)
                    {
                        Heroes_Number++;

                        NextCharacter.Location_Locker.EnterWriteLock();
                        try
                        {
                            NextCharacter.Area = this;
                            NextCharacter.Region = GetRegion(NextCharacter.Location);
                        }
                        finally { NextCharacter.Location_Locker.ExitWriteLock(); }

                        SendData(NextCharacter);
                        NextCharacter.Region.Characters_Add(NextCharacter);

                        if ((GameManager.World == this) && (GameManager.CallToArms)) GameManager.AssignHero(NextCharacter);
                    }

                    AddingCharacters.Clear();
                    AddingCharacters_Count = 0;
                }
                finally { AddingCharacters_Locker.ExitWriteLock(); }
            }
        }

        public virtual void Characters_Add(Character Character)
        {
            AddingCharacters_Locker.EnterWriteLock();
            try
            {
                AddingCharacters.Add(Character);
                AddingCharacters_Count++;
            }
            finally { AddingCharacters_Locker.ExitWriteLock(); }
        }

        public Character Characters_Get(Region Region, string Name)
        {
            UpdateLocker.EnterReadLock();
            try
            {
                Character Target = null;
                for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                    for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                        if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                            if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            {
                                Target = Regions[Region.Index.X + Row, Region.Index.Y + Column].Characters_Get(Name);
                                if (Target != null) return Target;
                            }

                return null;
            }
            finally { UpdateLocker.ExitReadLock(); }
        }

        public void Characters_Remove(Character Character)
        {
            RemovingCharacters_Locker.EnterWriteLock();
            try
            {
                RemovingCharacters.Add(Character);
                RemovingCharacters_Count++;
            }
            finally { RemovingCharacters_Locker.ExitWriteLock(); }
        }

        public void Characters_Teleport(Character Character)
        {
            TeleportingCharacters_Locker.EnterWriteLock();
            try
            {
                TeleportingCharacters.Add(Character);
                TeleportingCharacters_Count++;
            }
            finally { TeleportingCharacters_Locker.ExitWriteLock(); }
        }

        public void Characters_Leave(Character Character, string TargetArea, Point TargetLocation)
        {
            LeavingCharacters_Locker.EnterWriteLock();
            try
            {
                LeavingCharacters.Add(Character);
                LeavingCharacters_TargetArea.Add(TargetArea);
                LeavingCharacters_TargetLocation.Add(TargetLocation);
                LeavingCharacters_Count++;
            }
            finally { LeavingCharacters_Locker.ExitWriteLock(); }
        }

        private void Process_RemovingCharacters()
        {
            if (RemovingCharacters_Count != 0)
            {
                RemovingCharacters_Locker.EnterWriteLock();
                try
                {
                    foreach (Character NextCharacter in RemovingCharacters)
                    {
                        NextCharacter.Region.Characters_Remove(NextCharacter);

                        BroadcastCommand(NextCharacter.Region, Connection.Command.Hero_Remove, NextCharacter.Name);

                        //Character.Area = null;
                        //Character.Region = null;              In order to be able to broadcast commands

                        if (Name == GameManager.World.Name)
                            if (NextCharacter.Battlefield != null) NextCharacter.Battlefield.Fronts[NextCharacter.FactionID].Heroes_Remove(NextCharacter);

                        Heroes_Number--;
                    }

                    RemovingCharacters.Clear();
                    RemovingCharacters_Count = 0;
                }
                finally { RemovingCharacters_Locker.ExitWriteLock(); }
            }
        }

        private void Process_TeleportingCharacters()
        {
            if (TeleportingCharacters_Count != 0)
            {
                TeleportingCharacters_Locker.EnterWriteLock();
                try
                {
                    foreach (Character NextCharacter in TeleportingCharacters)
                    {
                        NextCharacter.Region.Characters_Remove(NextCharacter);

                        BroadcastCommand(NextCharacter.Region, Connection.Command.Hero_Remove, NextCharacter.Name);

                        //NextCharacter.Area = null;
                        //NextCharacter.Region = null;

                        if (Name == GameManager.World.Name)
                            if (NextCharacter.Battlefield != null) NextCharacter.Battlefield.Fronts[NextCharacter.FactionID].Heroes_Remove(NextCharacter);

                        Heroes_Number--;

                        GameManager.TeleportHero(NextCharacter);
                    }

                    TeleportingCharacters.Clear();
                    TeleportingCharacters_Count = 0;
                }
                finally { TeleportingCharacters_Locker.ExitWriteLock(); }
            }

        }

        private void Process_LeavingCharacters()
        {
            if (LeavingCharacters_Count != 0)
            {
                LeavingCharacters_Locker.EnterWriteLock();
                try
                {
                    int Current = 0;
                    foreach (Character NextCharacter in LeavingCharacters)
                    {
                        NextCharacter.Region.Characters_Remove(NextCharacter);

                        BroadcastCommand(NextCharacter.Region, Connection.Command.Hero_Remove, NextCharacter.Name);

                        //NextCharacter.Area = null;
                        //NextCharacter.Region = null;

                        if (Name == GameManager.World.Name)
                            if (NextCharacter.Battlefield != null) NextCharacter.Battlefield.Fronts[NextCharacter.FactionID].Heroes_Remove(NextCharacter);

                        Heroes_Number--;

                        GameManager.CreateHero(NextCharacter, LeavingCharacters_TargetArea[Current], LeavingCharacters_TargetLocation[Current]);

                        Current++;
                    }

                    LeavingCharacters.Clear();
                    LeavingCharacters_TargetArea.Clear();
                    LeavingCharacters_TargetLocation.Clear();
                    LeavingCharacters_Count = 0;
                }
                finally { LeavingCharacters_Locker.ExitWriteLock(); }
            }
        }


        private void Process_AddingAgents()
        {
            if (AddingAgents_Count != 0)
            {
                AddingAgents_Locker.EnterWriteLock();
                try
                {
                    foreach (Agent NextAgent in AddingAgents)
                    {
                        Heroes_Number++;

                        NextAgent.Location_Locker.EnterWriteLock();
                        try
                        {
                            NextAgent.Area = this;
                            NextAgent.Region = GetRegion(NextAgent.Location);
                        }
                        finally { NextAgent.Location_Locker.ExitWriteLock(); }

                        SendData(NextAgent);
                        NextAgent.Region.Agents_Add(NextAgent);

                        if ((GameManager.World == this) && (GameManager.CallToArms)) GameManager.AssignHero(NextAgent);

                        NextAgent.Mind.NextPath();
                    }

                    AddingAgents_Count = 0;
                    AddingAgents.Clear();
                }
                finally { AddingAgents_Locker.ExitWriteLock(); }
            }
        }

        public void Agents_Add(Agent Agent)
        {
            AddingAgents_Locker.EnterWriteLock();
            try
            {
                AddingAgents.Add(Agent);
                AddingAgents_Count++;
            }
            finally { AddingAgents_Locker.ExitWriteLock(); }
        }

        public Agent Agents_Get(Region Region, string Name)
        {
            UpdateLocker.EnterReadLock();
            try
            {
                Agent Target = null;
                for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                    for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                        if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                            if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            {
                                Target = Regions[Region.Index.X + Row, Region.Index.Y + Column].Agents_Get(Name);
                                if (Target != null) return Target;
                            }

                return null;
            }
            finally { UpdateLocker.ExitReadLock(); }
        }

        public void Agents_Remove(Agent Agent)
        {
            RemovingAgents_Locker.EnterWriteLock();
            try
            {
                RemovingAgents.Add(Agent);
                RemovingAgents_Count++;
            }
            finally { RemovingAgents_Locker.ExitWriteLock(); }
        }

        public void Agents_Teleport(Agent Agent)
        {
            TeleportingAgents_Locker.EnterWriteLock();
            try
            {
                TeleportingAgents.Add(Agent);
                TeleportingAgents_Count++;
            }
            finally { TeleportingAgents_Locker.ExitWriteLock(); }
        }

        public void Agents_Leave(Agent Agent, string TargetArea, Point TargetLocation)
        {
            LeavingAgents_Locker.EnterWriteLock();
            try
            {
                LeavingAgents.Add(Agent);
                LeavingAgents_TargetArea.Add(TargetArea);
                LeavingAgents_TargetLocation.Add(TargetLocation);
                LeavingAgents_Count++;
            }
            finally { LeavingAgents_Locker.ExitWriteLock(); }
        }

        private void Process_RemovingAgents()
        {
            if (RemovingAgents_Count != 0)
            {
                RemovingAgents_Locker.EnterWriteLock();
                try
                {
                    foreach (Agent NextAgent in RemovingAgents)
                    {
                        NextAgent.Region.Agents_Remove(NextAgent);

                        BroadcastCommand(NextAgent.Region, Connection.Command.Hero_Remove, NextAgent.Name);

                        //NextAgent.Area = null;
                        //NextAgent.Region = null;

                        if (Name == GameManager.World.Name)
                            if (NextAgent.Battlefield != null) NextAgent.Battlefield.Fronts[NextAgent.FactionID].Heroes_Remove(NextAgent);

                        Heroes_Number--;
                    }

                    RemovingAgents.Clear();
                    RemovingAgents_Count = 0;
                }
                finally { RemovingAgents_Locker.ExitWriteLock(); }
            }
        }

        private void Process_TeleportingAgents()
        {
            if (TeleportingAgents_Count != 0)
            {
                TeleportingAgents_Locker.EnterWriteLock();
                try
                {
                    foreach (Agent NextAgent in TeleportingAgents)
                    {
                        NextAgent.Region.Agents_Remove(NextAgent);

                        BroadcastCommand(NextAgent.Region, Connection.Command.Hero_Remove, NextAgent.Name);

                        //NextAgent.Area = null;
                        //NextAgent.Region = null;

                        if (Name == GameManager.World.Name)
                            if (NextAgent.Battlefield != null) NextAgent.Battlefield.Fronts[NextAgent.FactionID].Heroes_Remove(NextAgent);

                        Heroes_Number--;

                        GameManager.TeleportHero(NextAgent);
                    }

                    TeleportingAgents.Clear();
                    TeleportingAgents_Count = 0;
                }
                finally { TeleportingAgents_Locker.ExitWriteLock(); }
            }
        }

        private void Process_LeavingAgents()
        {
            if (LeavingAgents_Count != 0)
            {
                LeavingAgents_Locker.EnterWriteLock();
                try
                {
                    int Current = 0;
                    foreach (Agent NextAgent in LeavingAgents)
                    {
                        NextAgent.Region.Agents_Remove(NextAgent);

                        BroadcastCommand(NextAgent.Region, Connection.Command.Hero_Remove, NextAgent.Name);

                        //NextAgent.Area = null;
                        //NextAgent.Region = null;

                        if (Name == GameManager.World.Name)
                            if (NextAgent.Battlefield != null) NextAgent.Battlefield.Fronts[NextAgent.FactionID].Heroes_Remove(NextAgent);

                        Heroes_Number--;

                        GameManager.CreateHero(NextAgent, LeavingAgents_TargetArea[Current], LeavingAgents_TargetLocation[Current]);

                        Current++;
                    }

                    LeavingAgents.Clear();
                    LeavingAgents_TargetArea.Clear();
                    LeavingAgents_TargetLocation.Clear();
                    LeavingAgents_Count = 0;
                }
                finally { LeavingAgents_Locker.ExitWriteLock(); }
            }
        }


        public void Persons_Add(Person Person)
        {
            Person.Location_Locker.EnterWriteLock();
            try
            {
                Person.Area = this;
                Person.Region = GetRegion(Person.Location);
            }
            finally { Person.Location_Locker.ExitWriteLock(); }

            Person.Mind.NextPath();
            Person.Region.Persons_Add(Person);

            Person.Broadcast_Enter();
        }

        public Person Persons_Get(Region Region, uint ID)
        {
            Person Target = null;
            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                        {
                            Target = Regions[Region.Index.X + Row, Region.Index.Y + Column].Persons_Get(ID);
                            if (Target != null) return Target;
                        }

            return null;
        }

        public void Persons_Remove(Person Person)
        {
            Person.Region.Persons_Remove(Person);

            BroadcastCommand(Person.Region, Connection.Command.Person_Remove, Person.ID.ToString());
            Person.Area.Creatures_Generator.Free(Person.ID);

            //Person.Area = null;
            //Person.Region = null;              In order to be able to broadcast commands!         
        }


        public void Creatures_Add(Creature Creature)
        {
            Creatures_Number++;

            Creature.Location_Locker.EnterWriteLock();
            try
            {
                Creature.Area = this;
                Creature.Region = GetRegion(Creature.Location);
            }
            finally { Creature.Location_Locker.ExitWriteLock(); }

            Creature.Mind.NextPath();
            Creature.Region.Creatures_Add(Creature);

            Creature.Broadcast_Enter();
        }

        public Creature Creatures_Get(Region Region, uint ID)
        {
            Creature Target = null;
            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                        {
                            Target = Regions[Region.Index.X + Row, Region.Index.Y + Column].Creatures_Get(ID);
                            if (Target != null) return Target;
                        }

            return null;
        }

        public void Creatures_Remove(Creature Creature)
        {
            Creature.Region.Creatures_Remove(Creature);

            BroadcastCommand(Creature.Region, Connection.Command.Creature_Remove, Creature.ID.ToString());
            Creature.Area.Creatures_Generator.Free(Creature.ID);

          //Creature.Area = null;
          //Creature.Region = null;              In order to be able to broadcast commands!         

            Creatures_Number--;
        }


        public void Corpses_Add(Corpse Corpse)
        {
            Corpse.Region = GetRegion(Corpse.Location);
            Corpse.Region.Corpses_Add(Corpse);

            BroadcastCommand(Corpse.Region, Connection.Command.Corpse_Add, Corpse.GetData());
        }

        public Corpse Corpses_Get(Region Region,uint ID)
        {
            Corpse Target = null;
            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                        {
                            Target = Regions[Region.Index.X + Row, Region.Index.Y + Column].Corpses_Get(ID);
                            if (Target != null) return Target;
                        }

            return null;
        }

        public void Corpses_Remove(Corpse Corpse)
        {
            Corpse.Region.Corpses_Remove(Corpse);

            BroadcastCommand(Corpse.Region, Connection.Command.Corpse_Remove, Corpse.ID.ToString());

            Corpse.Region = null;
        }


        private void Process_AddingMissiles()
        {
            if (AddingMissiles_Count != 0)
            {
                AddingMissiles_Locker.EnterWriteLock();
                try
                {
                    foreach (Missile NextMissile in AddingMissiles)
                    {
                        NextMissile.ID = Missiles_Generator.Next();
                        NextMissile.Region = GetRegion(NextMissile.Location);

                        NextMissile.Region.Missiles_Add(NextMissile);

                        BroadcastCommand(NextMissile.Region, Connection.Command.Missile_Add, NextMissile.GetData());
                    }

                    AddingMissiles.Clear();
                    AddingMissiles_Count = 0;
                }
                finally { AddingMissiles_Locker.ExitWriteLock(); }
            }
        }

        public void Missiles_Add(Missile Missile)
        {
            AddingMissiles_Locker.EnterWriteLock();
            try
            {
                AddingMissiles.Add(Missile);
                AddingMissiles_Count++;
            }
            finally { AddingMissiles_Locker.ExitWriteLock(); }
        }

        public void Missiles_Remove(Missile Missile)
        {
            RemovingMissiles_Locker.EnterWriteLock();
            try
            {
                RemovingMissiles.Add(Missile);
                RemovingMissiles_Count++;
            }
            finally { RemovingMissiles_Locker.ExitWriteLock(); }
        }

        private void Process_RemovingMissiles()
        {
            if (RemovingMissiles_Count != 0)
            {
                RemovingMissiles_Locker.EnterWriteLock();
                try
                {
                    foreach (Missile NextMissile in RemovingMissiles)
                    {
                        if (NextMissile.Area != null)   //  TODO !!! Sometimes occurs, so need to check!
                        {
                            NextMissile.Region.Missiles_Remove(NextMissile);

                            BroadcastCommand(NextMissile.Region, Connection.Command.Missile_Remove, NextMissile.ID.ToString());
                            Missiles_Generator.Free(NextMissile.ID);

                            NextMissile.Area = null;
                            NextMissile.Region = null;
                        }
                    }

                    RemovingMissiles.Clear();
                    RemovingMissiles_Count = 0;
                }
                finally { RemovingMissiles_Locker.ExitWriteLock(); }
            }
        }


        private void Process_AddingSplashes()
        {
            if (AddingSplashes_Count != 0)
            {
                Console.WriteLine(">Processing Added Splashes..");
                AddingSplashes_Locker.EnterWriteLock();
                try
                {
                    foreach (Splash NextSplash in AddingSplashes)
                    {
                        NextSplash.ID = Splashes_Generator.Next();
                        NextSplash.Region = GetRegion(NextSplash.Location);

                        NextSplash.Region.Splashes_Add(NextSplash);

                        BroadcastCommand(NextSplash.Region, Connection.Command.Splash_Add, NextSplash.GetData());
                    }

                    AddingSplashes.Clear();
                    AddingSplashes_Count = 0;
                }
                finally { AddingSplashes_Locker.ExitWriteLock(); }
            }
        }

        public void Splashes_Add(Splash Splash)
        {
            Console.WriteLine(">Adding Splash..");
            AddingSplashes_Locker.EnterWriteLock();
            try
            {
                AddingSplashes.Add(Splash);
                AddingSplashes_Count++;
            }
            finally { AddingSplashes_Locker.ExitWriteLock(); }
        }

        public void Splashes_Remove(Splash Splash)
        {
            RemovingSplashes_Locker.EnterWriteLock();
            try
            {
                RemovingSplashes.Add(Splash);
                RemovingSplashes_Count++;
            }
            finally { RemovingSplashes_Locker.ExitWriteLock(); }
        }

        private void Process_RemovingSplashes()
        {
            if (RemovingSplashes_Count != 0)
            {
                RemovingSplashes_Locker.EnterWriteLock();
                try
                {
                    foreach (Splash NextSplash in RemovingSplashes)
                    {
                        NextSplash.Region.Splashes_Remove(NextSplash);

                        BroadcastCommand(NextSplash.Region, Connection.Command.Splash_Remove, NextSplash.ID.ToString());
                        Splashes_Generator.Free(NextSplash.ID);

                        NextSplash.Area = null;
                        NextSplash.Region = null;
                    }

                    RemovingSplashes.Clear();
                    RemovingSplashes_Count = 0;
                }
                finally { RemovingSplashes_Locker.ExitWriteLock(); }
            }
        }
    }
}
