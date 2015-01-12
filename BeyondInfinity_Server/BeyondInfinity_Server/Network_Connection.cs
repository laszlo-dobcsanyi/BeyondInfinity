using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;

namespace BeyondInfinity_Server
{
    public delegate void CommandHandler(int Command, string Data);

    public sealed class Connection
    {
        private Character Character;

        private Socket Gateway;
        private int Port;

        private IPAddress ClientAddress;
        private int ClientPort;

        private CommandHandler CommandHandler;

        private string CharacterName;
        public Connection(IPAddress clientaddress, int port, string charactername)
        {
            ClientAddress = clientaddress;
            Port = port;
            CharacterName = charactername;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("\t * New Connection @ {0} for {1}!", Port, ClientAddress);

            Gateway = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Gateway.Bind(new IPEndPoint(NetworkManager.ServerAddress, Port));
            //Gateway.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontFragment, true);
            Gateway.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            //Gateway.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName., true);
            Gateway.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            try
            {
                EndPoint Sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] Data = new byte[256];
                int ReceivedBytes = Gateway.ReceiveFrom(Data, ref Sender);

                ClientPort = ((IPEndPoint)Sender).Port;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\t ->Connection from {0}:{1}!", ((IPEndPoint)Sender).Address, ((IPEndPoint)Sender).Port);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t ! No Connection from {0}!", ClientAddress);
                Shutdown();
                return;
            }

            CommandHandler += ProcessConnectionCommand;

