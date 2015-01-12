using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Region
    {
        public Point Index;
        public uint Influence = 0;

        public Region(Area area, Point index)
        {
            Area = area;
            Index = index;
        }

        public double PrevUpdate;
        public double[] PrevUpdates = new double[9];
        public void Update(object elapsedtime)
        {
            DateTime StartTime = DateTime.Now;
            double ElapsedTime = (double)elapsedtime;

            if (Characters_Number != 0) Update_Characters(ElapsedTime);
            if (Agents_Number != 0) Update_Agents(ElapsedTime);
            if (Persons_Number != 0) Update_Persons(ElapsedTime);
            if (Creatures_Number != 0) Update_Creatures(ElapsedTime);
            if (Missiles_Number != 0) Update_Missiles(ElapsedTime);
            if (Splashes_Number != 0) Update_Splashes(ElapsedTime);

            if (Portals.Count != 0) Collision_Portals();
            if (Missiles_Number != 0) Collision_Missiles();

            Area.Regions_Updated--;
            PrevUpdate = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Characters(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Character> DeadCharacters = new List<Character>();
            List<Character> MovingCharacters = new List<Character>();
            List<Character> TeleportingCharacters = new List<Character>();

            foreach (Character NextCharacter in Characters)
            {
                if (NextCharacter.Status_Dead) DeadCharacters.Add(NextCharacter);
                else
                {
                    NextCharacter.Update(ElapsedTime);
                    if (NextCharacter.Region_Moving) MovingCharacters.Add(NextCharacter);
                    if (NextCharacter.Status_Teleporting) TeleportingCharacters.Add(NextCharacter);
                }
            }

            foreach (Character NextCharacter in DeadCharacters)
                NextCharacter.Status_Death(NextCharacter.Killer);

            foreach (Character NextCharacter in MovingCharacters)
                Area.Hero_Move(NextCharacter);

            foreach (Character NextCharacter in TeleportingCharacters)
            {
                GameManager.Factions[NextCharacter.FactionID].Summon(NextCharacter);
                NextCharacter.Status_Teleporting = false;
            }

            PrevUpdates[0] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Agents(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Agent> DeadAgents = new List<Agent>();
            List<Agent> MovingAgents = new List<Agent>();

            foreach (Agent NextAgent in Agents)
            {
                if (NextAgent.Status_Dead) DeadAgents.Add(NextAgent);
                else
                {
                    NextAgent.Update(ElapsedTime);
                    if (NextAgent.Region_Moving) MovingAgents.Add(NextAgent);
                    else NextAgent.Mind.Update(ElapsedTime);
                }
            }

            foreach (Agent NextAgent in DeadAgents)
                NextAgent.Status_Death(NextAgent.Killer);

            foreach (Agent NextAgent in MovingAgents)
                Area.Hero_Move(NextAgent);

            PrevUpdates[1] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Persons(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Person> DeadPersons = new List<Person>();
            List<Person> MovingPersons = new List<Person>();

            foreach (Person NextPerson in Persons)
            {
                if (NextPerson.Status_Dead) DeadPersons.Add(NextPerson);
                else
                {
                    NextPerson.Update(ElapsedTime);
                    if (NextPerson.Region_Moving) MovingPersons.Add(NextPerson);
                    else NextPerson.Mind.Update(ElapsedTime);
                }
            }

            foreach (Person NextPerson in DeadPersons)
                NextPerson.Status_Death(NextPerson.Killer);

            foreach (Person NextPerson in MovingPersons)
                Area.Person_Move(NextPerson);

            PrevUpdates[2] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Creatures(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Creature> DeadCreatures = new List<Creature>();
            List<Creature> MovingCreatures = new List<Creature>();

            foreach (Creature NextCreature in Creatures)
            {
                if (NextCreature.Status_Dead) DeadCreatures.Add(NextCreature);
                else
                {
                    NextCreature.Update(ElapsedTime);
                    if (NextCreature.Region_Moving) MovingCreatures.Add(NextCreature);
                    else NextCreature.Mind.Update(ElapsedTime);
                }
            }

            foreach (Creature NextCreature in DeadCreatures)
                NextCreature.Status_Death(NextCreature.Killer);

            foreach (Creature NextCreature in MovingCreatures)
                Area.Creature_Move(NextCreature);

            PrevUpdates[3] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Missiles(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Missile> MovingMissiles = new List<Missile>();
            List<Missile> RemovableMissiles = new List<Missile>();

            foreach (Missile NextMissile in Missiles)
                switch (NextMissile.Update(ElapsedTime))
                {
                    case 1: MovingMissiles.Add(NextMissile); break;
                    case 2: RemovableMissiles.Add(NextMissile); break;
                }

            foreach (Missile NextMissile in RemovableMissiles)
                Area.Missiles_Remove(NextMissile);

            foreach (Missile NextMissile in MovingMissiles)
                Area.Missile_Move(NextMissile);

            PrevUpdates[4] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Update_Splashes(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            List<Splash> RemovableSplashes = new List<Splash>();

            foreach (Splash NextSplash in Splashes)
                if (NextSplash.Update(ElapsedTime))
                    RemovableSplashes.Add(NextSplash);

            foreach (Splash NextSplash in RemovableSplashes)
                Area.Splashes_Remove(NextSplash);

            PrevUpdates[5] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        public static bool Collide(RectangleF Rectangle, PointF Point)
        {
            if (Rectangle.X <= Point.X)
                if (Rectangle.Y <= Point.Y)
                    if (Point.X < Rectangle.X + Rectangle.Width)
                        if (Point.Y < Rectangle.Y + Rectangle.Height)
                            return true;
            return false;
        }

        private void Collision_Missiles()
        {
            DateTime StartTime = DateTime.Now;

            List<Missile> RemovableMissiles = new List<Missile>();

            List<Hero> DeadHeroes = new List<Hero>();
            List<Person> DeadPersons = new List<Person>();
            List<Creature> DeadCreatures = new List<Creature>();

            foreach (Missile NextMissile in Missiles)
            {
                for (int Column = -1; Column <= 1; Column++)
                    for (int Row = -1; Row <= 1; Row++)
                        if ((0 <= Index.X + Row) && (Index.X + Row <= Area.MapSize.Width * Area.Regions_Multiplier))
                            if ((0 <= Index.Y + Column) && (Index.Y + Column <= Area.MapSize.Height * Area.Regions_Multiplier))
                            {
                                bool Collided = false;

                                foreach (Character NextCharacter in Area.Regions[Index.X + Row, Index.Y + Column].Characters)
                                    if (NextCharacter != NextMissile.Spell.Caster)
                                        if (Collide(new RectangleF(NextCharacter.Location.X - 32, NextCharacter.Location.Y - 32, 64, 64), NextMissile.Location))
                                        {
                                            NextMissile.Spell.Effect(NextMissile.Spell.Caster, NextCharacter, NextMissile.Rank, NextMissile.BonusRanked);
                                            Collided = true;

                                            if (NextCharacter.Status_Dead) DeadHeroes.Add(NextCharacter);
                                        }

                                foreach (Agent NextAgent in Area.Regions[Index.X + Row, Index.Y + Column].Agents)
                                    if (NextAgent != NextMissile.Spell.Caster)
                                        if (Collide(new RectangleF(NextAgent.Location.X - 32, NextAgent.Location.Y - 32, 64, 64), NextMissile.Location))
                                        {
                                            NextMissile.Spell.Effect(NextMissile.Spell.Caster, NextAgent, NextMissile.Rank, NextMissile.BonusRanked);
                                            Collided = true;

                                            if (NextAgent.Status_Dead) DeadHeroes.Add(NextAgent);
                                        }

                                foreach (Person NextPerson in Area.Regions[Index.X + Row, Index.Y + Column].Persons)
                                    if (NextPerson != NextMissile.Spell.Caster)
                                        if (Collide(new RectangleF(NextPerson.Location.X - 32, NextPerson.Location.Y - 32, 64, 64), NextMissile.Location))
                                        {
                                            NextMissile.Spell.Effect(NextMissile.Spell.Caster, NextPerson, NextMissile.Rank, NextMissile.BonusRanked);
                                            Collided = true;

                                            if (NextPerson.Status_Dead) DeadPersons.Add(NextPerson);
                                        }

                                foreach (Creature NextCreature in Area.Regions[Index.X + Row, Index.Y + Column].Creatures)
                                    if (NextCreature != NextMissile.Spell.Caster)
                                        if (Collide(new RectangleF(NextCreature.Location.X - 32, NextCreature.Location.Y - 32, 64, 64), NextMissile.Location))
                                        {
                                            NextMissile.Spell.Effect(NextMissile.Spell.Caster, NextCreature, NextMissile.Rank, NextMissile.BonusRanked);
                                            Collided = true;

                                            if (NextCreature.Status_Dead) DeadCreatures.Add(NextCreature);
                                        }

                                if (Collided)
                                {
                                    RemovableMissiles.Add(NextMissile);

                                    foreach (Hero NextHero in DeadHeroes)
                                        NextHero.Status_Death(NextMissile.Spell.Caster);

                                    foreach (Person NextPerson in DeadPersons)
                                        NextPerson.Status_Death(NextMissile.Spell.Caster);

                                    foreach (Creature NextCreature in DeadCreatures)
                                        NextCreature.Status_Death(NextMissile.Spell.Caster);
                                }
                            }
            }

            foreach (Missile NextMissile in RemovableMissiles)
                Area.Missiles_Remove(NextMissile);

            PrevUpdates[6] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        private void Collision_Portals()
        {
            DateTime StartTime = DateTime.Now;

            List<Hero> SummonedHeroes = new List<Hero>();
            List<Portal> SummoningPortals = new List<Portal>();

            foreach (Portal NextPortal in Portals)
                for (int Column = -1; Column <= 1; Column++)
                    for (int Row = -1; Row <= 1; Row++)
                        if ((0 <= Index.X + Row) && (Index.X + Row <= Area.MapSize.Width * Area.Regions_Multiplier))
                            if ((0 <= Index.Y + Column) && (Index.Y + Column <= Area.MapSize.Height * Area.Regions_Multiplier))
                            {
                                foreach (Character NextCharacter in Area.Regions[Index.X + Row, Index.Y + Column].Characters)
                                    if (Collide(new RectangleF(NextCharacter.Location.X - 32, NextCharacter.Location.Y - 32, 64, 64), NextPortal.Location))
                                    {
                                        SummonedHeroes.Add(NextCharacter);
                                        SummoningPortals.Add(NextPortal);
                                        break;
                                    }

                                foreach (Agent NextAgent in Area.Regions[Index.X + Row, Index.Y + Column].Agents)
                                    if (Collide(new RectangleF(NextAgent.Location.X - 32, NextAgent.Location.Y - 32, 64, 64), NextPortal.Location))
                                    {
                                        SummonedHeroes.Add(NextAgent);
                                        SummoningPortals.Add(NextPortal);
                                        break;
                                    }
                            }


            for (int Current = 0; Current < SummonedHeroes.Count; Current++)
                if (SummoningPortals[Current].HomePortal) SummonedHeroes[Current].Area.Heroes_Teleport(SummonedHeroes[Current]);
                else
                {
                    if (SummonedHeroes[Current].Area.Name.Equals(SummoningPortals[Current].TargetArea))
                        SummonedHeroes[Current].Location_Set(SummoningPortals[Current].TargetLocation);
                    else
                        SummonedHeroes[Current].Area.Heroes_Leave(SummonedHeroes[Current],SummoningPortals[Current].TargetArea, SummoningPortals[Current].TargetLocation);
                }

            PrevUpdates[7] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        public void ProcessMinds(double ElapsedTime)
        {
            DateTime StartTime = DateTime.Now;

            //double ElapsedTime = (double)elapsedtime;

            foreach (Agent NextAgent in Agents)
                NextAgent.Mind.Process(ElapsedTime);

            foreach (Person NextPerson in Persons)
                NextPerson.Mind.Process(ElapsedTime);

            foreach (Creature NextCreature in Creatures)
                NextCreature.Mind.Process(ElapsedTime);


            PrevUpdates[8] = (DateTime.Now - StartTime).TotalMilliseconds;
        }

        public void SendEnterData(Character Character)
        {
            foreach (Character NextCharacter in Characters)
                if (NextCharacter.Status_Invisible <= 0)
                    NextCharacter.Broadcast_Enter(Character.Connection);

            foreach (Agent NextAgent in Agents)
                if (NextAgent.Status_Invisible <= 0)
                    NextAgent.Broadcast_Enter(Character.Connection);

            foreach (Person NextPerson in Persons)
                if (NextPerson.Status_Invisible <= 0)
                    NextPerson.Broadcast_Enter(Character.Connection);

            foreach (Creature NextCreature in Creatures)
                if (NextCreature.Status_Invisible <= 0)
                    NextCreature.Broadcast_Enter(Character.Connection);

            foreach (Corpse NextCorpse in Corpses)
                Character.Connection.Send(Connection.Command.Corpse_Add, NextCorpse.GetData());

            foreach (Missile NextMissile in Missiles)
                Character.Connection.Send(Connection.Command.Missile_Add, NextMissile.GetData());

            foreach (Portal NextPortal in Portals)
                Character.Connection.Send(Connection.Command.Portal_Add, NextPortal.GetData());

            foreach (Splash NextSplash in Splashes)
                Character.Connection.Send(Connection.Command.Splash_Add, NextSplash.GetData());
        }

        public void SendLeaveData(Character Character)
        {
            foreach (Character NextCharacter in Characters)
                Character.Connection.Send(Connection.Command.Hero_Remove, NextCharacter.Name);

            foreach (Agent NextAgent in Agents)
                Character.Connection.Send(Connection.Command.Hero_Remove, NextAgent.Name);

            foreach (Person NextPerson in Persons)
                Character.Connection.Send(Connection.Command.Person_Remove, NextPerson.ID.ToString());

            foreach (Creature NextCreature in Creatures)
                Character.Connection.Send(Connection.Command.Creature_Remove, NextCreature.ID.ToString());

            foreach (Corpse NextCorpse in Corpses)
                Character.Connection.Send(Connection.Command.Corpse_Remove, NextCorpse.ID.ToString());

            foreach (Missile NextMissile in Missiles)
                Character.Connection.Send(Connection.Command.Missile_Remove, NextMissile.ID.ToString());

            foreach (Splash NextSplash in Splashes)
                Character.Connection.Send(Connection.Command.Splash_Remove, NextSplash.ID.ToString());

            foreach (Portal NextPortal in Portals)
                Character.Connection.Send(Connection.Command.Portal_Remove, NextPortal.Name);
        }

        public void BroadcastCommand(Connection.Command Command, string Data)
        {
            foreach (Character NextCharacter in Characters)
                if (NextCharacter != null)
                    NextCharacter.Connection.Send(Command, Data);
        }
    }
}
