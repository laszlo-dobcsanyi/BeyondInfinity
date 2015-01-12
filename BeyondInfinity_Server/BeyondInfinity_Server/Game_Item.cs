using System;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed class Attribute
    {
        public uint ID;
        public uint Value;

        public Attribute(uint id, uint value)
        {
            ID = id;
            Value = value;
        }
    }

    public sealed class Item: Equipment
    {
        public uint Level;
        public uint School;

        public string Name;
        public string Icon;

        public List<Attribute> Attributes = new List<Attribute>();

        private static Random Random = new Random();

        public Item(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Slot = Convert.ToUInt32(Arguments[0]);
            School = Convert.ToUInt32(Arguments[1]);
            Name = Arguments[2];
            Icon = Arguments[3];

            uint Number = Convert.ToUInt32(Arguments[4]);
            for (int Current = 0; Current < Number; Current++)
            {
                Attributes.Add(new Attribute(Convert.ToUInt32(Arguments[5 + Current * 2]), Convert.ToUInt32(Arguments[6 + Current * 2])));
                Level += Attributes[Current].Value;
            }
        }

        public Item(Character Character)
        {
            Slot = (uint)Random.Next(8);
            School = (uint)Random.Next(6);
            Name = Slot_Prefixes[School] + " " + Slot_Names[Slot];
            Icon = "item_" + Slot_Icons[Slot] + School;

            int Attributes_Number = Random.Next(3) + 1;
            int Attributes_Value = (int)Character.Equipped[Slot].Level / Character.Equipped[Slot].Attributes.Count;
            uint[] Used_IDs = new uint[Attributes_Number];

            uint NextID;
            for (int Current = 0; Current < Attributes_Number; Current++)
            {
                bool Found;
                do
                {
                    Found = false;
                    NextID = (uint)Random.Next(7);

                    for (int CurrentID = 0; CurrentID < Current; CurrentID++)
                        if (Used_IDs[CurrentID] == NextID)
                        {
                            Found = true;
                            break;
                        }

                }
                while (Found);

                Attributes.Add(new Attribute(NextID, (uint)Random.Next((Attributes_Value - 10) < 0 ? 0 : Attributes_Value - 10, 80 < (Attributes_Value + 30) ? 80 : Attributes_Value + 30) + 1));
                Level += Attributes[Current].Value;

                Used_IDs[Current] = NextID;
            }
        }

        public Item(int Power, int Range)
        {
            Slot = (uint)Random.Next(8);
            School = (uint)Random.Next(6);
            Name = Slot_Prefixes[School] + " " + Slot_Names[Slot];
            Icon = "item_" + Slot_Icons[Slot] + School;

            int Attributes_Number = Random.Next(3) + 1;
            uint[] Used_IDs = new uint[Attributes_Number];

            uint NextID;
            for (int Current = 0; Current < Attributes_Number; Current++)
            {
                bool Found;
                do
                {
                    Found = false;
                    NextID = (uint)Random.Next(7);

                    for (int CurrentID = 0; CurrentID < Current; CurrentID++)
                        if (Used_IDs[CurrentID] == NextID)
                        {
                            Found = true;
                            break;
                        }

                }
                while (Found);

                Attributes.Add(new Attribute(NextID,(uint)(Power + Random.Next(Range))));
                Level += Attributes[Current].Value;

                Used_IDs[Current] = NextID;
            }
        }

        public Item(int slot, int school, uint level)
        {
            Slot = (uint)slot;
            School = (uint)school;
            Name = Slot_Prefixes[School] + " " + Slot_Names[Slot];
            Icon = "item_" + Slot_Icons[Slot] + School;

            int Attributes_Number = Random.Next(3) + 1;
            int Attributes_Value = (int)level / Attributes_Number;
            uint[] Used_IDs = new uint[Attributes_Number];

            uint NextID;
            for (int Current = 0; Current < Attributes_Number; Current++)
            {
                bool Found;
                do
                {
                    Found = false;
                    NextID = (uint)Random.Next(7);

                    for (int CurrentID = 0; CurrentID < Current; CurrentID++)
                        if (Used_IDs[CurrentID] == NextID)
                        {
                            Found = true;
                            break;
                        }

                }
                while (Found);

                Attributes.Add(new Attribute(NextID, (uint)Random.Next((Attributes_Value - 10) < 0 ? 0 : Attributes_Value - 10, Attributes_Value + 10)));
                Level += Attributes[Current].Value;

                Used_IDs[Current] = NextID;
            }
        }

        public Item(uint slot, uint school, uint[] attributes)
        {
            Slot = slot;
            School = school;
            Name = Slot_Prefixes[School] + " " + Slot_Names[Slot];
            Icon = "item_" + Slot_Icons[Slot] + School;

            for (int Current = 0; Current < attributes.Length; Current++)
            {
                Attributes.Add(new Attribute(attributes[Current], (uint)(Random.Next(80) + 1)));
                Level += Attributes[Current].Value;
            }
        }

        public void Activate(Hero Hero)
        {
            foreach (Attribute NextAttribute in Attributes)
                switch (NextAttribute.ID)
                {
                    case 0: Hero.Global_Accuracy += NextAttribute.Value; break;
                    case 1: Hero.Global_ClearcastChance += NextAttribute.Value; break;
                    case 2: Hero.Global_Haste += NextAttribute.Value; break;
                    case 3: Hero.Global_Power += NextAttribute.Value; break;
                    case 4: Hero.Global_Resistance += NextAttribute.Value; break;
                    case 5: Hero.MaxEnergy += NextAttribute.Value; Hero.Energy += NextAttribute.Value; break;
                    case 6: Hero.Speed += NextAttribute.Value / 25; break;
                }

            Hero.ItemLevel += Level;
        }

        public void Deactivate(Hero Hero)
        {
            foreach (Attribute NextAttribute in Attributes)
                switch (NextAttribute.ID)
                {
                    case 0: Hero.Global_Accuracy -= NextAttribute.Value; break;
                    case 1: Hero.Global_ClearcastChance -= NextAttribute.Value; break;
                    case 2: Hero.Global_Haste -= NextAttribute.Value; break;
                    case 3: Hero.Global_Power -= NextAttribute.Value; break;
                    case 4: Hero.Global_Resistance -= NextAttribute.Value; break;
                    case 5: Hero.MaxEnery_Modify(-NextAttribute.Value); Hero.Energy_Modify(-NextAttribute.Value); break;
                    case 6: Hero.Speed_Modify(-NextAttribute.Value / 25); break;
                }

            Hero.ItemLevel -= Level;
        }

        public override string GetData()
        {
            string Data = Slot + "\t"+School+"\t" + Name + "\t" + Icon + "\t" + Attributes.Count;
            foreach (Attribute NextAttribute in Attributes)
                Data += "\t" + NextAttribute.ID + "\t" + NextAttribute.Value;
            return Data;
        }

        private string[] Slot_Prefixes = new string[6] { "Molten", "Nether", "Frozen", "Wild", "Shade", "Redemption" };

        private string[] Slot_Names = new string[9] { "Staff", "Wand", "Helmet", "Mantle", "Robe", "Gloves", "Pants", "Boots", "Glyph" };

        private string[] Slot_Icons = new string[8] { "weapon", "offhand", "head", "shoulders", "chest", "hands", "legs", "feet" };
    }
}
 