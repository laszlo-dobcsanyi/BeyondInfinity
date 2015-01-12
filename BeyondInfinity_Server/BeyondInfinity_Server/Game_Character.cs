using System;
using System.IO;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Character : Hero
    {
        public bool Status_Teleporting;
        public Connection Connection;

        public byte[,] Dialogs = new byte[64, 2];

        public Character(string name, Connection connection)
        {
            Name = name;
            Connection = connection;
         // try
            {
                StreamReader HeroFile = new StreamReader(@"data\heroes\" + Name + ".data");

                FactionID = Convert.ToUInt32(HeroFile.ReadLine());
                IconID = Convert.ToUInt32(HeroFile.ReadLine());
                Energy = Convert.ToDouble(HeroFile.ReadLine());
                MaxEnergy = 5000;
                Reputation = Convert.ToInt32(HeroFile.ReadLine());

                Connection.Send(Connection.Command.Character_Data, Name + "\t" + FactionID + "\t" + IconID + "\t" + ItemLevel + "\t" + Energy + "\t" + MaxEnergy + "\t" 
                    + Location.X + "\t" + Location.Y + "\t" + Rotation + "\t" + (Moving == false ? 0 : Speed));

                Spells = new Spell[6];
                for (uint Current = 0; Current < 6; Current++)
                    Spells_Add(new Spell(this,HeroFile.ReadLine()));

                int Number = Convert.ToInt32(HeroFile.ReadLine());
                for (int Current = 0; Current < Number; Current++)
                    Equipped_Add(new Item(HeroFile.ReadLine()));
                Calculate_SchoolPowers();

                Number = Convert.ToInt32(HeroFile.ReadLine());
                for (int Current = 0; Current < Number; Current++)
                {
                    string[] Data = HeroFile.ReadLine().Split(new char[] { '\t' }, 2);
                    uint Slot_Number = Convert.ToUInt32(Data[0]);
                    Backpack_Add(Slot_Number, Equipment.GetEquipment(this,Data[1]));
                }

                HeroFile.Close();
            }
          /*catch (Exception E)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\tCharacter {0} ! Error while loading :\n{1}", Name, E.Message);
            }*/

            Character Other = GameManager.GetCharacter(Name);
            if (Other != null) Other.Connection.Shutdown();

            Rotation = 0;
            Speed = 80;
            Moving = false;

            Group = new Group(this);
            GameManager.Groups_Add(Group);

            GameManager.Factions[FactionID].Heroes_Add(this);
            GameManager.TeleportHero(this);

            Connection.Send(Connection.Command.Flag_SetOwner, GameManager.GetFlagData());
            Connection.Send(Connection.Command.CalltoArms_SetPreTime, GameManager.CallToArms + "\t" + GameManager.EventStartTime);
            Connection.Send(Connection.Command.Character_SetReputation, Reputation.ToString());

            foreach (Item NextItem in Equipped)
                if (NextItem != null)
                    NextItem.Activate(this);
            Connection.Send(Connection.Command.Hero_SetItemLevel, Name + "\t" + ItemLevel);
        }

        public void Status_Teleport()
        {
            Status_Muted += 128;
            Status_Rooted += 128;

            Character Character = this as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Character_Mute, ".");

            Broadcast_Location();
        }

        public void Status_Teleported()
        {
            Status_Muted -= 128;
            Status_Rooted -= 128;

            if (Status_Muted <= 0)
            {
                Character Character = this as Character;
                if (Character != null) Character.Connection.Send(Connection.Command.Character_UnMute, ".");
            }

            Broadcast_Location();
        }
    }
}
