using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public delegate void CombatEvent();

    public sealed partial class Mind
    {
        public Unit Unit;
        public bool Mobile;
        public bool InCombat = false;

        public CombatEvent Combat_Enter;
        public CombatEvent Combat_Leave;

        public Mind(Unit unit, bool mobile)
        {
            Unit = unit;
            Mobile = mobile;

            GetEventHandlers();
            //Generate constants
        }

        private static Random Random = new Random();

        public void Update(double ElapsedTime)
        {
            Unit.Location_Locker.EnterReadLock();
            try
            {
                if (Mobile)
                {
                    Nods_Locker.EnterReadLock();
                    try
                    {
                        if (Region.Collide(new Rectangle(Nods[CurrentNod, 0] * 16, Nods[CurrentNod, 1] * 16, 16, 16), Unit.Location))
                        {
                            CurrentNod++;

                            if ((CurrentNod == MaxNods) && (Unit.Area.FileName == PathWays[CurrentPath].FileName))
                            {
                                NextPath();
                                CurrentNod++;
                            }

                            if (Nods[CurrentNod, 0] < Nods[CurrentNod - 1, 0]) { Unit.Rotate(180); return; }
                            if (Nods[CurrentNod, 1] < Nods[CurrentNod - 1, 1]) { Unit.Rotate(90); return; }
                            if (Nods[CurrentNod - 1, 0] < Nods[CurrentNod, 0]) { Unit.Rotate(0); return; }
                            if (Nods[CurrentNod - 1, 1] < Nods[CurrentNod, 1]) { Unit.Rotate(270); return; }
                        }
                    }
                    finally { Nods_Locker.ExitReadLock(); }
                }
            }
            finally { Unit.Location_Locker.ExitReadLock(); }
        }

        public void Process(double ElapsedTime)
        {
            if (IsInCombat())
            {
                if (InCombat == false)
                {
                    InCombat = true;
                    if (Combat_Enter != null) Combat_Enter();
                }
            }
            else if (InCombat == true)
            {
                InCombat = false;
                if (Combat_Leave != null) Combat_Leave();
            }

            Cast();

            /*Unit.Location_Locker.EnterReadLock();
            try
            {
                if (Mobile)
                {
                    double Degree = 0;

                    Nods_Locker.EnterReadLock();
                    try
                    {
                        if (Unit.Location.X < Nods[CurrentNod, 0] * 16 + 8)
                            Degree = (-Math.Atan((double)((Nods[CurrentNod, 1] * 16 + 8) - Unit.Location.Y) / (double)((Nods[CurrentNod, 0] * 16 + 8) - Unit.Location.X))) / Math.PI * 180;
                        if ((Nods[CurrentNod, 0] * 16 + 8) < Unit.Location.X)
                            Degree = (Math.PI - Math.Atan((double)((Nods[CurrentNod, 1] * 16 + 8) - Unit.Location.Y) / (double)((Nods[CurrentNod, 0] * 16 + 8) - Unit.Location.X))) / Math.PI * 180;

                        if (Degree < 0)
                            Degree += 360;

                        if (Unit.Location.X == (Nods[CurrentNod, 0] * 16 + 8))
                            if (Unit.Location.Y < (Nods[CurrentNod, 1] * 16 + 8))
                                Degree = 90;
                            else Degree = 270;
                    }
                    finally { Nods_Locker.ExitReadLock(); }

                    Unit.Rotate((int)Degree);
                }
            }
            finally { Unit.Location_Locker.ExitReadLock(); }*/
        }

        private int CurrentPath;
        private List<AreaLocation> PathWays;

        public void NewPath()
        {
            if (Mobile)
            {

                Unit.Location_Locker.EnterReadLock();
                try
                {
                    CurrentNod = 0;
                    CurrentPath = 0;
                    do
                    {
                        PathWays = GetPathWays(new List<AreaLocation>(), new AreaLocation(Unit.Area.FileName, new Point((int)Unit.Location.X, (int)Unit.Location.Y)), false, Unit.Schedule_Next());
                    } while (PathWays.Count == 0);
                    CalculateNods(new Node((int)PathWays[CurrentPath].Location.X / 16, (int)PathWays[CurrentPath].Location.Y / 16));
                }
                finally { Unit.Location_Locker.ExitReadLock(); }
            }
        }

        public void NextPath()
        {
            if (Mobile)
            {
                Unit.Location_Locker.EnterReadLock();
                try
                {
                    CurrentPath++;
                    CurrentNod = 0;

                    if ((PathWays == null) || (CurrentPath == PathWays.Count) || ((PathWays != null) && (PathWays[CurrentPath].FileName != Unit.Area.FileName)))
                    {
                        CurrentPath = 0;
                        do
                        {
                            PathWays = GetPathWays(new List<AreaLocation>(), new AreaLocation(Unit.Area.FileName, new Point((int)Unit.Location.X, (int)Unit.Location.Y)), false, Unit.Schedule_Next());
                        } while (PathWays.Count == 0);
                    }

                    CalculateNods(new Node((int)PathWays[CurrentPath].Location.X / 16, (int)PathWays[CurrentPath].Location.Y / 16));
                }
                finally { Unit.Location_Locker.ExitReadLock(); }
            }
        }

        private List<AreaLocation> GetPathWays(List<AreaLocation> Points, AreaLocation New, bool Add, WayPoint WayPoint)
        {
            if (Add) Points.Add(New);

            if (New.FileName == WayPoint.FileName)
                if (New.Location == WayPoint.Location) return Points;
                else
                {
                    Points.Add(new AreaLocation(WayPoint.FileName, WayPoint.Location));
                    return Points;
                }

            for (int Current = 0; Current < 3; Current++)
                if (AreaLocations[Current, 0].FileName == New.FileName)
                    if (AreaLocations[Current, 0].Location == New.Location)
                    {
                        List<AreaLocation> Returned = GetPathWays(Points, AreaLocations[Current, 1], false, WayPoint);
                        if (Returned != null) return Returned;
                    }
                    else
                    {
                        List<AreaLocation> Returned = GetPathWays(Points, AreaLocations[Current, 0], true, WayPoint);
                        if (Returned != null) return Returned;
                    }

            Console.WriteLine("Fatal Error!");
            return null;
        }

        public int[,] Nods;
        public int MaxNods = 0;
        public int CurrentNod = -1;
        public ReaderWriterLockSlim Nods_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static int[,] Directions = new int[4, 2] { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };
        private void CalculateNods(Node End)
        {
            Unit.Location_Locker.EnterReadLock();
            try
            {
                BinomialHeap Open = new BinomialHeap();
                byte[,] State = new byte[Unit.Area.MapSize.Width * 32, Unit.Area.MapSize.Height * 32];

                Node Start = new Node((int)(Unit.Location.X / 16), (int)(Unit.Location.Y / 16));
                Start.Parent = Start;
                Start.Value = Length(Start, End);
                Open.Add(Start);

                //Console.WriteLine("From {0}:{1} to {2}:{3}", Start.X, Start.Y, End.X, End.Y);
                while (Open.Count != 0)
                {
                    Node Current = Open.Pop();

                    if (End.X == Current.X && End.Y == Current.Y)
                    {
                        /*Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("From {0}:{1} to {2}:{3}", Start.X, Start.Y, End.X, End.Y);
                        Console.WriteLine("Path found!");*/

                        MaxNods = 2;
                        Node Previous = Current;
                        while (Previous.X != Start.X || Previous.Y != Start.Y)
                        {
                            MaxNods++;
                            Previous = Previous.PathParent;
                        }

                        Previous = Current;
                        Nods = new int[MaxNods, 2];
                        Nods[0, 0] = Start.X;
                        Nods[0, 1] = Start.Y;
                        for (int Index = 1; Index < MaxNods; Index++)
                        {
                            Nods[MaxNods - Index, 0] = Previous.X;
                            Nods[MaxNods - Index, 1] = Previous.Y;
                            Previous = Previous.PathParent;
                        }

                        /*for (int Index = 0; Index < MaxNods; Index++)
                            Console.WriteLine("{0}> {1}:{2}", Index, Nods[Index, 0], Nods[Index, 1]);*/
                        return;
                    }

                    State[Current.X, Current.Y] = 2;

                    //Console.WriteLine("Jumping from {0}:{1} ({2})", Current.X, Current.Y, Current.Value);
                    for (int Direction = 0; Direction < 4; Direction++)
                    {
                        Node Successor = new Node(Current.X + Directions[Direction, 0], Current.Y + Directions[Direction, 1]);
                        //Console.WriteLine(" ->Direction {0} ({1}:{2})", Direction, Successor.X,Successor.Y);
                        if (0 <= Successor.X && Successor.X < Unit.Area.MapSize.Width * 32)
                            if (0 <= Successor.Y && Successor.Y < Unit.Area.MapSize.Height * 32)
                                if (Unit.Area.MapGrid[Successor.X, Successor.Y] == 0)
                                {
                                    //Console.WriteLine("    ->Free");
                                    if (State[Successor.X, Successor.Y] == 0)
                                    {
                                        /*if (State[Next.X, Next.Y] == 0)
                                            Next.G = int.MaxValue;*/

                                        bool Found;
                                        Node Next;
                                        do
                                        {
                                            Found = true;
                                            Next = new Node(Successor.X + Directions[Direction, 0], Successor.Y + Directions[Direction, 1]);
                                            if (0 <= Next.X && Next.X < Unit.Area.MapSize.Width * 32)
                                                if (0 <= Next.Y && Next.Y < Unit.Area.MapSize.Height * 32)
                                                    if (Unit.Area.MapGrid[Next.X, Next.Y] == 0)
                                                        if (Length(Next, End) <= Length(Successor, End))
                                                        {
                                                            Successor = Next;
                                                            if (Program.MIND_TURNCHANCE < Random.NextDouble()) Found = false;
                                                        }
                                        }
                                        while (!Found);

                                        Successor.PathParent = Current;
                                        Successor.Value = Length(Successor, End); //Length(new Node(0, 0), Start) + Length(Start, Previous) +
                                        State[Successor.X, Successor.Y] = 1;
                                        Open.Add(Successor);
                                        //Console.WriteLine("Jumped to {0}:{1} ({2})", Successor.X, Successor.Y, Successor.Value);
                                    }
                                }
                    }
                }

                //DrawGrid();
                Console.WriteLine("No path found!");
                Console.ReadLine();
            }
            finally { Unit.Location_Locker.ExitReadLock(); }
        }

        private int Length(Node First, Node Second)
        {
            return (First.X < Second.X ? Second.X - First.X : First.X - Second.X) + (First.Y < Second.Y ? Second.Y - First.Y : First.Y - Second.Y);
            //return Math.Max(First.X < Second.X ? Second.X - First.X : First.X - Second.X, First.Y < Second.Y ? Second.Y - First.Y : First.Y - Second.Y);
        }

        private void Cast()
        {
            if (Unit.Status_Muted <= 0)
                for (int Current = 0; Current < Unit.Spells.Length; Current++)
                    if (Unit.Spells[Current].Cooldown == 0)
                    {
                        Unit Target = null;

                        switch (Unit.Spells[Current].Effect_ID)
                        {
                            case 0: Target = GetBestUnit(false, false); break;
                            case 1: Target = GetBestUnit(false, false); break;
                            case 2: Target = GetBestUnit(false, false); break;
                            case 3: Target = GetBestUnit(false, false); break;
                            case 4: Target = GetBestUnit(false, false); break;
                            case 5: Target = GetBestUnit(false, false); break;

                            case 6: Target = GetBestUnit(true, false); break;
                            case 7: Target = GetBestUnit(true,true); break;
                            case 8: Target = GetBestUnit(true, false); break;
                            case 9: Target = GetBestUnit(true, true); break;
                            case 10: Target = GetBestUnit(true, false); break;
                            case 11: Target = GetBestUnit(true, true); break;

                            case 12: Target = GetBestUnit(false, false); break;
                            case 13: Target = GetBestUnit(true, false); break;
                            case 14: Target = GetBestUnit(true, false); break;
                            case 15: Target = GetBestUnit(true, false); break;
                            case 16: Target = GetBestUnit(true, false); break;
                            case 17: Target = GetBestUnit(true, false); break;

                            case 18: Target = null; break;
                            case 19: Target = GetBestUnit(true, false); break;
                            case 20: Target = GetBestUnit(true, false); break;
                            case 21: Target = GetBestUnit(true, true); break;
                            case 22: Target = null; break;
                            case 23: Target = GetBestUnit(false, true); break;

                            case 36: Target = GetBestUnit(false, true); break;
                            case 37: Target = GetBestUnit(false, true); break;
                            case 38: Target = GetBestUnit(false, true); break;
                            case 39: Target = GetBestUnit(false, true); break;
                            case 40: Target = GetBestUnit(false, true); break;
                            case 41: Target = GetBestUnit(false, true); break;

                            case 42: Target = GetBestUnit(true, true); break;
                            case 43: Target = GetBestUnit(true, true); break;
                            case 44: Target = GetBestUnit(true, true); break;
                            case 45: Target = GetBestUnit(true, true); break;
                            case 46: Target = GetBestUnit(true, true); break;
                            case 47: Target = GetBestUnit(true, true); break;
                        }

                        if (Target != null)
                        {
                            int Rank = Random.Next(6);

                            Unit.Spells[Current].Trigger(Rank, Target);
                            //Unit.Spells[Current].Cooldown_Set(Rank);
                            return;
                        }
                    }

            /*if (0 < Unit.Energy - Spell.EnergyCost[Unit.Spells[Current].Effect_ID / 6, Unit.Spells[Current].Effect_ID % 6])
                switch (Unit.Spells[Current].Effect_ID / 6)
                {
                    //Damage
                    case 0:
                        double MinEnergy = double.MaxValue;
                        double MaxEnergy = 1;

                        for (int Column = -1; Column < 2; Column++)
                            for (int Row = -1; Row < 2; Row++)
                            {
                                Point Index = new Point(Unit.Region.Index.X + Row, Unit.Region.Index.Y + Column);
                                if ((0 <= Index.X) && (Index.X < Unit.Area.MapSize.Width * 2 + 1))
                                    if ((0 <= Index.Y) && (Index.Y < Unit.Area.MapSize.Height * 2 + 1))
                                    {
                                        Unit.Area.Regions[Index.X, Index.Y].Characters_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Character NextCharacter in Unit.Area.Regions[Index.X, Index.Y].Characters)
                                                if (NextCharacter.FactionID != Unit.FactionID)
                                                    if (NextCharacter.Status_Invisible <= 0)
                                                    {
                                                        if (NextCharacter.Energy < MinEnergy)
                                                        {
                                                            Target = NextCharacter;
                                                            MinEnergy = Target.Energy;
                                                        }

                                                        if (MaxEnergy < NextCharacter.Energy) MaxEnergy = NextCharacter.Energy;
                                                    }
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Characters_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Agents_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Agent NextAgent in Unit.Area.Regions[Index.X, Index.Y].Agents)
                                                if (NextAgent.FactionID != Unit.FactionID)
                                                    if (NextAgent.Status_Invisible <= 0)
                                                    {
                                                        if (NextAgent.Energy < MinEnergy)
                                                        {
                                                            Target = NextAgent;
                                                            MinEnergy = Target.Energy;
                                                        }

                                                        if (MaxEnergy < NextAgent.Energy) MaxEnergy = NextAgent.Energy;
                                                    }
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Agents_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Persons_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Person NextPerson in Unit.Area.Regions[Index.X, Index.Y].Persons)
                                                if (NextPerson.FactionID != Unit.FactionID)
                                                    if (NextPerson.Status_Invisible <= 0)
                                                    {
                                                        if (NextPerson.Energy < MinEnergy)
                                                        {
                                                            Target = NextPerson;
                                                            MinEnergy = Target.Energy;
                                                        }

                                                        if (MaxEnergy < NextPerson.Energy) MaxEnergy = NextPerson.Energy;
                                                    }
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Persons_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Creatures_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Creature NextCreature in Unit.Area.Regions[Index.X, Index.Y].Creatures)
                                                if (NextCreature.FactionID != Unit.FactionID)
                                                    if (NextCreature.Status_Invisible <= 0)
                                                    {
                                                        if (NextCreature.Energy < MinEnergy)
                                                        {
                                                            Target = NextCreature;
                                                            MinEnergy = Target.Energy;
                                                        }

                                                        if (MaxEnergy < NextCreature.Energy) MaxEnergy = NextCreature.Energy;
                                                    }
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Creatures_Locker.ExitReadLock(); }
                                    }
                            }

                        if (Target != null)
                        {
                            Value = Target.MaxEnergy / Target.Energy;
                            if (BestValue < Value)
                            {
                                BestSpell = Current;
                                BestTarget = Target;
                                BestValue = Value;
                            }
                        }
                        break;

                    //Heal
                    case 6:
                        Target = null;
                        MinEnergy = int.MaxValue;
                        MaxEnergy = 1;

                        for (int Column = -1; Column < 2; Column++)
                            for (int Row = -1; Row < 2; Row++)
                            {
                                Point Index = new Point(Unit.Region.Index.X + Row, Unit.Region.Index.Y + Column);
                                if ((0 <= Index.X) && (Index.X < Unit.Area.MapSize.Width * 2 + 1))
                                    if ((0 <= Index.Y) && (Index.Y < Unit.Area.MapSize.Height * 2 + 1))
                                    {
                                        Unit.Area.Regions[Index.X, Index.Y].Characters_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Character NextCharacter in Unit.Area.Regions[Index.X, Index.Y].Characters)
                                                if (NextCharacter.FactionID == Unit.FactionID)
                                                    if (NextCharacter.Status_Invisible <= 0)
                                                        if (NextCharacter.Energy != NextCharacter.MaxEnergy)
                                                            if (NextCharacter.Energy < MinEnergy)
                                                                Target = NextCharacter;
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Characters_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Agents_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Agent NextAgent in Unit.Area.Regions[Index.X, Index.Y].Agents)
                                                if (NextAgent.FactionID == Unit.FactionID)
                                                    if (NextAgent.Status_Invisible <= 0)
                                                        if (NextAgent.Energy != NextAgent.MaxEnergy)
                                                            if (NextAgent.Energy < MinEnergy)
                                                                Target = NextAgent;
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Agents_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Persons_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Person NextPerson in Unit.Area.Regions[Index.X, Index.Y].Persons)
                                                if (NextPerson.FactionID == Unit.FactionID)
                                                    if (NextPerson.Status_Invisible <= 0)
                                                        if (NextPerson.Energy != NextPerson.MaxEnergy)
                                                            if (NextPerson.Energy < MinEnergy)
                                                                Target = NextPerson;
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Persons_Locker.ExitReadLock(); }

                                        Unit.Area.Regions[Index.X, Index.Y].Creatures_Locker.EnterReadLock();
                                        try
                                        {
                                            foreach (Creature NextCreature in Unit.Area.Regions[Index.X, Index.Y].Creatures)
                                                if (NextCreature.FactionID == Unit.FactionID)
                                                    if (NextCreature.Status_Invisible <= 0)
                                                        if (NextCreature.Energy != NextCreature.MaxEnergy)
                                                            if (NextCreature.Energy < MinEnergy)
                                                                Target = NextCreature;
                                        }
                                        finally { Unit.Area.Regions[Index.X, Index.Y].Creatures_Locker.ExitReadLock(); }
                                    }
                            }*/

        }

        private Unit GetBestUnit(bool EnergyMax, bool Friendly)
        {
            double MinEnergy = double.MaxValue;
            double MaxEnergy = 1;
            Unit Target = null;

            for (int Column = -1; Column < 2; Column++)
                for (int Row = -1; Row < 2; Row++)
                {
                    Point Index = new Point(Unit.Region.Index.X + Row, Unit.Region.Index.Y + Column);
                    if ((0 <= Index.X) && (Index.X <= Unit.Area.MapSize.Width * Unit.Area.Regions_Multiplier))
                        if ((0 <= Index.Y) && (Index.Y <= Unit.Area.MapSize.Height * Unit.Area.Regions_Multiplier))
                        {
                            foreach (Character NextCharacter in Unit.Area.Regions[Index.X, Index.Y].Characters)
                                if (Friendly ? (NextCharacter.FactionID == Unit.FactionID) : (NextCharacter.FactionID != Unit.FactionID))
                                    if (NextCharacter.Status_Invisible <= 0)
                                        if (EnergyMax ? (NextCharacter.Energy < MinEnergy) : (NextCharacter.Energy < MinEnergy))
                                            if (Random.NextDouble() < Program.MIND_TARGETCHANCE)
                                            {
                                                Target = NextCharacter;
                                                if (EnergyMax) MaxEnergy = Target.Energy;
                                                else MinEnergy = Target.Energy;
                                            }

                            foreach (Agent NextAgent in Unit.Area.Regions[Index.X, Index.Y].Agents)
                                if (Friendly ? (NextAgent.FactionID == Unit.FactionID) : (NextAgent.FactionID != Unit.FactionID))
                                    if (NextAgent.Status_Invisible <= 0)
                                        if (EnergyMax ? (MaxEnergy < NextAgent.Energy) : (NextAgent.Energy < MinEnergy))
                                            if (Random.NextDouble() < Program.MIND_TARGETCHANCE)
                                            {
                                                Target = NextAgent;
                                                if (EnergyMax) MaxEnergy = Target.Energy;
                                                else MinEnergy = Target.Energy;
                                            }

                            foreach (Person NextPerson in Unit.Area.Regions[Index.X, Index.Y].Persons)
                                if (Friendly ? (NextPerson.FactionID == Unit.FactionID) : (NextPerson.FactionID != Unit.FactionID))
                                    if (NextPerson.Status_Invisible <= 0)
                                        if (EnergyMax ? (MaxEnergy < NextPerson.Energy) : (NextPerson.Energy < MinEnergy))
                                        {
                                            Target = NextPerson;
                                            if (EnergyMax) MaxEnergy = Target.Energy;
                                            else MinEnergy = Target.Energy;
                                        }

                            foreach (Creature NextCreature in Unit.Area.Regions[Index.X, Index.Y].Creatures)
                                if (Friendly ? (NextCreature.FactionID == Unit.FactionID) : (NextCreature.FactionID != Unit.FactionID))
                                    if (NextCreature.Status_Invisible <= 0)
                                        if (EnergyMax ? (MaxEnergy < NextCreature.Energy) : (NextCreature.Energy < MinEnergy))
                                            if (Random.NextDouble() < Program.MIND_TARGETCHANCE)
                                            {
                                                Target = NextCreature;
                                                if (EnergyMax) MaxEnergy = Target.Energy;
                                                else MinEnergy = Target.Energy;
                                            }
                        }
                }

            return Target;
        }

        private bool IsInCombat()
        {
            for (int Column = -1; Column < 2; Column++)
                for (int Row = -1; Row < 2; Row++)
                {
                    Point Index = new Point(Unit.Region.Index.X + Row, Unit.Region.Index.Y + Column);
                    if ((0 <= Index.X) && (Index.X <= Unit.Area.MapSize.Width * Unit.Area.Regions_Multiplier))
                        if ((0 <= Index.Y) && (Index.Y <= Unit.Area.MapSize.Height * Unit.Area.Regions_Multiplier))
                        {
                            foreach (Character NextCharacter in Unit.Area.Regions[Index.X, Index.Y].Characters)
                                if (NextCharacter.FactionID != Unit.FactionID)
                                    if (NextCharacter.Status_Invisible <= 0)
                                        return true;

                            foreach (Agent NextAgent in Unit.Area.Regions[Index.X, Index.Y].Agents)
                                if (NextAgent.FactionID != Unit.FactionID)
                                    if (NextAgent.Status_Invisible <= 0)
                                        return true;

                            foreach (Person NextPerson in Unit.Area.Regions[Index.X, Index.Y].Persons)
                                if (NextPerson.FactionID != Unit.FactionID)
                                    if (NextPerson.Status_Invisible <= 0)
                                        return true;

                            foreach (Creature NextCreature in Unit.Area.Regions[Index.X, Index.Y].Creatures)
                                if (NextCreature.FactionID != Unit.FactionID)
                                    if (NextCreature.Status_Invisible <= 0)
                                        return true;
                        }
                }

            return false;
        }

        private static AreaLocation[,] AreaLocations = new AreaLocation[3, 2]
        {
            {new AreaLocation("underbog0", new Point(2400,600)),new AreaLocation("oldworld", new Point(466,2912))},
            {new AreaLocation("underbog1", new Point(2400,600)),new AreaLocation("oldworld", new Point(3566,2912))},
            {new AreaLocation("underbog2", new Point(2400,600)),new AreaLocation("oldworld", new Point(2016,224))},
          /*{new AreaLocation("oldworld", new Point(2048,2248)),new AreaLocation("hole", new Point(512,700))},
            {new AreaLocation("hole", new Point(128,700)),new AreaLocation("oldworld", new Point(2048,2120))},*/
        };
        private sealed class AreaLocation
        {
            public string FileName;
            public Point Location;

            public AreaLocation(string filename, Point location)
            {
                FileName = filename;
                Location = location;
            }
        }

        public sealed class Node
        {
            public int X, Y;

            public Node Parent;
            public int Degree;
            public int Value;
            public Node Child;
            public Node Bro;

            public Node PathParent;

            public Node(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public sealed class BinomialHeap
        {
            public int Count = 0;
            private Heap DataHeap = new Heap();

            public void Add(Node Node)
            {
                Heap NodeHeap = new Heap();
                Node.Parent = null;
                Node.Degree = 0;
                Node.Child = null;
                Node.Bro = null;
                NodeHeap.RootNode = Node;

                DataHeap = Heap.Combine(NodeHeap, DataHeap);
                Count++;
            }

            public Node Pop()
            {
                //Console.WriteLine(" ->Getting Min Node..");
                Node MinNode = null;
                int MinValue = int.MaxValue;
                Node Current = DataHeap.RootNode;
                Node Previous = null;

                while (Current != null)
                {
                    if (Current.Value < MinValue)
                    {
                        MinNode = Current;
                        MinValue = Current.Value;
                    }

                    Previous = Current;
                    Current = Current.Bro;
                }

                //Console.WriteLine(" ->Separating stacks..");
                Heap Other = new Heap();
                if (DataHeap.RootNode.Bro != null)
                {
                    Node OtherCurrent = null;
                    Current = DataHeap.RootNode;
                    while (Current != null)
                    {
                        if (Current != MinNode)
                            if (OtherCurrent == null)
                            {
                                Other.RootNode = Current;
                                OtherCurrent = Other.RootNode;
                            }
                            else
                            {
                                OtherCurrent.Bro = Current;
                                OtherCurrent = OtherCurrent.Bro;
                            }

                        Current = Current.Bro;
                    }
                    OtherCurrent.Bro = null;
                }

                if (MinNode.Child != null)
                {
                    Heap Reversed = new Heap();
                    //Console.WriteLine("\t ->Reversing..");
                    Reversed.RootNode = Reverse(null, MinNode.Child);

                    if (Other.RootNode != null)
                    {
                        //Console.WriteLine("\t ->Combining..");
                        DataHeap = Heap.Combine(Other, Reversed);
                    }
                    else
                    {
                        //Console.WriteLine("\t ->No Combining..");
                        DataHeap = Reversed;
                    }
                }
                else DataHeap = Other;

                //Console.WriteLine(" ->Returning..");
                Count--;
                return MinNode;
            }

            private Node Reverse(Node Previous, Node Current)
            {
                Node Start;
                if (Current.Bro != null)
                    Start = Reverse(Current, Current.Bro);
                else Start = Current;

                Current.Bro = Previous;
                return Start;
            }

            private sealed class Heap
            {
                public Node RootNode;

                public Heap()
                {
                    RootNode = null;
                }

                public Node Pop()
                {
                    return null;
                }

                public static Heap Combine(Heap Heap1, Heap Heap2)
                {
                    Heap ThisHeap = new Heap();
                    ThisHeap.RootNode = Heap1.RootNode;
                    Node ThisNode = ThisHeap.RootNode;

                    Node Current1 = Heap1.RootNode.Bro;
                    Node Current2 = Heap2.RootNode;

                    while ((Current1 != null) || (Current2 != null))
                    {
                        if (Current1 != null)
                        {
                            if (Current2 != null)
                                if (Current1.Degree < Current2.Degree)
                                {
                                    ThisNode.Bro = Current1;
                                    Current1 = Current1.Bro;
                                }
                                else
                                {
                                    ThisNode.Bro = Current2;
                                    Current2 = Current2.Bro;
                                }

                            else
                            {
                                ThisNode.Bro = Current1;
                                Current1 = Current1.Bro;
                            }
                        }
                        else
                        {
                            ThisNode.Bro = Current2;
                            Current2 = Current2.Bro;
                        }

                        ThisNode = ThisNode.Bro;
                    }

                    if (ThisHeap.RootNode == null) return ThisHeap;

                    Node Previous = null;
                    Node Current = ThisHeap.RootNode;
                    Node Next = ThisHeap.RootNode.Bro;

                    while (Next != null)
                    {
                        if ((Current.Degree != Next.Degree) || (Next.Bro != null && Next.Bro.Degree == Current.Degree))
                        {
                            Previous = Current;
                            Current = Next;
                        }
                        else
                        {
                            if (Current.Value <= Next.Value)
                            {
                                Current.Bro = Next.Bro;

                                Next.Parent = Current;
                                Next.Bro = Current.Child;
                                Current.Child = Next;
                                Current.Degree++;
                            }
                            else
                            {
                                if (Previous == null) ThisHeap.RootNode = Next;
                                else Previous.Bro = Next;

                                Current.Parent = Next;
                                Current.Bro = Next.Child;
                                Next.Child = Current;
                                Next.Degree++;

                                Current = Next;
                            }
                        }

                        Next = Current.Bro;
                    }

                    return ThisHeap;
                }

                private Node Get_MinNode()
                {
                    Node MinNode = null;
                    int MinValue = int.MaxValue;
                    Node Current = RootNode;

                    while (Current != null)
                    {
                        if (Current.Value < MinValue)
                        {
                            MinNode = Current;
                            MinValue = Current.Value;
                        }

                        Current = Current.Bro;
                    }

                    return MinNode;
                }

                private void Connect(Node Node1, Node Node2)
                {
                    Node1.Parent = Node2;
                    Node2.Child = Node1;
                    Node1.Child = Node2.Child;
                    Node2.Degree++;
                }
            }
        }
    }

    public sealed class WayPoint
    {
        public string FileName;
        public Point Location;
        public int Weight;
        public int Delay;

        public WayPoint(string Data)
        {
            string[] Arguments = Data.Split('\t');
            FileName = Arguments[0];
            Location = new Point(Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]));
            Weight = 0;
            Delay = 0;
        }

        public WayPoint(string filename, int x, int y)
        {
            FileName = filename;
            Location = new Point(x, y);
            Weight = 0;
            Delay = 0;
        }
    }
}
