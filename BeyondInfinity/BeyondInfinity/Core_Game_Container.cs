using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity
{
    public partial class Game
    {
        public static string Area = "";

        public static int Battlefield;
        public static bool CalltoArms;
        public static DateTime EventTimer;

        public static bool Arena;
        public static DateTime ArenaTimer;

        public static Corpse Corpse_Looting;

        public static Character Character;
        //public static ReaderWriterLockSlim Character_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static Unit Target;
        public static ReaderWriterLockSlim Target_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Hero> Heroes = new List<Hero>();
        public static ReaderWriterLockSlim Heroes_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Person> Persons = new List<Person>();
        public static ReaderWriterLockSlim Persons_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Creature> Creatures = new List<Creature>();
        public static ReaderWriterLockSlim Creatures_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Corpse> Corpses = new List<Corpse>();
        public static ReaderWriterLockSlim Corpses_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Missile> Missiles = new List<Missile>();
        public static ReaderWriterLockSlim Missiles_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Splash> Splashes = new List<Splash>();
        public static ReaderWriterLockSlim Splashes_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Portal> Portals = new List<Portal>();

        public static Battlefield[] Battlefields = new Battlefield[4];
        public static Flag[] Flags = new Flag[22]
        {
            new Flag(0, new Point(1360, 3149)),
            new Flag(1, new Point(2672, 2672)),
            new Flag(2, new Point(2016, 2910)),
            new Flag(3, new Point(2342, 3238)),
            new Flag(4, new Point(1688, 2584)),
            new Flag(5, new Point(2254, 1122)),
            new Flag(6, new Point(2908, 1122)),
            new Flag(7, new Point(3324, 2018)),
            new Flag(8, new Point(2670, 2018)),
            new Flag(9, new Point(2792, 1570)),
            new Flag(10, new Point(1122, 2256)),
            new Flag(11, new Point(794, 1690)),
            new Flag(12, new Point(1242, 1572)),
            new Flag(13, new Point(1688, 1448)),
            new Flag(14, new Point(1362, 884)),
            new Flag(15, new Point(1360, 2670)),
            new Flag(16, new Point(1536, 2018)),
            new Flag(17, new Point(1744, 1122)),
            new Flag(18, new Point(2254, 1600)),
            new Flag(19, new Point(2908, 2256)),
            new Flag(20, new Point(2256, 2430)),
            new Flag(21, new Point(2016, 2016))
        };

        public static void Heroes_Add(Hero Hero)
        {
            Heroes_Locker.EnterWriteLock();
            try
            {
                Heroes.Add(Hero);
            }
            finally { Heroes_Locker.ExitWriteLock(); }

            Game.Character.Group.Members_Locker.EnterReadLock();
            try
            {
                foreach (Unknown NextUnknown in Character.Group.Members)
                    if ((NextUnknown.Name == Hero.Name) && (!Hero.InGroup))
                    {
                        Hero.InGroup = true;
                        return;
                    }

                if (Arena) Hero.Faction = Character.Faction + 1;
            }
            finally { Game.Character.Group.Members_Locker.ExitReadLock(); }
        }

        public static Hero Heroes_Get(string Name)
        {
            for (int Current = 0; Current < 5; Current++)
            {
                Heroes_Locker.EnterReadLock();
                try
                {
                    if (Character.Name == Name) return Character;
                    foreach (Hero NextHero in Heroes)
                        if (NextHero != null)
                            if (NextHero.Name == Name)
                                return NextHero;
                }
                finally { Heroes_Locker.ExitReadLock(); }

                Thread.Sleep(5);
            }

            return null;
        }

        public static void Heroes_Remove(string Name)
        {
            for (int Current = 0; Current < 5; Current++)
            {
                Heroes_Locker.EnterWriteLock();
                try
                {
                    foreach (Hero NextHero in Heroes)
                        if (NextHero.Name == Name)
                        {
                            Heroes.Remove(NextHero);
                            if (Target != null)
                                if (Target.Name == Name)
                                {
                                    Target_Locker.EnterWriteLock();
                                    try
                                    {
                                        Target = null;
                                    }
                                    finally { Target_Locker.ExitWriteLock(); }
                                }
                            return;
                        }
                }
                finally { Heroes_Locker.ExitWriteLock(); }

                Thread.Sleep(5);
            }

            return;
        }

        public static void Heroes_Clear()
        {
            Heroes_Locker.EnterWriteLock();
            try
            {
                Heroes.Clear();
            }
            finally { Heroes_Locker.ExitWriteLock(); }
        }


        public static void Persons_Add(Person Person)
        {
            Persons_Locker.EnterWriteLock();
            try
            {
                Persons.Add(Person);
            }
            finally { Persons_Locker.ExitWriteLock(); }
        }

        public static Person Persons_Get(uint ID)
        {
            for (int Current = 0; Current < 5; Current++)
            {
                Persons_Locker.EnterReadLock();
                try
                {
                    foreach (Person NextPerson in Persons)
                        if (NextPerson.ID == ID)
                            return NextPerson;
                }
                finally { Persons_Locker.ExitReadLock(); }

                Thread.Sleep(5);
            }

            return null;
        }

        public static void Persons_Remove(uint ID)
        {
            Persons_Locker.EnterWriteLock();
            try
            {
                foreach (Person NextPerson in Persons)
                    if (NextPerson.ID == ID)
                    {
                        Persons.Remove(NextPerson);

                        Person TargetPerson = Target as Person;
                        if (TargetPerson != null)
                            if (TargetPerson.ID == ID)
                            {
                                Target_Locker.EnterWriteLock();
                                try
                                {
                                    Target = null;
                                }
                                finally { Target_Locker.ExitWriteLock(); }
                            }

                        return;
                    }
            }
            finally { Persons_Locker.ExitWriteLock(); }
        }

        public static void Persons_Clear()
        {
            Persons_Locker.EnterWriteLock();
            try
            {
                Persons.Clear();
            }
            finally { Persons_Locker.ExitWriteLock(); }
        }


        public static void Creatures_Add(Creature Creature)
        {
            Creatures_Locker.EnterWriteLock();
            try
            {
                Creatures.Add(Creature);
            }
            finally { Creatures_Locker.ExitWriteLock(); }
        }

        public static Creature Creatures_Get(uint ID)
        {
            for (int Current = 0; Current < 5; Current++)
            {
                Creatures_Locker.EnterReadLock();
                try
                {
                    foreach (Creature NextCreature in Creatures)
                        if (NextCreature.ID == ID)
                            return NextCreature;
                }
                finally { Creatures_Locker.ExitReadLock(); }

                Thread.Sleep(10);
            }

            return null;
        }

        public static void Creatures_Remove(uint ID)
        {
            Creatures_Locker.EnterWriteLock();
            try
            {
                foreach (Creature NextCreature in Creatures)
                    if (NextCreature.ID == ID)
                    {
                        Creatures.Remove(NextCreature);

                        Creature TargetCreature = Target as Creature;
                        if (TargetCreature != null)
                            if (TargetCreature.ID == ID)
                            {
                                Target_Locker.EnterWriteLock();
                                try
                                {
                                    Target = null;
                                }
                                finally { Target_Locker.ExitWriteLock(); }
                            }
                        return;
                    }
            }
            finally { Creatures_Locker.ExitWriteLock(); }
        }

        public static void Creatures_Clear()
        {
            Creatures_Locker.EnterWriteLock();
            try
            {
                Creatures.Clear();
            }
            finally { Creatures_Locker.ExitWriteLock(); }
        }


        public static void Corpses_Add(Corpse Corpse)
        {
            Corpses_Locker.EnterWriteLock();
            try
            {
                Corpses.Add(Corpse);
            }
            finally { Corpses_Locker.ExitWriteLock(); }
        }

        public static void Corpses_Remove(uint ID)
        {
            Corpses_Locker.EnterWriteLock();
            try
            {
                foreach (Corpse NextCorpse in Corpses)
                    if (NextCorpse.ID == ID)
                    {
                        Corpses.Remove(NextCorpse);
                        return;
                    }
            }
            finally { Corpses_Locker.ExitWriteLock(); }
        }

        public static void Corpses_Clear()
        {
            Corpses_Locker.EnterWriteLock();
            try
            {
                Corpses.Clear();
            }
            finally { Corpses_Locker.ExitWriteLock(); }
        }


        public static void Missiles_Add(Missile Missile)
        {
            Missiles_Locker.EnterWriteLock();
            try
            {
                Missiles.Add(Missile);
            }
            finally { Missiles_Locker.ExitWriteLock(); }
        }

        public static void Missiles_Remove(uint ID)
        {
            for (int Current = 0; Current < 5; Current++)
            {
                Missiles_Locker.EnterWriteLock();
                try
                {
                    foreach (Missile NextMissile in Missiles)
                        if (NextMissile.ID == ID)
                        {
                            Missiles.Remove(NextMissile);
                            return;
                        }
                }
                finally { Missiles_Locker.ExitWriteLock(); }

                Thread.Sleep(5);
            }
            return;
        }

        public static void Missiles_Clear()
        {
            Missiles_Locker.EnterWriteLock();
            try
            {
                Missiles.Clear();
            }
            finally { Missiles_Locker.ExitWriteLock(); }
        }


        public static void Splashes_Add(Splash Splash)
        {
            Splashes_Locker.EnterWriteLock();
            try
            {
                Splashes.Add(Splash);
            }
            finally { Splashes_Locker.ExitWriteLock(); }
        }

        public static void Splashes_Remove(uint ID)
        {
            Splashes_Locker.EnterWriteLock();
            try
            {
                foreach(Splash NextSplash in Splashes)
                    if (NextSplash.ID == ID)
                    {
                        Splashes.Remove(NextSplash);
                        return;
                    }
            }
            finally { Splashes_Locker.ExitWriteLock(); }
        }

        public static void Splashes_Clear()
        {
            Splashes_Locker.EnterWriteLock();
            try
            {
                Splashes.Clear();
            }
            finally { Splashes_Locker.ExitWriteLock(); }
        }
    }
}
