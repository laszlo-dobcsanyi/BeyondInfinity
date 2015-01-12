using System;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Attribute
    {
        public uint ID;
        public uint Value;

        public Attribute(uint id, uint value)
        {
            ID = id;
            Value = value;
        }
    }

    public class Item : Equipment
    {
        public uint School;
        public uint Level;

        public Attribute[] Attributes;

        public Item(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Slot = Convert.ToUInt32(Arguments[1]);
            School = Convert.ToUInt32(Arguments[2]);
            Name = Arguments[3];
            Icon = TextureLoader.FromFile(Program.GameForm.Device, @"data\icons\" + Arguments[4] + ".png");

            int Number = Convert.ToInt32(Arguments[5]);
            Attributes = new Attribute[Number];
            for (int Current = 0; Current < Number; Current++)
            {
                Attributes[Current] = new Attribute(Convert.ToUInt32(Arguments[6 + Current * 2]), Convert.ToUInt32(Arguments[7 + Current * 2]));
                Level += Attributes[Current].Value;
            }
        }

        public void Activate()
        {
            foreach (Attribute NextAttribute in Attributes)
                switch (NextAttribute.ID)
                {
                    case 0: Game.Character.Global_Accuracy += (int)NextAttribute.Value; break;
                    case 1: Game.Character.Global_ClearcastChance += (int)NextAttribute.Value; break;
                    case 2: Game.Character.Global_Haste += (int)NextAttribute.Value; break;
                    case 3: Game.Character.Global_Power += (int)NextAttribute.Value; break;
                    case 4: Game.Character.Global_Resistance += (int)NextAttribute.Value; break;
                }
        }

        public void Deactivate()
        {
            foreach (Attribute NextAttribute in Attributes)
                switch (NextAttribute.ID)
                {
                    case 0: Game.Character.Global_Accuracy -= (int)NextAttribute.Value; break;
                    case 1: Game.Character.Global_ClearcastChance -= (int)NextAttribute.Value; break;
                    case 2: Game.Character.Global_Haste -= (int)NextAttribute.Value; break;
                    case 3: Game.Character.Global_Power -= (int)NextAttribute.Value; break;
                    case 4: Game.Character.Global_Resistance -= (int)NextAttribute.Value; break;
                }
        }

    }
}
