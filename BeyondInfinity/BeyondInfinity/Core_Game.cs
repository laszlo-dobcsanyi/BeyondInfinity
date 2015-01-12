using System;

namespace BeyondInfinity
{
    public static partial class Game
    {
        static Game()
        {
            Battlefields[0] = new Battlefield(5);
            Battlefields[0].Flags[0] = Flags[0]; Flags[0].Battlefield = Battlefields[0];
            Battlefields[0].Flags[1] = Flags[1]; Flags[1].Battlefield = Battlefields[0];
            Battlefields[0].Flags[2] = Flags[2]; Flags[2].Battlefield = Battlefields[0];
            Battlefields[0].Flags[3] = Flags[3]; Flags[3].Battlefield = Battlefields[0];
            Battlefields[0].Flags[4] = Flags[4]; Flags[4].Battlefield = Battlefields[0];
            Battlefields[0].CalculateFlags();

            Battlefields[1] = new Battlefield(5);
            Battlefields[1].Flags[0] = Flags[5]; Flags[5].Battlefield = Battlefields[1];
            Battlefields[1].Flags[1] = Flags[6]; Flags[6].Battlefield = Battlefields[1];
            Battlefields[1].Flags[2] = Flags[7]; Flags[7].Battlefield = Battlefields[1];
            Battlefields[1].Flags[3] = Flags[8]; Flags[8].Battlefield = Battlefields[1];
            Battlefields[1].Flags[4] = Flags[9]; Flags[9].Battlefield = Battlefields[1];
            Battlefields[1].CalculateFlags();

            Battlefields[2] = new Battlefield(5);
            Battlefields[2].Flags[0] = Flags[10]; Flags[10].Battlefield = Battlefields[2];
            Battlefields[2].Flags[1] = Flags[11]; Flags[11].Battlefield = Battlefields[2];
            Battlefields[2].Flags[2] = Flags[12]; Flags[12].Battlefield = Battlefields[2];
            Battlefields[2].Flags[3] = Flags[13]; Flags[13].Battlefield = Battlefields[2];
            Battlefields[2].Flags[4] = Flags[14]; Flags[14].Battlefield = Battlefields[2];
            Battlefields[2].CalculateFlags();

            Battlefields[3] = new Battlefield(7);
            Battlefields[3].Flags[0] = Flags[15]; Flags[15].Battlefield = Battlefields[3];
            Battlefields[3].Flags[1] = Flags[16]; Flags[16].Battlefield = Battlefields[3];
            Battlefields[3].Flags[2] = Flags[17]; Flags[17].Battlefield = Battlefields[3];
            Battlefields[3].Flags[3] = Flags[18]; Flags[18].Battlefield = Battlefields[3];
            Battlefields[3].Flags[4] = Flags[19]; Flags[19].Battlefield = Battlefields[3];
            Battlefields[3].Flags[5] = Flags[20]; Flags[20].Battlefield = Battlefields[3];
            Battlefields[3].Flags[6] = Flags[21]; Flags[21].Battlefield = Battlefields[3];
            Battlefields[3].CalculateFlags();
        }

