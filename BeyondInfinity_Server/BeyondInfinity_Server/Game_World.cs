using System;
using System.Threading;

namespace BeyondInfinity_Server
{
    public sealed class World : Area
    {
        public World() : base("oldworld") { }

        /*public new void Heroes_Add(Hero Hero)
        {
            Character Character = Hero as Character;
            if (Character != null)
                Characters_Add(Character);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Add(Agent);

            if (GameManager.CallToArms) GameManager.AssignHero(Hero);
        }*/

        /*public new void Heroes_Remove(Hero Hero)
        {
            Hero.Battlefield.Fronts[Hero.FactionID].Heroes_Remove(Hero);

            Character Character = Hero as Character;
            if (Character != null)
                Characters_Remove(Character);

            Agent Agent = Hero as Agent;
            if (Agent != null)
                Agents_Remove(Agent);
        }*/
    }
}
