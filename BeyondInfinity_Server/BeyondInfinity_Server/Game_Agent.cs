using System;
using System.IO;
using System.Drawing;

namespace BeyondInfinity_Server
{
    public sealed class Agent : Hero
    {
        private static Generator ID_Generator = new Generator(Program.AGENTS_NUMBER*3);
        public Mind Mind;

        public Agent(uint factionid)
        {
            ID = ID_Generator.Next();
            int School = Random.Next(6);

            Name = "#Bot " + ID + " (" + School + ")#";
            Mind = new Mind(this, true);

            string AreaName = "";
            AreaName = "underbog" + factionid;
            FactionID = factionid;
            IconID = (uint)Random.Next(24);
            Energy = 5000;
            MaxEnergy = 5000;
            Reputation = 0;
            Location = new Point(3350, 1300);

            Spells = new Spell[6];
            for (uint Current = 0; Current < 6; Current++)
            {
                string Data = "200\t200\t200\t" + ((Current < 4 ? Current : Current + 2) * 6 + School) + "\t200\t200\t200";
                Spells_Add(new Spell(this,Data));
            }

            Calculate_SchoolPowers();

            Rotation = 0;
            Speed = 50;
            Moving = false;

            Group = new Group(this);
            GameManager.Groups_Add(Group);

            GameManager.Factions[FactionID].Heroes_Add(this);
            GameManager.Keeps[FactionID].Heroes_Add(this);
        }

        public override void Stuck()
        {
            base.Stuck();
            Console.WriteLine("{0} Stuck! Calculating New Path..", Name);
            Mind.NewPath();
        }

        public override void Location_Set(PointF location)
        {
            base.Location_Set(location);
            Mind.NextPath();
        }
    }
}
