using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public partial class Area
    {
        public string Name;
        public string FileName = "";

        public bool Loading = true;

        public uint Creatures_PowerBonus;
        public uint Creatures_PowerMultiplier;

        public Area(Group group)
        {
           
            Regions_Size = 256;
            Regions_Multiplier = 512 / Regions_Size;
            Regions_Neighbourhood = new Size((Program.AREA_NEIGHBOURHOOD_WIDTH / 512) * Regions_Multiplier, (Program.AREA_NEIGHBOURHOOD_HEIGHT / 512) * Regions_Multiplier);

            group.Characters_Locker.EnterReadLock();
            try
            {
                uint ItemLevel = 0;
                foreach (Character NextCharacter in group.Characters)
                    if (ItemLevel < NextCharacter.ItemLevel)
                        ItemLevel = NextCharacter.ItemLevel;

                Creatures_PowerBonus = ItemLevel / 5;
            }
            finally { group.Characters_Locker.ExitReadLock(); }
            Creatures_PowerMultiplier = group.Characters_Number;

            Console.WriteLine("\t\t\t>Creating Area ({0}*{1})..", Creatures_PowerMultiplier, Creatures_PowerBonus);

            Thread = new Thread(new ThreadStart(Main));
            Thread.Priority = ThreadPriority.Highest;
            Thread.Start();
        }

        public Area(string filename)
        {
            Regions_Size = 256;
            Regions_Multiplier = 512 / Regions_Size;
            Regions_Neighbourhood = new Size((Program.AREA_NEIGHBOURHOOD_WIDTH / 512) * Regions_Multiplier, (Program.AREA_NEIGHBOURHOOD_HEIGHT / 512) * Regions_Multiplier);


            FileName = filename;
            Creatures_PowerBonus = Program.AREA_CREATURESPOWER;
            Creatures_PowerMultiplier = Program.AREA_CREATURESMULTIPLIER;

            Console.WriteLine("\t\t\t>Loading Area ({0}*{1})..", Creatures_PowerMultiplier, Creatures_PowerBonus);

            Thread = new Thread(new ThreadStart(Main));
            Thread.Priority = ThreadPriority.Highest;
            Thread.Start();
        }

        public Area(Group group, string filename)
        {
            Regions_Size = 256;
            Regions_Multiplier = 512 / Regions_Size;
            Regions_Neighbourhood = new Size((Program.AREA_NEIGHBOURHOOD_WIDTH / 512) * Regions_Multiplier, (Program.AREA_NEIGHBOURHOOD_HEIGHT / 512) * Regions_Multiplier);

            FileName = filename;
            group.Characters_Locker.EnterReadLock();
            try
            {
                uint ItemLevel = 0;
                foreach (Character NextCharacter in group.Characters)
                    ItemLevel += NextCharacter.ItemLevel;

                Creatures_PowerBonus = ItemLevel / 5;
            }
            finally { group.Characters_Locker.ExitReadLock(); }
            Creatures_PowerMultiplier = group.Characters_Number;

            Console.WriteLine("\t\t\t>Loading Area ({0}*{1})..", Creatures_PowerMultiplier, Creatures_PowerBonus);

            Thread = new Thread(new ThreadStart(Main));
            Thread.Priority = ThreadPriority.Highest;
            Thread.Start();
        }
        private void Main()
        {
            if (FileName != "") Load();
            else Create(5, 5);

            Update();
        }


        private static int[, ,] TileGrids;
        public static void LoadTileGrids()
        {
            TileGrids = new int[11, 32, 32];
            string[] Buff;
            for (int Current = 0; Current < 11; Current++)
            {
                try
                {
                    StreamReader GridFile = new StreamReader(@"data\grids\" + Current + ".data");
                    for (int Column = 0; Column < 32; Column++)
                    {
                        Buff = GridFile.ReadLine().Split(' ');
                        for (int Row = 0; Row < 32; Row++)
                            TileGrids[Current, Row, Column] = Convert.ToInt32(Buff[Row]);
                    }
                    GridFile.Close();
                }
                catch (Exception E)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" ! Error while loading Tile Grid{0} :\n{1}", Current, E.Message);
                }
            }
        }


        private void Load()
        {
            Loading = true;
            DateTime LoadTime = DateTime.Now;

            StreamReader WorldFile = new StreamReader(@"data\areas\" + FileName + ".data");
            Name = WorldFile.ReadLine();

            DateTime StartTime = DateTime.Now;

            string[] Arguments = WorldFile.ReadLine().Split('\t');
            MapSize = new System.Drawing.Size(Convert.ToInt32(Arguments[0]), Convert.ToInt32(Arguments[1]));
            MapGrid = new int[MapSize.Width * 32, MapSize.Height * 32];
            for (int Row = 0; Row < MapSize.Height * 32; Row++)
            {
                Arguments = WorldFile.ReadLine().Split(' ');
                for (int Column = 0; Column < MapSize.Width * 32; Column++)
                    MapGrid[Column, Row] = Convert.ToInt32(Arguments[Column]);
            }

            MapTiles = new int[MapSize.Width, MapSize.Height];
            for (int Column = 0; Column < MapSize.Height; Column++)
                for (int Row = 0; Row < MapSize.Width; Row++)
                    MapTiles[Row, Column] = Column * MapSize.Width + Row;

            Console.WriteLine("\t\t\t\t>Loaded Grid in {0}", (DateTime.Now - StartTime).TotalMilliseconds);

            Regions = new Region[MapSize.Width * Regions_Multiplier + 1, MapSize.Height * Regions_Multiplier + 1];
            StartTime = DateTime.Now;
            for (int Column = 0; Column <= MapSize.Height * Regions_Multiplier; Column++)
                for (int Row = 0; Row <= MapSize.Width * Regions_Multiplier; Row++)
                    Regions[Row, Column] = new Region(this, new Point(Row, Column));

            Console.WriteLine("\t\t\t\t>Regions Created in {0}", (DateTime.Now - StartTime).TotalMilliseconds);


            StartTime = DateTime.Now;
            string Command = WorldFile.ReadLine();
            while (Command != null)
            {
                Run(Command);
                Command = WorldFile.ReadLine();
            }

            Console.WriteLine("\t\t\t\t>Scripts Finished in {0}", (DateTime.Now - StartTime).TotalMilliseconds);

            WorldFile.Close();

            Loading = false;
        }

        private void Create(int Width, int Height)
        {
            Loading = true;

            DateTime StartTime = DateTime.Now;
            Name = "Hole";
            FileName = "hole";
            MapSize = new System.Drawing.Size(Width * 3, Height * 3);
            MapGrid = new int[MapSize.Width * 32, MapSize.Height * 32];
            MapTiles = new int[Width * 3, Height * 3];

            int[,] Directions = new int[4, 2] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };

            int End = 1;
            int[,] NextRoom = new int[Width * Height, 2];
            int[, ,] RoomData = new int[Width, Height, 5];

            NextRoom[0, 0] = Width / 2; NextRoom[0, 1] = Height / 2;
            for (int Current = 0; Current < 4; Current++)
                RoomData[NextRoom[0, 0], NextRoom[0, 1], Current] = 1;

            for (int Current = 0; Current < Width; Current++)
                RoomData[Current, 0, 3] = 2;
            for (int Current = 0; Current < Width; Current++)
                RoomData[Current, Height - 1, 1] = 2;
            for (int Current = 0; Current < Height; Current++)
                RoomData[0, Current, 2] = 2;
            for (int Current = 0; Current < Height; Current++)
                RoomData[Width - 1, Current, 0] = 2;

            MapTiles[NextRoom[0, 0] * 3 + 1, NextRoom[0, 1] * 3 + 1] = 1;
            MapTiles[NextRoom[0, 0] * 3 + 2, NextRoom[0, 1] * 3 + 2] = 6;
            MapTiles[NextRoom[0, 0] * 3 + 0, NextRoom[0, 1] * 3 + 2] = 7;
            MapTiles[NextRoom[0, 0] * 3 + 0, NextRoom[0, 1] * 3 + 0] = 8;
            MapTiles[NextRoom[0, 0] * 3 + 2, NextRoom[0, 1] * 3 + 0] = 9;

            for (int Direction = 0; Direction < 4; Direction++)
            {
                NextRoom[End, 0] = NextRoom[0, 0] + Directions[Direction, 0];
                NextRoom[End, 1] = NextRoom[0, 1] + Directions[Direction, 1];
                RoomData[NextRoom[End, 0], NextRoom[End, 1], (Direction + 2) % 4] = 1;
                RoomData[NextRoom[End, 0], NextRoom[End, 1], 4] = 2;
                End++;
                MapTiles[NextRoom[0, 0] * 3 + 1 + Directions[Direction, 0], NextRoom[0, 1] * 3 + 1 + Directions[Direction, 1]] = 1;
            }
            RoomData[NextRoom[0, 0], NextRoom[0, 1], 4] = 1;
            Console.WriteLine("\t\t\t\t>Initialized in {0} ms!", (DateTime.Now - StartTime).TotalMilliseconds);

            StartTime = DateTime.Now;
            Random Random = new Random();
            for (int Current = 1; Current < End; Current++)
            {
                MapTiles[NextRoom[Current, 0] * 3 + 1, NextRoom[Current, 1] * 3 + 1] = 10;
                MapTiles[NextRoom[Current, 0] * 3 + 2, NextRoom[Current, 1] * 3 + 2] = 6;
                MapTiles[NextRoom[Current, 0] * 3 + 0, NextRoom[Current, 1] * 3 + 2] = 7;
                MapTiles[NextRoom[Current, 0] * 3 + 0, NextRoom[Current, 1] * 3 + 0] = 8;
                MapTiles[NextRoom[Current, 0] * 3 + 2, NextRoom[Current, 1] * 3 + 0] = 9;

                for (int Direction = 0; Direction < 4; Direction++)
                {
                    if (RoomData[NextRoom[Current, 0], NextRoom[Current, 1], Direction] == 0)
                    {
                        if (RoomData[NextRoom[Current, 0] + Directions[Direction, 0], NextRoom[Current, 1] + Directions[Direction, 1], 4] == 1)
                        {
                            if (RoomData[NextRoom[Current, 0] + Directions[Direction, 0], NextRoom[Current, 1] + Directions[Direction, 1], (Direction + 2) % 4] != 0)
                                RoomData[NextRoom[Current, 0], NextRoom[Current, 1], Direction] = RoomData[NextRoom[Current, 0] + Directions[Direction, 0], NextRoom[Current, 1] + Directions[Direction, 1], (Direction + 2) % 4];
                        }
                        else
                            RoomData[NextRoom[Current, 0], NextRoom[Current, 1], Direction] = Random.Next(0, 2) + 1;
                    }

                    if (RoomData[NextRoom[Current, 0], NextRoom[Current, 1], Direction] == 1)
                    {
                        if (RoomData[NextRoom[Current, 0] + Directions[Direction, 0], NextRoom[Current, 1] + Directions[Direction, 1], 4] == 0)
                        {
                            NextRoom[End, 0] = NextRoom[Current, 0] + Directions[Direction, 0];
                            NextRoom[End, 1] = NextRoom[Current, 1] + Directions[Direction, 1];
                            RoomData[NextRoom[End, 0], NextRoom[End, 1], (Direction + 2) % 4] = 1;
                            RoomData[NextRoom[End, 0], NextRoom[End, 1], 4] = 2;
                            End++;
                        }
                        MapTiles[NextRoom[Current, 0] * 3 + 1 + Directions[Direction, 0], NextRoom[Current, 1] * 3 + 1 + Directions[Direction, 1]] = 1;
                    }
                    else MapTiles[NextRoom[Current, 0] * 3 + 1 + Directions[Direction, 0], NextRoom[Current, 1] * 3 + 1 + Directions[Direction, 1]] = Direction + 2;
                }

                RoomData[NextRoom[Current, 0], NextRoom[Current, 1], 4] = 1;
            }

            Console.WriteLine("\t\t\t\t>Dungeon Generated in {0} ms!", (DateTime.Now - StartTime).TotalMilliseconds);
            //DrawDungeon(Width, Height, MapTiles);

            StartTime = DateTime.Now;
            for (int Column = 0; Column < MapSize.Height; Column++)
                for (int Row = 0; Row < MapSize.Width; Row++)
                    for (int TileColumn = 0; TileColumn < 32; TileColumn++)
                        for (int TileRow = 0; TileRow < 32; TileRow++)
                            MapGrid[Row * 32 + TileRow, Column * 32 + TileColumn] = TileGrids[MapTiles[Row, Column], TileRow, TileColumn];
            Console.WriteLine("\t\t\t\t>Grid Created in {0}", (DateTime.Now - StartTime).TotalMilliseconds);

            Regions = new Region[MapSize.Width * Regions_Multiplier + 1, MapSize.Height * Regions_Multiplier + 1];
            StartTime = DateTime.Now;
            for (int Column = 0; Column <= MapSize.Height * Regions_Multiplier; Column++)
                for (int Row = 0; Row <= MapSize.Width * Regions_Multiplier; Row++)
                    Regions[Row, Column] = new Region(this, new Point(Row, Column));
            Console.WriteLine("\t\t\t\t>Regions Created in {0}", (DateTime.Now - StartTime).TotalMilliseconds);

            StartTime = DateTime.Now;
            for (int Room = 1; Room < End; Room++)
            {
                int InRoom = Random.Next(3) + 1;
                for (int Current = 0; Current < InRoom; Current++)
                {
                    Creature Creature = new Creature(this, NextRoom[Room, 0] * 3 * 512 + Random.Next(256, 1280), NextRoom[Room, 1] * 3 * 512 + Random.Next(256, 1280));

                    Creatures_Number++;
                    Creature.Area = this;
                    Creature.Region = GetRegion(Creature.Location);

                    Creature.Mind.NextPath();
                    Creature.Region.Creatures.Add(Creature);
                    Creature.Region.Creatures_Number++;
                }
            }
            Console.WriteLine("\t\t\t\t>Creatures Added in {0}", (DateTime.Now - StartTime).TotalMilliseconds);


            Loading = false;
        }

        private void Run(string Command)
        {
            string[] Arguments = Command.Split('\t');
            switch (Arguments[0])
            {
                case "AREA_ADDCREATURE": Creatures_Add(new Creature(this, Convert.ToInt32(Arguments[1]), Convert.ToInt32(Arguments[2]), Convert.ToInt32(Arguments[3]))); break;
                case "AREA_ADDPERSON": Persons_Add(new Person(this, Command.Split(new char[] { '\t' }, 2)[1])); break;
                case "AREA_ADDPORTAL": Portal NewPortal = new Portal(Command.Split(new char[] { '\t' }, 2)[1]); GetRegion(NewPortal.Location).Portals.Add(NewPortal); break;
            }
        }

        public int Regions_Updated = 0;
        private Thread Thread;
        private void Update()
        {
            DateTime LastUpdate = DateTime.Now;
            DateTime LastMindUpdate = DateTime.Now;

            while (true)
            {
                UpdateLocker.EnterWriteLock();
                Process_RemovingCharacters();
                Process_TeleportingCharacters();
                Process_LeavingCharacters();
                Process_RemovingAgents();
                Process_TeleportingAgents();
                Process_LeavingAgents();
                Process_RemovingMissiles();
                Process_RemovingSplashes();
                UpdateLocker.ExitWriteLock();

                TimeSpan ElapsedTime = DateTime.Now - LastUpdate;
                LastUpdate = DateTime.Now;

                if (Heroes_Number != 0)
                    if (ElapsedTime.TotalMilliseconds < Program.AREA_MAXLATENCY)
                    {
                        for (int Column = 0; Column <= MapSize.Height * Regions_Multiplier; Column++)
                            for (int Row = 0; Row <= MapSize.Width * Regions_Multiplier; Row++)
                                if (Regions[Row, Column].Influence != 0)
                                {
                                    Regions_Updated++;
                                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Regions[Row, Column].Update), ElapsedTime.TotalMilliseconds);
                                    Regions[Row, Column].Update(ElapsedTime.TotalMilliseconds);
                                }
                    }
                    else
                    {
                        Console.WriteLine(" ! {0} Update : {1}!", FileName, ElapsedTime.TotalMilliseconds);
                        for (int Column = 0; Column <= MapSize.Height * Regions_Multiplier; Column++)
                            for (int Row = 0; Row <= MapSize.Width * Regions_Multiplier; Row++)
                                if (Regions[Row, Column].PrevUpdate > 20)
                                {
                                    Console.WriteLine(" ->Region [{0}:{1}] {2}ms", Row, Column, Regions[Row, Column].PrevUpdate);
                                    for (int Current = 0; Current < 9; Current++)
                                        Console.WriteLine("\t ->Update{0}: {1}ms", Current, Regions[Row, Column].PrevUpdates[Current]);
                                }
                    }

                while (Regions_Updated != 0) Thread.Sleep(5);

                TimeSpan ElapsedMindTime = DateTime.Now - LastMindUpdate;
                if (Program.AREA_MINDUPDATE < ElapsedMindTime.TotalMilliseconds)
                {
                    UpdateLocker.EnterReadLock();
                    try
                    {
                        for (int Column = 0; Column < MapSize.Height * Regions_Multiplier + 1; Column++)
                            for (int Row = 0; Row < MapSize.Width * Regions_Multiplier + 1; Row++)
                                if (Regions[Row, Column].Influence != 0)
                                    Regions[Row, Column].ProcessMinds(ElapsedTime.TotalMilliseconds);
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(Regions[Row, Column].ProcessMinds), ElapsedTime.TotalMilliseconds);
                    }
                    finally { UpdateLocker.ExitReadLock(); }

                    LastMindUpdate = DateTime.Now;
                }

                UpdateLocker.EnterWriteLock();
                Process_AddingCharacters();
                Process_AddingAgents();
                Process_AddingMissiles();
                Process_AddingSplashes();
                UpdateLocker.ExitWriteLock();

                System.Threading.Thread.Sleep(Program.AREA_UPADTE);
            }
        }


        public void SendData(Character Character)
        {
            Character.Connection.Send(Connection.Command.Loading, Name + "\t" + MapSize.Width + "\t" + MapSize.Height);
            Character.Connection.Send(Connection.Command.Hero_SetPosition, Character.Name + "\t" + (int)Character.Location.X + "\t" + (int)Character.Location.Y + "\t" + Character.Rotation + "\t" + 0);

            for (int Column = 0; Column < MapSize.Height; Column++)
                for (int Row = 0; Row < MapSize.Width; Row++)
                    Character.Connection.Send(Connection.Command.LoadTile, Row + "\t" + Column + "\t" + MapTiles[Row, Column]);

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Character.Region.Index.X + Row) && (Character.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Character.Region.Index.Y + Column) && (Character.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                        {
                            Regions[Character.Region.Index.X + Row, Character.Region.Index.Y + Column].SendEnterData(Character);
                            Character.Broadcast_Enter(Regions[Character.Region.Index.X + Row, Character.Region.Index.Y + Column]);
                        }

            Character.Connection.Send(Connection.Command.Loaded, "!");
        }

        public void SendData(Agent Agent)
        {
            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Agent.Region.Index.X + Row) && (Agent.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Agent.Region.Index.Y + Column) && (Agent.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            Agent.Broadcast_Enter(Regions[Agent.Region.Index.X + Row, Agent.Region.Index.Y + Column]);
        }


        public bool IsValidGroundLocation(PointF Location)
        {
            if (MapGrid[(int)(Location.X / 16), (int)(Location.Y / 16)] == 0) return true;
            return false;
        }

        public bool IsValidAirLocation(PointF Location)
        {
            if (MapGrid[(int)(Location.X / 16), (int)(Location.Y / 16)] != 1) return true;
            return false;
        }

        public PointF GetCollisionPoint(PointF StartPoint, double Rotation, double Length)
        {
            PointF Location = StartPoint;
            PointF NextLocation = StartPoint;
            PointF Vector = new PointF((float)(Math.Cos((double)Rotation / 180 * Math.PI)), (float)(Math.Sin((double)Rotation / 180 * Math.PI)));
            for (int Current = 0; Current < Length; Current++)
            {
                NextLocation.X += Vector.X;
                NextLocation.Y -= Vector.Y;

                if (IsValidGroundLocation(NextLocation)) Location = NextLocation;
                else return Location;
            }
            return Location;
        }

        public Region GetRegion(PointF Location)
        {
            return Regions[(int)(Location.X / Regions_Size), (int)(Location.Y / Regions_Size)];
        }


        public void Hero_Move(Hero Hero)
        {
            Hero.Region_Moving = false;
            Region NextRegion = GetRegion(Hero.Location);

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Hero.Region.Index.X + Row) && (Hero.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Hero.Region.Index.Y + Column) && (Hero.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(NextRegion.Index.X - Regions_Neighbourhood.Width, NextRegion.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(Hero.Region.Index.X + Row, Hero.Region.Index.Y + Column)))
                            {
                                Character Character = Hero as Character;
                                if (Character != null) Regions[Hero.Region.Index.X + Row, Hero.Region.Index.Y + Column].SendLeaveData(Character);
                                Regions[Hero.Region.Index.X + Row, Hero.Region.Index.Y + Column].BroadcastCommand(Connection.Command.Hero_Remove, Hero.Name);
                            }

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= NextRegion.Index.X + Row) && (NextRegion.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= NextRegion.Index.Y + Column) && (NextRegion.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(Hero.Region.Index.X - Regions_Neighbourhood.Width, Hero.Region.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(NextRegion.Index.X + Row, NextRegion.Index.Y + Column)))
                            {
                                Character Character = Hero as Character;
                                if (Character != null) Regions[NextRegion.Index.X + Row, NextRegion.Index.Y + Column].SendEnterData(Character);

                                Hero.Broadcast_Enter(Regions[NextRegion.Index.X + Row, NextRegion.Index.Y + Column]);
                            }

            Hero.Region.Heroes_Remove(Hero);
            Hero.Region = NextRegion;
            NextRegion.Heroes_Add(Hero);
        }

        public void Person_Move(Person Person)
        {
            Person.Region_Moving = false;
            Region NextRegion = GetRegion(Person.Location);

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Person.Region.Index.X + Row) && (Person.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Person.Region.Index.Y + Column) && (Person.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(NextRegion.Index.X - Regions_Neighbourhood.Width, NextRegion.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(Person.Region.Index.X + Row, Person.Region.Index.Y + Column)))
                                Regions[Person.Region.Index.X + Row, Person.Region.Index.Y + Column].BroadcastCommand(Connection.Command.Creature_Remove, Person.ID.ToString());

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= NextRegion.Index.X + Row) && (NextRegion.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= NextRegion.Index.Y + Column) && (NextRegion.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(Person.Region.Index.X - Regions_Neighbourhood.Width, Person.Region.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(NextRegion.Index.X + Row, NextRegion.Index.Y + Column)))
                                Person.Broadcast_Enter(Regions[NextRegion.Index.X + Row, NextRegion.Index.Y + Column]);

            Person.Region.Persons_Remove(Person);
            Person.Region = NextRegion;
            NextRegion.Persons_Add(Person);
        }

        public void Creature_Move(Creature Creature)
        {
            Creature.Region_Moving = false;
            Region NextRegion = GetRegion(Creature.Location);

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Creature.Region.Index.X + Row) && (Creature.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Creature.Region.Index.Y + Column) && (Creature.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(NextRegion.Index.X - Regions_Neighbourhood.Width, NextRegion.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(Creature.Region.Index.X + Row, Creature.Region.Index.Y + Column)))
                                Regions[Creature.Region.Index.X + Row, Creature.Region.Index.Y + Column].BroadcastCommand(Connection.Command.Creature_Remove, Creature.ID.ToString());

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= NextRegion.Index.X + Row) && (NextRegion.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= NextRegion.Index.Y + Column) && (NextRegion.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(Creature.Region.Index.X - Regions_Neighbourhood.Width, Creature.Region.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(NextRegion.Index.X + Row, NextRegion.Index.Y + Column)))
                                Creature.Broadcast_Enter(Regions[NextRegion.Index.X + Row, NextRegion.Index.Y + Column]);

            Creature.Region.Creatures_Remove(Creature);
            Creature.Region = NextRegion;
            NextRegion.Creatures_Add(Creature);
        }

        public void Missile_Move(Missile Missile)
        {
            Region NextRegion = GetRegion(Missile.Location);

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Missile.Region.Index.X + Row) && (Missile.Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Missile.Region.Index.Y + Column) && (Missile.Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(NextRegion.Index.X - Regions_Neighbourhood.Width, NextRegion.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(Missile.Region.Index.X + Row, Missile.Region.Index.Y + Column)))
                                Regions[Missile.Region.Index.X + Row, Missile.Region.Index.Y + Column].BroadcastCommand(Connection.Command.Missile_Remove, Missile.ID.ToString());

            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= NextRegion.Index.X + Row) && (NextRegion.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= NextRegion.Index.Y + Column) && (NextRegion.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            if (!Region.Collide(new RectangleF(Missile.Region.Index.X - Regions_Neighbourhood.Width, Missile.Region.Index.Y - Regions_Neighbourhood.Height,
                                2 * Regions_Neighbourhood.Width + 1, 2 * Regions_Neighbourhood.Height + 1), new PointF(NextRegion.Index.X + Row, NextRegion.Index.Y + Column)))
                                Regions[NextRegion.Index.X + Row, NextRegion.Index.Y + Column].BroadcastCommand(Connection.Command.Missile_Add, Missile.GetData());

            Missile.Region.Missiles_Remove(Missile);
            Missile.Region = NextRegion;
            NextRegion.Missiles_Add(Missile);
        }


        public void BroadcastCommand(Connection.Command Command, string Data)
        {
            foreach (Region NextRegion in Regions)
                NextRegion.BroadcastCommand(Command, Data);
        }

        public void BroadcastCommand(Region Region, Connection.Command Command, string Data)
        {
            for (int Column = -Regions_Neighbourhood.Height; Column <= Regions_Neighbourhood.Height; Column++)
                for (int Row = -Regions_Neighbourhood.Width; Row <= Regions_Neighbourhood.Width; Row++)
                    if ((0 <= Region.Index.X + Row) && (Region.Index.X + Row <= MapSize.Width * Regions_Multiplier))
                        if ((0 <= Region.Index.Y + Column) && (Region.Index.Y + Column <= MapSize.Height * Regions_Multiplier))
                            Regions[Region.Index.X + Row, Region.Index.Y + Column].BroadcastCommand(Command, Data);
        }
    }
}
