using System;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity
{
    public sealed class Character : Hero
    {
        public int Reputation;

        public bool Muted = false;
        public bool Invisible = false;

        public Group Group = new Group();

        public Spell[] Spells = new Spell[6];

        public Item[] Equipped = new Item[8];
        public Equipment[] Equipment_Backpack = new Equipment[10];
        public Equipment[] Equipment_Loot = new Equipment[5];
        public ReaderWriterLockSlim Equipment_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public double Global_Accuracy = 1000;
        public double Global_ClearcastChance = 100;
        public double Global_Resistance = 1000;
        public double Global_Haste = 1000;
        public double Global_Power = 0; 
        public int[] Global_SchoolPowers = new int[6];

        public Character(string Data)
            : base(Data)
        {
            Program.Loaded = 1;
        }


        public void Equipped_Add(Item Item)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Equipped[Item.Slot] = Item;
                Item.Activate();
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }

        public void Backpack_Add(uint Number, Equipment Equipment)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Equipment_Backpack[Number] = Equipment;
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }

        public void Equip(uint BackpackNumber)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Item Item_Backpack = Equipment_Backpack[BackpackNumber] as Item;
                if (Item_Backpack != null)
                {
                    Item Item_Equipped = Equipped[Item_Backpack.Slot];

                    Equipped[Item_Backpack.Slot] = Item_Backpack;
                    Equipment_Backpack[BackpackNumber] = Item_Equipped;

                    if (Item_Equipped != null)
                        Item_Equipped.Deactivate();
                    Item_Backpack.Activate();
                    return;
                }

                Spell Spell_Backpack = Equipment_Backpack[BackpackNumber] as Spell;
                if (Spell_Backpack != null)
                {
                    Spell Spell_Book = Spells[35 < Spell_Backpack.Effect ? Spell_Backpack.Effect / 6 - 2 : Spell_Backpack.Effect / 6];

                    Spells[35 < Spell_Backpack.Effect ? Spell_Backpack.Effect / 6 - 2 : Spell_Backpack.Effect / 6] = Spell_Backpack;
                    Equipment_Backpack[BackpackNumber] = Spell_Book;
                    return;
                }
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }

        public void Delete(uint BackpackNumber)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Equipment_Backpack[BackpackNumber] = null;
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }


        public void ClearLoot()
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                for (int Current = 0; Current < 5; Current++)
                    if (Equipment_Loot[Current] != null)
                        Equipment_Loot[Current] = null;
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }

        public void Loot_Add(uint Number, Equipment Equipment)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Equipment_Loot[Number] = Equipment;
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }

        public void Loot(uint LootNumber, uint SlotNumber)
        {
            Equipment_Locker.EnterWriteLock();
            try
            {
                Equipment_Backpack[SlotNumber] = Equipment_Loot[LootNumber];
                Equipment_Loot[LootNumber] = null;
            }
            finally { Equipment_Locker.ExitWriteLock(); }
        }



        public override void Marks_Add(Mark Mark)
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                Marks.Add(Mark);
                Mark.Effect_Start();
            }
            finally { Marks_Locker.ExitWriteLock(); }
        }

        public override void Marks_Remove(uint ID)
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (NextMark.ID == ID)
                    {
                        Marks.Remove(NextMark);
                        NextMark.Effect_End();
                        return;
                    }
            }
            finally { Marks_Locker.ExitWriteLock(); }
        }


        public void Dispose()
        {
            CombatTexts_Clear();
            Impacts.Clear();
            Marks.Clear();
        }
    }
}