        private static DateTime LastUpdate = DateTime.Now;
        public static void Update()
        {
            TimeSpan ElapsedTime = DateTime.Now - LastUpdate;
            LastUpdate = DateTime.Now;

            if (Character != null)
            {
                Character.Update(ElapsedTime.TotalMilliseconds);
                foreach (Spell NextSpell in Character.Spells)
                    if (NextSpell != null)
                        NextSpell.Update(ElapsedTime.TotalMilliseconds);

                if (Character.Speed != 0)
                {
                    if (Game.Corpse_Looting != null)
                    {
                        Game.Corpse_Looting.Looting = false;
                        Game.Corpse_Looting = null;
                    }
                    Character.ClearLoot();
                }
            }

            for (int Current = Messages_Count - 1; 0 <= Current; Current--)
            {
                Messages[Current].Elapsed += ElapsedTime.TotalMilliseconds;
                if (3000 < Messages[Current].Elapsed)
                {
                    for (int Previous = Current + 1; Previous < Messages_Count; Previous++)
                        Messages[Previous - 1] = Messages[Previous];
                    Messages_Count--;
                }
            }

            Heroes_Locker.EnterReadLock();
            try
            {
                foreach (Hero NextHero in Heroes)
                    NextHero.Update(ElapsedTime.TotalMilliseconds);
            }
            finally { Heroes_Locker.ExitReadLock(); }

            Persons_Locker.EnterReadLock();
            try
            {
                foreach (Person NextPerson in Persons)
                    NextPerson.Update(ElapsedTime.TotalMilliseconds);
            }
            finally { Persons_Locker.ExitReadLock(); }

            Creatures_Locker.EnterReadLock();
            try
            {
                foreach (Creature NextCreature in Creatures)
                    NextCreature.Update(ElapsedTime.TotalMilliseconds);

            }
            finally { Creatures_Locker.ExitReadLock(); }

            Missiles_Locker.EnterReadLock();
            try
            {
                foreach (Missile NextMissile in Missiles)
                    NextMissile.Update(ElapsedTime.TotalMilliseconds);
            }
            finally { Missiles_Locker.ExitReadLock(); }

            Splashes_Locker.EnterReadLock();
            try
            {
                foreach (Splash NextSplash in Splashes)
                    NextSplash.Update(ElapsedTime.TotalMilliseconds);
            }
            finally { Splashes_Locker.ExitReadLock(); }
        }

        public static string[] Chat_Left = new string[7];
        public static string[] Chat_Right = new string[7];

        public static string Chat_LeftString;
        public static string Chat_RightString;

        public static void Chat_AddLeft(string Message)
        {
            for (int Current = 6; 0 < Current; Current--)
                Chat_Left[Current] = Chat_Left[Current - 1];
            Chat_Left[0] = Message;

            Chat_LeftString = Chat_Left[0];
            for (int Current = 1; Current < 7; Current++)
                Chat_LeftString += "\n" + Chat_Left[Current];
        }

        public static void Chat_AddRight(string Message)
        {
            for (int Current = 6; 0 < Current; Current--)
                Chat_Right[Current] = Chat_Right[Current - 1];
            Chat_Right[0] = Message;

            Chat_RightString = Chat_Right[0];
            for (int Current = 1; Current < 7; Current++)
                Chat_RightString += "\n" + Chat_Right[Current];
        }

        public static int Messages_Count = 0;
        public static Message[] Messages = new Message[5];

        public static void Messages_Add(int Number)
        {
            if (Messages_Count < 5)
            {
                Messages[Messages_Count] = new Message(Number);
                Messages_Count++;
            }
            else
            {
                for (int Current = 0; Current < 4; Current++)
                    Messages[Current] = Messages[Current + 1];

                Messages[4] = new Message(Number);
                Messages_Count = 5;
            }
        }

        public static void Dispose(bool NewInstance)
        {
            Program.GameForm.Device_Locker.EnterWriteLock();
            try
            {
                Heroes_Clear();
                Persons_Clear();
                Creatures_Clear();
                Corpses_Clear();
                Missiles_Clear();
                Splashes_Clear();

                Portals.Clear();

                if (Target != Game.Character) Target = null;

                if (NewInstance)
                {
                    Arena = false;

                    Chat_Left = new string[10];
                    Chat_LeftString = "";
                    Chat_Right = new string[10];
                    Chat_RightString = "";

                    Character = null;
                    Program.Loaded = 0;
                }

                GC.Collect();
            }
            finally { Program.GameForm.Device_Locker.ExitWriteLock(); }
        }

        public class Message
        {

            public string Text;
            private string[] Texts = new string[3]{"Out of Range!", "Clearcast!", "Miss!"};
            public double Elapsed = 0;

            public Message(int number)
            {
                Text = Texts[number];
            }
        }
    }
}
