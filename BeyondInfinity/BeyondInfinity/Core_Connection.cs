using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;

namespace BeyondInfinity
{
    public static partial class Program
    {
        public static IPAddress ServerAddress;

        public static UdpClient Connection;

        public static UInt16 Port = 0;

        private static System.Timers.Timer Pinger;
        private static void Listen()
        {
            Connection = new UdpClient();
            Connection.Connect(new IPEndPoint(ServerAddress, Port));
            Connection.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024 * 128 * 256);
            byte[] Data = new byte[0];
            Connection.Send(Data, 0);

            Receiver = new Thread(new ThreadStart(Receive));
            Receiver.Start();

            Pinger = new System.Timers.Timer(2000);
            Pinger.Elapsed += new System.Timers.ElapsedEventHandler(Pinger_Elapsed);
            Pinger.Start();
        }

        private static Thread Receiver;
        private static void Receive()
        {
            while (Connection.Client.Connected)
            {
                try
                {
                    IPEndPoint Sender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] Data = Connection.Receive(ref Sender);
                    //string[] Message = Encoding.Unicode.GetString(Data).Split(new char[] { ':' }, 2);

                    //ProcessCommand((Command)Convert.ToByte(Message[0]), Message[1]);
                    if (!GameForm.Shutdown) ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessCommand), Data);
                    else break;
                }
                catch
                {
                    Program.GameForm.Shutdown = true;
                    break;
                }
            }
        }

        public static int Loaded = 0;
        public static bool Loading = true;
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
        public static uint Received = 0;
        private static void ProcessCommand(object parameter)
        {
            byte[] Packets = (byte[])parameter;
            string[] Message = Encoding.Unicode.GetString(Packets).Split(new char[] { ':' }, 2);
            Command Command = (Command)Convert.ToByte(Message[0]);
            string Data = Message[1];

            Received++;
            string[] Arguments = Data.Split('\t');

            if (!GameForm.Shutdown)
            {
                switch (Command)
                {
                    //SHUTDOWN
                    case Command.Shutdown:
                        Application.Exit();
                        break;
                    //LOGIN
                    //Port is set by login server, that means Authenticated
                    /* case 1:
                           if (Data == "OK") Authenticated = true;
                           else Authenticated = false;
                           break;*/
                    //LOADING
                    case Command.Loading:
                        Loading = true;

                        GameForm.Map_Locker.EnterWriteLock();
                        try
                        {
                            GameForm.Phase_Loading();
                            Game.Dispose(false);

                            GameForm.LoadMap(Data);
                        }
                        finally { GameForm.Map_Locker.ExitWriteLock(); }
                        break;

                    case Command.LoadTile:
                        //if (Data != "Arena") Game.Arena = false;
                        GameForm.LoadRegion(Data);
                        break;

                    case Command.Loaded:
                        Loading = false;
                        break;

                    case Command.Character_Data:
                        Game.Character = new Character(Data);
                        break;

                    //Add Spell
                    case Command.Character_AddSpell:
                        while (Program.Loaded < 1) System.Threading.Thread.Sleep(5);
                        Game.Character.Spells[Convert.ToUInt32(Arguments[0])] = new Spell(Data);
                        Program.Loaded++;
                        break;

                    //Cooldown of Spell
                    case Command.Character_SpellCooldown:
                        int SpellSlot = Convert.ToInt32(Arguments[0]);
                        Game.Character.Spells[SpellSlot].Cooldown_Set(Convert.ToSingle(Arguments[1]));
                        Game.Character.Spells[SpellSlot].RandomBonusRank = Convert.ToInt32(Arguments[2]);
                        break;

                    //Global Cooldown
                    case Command.Character_GlobalCooldown:
                        float Cooldown = Convert.ToSingle(Data);
                        foreach (Spell NextSpell in Game.Character.Spells)
                            if (NextSpell.Cooldown < Cooldown) NextSpell.Cooldown_Set(Cooldown);
                        break;

                    case Command.Character_Mute:
                        Game.Character.Muted = true;
                        break;

                    case Command.Character_UnMute:
                        Game.Character.Muted = false;
                        break;

                    case Command.Character_SchoolPower:
                        for (int Current = 0; Current < 6; Current++)
                            Game.Character.Global_SchoolPowers[Current] = Convert.ToInt32(Arguments[Current]);
                        break;

                    case Command.Character_SetAttribute:
                        switch (Arguments[0])
                        {
                            case "0": Game.Character.Global_Accuracy = Convert.ToDouble(Arguments[1]); break;
                            case "1": Game.Character.Global_ClearcastChance = Convert.ToDouble(Arguments[1]); break;
                            case "2": Game.Character.Global_Haste = Convert.ToDouble(Arguments[1]); break;
                            case "3": Game.Character.Global_Power = Convert.ToDouble(Arguments[1]); break;
                            case "4": Game.Character.Global_Resistance = Convert.ToDouble(Arguments[1]); break;
                        }
                        break;

                    case Command.Chat:
                        string[] Messages = Data.Split(new char[] { ':' }, 2);
                        switch (Messages[0])
                        {
                            case "f": Game.Chat_AddLeft("Faction> " + Messages[1]); break;
                            case "b": Game.Chat_AddLeft("Battle> " + Messages[1]); break;
                            case "a": Game.Chat_AddLeft(Messages[1]); break;
                            case "g": Game.Chat_AddRight("Group> " + Messages[1]); break;
                            case "s": Game.Chat_AddRight(Messages[1]); break;
                        }
                        break;

                    case Command.Dialog:
                        GameForm.OpenDialog(Data);
                        break;

                    case Command.Message:
                        Game.Messages_Add(Convert.ToInt32(Data));
                        break;

                    case Command.Character_GroupAdd:
                        Game.Character.Group.Add(Data);
                        break;

                    case Command.Character_GroupRemove:
                        Game.Character.Group.Remove(Data);
                        break;

                    case Command.Character_GroupClear:
                        Game.Character.Group.Clear();
                        break;

                    //Add Equipped Item
                    case Command.Character_AddEquipped:
                        Game.Character.Equipped_Add(new Item(Data));
                        break;

                    //Add Backpack Item
                    case Command.Character_AddBackpack:
                        Game.Character.Backpack_Add(Convert.ToUInt32(Arguments[0]), Equipment.GetEquipment(Data));
                        break;

                    //Clear Loot Items
                    case Command.Character_LootClear:
                        Game.Character.ClearLoot();
                        break;

                    //Add Loot Equipment
                    case Command.Character_AddLoot:
                        Game.Character.Loot_Add(Convert.ToUInt32(Arguments[0]), Equipment.GetEquipment(Data));
                        break;

                    //Loot
                    case Command.Character_Loot:
                        Game.Character.Loot(Convert.ToUInt32(Arguments[0]), Convert.ToUInt32(Arguments[1]));
                        break;

                    //Equip
                    case Command.Character_Equip:
                        Game.Character.Equip(Convert.ToUInt32(Data));
                        break;

                    //Destroy Backpack Item
                    case Command.Character_BackpackDestroy:
                        Game.Character.Delete(Convert.ToUInt32(Data));
                        break;

                    case Command.Character_SetBattlefield:
                        Game.Battlefield = Convert.ToInt32(Data);
                        break;

                    case Command.Character_SetReputation:
                        Game.Character.Reputation = Convert.ToInt32(Data);
                        break;


                    case Command.Hero_Add:
                        Hero Target = new Hero(Data);

                        if (Game.Character.Name != Target.Name) Game.Heroes_Add(Target);
                        else
                        {
                            Game.Character.Impacts_Clear();
                            Game.Character.Marks_Clear();
                            Game.Character.Invisible = false;
                        }
                        break;

                    case Command.Hero_Remove:
                        if (Data != Game.Character.Name) Game.Heroes_Remove(Data);
                        else Game.Character.Invisible = true;
                        break;

                    case Command.Hero_SetEnergyForced:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Hero_SetMaxEnergy:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.MaxEnergy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Hero_SetPosition:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                        {
                            Target.Location = new System.Drawing.PointF(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                            Target.Rotation = Convert.ToInt32(Arguments[3]);
                            Target.Speed = Convert.ToDouble(Arguments[4]);
                        }
                        break;

                    case Command.Hero_SetEnergy:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            if (Target == Game.Character)
                                Target.Energy_Set(Convert.ToInt32(Arguments[1]));
                            else
                                if (Arguments[2] == Game.Character.Name) Target.Energy_Set(Convert.ToInt32(Arguments[1]));
                                else Target.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Hero_SetItemLevel:
                        Target = Game.Heroes_Get(Arguments[0]);
                        if (Target != null)
                            Target.ItemLevel = (uint)(Convert.ToUInt32(Arguments[1]) * 0.6);
                        break;

                    case Command.Hero_Marks_Add:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.Marks_Add(new Mark(Data));
                        break;

                    case Command.Hero_Marks_SetStack:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                        {
                            Mark TargetMark = Target.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                            {
                                int CurrentStack = TargetMark.Stack;
                                TargetMark.Stack = Convert.ToInt32(Arguments[2]);
                                if (Arguments[0] == Game.Character.Name)
                                    TargetMark.Effect_StackModify(TargetMark.Stack - CurrentStack);
                            }
                        }
                        break;

                    case Command.Hero_Marks_SetDuration:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                        {
                            Mark TargetMark = Target.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                                TargetMark.Duration_Set(Convert.ToDouble(Arguments[2]));
                        }
                        break;

                    case Command.Hero_Marks_Remove:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.Marks_Remove(Convert.ToUInt32(Arguments[1]));
                        break;

                    case Command.Hero_Clear:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                        {
                            Target.Marks_Clear();
                            Target.Impacts_Clear();
                        }
                        break;

                    case Command.Hero_Impacts_Add:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.Impacts_Add(new Impact(Data));
                        break;

                    case Command.Hero_Impacts_Remove:
                        Target = Game.Heroes_Get(Arguments[0]);

                        if (Target != null)
                            Target.Impacts_Remove(Convert.ToUInt32(Arguments[1]));

                        break;


                    case Command.Person_Add:
                        Game.Persons_Add(new Person(Data));
                        break;

                    case Command.Person_Remove:
                        Game.Persons_Remove(Convert.ToUInt32(Data));
                        break;

                    case Command.Person_SetEnergyForced:
                        Person Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Person_SetMaxEnergy:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.MaxEnergy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Person_SetPosition:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                        {
                            Person.Location = new System.Drawing.PointF(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                            Person.Rotation = Convert.ToInt32(Arguments[3]);
                            Person.Speed = Convert.ToDouble(Arguments[4]);
                        }
                        break;

                    case Command.Person_SetEnergy:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            if (Arguments[2] == Game.Character.Name) Person.Energy_Set(Convert.ToInt32(Arguments[1]));
                            else Person.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Person_SetItemLevel:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.ItemLevel = (uint)(Convert.ToUInt32(Arguments[1]) * 0.6);
                        break;

                    case Command.Person_Marks_Add:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.Marks_Add(new Mark(Data));
                        break;

                    case Command.Person_Marks_SetStack:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                        {
                            Mark TargetMark = Person.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                                TargetMark.Stack = Convert.ToInt32(Arguments[2]);
                        }
                        break;

                    case Command.Person_Marks_SetDuration:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                        {
                            Mark TargetMark = Person.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                                TargetMark.Duration_Set(Convert.ToDouble(Arguments[2]));
                        }
                        break;

                    case Command.Person_Marks_Remove:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.Marks_Remove(Convert.ToUInt32(Arguments[1]));
                        break;

                    case Command.Person_Clear:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                        {
                            Person.Marks_Clear();
                            Person.Impacts_Clear();
                        }
                        break;

                    case Command.Person_Impacts_Add:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.Impacts_Add(new Impact(Data));
                        break;

                    case Command.Person_Impacts_Remove:
                        Person = Game.Persons_Get(Convert.ToUInt32(Arguments[0]));

                        if (Person != null)
                            Person.Impacts_Remove(Convert.ToUInt32(Arguments[1]));
                        break;


                    case Command.Creature_Add:
                        Game.Creatures_Add(new Creature(Data));
                        break;

                    case Command.Creature_Remove:
                        Game.Creatures_Remove(Convert.ToUInt32(Data));
                        break;

                    case Command.Creature_SetEnergyForced:
                        Creature Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Creature_SetMaxEnergy:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.MaxEnergy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Creature_SetPosition:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                        {
                            Creature.Location = new System.Drawing.PointF(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
                            Creature.Rotation = Convert.ToInt32(Arguments[3]);
                            Creature.Speed = Convert.ToDouble(Arguments[4]);
                        }
                        break;

                    case Command.Creature_SetEnergy:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            if (Arguments[2] == Game.Character.Name) Creature.Energy_Set(Convert.ToInt32(Arguments[1]));
                            else Creature.Energy = Convert.ToInt32(Arguments[1]);
                        break;

                    case Command.Creature_SetItemLevel:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.ItemLevel = (uint)(Convert.ToUInt32(Arguments[1]) * 0.6);
                        break;

                    case Command.Creature_Marks_Add:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.Marks_Add(new Mark(Data));
                        break;

                    case Command.Creature_Marks_SetStack:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                        {
                            Mark TargetMark = Creature.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                                TargetMark.Stack = Convert.ToInt32(Arguments[2]);
                        }
                        break;

                    case Command.Creature_Marks_SetDuration:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                        {
                            Mark TargetMark = Creature.Marks_Get(Convert.ToUInt32(Arguments[1]));
                            if (TargetMark != null)
                                TargetMark.Duration_Set(Convert.ToDouble(Arguments[2]));
                        }
                        break;

                    case Command.Creature_Marks_Remove:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.Marks_Remove(Convert.ToUInt32(Arguments[1]));
                        break;

                    case Command.Creature_Clear:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                        {
                            Creature.Marks_Clear();
                            Creature.Impacts_Clear();
                        }
                        break;

                    case Command.Creature_Impacts_Add:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.Impacts_Add(new Impact(Data));
                        break;

                    case Command.Creature_Impacts_Remove:
                        Creature = Game.Creatures_Get(Convert.ToUInt32(Arguments[0]));

                        if (Creature != null)
                            Creature.Impacts_Remove(Convert.ToUInt32(Arguments[1]));
                        break;


                    case Command.Corpse_Add:
                        Game.Corpses_Add(new Corpse(Data));
                        break;

                    case Command.Corpse_Remove:
                        Game.Corpses_Remove(Convert.ToUInt32(Data));
                        break;


                    case Command.Missile_Add:
                        Game.Missiles_Add(new Missile(Data));
                        break;

                    case Command.Missile_Remove:
                        Game.Missiles_Remove(Convert.ToUInt32(Data));
                        break;


                    case Command.Splash_Add:
                        Game.Splashes_Add(new Splash(Data));
                        break;

                    case Command.Splash_Remove:
                        Game.Splashes_Remove(Convert.ToUInt32(Arguments[0]));
                        break;


                    case Command.Portal_Add:
                        Game.Portals.Add(new Portal(Data));
                        break;

                    case Command.Portal_Remove:
                        foreach (Portal NextPortal in Game.Portals)
                            if (NextPortal.Name == Arguments[0])
                            {
                                Game.Portals.Remove(NextPortal);
                                return;
                            }
                        break;

                    case Command.Flag_SetOwner:
                        for (int Current = 0; Current < 22; Current++)
                            Game.Flags[Current].Owner = Convert.ToInt32(Arguments[Current]);
                        break;


                    case Command.CalltoArms_SetPreTime:
                        Game.CalltoArms = Convert.ToBoolean(Arguments[0]);
                        Game.EventTimer = Convert.ToDateTime(Arguments[1]);
                        Game.Battlefield = -1;
                        break;

                    case Command.CalltoArms_True:
                        Game.CalltoArms = true;
                        Game.EventTimer = DateTime.Now;
                        break;

                    case Command.CalltoArms_False:
                        Game.CalltoArms = false;
                        Game.EventTimer = DateTime.Now;
                        break;

                    case Command.CalltoArms_CaptureFlag:
                        uint FlagID = Convert.ToUInt32(Arguments[0]);
                        Game.Flags[FlagID].Owner = Convert.ToInt32(Arguments[1]);
                        Game.Flags[FlagID].Battlefield.CalculateFlags();
                        break;

                    case Command.Arena_Enter:
                        Game.ArenaTimer = Convert.ToDateTime(Data);
                        Game.Arena = true;
                        break;

                    case Command.Arena_Leave:
                        Game.Arena = false;
                        break;

                    case Command.ArenaQueue_Join:
                        GameForm.InArenaQueue = true;
                        break;

                    case Command.ArenaQueue_Leave:
                        GameForm.InArenaQueue = false;
                        break;
                }
            }
        }

        public static void Send(byte Number, string Data)
        {
            if (Connection.Client != null)
            {
                byte[] Message = Encoding.Unicode.GetBytes((string)(Number + ":" + Data));
                Connection.Send(Message, Message.Length);
            }
        }

        private static void Pinger_Elapsed(object Sender, System.Timers.ElapsedEventArgs Event)
        {
            Send(255, ".");
        }

        public static void Disconnect()
        {
            if (Connection != null)
            {
                Receiver.Abort();

                if (Connection.Client.Connected)
                {
                    Pinger.Stop();

                    Connection.Close();
                }
            }
        }
    }
}
