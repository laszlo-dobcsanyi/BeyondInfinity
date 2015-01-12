using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public class Hero : Unit
    {
        public Group Group;

        public int Reputation;
        public Battlefield Battlefield;

        public Item[] Equipped = new Item[8];
        public Equipment[] Backpack = new Equipment[10];
        public ReaderWriterLockSlim Equipments_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Save()
        {
          //try
            {
                StreamWriter HeroFile = new StreamWriter(@"data\heroes\" + Name + ".data");

                HeroFile.WriteLine(FactionID);
                HeroFile.WriteLine(IconID);
                HeroFile.WriteLine(Energy);
                HeroFile.WriteLine(Reputation);

                for (uint Current = 0; Current < 6; Current++)
                    HeroFile.WriteLine(Spells[Current].GetSaveData());

                int Number = 0;
                for (int Current = 0; Current < 8; Current++)
                    if (Equipped[Current] != null)
                        Number++;
                HeroFile.WriteLine(Number);

                for (int Current = 0; Current < 8; Current++)
                    if (Equipped[Current] != null)
                        HeroFile.WriteLine(Equipped[Current].GetData());

                Number = 0;
                for (int Current = 0; Current < 10; Current++)
                    if (Backpack[Current] != null)
                        Number++;
                HeroFile.WriteLine(Number);

                for (int Current = 0; Current < 10; Current++)
                    if (Backpack[Current] != null)
                        HeroFile.WriteLine(Current + "\t" + Backpack[Current].GetData());

                HeroFile.Close();
            }
          /*catch (Exception E)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\tHero {0} ! Error while saving :\n{1}", Name, E.Message);
            }*/
        }

        protected void Spells_Add(Spell Spell)
        {
            Spells[Spell.BookSlot] = Spell;

            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_AddSpell, Spell.BookSlot + "\t" + Spell.GetData());
        }


        protected void Equipped_Add(Item Item)
        {
            Equipments_Locker.EnterWriteLock();
            try
            {
                Equipped[Item.Slot] = Item;
            }
            finally { Equipments_Locker.ExitWriteLock(); }

            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_AddEquipped, Item.Slot + "\t" + Item.GetData());
        }

        protected void Backpack_Add(uint Number, Equipment Equipment)
        {
            Equipments_Locker.EnterWriteLock();
            try
            {
                Backpack[Number] = Equipment;
                Character Character = this as Character;
                if (Character != null) Character.Connection.Send(Connection.Command.Character_AddBackpack, Number + "\t" + Equipment.GetData());
            }
            finally { Equipments_Locker.ExitWriteLock(); }
        }


        public bool Backpack_Add(Equipment Equipment)
        {
            Equipments_Locker.EnterWriteLock();
            try
            {
                for (int CurrentSlot = 0; CurrentSlot < 10; CurrentSlot++)
                    if (Backpack[CurrentSlot] == null)
                    {
                        Backpack[CurrentSlot] = Equipment;

                        Character Character = this as Character;
                        if (Character != null) Character.Connection.Send(Connection.Command.Character_AddBackpack, CurrentSlot + "\t" + Equipment.GetData());

                        Save();
                        return true;
                    }
                return false;
            }
            finally { Equipments_Locker.ExitWriteLock(); }
        }

        public bool Backpack_Loot(uint LootNumber, Corpse Corpse)
        {
            Corpse.Loot_Lock.EnterWriteLock();
            try
            {
                foreach (Loot NextLoot in Corpse.Loot)
                    if (NextLoot.Owner == Name)
                    {
                        Equipments_Locker.EnterWriteLock();
                        try
                        {
                            for (int CurrentSlot = 0; CurrentSlot < 10; CurrentSlot++)
                                if (Backpack[CurrentSlot] == null)
                                {
                                    Backpack[CurrentSlot] = NextLoot.Equipments[(int)LootNumber];
                                    NextLoot.Equipments[(int)LootNumber] = null;
                                    NextLoot.Equipments_Number--;

                                    if (NextLoot.Equipments_Number == 0) Corpse.Loot.Remove(NextLoot);

                                    Character Character = this as Character;
                                    if (Character != null) Character.Connection.Send(Connection.Command.Character_Loot, LootNumber + "\t" + CurrentSlot);

                                    if (Corpse.Loot.Count == 0)
                                        Area.Corpses_Remove(Corpse);

                                    Save();
                                    return true;
                                }
                            return false;
                        }
                        finally { Equipments_Locker.ExitWriteLock(); }
                    }
                return false;
            }
            finally { Corpse.Loot_Lock.ExitWriteLock(); }
        }

        public bool Backpack_Equip(uint BackpackNumber)
        {
            Equipments_Locker.EnterWriteLock();
            try
            {
                Item Backpack_Item = Backpack[BackpackNumber] as Item;
                if (Backpack_Item != null)
                {
                    Item Equipped_Item = Equipped[Backpack_Item.Slot];
                    if (Equipped_Item != null)
                    {
                        Equipped_Item.Deactivate(this);
                        Equipped[Backpack_Item.Slot] = Backpack_Item;
                        Equipped[Backpack_Item.Slot].Activate(this);
                        Backpack[BackpackNumber] = Equipped_Item;
                    }
                    else
                    {
                        Equipped[Backpack_Item.Slot] = Backpack_Item;
                        Backpack_Item.Activate(this);
                        Backpack[BackpackNumber] = null;
                    }
                    Broadcast_ItemLevel();
                    Calculate_SchoolPowers();

                    Character Character = this as Character;
                    if (Character != null) Character.Connection.Send(Connection.Command.Character_Equip, BackpackNumber.ToString());
                    return true;
                }

                Spell Backpack_Spell = Backpack[BackpackNumber] as Spell;
                if (Backpack_Spell != null)
                {
                    Spell Book_Spell = Spells[Backpack_Spell.BookSlot];

                    Spells[Backpack_Spell.BookSlot] = Backpack_Spell;
                    Backpack[BackpackNumber] = Book_Spell;

                    Calculate_SchoolPowers();

                    Character Character = this as Character;
                    if (Character != null) Character.Connection.Send(Connection.Command.Character_Equip, BackpackNumber.ToString());
                    Backpack_Spell.Cooldown_Set(5);
                    return true;
                }
                return false;
            }
            finally { Equipments_Locker.ExitWriteLock(); }
        }

        public void Backpack_Delete(uint BackpackNumber)
        {
            Equipments_Locker.EnterWriteLock();
            try
            {
                if (Backpack[BackpackNumber] != null)
                {
                    Backpack[BackpackNumber] = null;

                    Character Character = this as Character;
                    if (Character != null) Character.Connection.Send(Connection.Command.Character_BackpackDestroy, BackpackNumber.ToString());
                }
            }
            finally { Equipments_Locker.ExitWriteLock(); }
        }

        protected void Calculate_SchoolPowers()
        {
            int[] Schools_Items = new int[6];
            int[] Schools_Spells = new int[6];

            for (int Current = 2; Current < 8; Current++)
                if (Equipped[Current] != null)
                    Schools_Items[Equipped[Current].School]++;

            for (int Current = 0; Current < 6; Current++)
                Schools_Spells[Spells[Current].Effect_ID % 6]++;

            string SchoolPowersData = "";
            for (int Current = 0; Current < 6; Current++)
            {
                Global_SchoolPowers[Current] = Math.Min(Schools_Spells[Current], Schools_Items[Current]) * 10;
                SchoolPowersData += Global_SchoolPowers[Current] + "\t";
            }

            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_SchoolPower, SchoolPowersData);
        }

        public void Reputation_Modify(int Value)
        {
            Reputation += Value;
            if (Reputation < 0) Reputation = 0;

            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_SetReputation, Reputation.ToString());
        }

        public override void Status_Death(Unit KillerUnit)
        {
            if (Status_Dead)
            {
                Status_Dead = false;

                Status_Invulnerate();

                Clear();
                Energy_Set(MaxEnergy);

                Status_Uninvulnerate();


                GameManager.Factions[FactionID].Summon(this);
                if (Battlefield != null) Battlefield.Fronts[FactionID].Heroes_Remove(this);

                Hero KillerHero = KillerUnit as Hero;
                if (KillerHero != null)
                    if (KillerHero.FactionID != FactionID) KillerHero.Reputation_Modify(Program.HONORABLEKILL_BONUS);
                    else KillerHero.Reputation_Modify(-Program.HONORABLEKILL_BONUS*5);
                else
                {
                    KillerHero = Killer as Hero;
                    if (KillerHero != null)
                        if (KillerHero.FactionID != FactionID) KillerHero.Reputation_Modify(100);
                        else KillerHero.Reputation_Modify(-Program.HONORABLEKILL_BONUS);
                }

                //if (KillerHero != null) Console.WriteLine("{0} killed by {1}!", Name, KillerHero.Name);
                //else Console.WriteLine("{0} is dead!", Name);
            }
        }

        public override void GlobalCooldown_Set(float Value)
        {
            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_GlobalCooldown, Value.ToString());

            base.GlobalCooldown_Set(Value);
        }


        private WayPoint PreviousWayPoint = null;
        public override WayPoint Schedule_Next()
        {
            WayPoint CurrentWayPoint;
            if (GameManager.CallToArms)
                if (Battlefield != null)
                {
                    if (Battlefield.ID == 3)
                    {
                        do
                        {
                            CurrentWayPoint = Schedule_CalltoArms[Battlefield.ID, Random.Next(7)];
                        } while (CurrentWayPoint == PreviousWayPoint);
                    }
                    else
                    {
                        do
                        {
                            CurrentWayPoint = Schedule_CalltoArms[Battlefield.ID, Random.Next(5)];
                        } while (CurrentWayPoint == PreviousWayPoint);
                    }

                    int FlagID = Battlefield.GetFlagID(Location);
                    if (FlagID != -1) Battlefield.Flags[FlagID].Cap(this);

                    PreviousWayPoint = CurrentWayPoint;
                    return CurrentWayPoint;
                }

            do
            {
                CurrentWayPoint = Schedule[Random.Next(25)];
            } while (CurrentWayPoint == PreviousWayPoint);

            PreviousWayPoint = CurrentWayPoint;
            return CurrentWayPoint;
        }

        public void Communicate(string Message)
        {
            string[] Arguments = Message.Split(new char[] { ' ' }, 2);

            switch (Arguments[0])
            {
                case "/f": GameManager.Factions[FactionID].BroadcastCommand(Connection.Command.Chat, "f:" + Name + ": " + Arguments[1]); break;
                case "/b": if (Battlefield != null) Battlefield.BroadcastCommand(FactionID, Connection.Command.Chat, "b:" + Name + ": " + Arguments[1]); break;
                case "/g": Group.BroadcastCommand(Connection.Command.Chat, "g:" + Name + ": " + Arguments[1]); break;
                default: Area.BroadcastCommand(Region, Connection.Command.Chat, "s:" + Name + ": " + Arguments[1]); break;
            }
        }

        #region Broadcast
        public override void Broadcast_Enter()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Add, Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t" 
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
            Region.BroadcastCommand(Connection.Command.Hero_Add, Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t" 
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Region.BroadcastCommand(Connection.Command.Hero_Impacts_Add, Name + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Region.BroadcastCommand(Connection.Command.Hero_Marks_Add, Name + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Enter(Connection Connection)
        {
            Connection.Send(Connection.Command.Hero_Add, Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t"
                + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    Connection.Send(Connection.Command.Hero_Impacts_Add, Name + "\t" + NextImpact.GetData());
            }
            finally { Impacts_Locker.ExitReadLock(); }

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    Connection.Send(Connection.Command.Hero_Marks_Add, Name + "\t" + NextMark.GetData());
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public override void Broadcast_Leave()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Remove, Name);
        }

        public override void Broadcast_Energy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_SetEnergyForced, Name + "\t" + (int)Energy);
        }

        public override void Broadcast_EnergyModify(Unit Caster)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_SetEnergy, Name + "\t" + (int)Energy + "\t" + Caster.Name);
        }

        public override void Broadcast_MaxEnergy()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_SetMaxEnergy, Name + "\t" + (int)MaxEnergy);
        }

        public override void Broadcast_Location()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_SetPosition, Name + "\t" + (int)Location.X + "\t" + (int)Location.Y + "\t" + Rotation + "\t" + (Status_Rooted <= 0 ? Moving == false ? 0 : Speed : 0));
        }

        public override void Broadcast_ItemLevel()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_SetItemLevel, Name + "\t" + ItemLevel);
        }

        public override void Broadcast_MarksAdd(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Marks_Add, Name + "\t" + Mark.GetData());
        }

        public override void Broadcast_MarkStack(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Marks_SetStack, Name + "\t" + Mark.Effect_ID + "\t" + Mark.Stack);
        }

        public override void Broadcast_MarkDuration(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Marks_SetDuration, Name + "\t" + Mark.Effect_ID + "\t" + Mark.Periods[Mark.Effect_ID / 6, Mark.Effect_ID % 6] * Mark.Intervals[Mark.Effect_ID / 6, Mark.Effect_ID % 6, Mark.Stack - 1] * 1000 * Mark.Multiplier);
        }

        public override void Broadcast_MarksRemove(Mark Mark)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Marks_Remove, Name + "\t" + Mark.ID);
        }

        public override void Broadcast_ImpactsAdd(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Impacts_Add, Name + "\t" + Impact.GetData());
        }

        public override void Broadcast_ImpactsRemove(Impact Impact)
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Impacts_Remove, Name + "\t" + Impact.ID);
        }

        public override void Broadcast_Clear()
        {
            Area.BroadcastCommand(Region, Connection.Command.Hero_Clear, Name);
        }
        #endregion

        private static WayPoint[] Schedule = new WayPoint[25]
        {
            new WayPoint("oldworld\t466\t2912"),//0
            new WayPoint("oldworld\t3566\t2912"),//1
            new WayPoint("oldworld\t2016\t224"),//2
            new WayPoint("oldworld\t1360\t3149"),//3
            new WayPoint("oldworld\t2672\t2672"),//4
            new WayPoint("oldworld\t2016\t2910"),//5
            new WayPoint("oldworld\t2342\t3238"),//6
            new WayPoint("oldworld\t1688\t2584"),//7
            new WayPoint("oldworld\t2254\t1122"),//8
            new WayPoint("oldworld\t2908\t1122"),//9
            new WayPoint("oldworld\t3324\t2018"),//10
            new WayPoint("oldworld\t2670\t2018"),//11
            new WayPoint("oldworld\t2792\t1570"),//12
            new WayPoint("oldworld\t1122\t2256"),//13
            new WayPoint("oldworld\t794\t1690"),//14
            new WayPoint("oldworld\t1242\t1572"),//15
            new WayPoint("oldworld\t1688\t1448"),//16
            new WayPoint("oldworld\t1362\t884"),//17
            new WayPoint("oldworld\t1360\t2670"),//18
            new WayPoint("oldworld\t1536\t2018"),//19
            new WayPoint("oldworld\t1744\t1122"),//20
            new WayPoint("oldworld\t2254\t1600"),//21
            new WayPoint("oldworld\t2908\t2256"),//22
            new WayPoint("oldworld\t2256\t2430"),//23
            new WayPoint("oldworld\t2016\t2016")//24
        };
        private static WayPoint[,] Schedule_CalltoArms = new WayPoint[4, 7]
        {
            {Schedule[3],Schedule[4],Schedule[5],Schedule[6],Schedule[7],null,null},
            {Schedule[8],Schedule[9],Schedule[10],Schedule[11],Schedule[12],null,null},
            {Schedule[13],Schedule[14],Schedule[15],Schedule[16],Schedule[17],null,null},
            {Schedule[18],Schedule[19],Schedule[20],Schedule[21],Schedule[22],Schedule[23],Schedule[24]}
        };
    }
}
