using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Dungeon : Area
    {
        public uint GroupID;
        public uint Heroes_Cap;

        public Dungeon(Hero Hero)
            : base(Hero.Group)
        {
            GroupID = Hero.Group.ID;
            Heroes_Cap = Hero.Group.Characters_Number;

            Heroes_Add(Hero);

            Hero.Group.Dungeons_Locker.EnterWriteLock();
            try
            {
                Hero.Group.Dungeons.Add(this);
            }
            finally { Hero.Group.Dungeons_Locker.ExitWriteLock(); }

            Console.WriteLine("\t\t * {0} Created for Group {1}!", Name, Hero.Group.ID);
        }

        public Dungeon(string areaname,Hero Hero)
            : base(Hero.Group,areaname)
        {
            GroupID = Hero.Group.ID;
            Heroes_Cap = Hero.Group.Characters_Number;

            Heroes_Add(Hero);

            Hero.Group.Dungeons_Locker.EnterWriteLock();
            try
            {
                Hero.Group.Dungeons.Add(this);
            }
            finally { Hero.Group.Dungeons_Locker.ExitWriteLock(); }

            Console.WriteLine("\t\t * {0} Loaded for Group {1}!", Name, Hero.Group.ID);
        }

        public override void Heroes_Add(Hero Hero)
        {
            if (Heroes_Number == Heroes_Cap) GameManager.TeleportHero(Hero);
            else base.Heroes_Add(Hero);
        }

        public override void Heroes_Remove(Hero Hero)
        {
            base.Heroes_Remove(Hero);

            /*Hero.Group.Dungeons_Locker.EnterWriteLock();
              try
              {
                  Hero.Group.Dungeons.Remove(this);
              }
              finally { Hero.Group.Dungeons_Locker.ExitWriteLock(); }*/

            if ((Heroes_Number == 0) && (Creatures_Number == 0)) GameManager.Dungeons_Remove(this);
        }
    }
}
