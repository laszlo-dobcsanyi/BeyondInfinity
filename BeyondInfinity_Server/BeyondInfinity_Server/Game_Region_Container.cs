using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Region
    {
        public Area Area;

        public uint Characters_Number = 0;
        public List<Character> Characters = new List<Character>();

        public uint Agents_Number = 0;
        public List<Agent> Agents = new List<Agent>();

        public uint Persons_Number = 0;
        public List<Person> Persons = new List<Person>();

        public uint Creatures_Number = 0;
        public List<Creature> Creatures = new List<Creature>();

        public List<Corpse> Corpses = new List<Corpse>();

        public List<Portal> Portals = new List<Portal>();

        public uint Missiles_Number = 0;
        public List<Missile> Missiles = new List<Missile>();

        public uint Splashes_Number = 0;
        public List<Splash> Splashes = new List<Splash>();

        public void Heroes_Add(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
            {
                Characters_Add(Character);
                return;
            }

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Add(Agent);
        }

        public void Heroes_Remove(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
            {
                Characters_Remove(Character);
                return;
            }

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Remove(Agent);
        }


        public void Characters_Add(Character Character)
        {
            Characters.Add(Character);
            Characters_Number++;

            for (int Column = -Area.Regions_Neighbourhood.Height; Column <= Area.Regions_Neighbourhood.Height; Column++)
                for (int Row = -Area.Regions_Neighbourhood.Width; Row <= Area.Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Character.Region.Index.X + Row) && (Character.Region.Index.X + Row <= Character.Area.MapSize.Width * Area.Regions_Multiplier))
                        if ((0 <= Character.Region.Index.Y + Column) && (Character.Region.Index.Y + Column <= Character.Area.MapSize.Height * Area.Regions_Multiplier))
                            Character.Area.Regions[Character.Region.Index.X + Row, Character.Region.Index.Y + Column].Influence++;
        }

        public Character Characters_Get(string Name)
        {
            foreach (Character NextCharacter in Characters)
                if (NextCharacter.Name == Name)
                    return NextCharacter;
            return null;
        }

        public void Characters_Remove(Character Character)
        {
            for (int Column = -Area.Regions_Neighbourhood.Height; Column <= Area.Regions_Neighbourhood.Height; Column++)
                for (int Row = -Area.Regions_Neighbourhood.Width; Row <= Area.Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Character.Region.Index.X + Row) && (Character.Region.Index.X + Row <= Character.Area.MapSize.Width * Area.Regions_Multiplier))
                        if ((0 <= Character.Region.Index.Y + Column) && (Character.Region.Index.Y + Column <= Character.Area.MapSize.Height * Area.Regions_Multiplier))
                            Character.Area.Regions[Character.Region.Index.X + Row, Character.Region.Index.Y + Column].Influence--;

            Characters_Number--;
            Characters.Remove(Character);
        }


        public void Agents_Add(Agent Agent)
        {
            Agents.Add(Agent);
            Agents_Number++;

            Influence++;
        }

        public Agent Agents_Get(string Name)
        {
            foreach (Agent NextAgent in Agents)
                if (NextAgent.Name == Name)
                    return NextAgent;
            return null;
        }

        public void Agents_Remove(Agent Agent)
        {
            Influence--;

            Agents_Number--;
            Agents.Remove(Agent);
        }


        public void Persons_Add(Person Person)
        {
            Persons.Add(Person);
            Persons_Number++;
        }

        public Person Persons_Get(uint ID)
        {
            foreach (Person NextPerson in Persons)
                if (NextPerson.ID == ID)
                    return NextPerson;
            return null;
        }

        public void Persons_Remove(Person Person)
        {
            Persons_Number--;
            Persons.Remove(Person);
        }


        public void Creatures_Add(Creature Creature)
        {
            Creatures.Add(Creature);
            Creatures_Number++;
        }

        public Creature Creatures_Get(uint ID)
        {
            foreach (Creature NextCreature in Creatures)
                if (NextCreature.ID == ID)
                    return NextCreature;
            return null;
        }

        public void Creatures_Remove(Creature Creature)
        {
            Creatures_Number--;
            Creatures.Remove(Creature);
        }


        public void Corpses_Add(Corpse Corpse)
        {
            Corpses.Add(Corpse);
        }

        public Corpse Corpses_Get(uint ID)
        {
            foreach (Corpse NextCorpse in Corpses)
                if (NextCorpse.ID == ID)
                    return NextCorpse;
            return null;
        }

        public void Corpses_Remove(Corpse Corpse)
        {
            Corpses.Remove(Corpse);
        }


        public void Missiles_Add(Missile Missile)
        {
            Missiles.Add(Missile);
            Missiles_Number++;

            Influence++;
        }

        public void Missiles_Remove(Missile Missile)
        {
            Missiles_Number--;
            Missiles.Remove(Missile);
        }


        public void Splashes_Add(Splash Splash)
        {
            Splashes.Add(Splash);
            Splashes_Number++;

            Influence++;
        }

        public void Splashes_Remove(Splash Splash)
        {
            Influence--;

            Splashes_Number--;
            Splashes.Remove(Splash);
        }
    }
}
