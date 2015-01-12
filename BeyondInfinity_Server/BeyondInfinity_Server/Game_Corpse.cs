using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed class Loot
    {
        public Equipment[] Equipments;
        public int Equipments_Number;

        public string Owner;

        public Loot(Character Character, int equipments_number)
        {
            Owner = Character.Name;
            Equipments_Number = equipments_number;

            Equipments = new Equipment[Equipments_Number];
            for (int Current = 0; Current < Equipments_Number; Current++)
                if (Character.Random.Next(2) == 0) Equipments[Current] = new Item(Character);
                else Equipments[Current] = new Spell(Character, 100, 200);
        }
    }

    public sealed class Corpse
    {
        public List<Loot> Loot = new List<Loot>(5);
        public ReaderWriterLockSlim Loot_Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public uint ID;
        public Region Region;
        public Point Location;

        public Corpse(Unit Victim, Unit Killer)
        {
            ID = Victim.ID;
            Location = new Point((int)Victim.Location.X, (int)Victim.Location.Y);

            if (Killer is Hero)
            {
                Loot_Lock.EnterWriteLock();
                try
                {
                    Group Group = (Killer as Hero).Group;

                    Group.Characters_Locker.EnterReadLock();
                    try
                    {
                        foreach (Character NextCharacter in Group.Characters)
                            Loot.Add(new Loot(NextCharacter, Character.Random.Next(5) + 1));
                    }
                    finally { Group.Characters_Locker.ExitReadLock(); }
                }
                finally { Loot_Lock.ExitWriteLock(); }
            }
        }

        public Loot GetLoot(Hero Hero)
        {
            Loot_Lock.EnterReadLock();
            try
            {
                foreach (Loot NextLoot in Loot)
                    if (NextLoot.Owner == Hero.Name)
                        return NextLoot;
                return null;
            }
            finally { Loot_Lock.ExitReadLock(); }
        }

        public void SendData(Character Character)
        {
            Loot_Lock.EnterReadLock();
            try
            {
                foreach (Loot NextLoot in Loot)
                    if (NextLoot.Owner == Character.Name)
                    {
                        Character.Connection.Send(Connection.Command.Character_LootClear, "!");
                        for (int Current = 0; Current < NextLoot.Equipments.Length; Current++)
                            if (NextLoot.Equipments[Current] != null)
                                Character.Connection.Send(Connection.Command.Character_AddLoot, Current + "\t" + NextLoot.Equipments[Current].GetData());
                        break;
                    }
            }
            finally { Loot_Lock.ExitReadLock(); }
        }

        public string GetData()
        {
            return ID + "\t" + Location.X + "\t" + Location.Y;
        }
    }
}