            Thread = new Thread(new ThreadStart(Receive));
            Thread.Start();
        }

        private Thread Thread;
        private void Receive()
        {
            while (true)
            {
                try
                {
                    EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);

                    byte[] Data = new byte[256];
                    int ReceivedBytes = Gateway.ReceiveFrom(Data, ref EndPoint);

                    if (((IPEndPoint)EndPoint).Address.Equals(ClientAddress))
                    {
                        string[] Arguments = Encoding.Unicode.GetString(Data, 0, ReceivedBytes).Split(':');

                        if (Arguments.Length == 2)
                            ProcessConnectionCommand(Convert.ToInt32(Arguments[0]), Arguments[1]);
                    }
                    else
                    {
                        Console.WriteLine("Data from Other IPAddress [{0}:{1}] Client IPAddress is [{2}:{3}]!",
                            ((IPEndPoint)EndPoint).Address, ((IPEndPoint)EndPoint).Port, ClientAddress, ClientPort);
                    }
                }
                catch
                {
                    Shutdown();
                    break;
                }
            }
        }

        private void ProcessConnectionCommand(int Command, string Data)
        {
            string[] Arguments = Data.Split('\t');

            Hero Target;
            int Number;
            int Rank;

            switch (Command)
            {
                case 0:
                    //Shutdown();
                    return;

                case 1:
                    Character = new Character(CharacterName, this);
                    return;
            }

            if (Character.Area != null)
                switch (Command)
                {
                    //Spell - Missile
                    case 12:
                        if (Character.Status_Muted <= 0)
                        {
                            Number = Convert.ToInt32(Arguments[0]);
                            Rank = Convert.ToInt32(Arguments[1]);

                            Character.Spells[Number].Trigger(Rank, Convert.ToDouble(Arguments[2]));
                            //Character.Spells[Number].Cooldown_Set(Rank);

                            //Character.Spells[Number - Number % 2].Cooldown_Set(Rank);
                            //Character.Spells[Number - Number % 2 + 1].Cooldown_Set(Rank);
                        }
                        break;

                    //Spell - Impact (Hero)3
                    case 13:
                        if (Character.Status_Muted <= 0)
                        {
                            Number = Convert.ToInt32(Arguments[0]);
                            Rank = Convert.ToInt32(Arguments[1]);

                            Target = Character.Area.Heroes_Get(Character.Region, Arguments[2]);
                            if (Target != null)
                                Character.Spells[Number].Trigger(Rank, Target);
                            //Character.Spells[Number].Cooldown_Set(Rank);


                            //Character.Spells[Number - Number % 2].Cooldown_Set(Rank);
                            //Character.Spells[Number - Number % 2 + 1].Cooldown_Set(Rank);
                        }
                        break;

                    //Spell - Impact (Person)
                    case 14:
                        Person TargetPerson;
                        if (Character.Status_Muted <= 0)
                        {
                            Number = Convert.ToInt32(Arguments[0]);
                            Rank = Convert.ToInt32(Arguments[1]);

                            TargetPerson = Character.Area.Persons_Get(Character.Region, Convert.ToUInt32(Arguments[2]));
                            if (TargetPerson != null)
                                Character.Spells[Number].Trigger(Rank, TargetPerson);
                            //Character.Spells[Number].Cooldown_Set(Rank);

                            //Character.Spells[Number - Number % 2].Cooldown_Set(Rank);
                            //Character.Spells[Number - Number % 2 + 1].Cooldown_Set(Rank);
                        }                        break;

                    //Spell - Impact (Creature)
                    case 15:
                        if (Character.Status_Muted <= 0)
                        {
                            Number = Convert.ToInt32(Arguments[0]);
                            Rank = Convert.ToInt32(Arguments[1]);

                            Creature TargetCreature = Character.Area.Creatures_Get(Character.Region, Convert.ToUInt32(Arguments[2]));
                            if (TargetCreature != null)
                                Character.Spells[Number].Trigger(Rank, TargetCreature);
                            //Character.Spells[Number].Cooldown_Set(Rank);

                            //Character.Spells[Number - Number % 2].Cooldown_Set(Rank);
                            //Character.Spells[Number - Number % 2 + 1].Cooldown_Set(Rank);
                        }
                        break;

                    //Spell - Splash
                    case 16:
                        if (Character.Status_Muted <= 0)
                        {
                            Number = Convert.ToInt32(Arguments[0]);
                            Rank = Convert.ToInt32(Arguments[1]);

                            Character.Spells[Number].Trigger(Rank, new PointF(Convert.ToSingle(Arguments[2]), Convert.ToSingle(Arguments[3])));
                            //Character.Spells[Number].Cooldown_Set(Rank);

                            //Character.Spells[Number - Number % 2].Cooldown_Set(Rank);
                            //Character.Spells[Number - Number % 2 + 1].Cooldown_Set(Rank);
                        }
                        break;

                    case 17:
                        Character.Communicate(Data);
                        break;

                    case 18:
                        TargetPerson = Character.Area.Persons_Get(Character.Region, Convert.ToUInt32(Arguments[0]));
                        if (TargetPerson != null)
                            Send(Connection.Command.Dialog, TargetPerson.Dialog(Character, Convert.ToByte(Arguments[1])));
                        break;

                    case 20:
                        Target = Character.Area.Heroes_Get(Character.Region, Data);
                        if (Target != null)
                            if (Target.Group.Characters.Count == 1)
                                Character.Group.Heroes_Add(Target);
                        break;

                    case 21:
                        Target = Character.Area.Heroes_Get(Character.Region, Data);
                        if (Target != null)
                            Character.Group.Heroes_Remove(Target);
                        break;

                    case 22:
                        GameManager.Arena_Join(Character.Group);
                        break;

                    //Loot
                    case 30:
                        Corpse Corpse = Character.Area.Corpses_Get(Character.Region, Convert.ToUInt32(Arguments[1]));
                        if (Corpse != null)
                            if (!Character.Moving)
                                Character.Backpack_Loot(Convert.ToUInt32(Arguments[0]), Corpse);
                        break;

                    //Equip
                    case 31:
                        Character.Backpack_Equip(Convert.ToUInt32(Data));
                        break;

                    //Get Loot
                    case 32:
                        Corpse = Character.Area.Corpses_Get(Character.Region, Convert.ToUInt32(Data));
                        if (Corpse != null)
                            if (Math.Sqrt(Math.Pow(Corpse.Location.X - Character.Location.X, 2) + Math.Pow(Corpse.Location.Y - Character.Location.Y, 2)) < 64)
                                Corpse.SendData(Character);
                        break;

                    //Buy Reputation Item
                    case 33:
                        int Attributes_Number = Convert.ToInt32(Arguments[2]);
                        uint[] Attributes_Data = new uint[Attributes_Number];
                        for (int Current = 0; Current < Attributes_Number; Current++)
                            {
                                Attributes_Data[Current] = Convert.ToUInt32(Arguments[3 + Current]);
                                for (int Previous = 0; Previous < Current; Previous++)
                                    if (Attributes_Data[Previous] == Attributes_Data[Current]) return;
                            }

                        if (Attributes_Number * 1000 <= Character.Reputation)
                        {
                            Character.Reputation_Modify(-Attributes_Number * 1000);
                            Character.Backpack_Add(new Item(Convert.ToUInt32(Arguments[0]), Convert.ToUInt32(Arguments[1]), Attributes_Data));
                        }
                        break;

                    //Delete
                    case 36:
                        Character.Backpack_Delete(Convert.ToUInt32(Data));
                        break;

                    case 102:
                        Character.Rotate(Convert.ToInt32(Data));
                        break;

                    case 103:
                        Character.Stop();
                        break;

                    case 155:
                        if (GameManager.CallToArms)
                        {
                            uint FlagID = Convert.ToUInt32(Data);
                            if (Character.Battlefield != null)
                            {
                                foreach (Flag NextFlag in Character.Battlefield.Flags)
                                    if (NextFlag.ID == FlagID)
                                        if (Math.Sqrt(Math.Pow(NextFlag.Location.X - Character.Location.X, 2) + Math.Pow(NextFlag.Location.Y - Character.Location.Y, 2)) < 64)
                                            NextFlag.Cap(Character);
                            }
                        }
                        break;

                    case 160:
                        Mark Mark = new Mark(Character, Character, 48, 1, 1);
                        Character.Marks_Locker.EnterWriteLock();
                        try
                        {
                            foreach(Mark NextMark in Character.Marks)
                                if (NextMark.Effect_ID == 48)
                                {
                                    Character.Broadcast_MarksRemove(NextMark);
                                    if (NextMark.EndEffect != null) NextMark.EndEffect();
                                    Character.Marks.Remove(NextMark);
                                    Mark.IDGenerator.Free(NextMark.ID);
                                    return;
                                }


                            Character.Broadcast_MarksAdd(Mark);
                            if (Mark.StartEffect != null) Mark.StartEffect();
                            Character.Marks.Add(Mark);
                        }
                        finally { Character.Marks_Locker.ExitWriteLock(); }
                        break;
                }
        }

        private uint Sent = 0;
        public enum Command
        {
            Shutdown,
            Loading,
            LoadTile,
            Loaded,
            Chat,
            Dialog,
            Message,

            Character_Data,
            Character_AddSpell,
            Character_SpellCooldown,
            Character_GlobalCooldown,
            Character_Mute,
            Character_UnMute,
            Character_SchoolPower,
            Character_SetAttribute,

            Character_GroupAdd,
            Character_GroupRemove,
            Character_GroupClear,

            Character_AddEquipped,
            Character_AddBackpack,
            Character_LootClear,
            Character_AddLoot,
            Character_Loot,
            Character_Equip,
            Character_BackpackDestroy,

            Character_SetBattlefield,
            Character_SetReputation,

            Hero_Add,
            Hero_Remove,
            Hero_SetEnergy,
            Hero_SetEnergyForced,
            Hero_SetMaxEnergy,
            Hero_SetPosition,
            Hero_SetItemLevel,

            Hero_Marks_Add,
            Hero_Marks_SetStack,
            Hero_Marks_SetDuration,
            Hero_Marks_Remove,
            Hero_Impacts_Add,
            Hero_Impacts_Remove,
            Hero_Clear,

            Creature_Add,
            Creature_Remove,
            Creature_SetEnergy,
            Creature_SetEnergyForced,
            Creature_SetMaxEnergy,
            Creature_SetPosition,
            Creature_SetItemLevel,

            Creature_Marks_Add,
            Creature_Marks_SetStack,
            Creature_Marks_SetDuration,
            Creature_Marks_Remove,
            Creature_Impacts_Add,
            Creature_Impacts_Remove,
            Creature_Clear,

            Person_Add,
            Person_Remove,
            Person_SetEnergy,
            Person_SetEnergyForced,
            Person_SetMaxEnergy,
            Person_SetPosition,
            Person_SetItemLevel,

            Person_Marks_Add,
            Person_Marks_SetStack,
            Person_Marks_SetDuration,
            Person_Marks_Remove,
            Person_Impacts_Add,
            Person_Impacts_Remove,
            Person_Clear,

            Corpse_Add,
            Corpse_Remove,

            Missile_Add,
            Missile_Remove,

            Splash_Add,
            Splash_Remove,

            Portal_Add,
            Portal_Remove,

            Flag_SetOwner,
            CalltoArms_SetPreTime,
            CalltoArms_True,
            CalltoArms_False,
            CalltoArms_CaptureFlag,

            Arena_Enter,
            Arena_Leave,
            ArenaQueue_Join,
            ArenaQueue_Leave
        }
        public void Send(Command Command, string Data)
        {
            Sent++;
            byte[] Message = System.Text.Encoding.Unicode.GetBytes((byte)Command + ":" + Data);
            //Gateway.SendTo(Message, new IPEndPoint(ClientAddress, ClientPort));
            try
            {
                if (!Terminated)
                    Gateway.BeginSendTo(Message, 0, Message.Length, SocketFlags.None, new IPEndPoint(ClientAddress, ClientPort), null, null);
            }
            catch (Exception E)
            {
                Console.WriteLine("\t ! Error while sending data: " + E.Message);
                Shutdown();
            }

            //ThreadPool.QueueUserWorkItem(new WaitCallback(SendCommand), Message);
        }

        /*private void SendCommand(object Data)
        {
            byte[] Message = (byte[])Data;
            SocketAsyncEventArgs Event = new SocketAsyncEventArgs();
            Event.SetBuffer(Message, 0, Message.Length);
            Event.RemoteEndPoint = new IPEndPoint(ClientAddress, ClientPort);

            Gateway.SendToAsync(Event);

            //Gateway.SendTo(Message, new IPEndPoint(ClientAddress, ClientPort));
        }*/

        private bool Terminated = false;
        public void Shutdown()
        {
            Terminated = true;
            if ((Character != null) && (Character.Area != null))
            {
                Character.Save();
                Character.Area.Heroes_Remove(Character);
                Character.Group.Heroes_Remove(Character);
                GameManager.Factions[Character.FactionID].Heroes_Remove(Character);
            }

            Thread.Abort();
            Gateway.Close();
            NetworkManager.PortGenerator.Free((uint)((Port - 1426) / 2));
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("\t # Connection Shut down({0})!", Sent);
        }
    }
}
