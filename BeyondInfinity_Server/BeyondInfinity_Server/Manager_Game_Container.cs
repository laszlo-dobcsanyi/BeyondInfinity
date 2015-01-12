using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public static partial class GameManager
    {
        public static World World;

        public static Keep[] Keeps = new Keep[3];

        public static Battlefield[] Battlefields = new Battlefield[4];

        public static Faction[] Factions = new Faction[3];
        public static ReaderWriterLockSlim Factions_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Dungeon> Dungeons = new List<Dungeon>();
        public static ReaderWriterLockSlim Dungeons_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Arena> Arenas = new List<Arena>();
        public static ReaderWriterLockSlim Arenas_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Group> Groups = new List<Group>();
        public static ReaderWriterLockSlim Groups_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static List<Agent> Agents = new List<Agent>();
        public static ReaderWriterLockSlim Agents_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
        public static ReaderWriterLockSlim Flags_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);


        public static void Dungeons_Add(Dungeon Dungeon)
        {
            Dungeons_Locker.EnterWriteLock();
            try
            {
                Dungeons.Add(Dungeon);
            }
            finally { Dungeons_Locker.ExitWriteLock(); }
        }

        public static void Dungeons_Remove(Dungeon Dungeon)
        {
            Dungeons_Locker.EnterWriteLock();
            try
            {
                Dungeons.Remove(Dungeon);
            }
            finally { Dungeons_Locker.ExitWriteLock(); }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t - {0} for Group {1} shut down!", Dungeon.Name, Dungeon.GroupID);
        }


        public static void Arenas_Add(Arena Arena)
        {
            Arenas_Locker.EnterWriteLock();
            try
            {
                Arenas.Add(Arena);
            }
            finally { Arenas_Locker.ExitWriteLock(); }
        }

        public static void Arenas_Remove(Arena Arena)
        {
            Arenas_Locker.EnterWriteLock();
            try
            {
                Arenas.Remove(Arena);
            }
            finally { Arenas_Locker.ExitWriteLock(); }
        }


        public static void Agents_Add(Agent Agent)
        {
            Agents_Locker.EnterWriteLock();
            try
            {
                Agents.Add(Agent);
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }

        public static void Agents_Remove(Agent Agent)
        {
            Agents_Locker.EnterWriteLock();
            try
            {
                Agents.Remove(Agent);
            }
            finally { Agents_Locker.ExitWriteLock(); }
        }


        public static Character GetCharacter(string Name)
        {
            foreach (Faction NextFaction in Factions)
            {
                Character NextCharacter = NextFaction.Characters_Get(Name);
                if (NextCharacter != null) return NextCharacter;
            }
            return null;
        }


        public static void Groups_Add(Group Group)
        {
            Groups_Locker.EnterWriteLock();
            try
            {
                Groups.Add(Group);
            }
            finally { Groups_Locker.ExitWriteLock(); }
        }

        public static void Groups_Remove(Group Group)
        {
            Groups_Locker.EnterWriteLock();
            try
            {
                Groups.Remove(Group);
                Group.IDGenerator.Free(Group.ID);
            }
            finally { Groups_Locker.ExitWriteLock(); }
        }
    }
}
