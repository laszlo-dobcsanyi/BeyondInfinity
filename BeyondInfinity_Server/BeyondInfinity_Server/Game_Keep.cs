using System;
using System.Drawing;
using System.Threading;

namespace BeyondInfinity_Server
{
    public sealed class Keep : Area
    {
        public int Faction;
        public Keep(int faction)
            : base("underbog" + faction)
        {
            Faction = faction;
        }
    }
}
