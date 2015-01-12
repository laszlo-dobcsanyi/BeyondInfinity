using System;
using System.Timers;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Arena : Area
    {
        public Group Team1;
        public uint Team1_Number;

        public Group Team2;
        public uint Team2_Number;

        public Arena(Group team1, Group team2)
            : base("arena")
        {
            DateTime StartTime = DateTime.Now.AddSeconds(30);

            Team1 = team1;
            Team1_Number = Team1.Characters_Number;

            Team1.Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Team1.Characters)
                {
                    NextCharacter.Area.Characters_Remove(NextCharacter);

                    NextCharacter.Moving = false;
                    NextCharacter.Location = new Point(860, 270);

                    Characters_Add(NextCharacter);

                    NextCharacter.Connection.Send(Connection.Command.Chat, "s:<<ARENA>> Starting in 30 seconds..\n");
                    NextCharacter.Connection.Send(Connection.Command.Arena_Enter, StartTime.ToString());
                }
            }
            finally { Team1.Characters_Locker.ExitReadLock(); }

            Team2 = team2;
            Team2_Number = Team2.Characters_Number;
            Team2.Characters_Locker.EnterReadLock();
            try
            {
                foreach (Character NextCharacter in Team2.Characters)
                {
                    NextCharacter.Area.Characters_Remove(NextCharacter);

                    NextCharacter.Moving = false;
                    NextCharacter.Location = new Point(710, 960);

                    Characters_Add(NextCharacter);

                    NextCharacter.Connection.Send(Connection.Command.Chat, "s:<<ARENA>> Starting in 30 seconds..\n");
                    NextCharacter.Connection.Send(Connection.Command.Arena_Enter, StartTime + "");
                }
            }
            finally { Team2.Characters_Locker.ExitReadLock(); }

            ArenaTimer = new Timer(10 * 1000);
            ArenaTimer.Elapsed += new ElapsedEventHandler(ArenaTimer_Elapsed);
            ArenaTimer.Start();
        }

        private byte Count = 0;
        private Timer ArenaTimer;
        private void ArenaTimer_Elapsed(object Sender, ElapsedEventArgs Event)
        {
            switch (Count)
            {
                case 0:
                    BroadcastCommand(Connection.Command.Chat, "s:<<ARENA>> Starting in 20 seconds..\n");
                    ArenaTimer.Start();
                    Count++;
                    return;

                case 1:
                    BroadcastCommand(Connection.Command.Chat, "s:<<ARENA>> Starting in 10 seconds..\n");
                    ArenaTimer.Interval = 5 * 1000;
                    ArenaTimer.Start();
                    Count++;
                    return;

                case 2:
                    BroadcastCommand(Connection.Command.Chat, "s:<<ARENA>> Starting in 5 seconds..\n");
                    ArenaTimer.Start();
                    Count++;
                    return;

                case 3:
                    BroadcastCommand(Connection.Command.Chat, "s:<<ARENA>> Let the fight begin!\n");
                    ArenaTimer.Dispose();

                    Team1.Characters_Locker.EnterReadLock();
                    try
                    {
                        foreach (Character NextCharacter in Team1.Characters)
                            if (NextCharacter.Area == this)
                                NextCharacter.Location_Set(new Point(490, 470));
                    }
                    finally { Team1.Characters_Locker.ExitReadLock(); }

                    Team2.Characters_Locker.EnterReadLock();
                    try
                    {
                        foreach (Character NextCharacter in Team2.Characters)
                            if (NextCharacter.Area == this)
                                NextCharacter.Location_Set(new Point(1100, 1040));
                    }
                    finally { Team2.Characters_Locker.ExitReadLock(); }
                    return;
            }
        }

        public override void Heroes_Remove(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null) Character.Connection.Send(Connection.Command.Arena_Leave, "!");
            base.Heroes_Remove(Hero);

            if (Hero.Group == Team1) Team1_Number--;
            else
            {
                if (Hero.Group == Team2) Team2_Number--;
                else Console.WriteLine("Fatal Arena error! (Not proper Groups?!)");
            }

            if (Team1_Number == 0)
            {               
                Team1.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Team1.Characters)
                        NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Leave, "!");
                }
                finally { Team1.Characters_Locker.ExitReadLock(); }

                Team2.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Team2.Characters)
                    {
                        NextCharacter.Connection.Send(Connection.Command.Arena_Leave, "!");
                        NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Leave, "!");
                        NextCharacter.Reputation_Modify(10000);

                        if (NextCharacter.Area == this)
                        {
                            NextCharacter.Status_Invulnerate();

                            NextCharacter.Clear();
                            NextCharacter.Energy_Set(NextCharacter.MaxEnergy);

                            base.Characters_Remove(NextCharacter);
                            NextCharacter.Area = null; //Needed to prevent spiral calling this method again!

                            NextCharacter.Area.Characters_Teleport(NextCharacter);

                            NextCharacter.Status_Uninvulnerate();
                        }
                    }
                }
                finally { Team2.Characters_Locker.ExitReadLock(); }

                GameManager.Arenas_Remove(this);

                Team1.InArenaQueue = false;
                Team2.InArenaQueue = false;

                return;
            }

            if (Team2_Number == 0)
            {
                Team2.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Team2.Characters)
                        NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Leave, "!");
                }
                finally { Team2.Characters_Locker.ExitReadLock(); }

                Team1.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in Team1.Characters)
                    {
                        NextCharacter.Connection.Send(Connection.Command.Arena_Leave, "!");
                        NextCharacter.Connection.Send(Connection.Command.ArenaQueue_Leave, "!");
                        NextCharacter.Reputation_Modify(10000);

                        if (NextCharacter.Area == this)
                        {
                            NextCharacter.Status_Invulnerate();

                            NextCharacter.Clear();
                            NextCharacter.Energy_Set(NextCharacter.MaxEnergy);

                            base.Characters_Remove(NextCharacter);
                            NextCharacter.Area = null; //Needed to prevent spiral calling this method again!

                            NextCharacter.Area.Characters_Teleport(NextCharacter);

                            NextCharacter.Status_Uninvulnerate();
                        }
                    }
                }
                finally { Team1.Characters_Locker.ExitReadLock(); }

                GameManager.Arenas_Remove(this);

                Team1.InArenaQueue = false;
                Team2.InArenaQueue = false;

                return;
            }
        }
    }
}
