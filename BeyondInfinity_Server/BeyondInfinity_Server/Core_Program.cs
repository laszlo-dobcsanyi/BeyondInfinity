using System;
using System.IO;

namespace BeyondInfinity_Server
{
    public sealed class Program
    {
        public const int THREADPOOL_MIN = 64;
        public const int THREADPOOL_MAX = 128;

        public const int AREA_UPADTE = 5;
        public const int AREA_MAXLATENCY = 50;
        public const int AREA_MINDUPDATE = 100;
        public const int AREA_NEIGHBOURHOOD_WIDTH = 512;
        public const int AREA_NEIGHBOURHOOD_HEIGHT = 512;

        public const double MIND_TURNCHANCE = 0.7;
        public const double MIND_TARGETCHANCE = 0.7;

        public const int CALLTOARMS_START = 1 * 60 * 1000;
        public const int CALLTOARMS_END = 5 * 60 * 1000;

        public const double CALLTOARMS_MULTIPLIER = 1;
        public const int HONORABLEKILL_BONUS = 1000;

        public const int AGENTS_NUMBER = 512;
        public const int CHARACTERS_NUMBER = 64;
        public const int CAPACITY = CHARACTERS_NUMBER + AGENTS_NUMBER * 3;

        public const int PERSONS_NAMESNUMBER = 3;
        public const int CREATURES_NAMESNUMBER = 16;

        public static string[] PERSONS_NAMES = new string[PERSONS_NAMESNUMBER] { "leader0", "leader1", "leader2" };
        public static string[] CREATURES_NAMES = new string[CREATURES_NAMESNUMBER] { "abomination", "bluespider", "bonewraith", "daemon", "dreadlord", "greenspider", "lich", "raider", "summoner",
            "boss_gangnam", "boss_csirigli_1", "boss_csirigli_2", "boss_csirigli_3", "boss_csirigli_4", "boss_csirigli_5","boss_yoga" };

        public const double BONUS_RANDOMRANK = 0.3f;

        public const double MULTIPLIER_MISSILE = 0.2f;
        public const double MULTIPLIER_TARGET = 0f;
        public const double MULTIPLIER_AREA = -0.2f;

        public const int AREA_CREATURESPOWER = 100;
        public const int AREA_CREATURESMULTIPLIER = 50;

        public static void Main()
        {
            Banner();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" ->Configurating..");

            Area.LoadTileGrids();
            Console.WriteLine("\t<- Tile Grids loaded!");

            Person.LoadPersonsData();
            Console.WriteLine("\t<- Persons' Data loaded!");

            Creature.LoadCreaturesData();
            Console.WriteLine("\t<- Creatures' Data loaded!");

            GameManager.Initialize();
            Console.WriteLine("\t<- Game Manager initialized!");

            DatabaseManager.Connect();
            Console.WriteLine("\t<- Database Manager initialized!");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("<- Configurated!");

            NetworkManager.OpenGateway();

            Random = new Random();
            Spawner = new System.Timers.Timer(100);
            Spawner.Elapsed += new System.Timers.ElapsedEventHandler(Spawner_Elapsed);
            Spawner.Start();

            do
            {
                ProcessCommand(Console.ReadLine());
                //BroadcastMessage(Console.ReadLine());
            } while (true);
        }

        private static int Added = 0;
        private static Random Random;
        private static System.Timers.Timer Spawner;
        private static void Spawner_Elapsed(object Sender, System.Timers.ElapsedEventArgs Event)
        {
            if (Added == AGENTS_NUMBER) Spawner.Stop();
            else
            {
                for (uint Faction = 0; Faction < 3; Faction++)
                    GameManager.Agents_Add(new Agent(Faction));

                Added++;
                Spawner.Interval = (Random.NextDouble() +1) * 100;

                Console.WriteLine(" + {0}. Agent added ({1})!", Added, Added * 3);
            }
        }

        private static void BroadcastMessage(string Message)
        {
            foreach (Faction NextFaction in GameManager.Factions)
            {
                NextFaction.Characters_Locker.EnterReadLock();
                try
                {
                    foreach (Character NextCharacter in NextFaction.Characters)
                        NextCharacter.Connection.Send(Connection.Command.Chat, "s:<<SERVER>>: " + Message + "\n");
                }
                finally { NextFaction.Characters_Locker.ExitReadLock(); }
            }
        }

        private static void ProcessCommand(string Command)
        {
            string[] Arguments = Command.Split(new char[] { ' ' }, 3);

            if (2 < Arguments.Length)
                if (Arguments[0] == "get_agent_nods")
                {
                    Area Area = null;
                    if (Arguments[1] == "oldworld") Area = GameManager.World;
                    if (Arguments[1] == "keep0") Area = GameManager.Keeps[0];
                    if (Arguments[1] == "keep1") Area = GameManager.Keeps[1];
                    if (Arguments[1] == "keep2") Area = GameManager.Keeps[2];

                    if (Area != null)
                    {
                        Console.WriteLine(">Getting Agent {0}..", Arguments[2]);

                        Agent Agent = null;
                        foreach (Region NextRegion in Area.Regions)
                        {
                            Agent = Area.Agents_Get(NextRegion, Arguments[2]);
                            if (Agent != null) break;
                        }

                        if (Agent != null)
                        {
                            Console.WriteLine("{0}:{1} -> {2} ({3})", Agent.Location.X, Agent.Location.Y, Agent.Rotation, Agent.Mind.MaxNods);
                            Agent.Mind.Nods_Locker.EnterReadLock();
                            try
                            {
                                for (int Current = 0; Current < Agent.Mind.Nods.Length / 2; Current++)
                                    Console.WriteLine("{0}{1} {2}", Agent.Mind.CurrentNod == Current ? "> " : Current.ToString() + " ", Agent.Mind.Nods[Current, 0] * 16, Agent.Mind.Nods[Current, 1] * 16);
                            }
                            finally { Agent.Mind.Nods_Locker.ExitReadLock(); }
                        }
                        else Console.WriteLine(">Agent not found!");
                    }
                    else Console.WriteLine(">Area not found!");
                }

            if (Arguments[0] == "get_area_updates")
            {
                Console.WriteLine("World Updates: {0}/{1}!", GameManager.World.Regions_Updated, (GameManager.World.MapSize.Width * GameManager.World.Regions_Multiplier + 1) * (GameManager.World.MapSize.Height * GameManager.World.Regions_Multiplier + 1));
                Console.WriteLine("Keep0 Updates: {0}/{1}!", GameManager.Keeps[0].Regions_Updated, (GameManager.Keeps[0].MapSize.Width * GameManager.Keeps[0].Regions_Multiplier + 1) * (GameManager.Keeps[0].MapSize.Height * GameManager.Keeps[0].Regions_Multiplier + 1));
                Console.WriteLine("Keep1 Updates: {0}/{1}!", GameManager.Keeps[1].Regions_Updated, (GameManager.Keeps[1].MapSize.Width * GameManager.Keeps[1].Regions_Multiplier + 1) * (GameManager.Keeps[1].MapSize.Height * GameManager.Keeps[1].Regions_Multiplier  + 1));
                Console.WriteLine("Keep2 Updates: {0}/{1}!", GameManager.Keeps[2].Regions_Updated, (GameManager.Keeps[2].MapSize.Width * GameManager.Keeps[2].Regions_Multiplier + 1) * (GameManager.Keeps[2].MapSize.Height * GameManager.Keeps[2].Regions_Multiplier + 1));
            }
            else Console.WriteLine(">Unknown Command!");
        }

        private static void Banner()
        {
            Console.Title = "Beyond Infinity Server";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"               ____                                     __     ");
            Console.WriteLine(@"              /\  _`\                                  /\ \    ");
            Console.WriteLine(@"              \ \ \L\ \    __  __  __    ___    ___    \_\ \   ");
            Console.WriteLine(@"               \ \  _ <' /'__`\\ \/\ \  / __`\/' _ `\  /'_` \  ");
            Console.WriteLine(@"                \ \ \L\ \\  __/ \ \_\ \/\ \L\ \\ \/\ \/\ \L\ \ ");
            Console.WriteLine(@"                 \ \____/ \____\/`____ \ \____/ \_\ \_\ \___,_\");
            Console.WriteLine(@"                  \/___/ \/____/`/___/> \/___/ \/_/\/_/\/__,_ /");
            Console.WriteLine(@"                                   /\___/                      ");
            Console.WriteLine(@"                                   \/__/                       ");
            Console.WriteLine();
            Console.WriteLine(@"           ______             ___                 __                ");
            Console.WriteLine(@"          /\__  _\          /'___\ __          __/\ \__             ");
            Console.WriteLine(@"          \/_/\ \/     ___ /\ \__//\_\    ___ /\_\ \ ,_\  __  __    ");
            Console.WriteLine(@"             \ \ \   /' _ `\ \ ,__\/\ \ /' _ `\/\ \ \ \/ /\ \/\ \   ");
            Console.WriteLine(@"              \_\ \__/\ \/\ \ \ \_/\ \ \/\ \/\ \ \ \ \ \_\ \ \_\ \  ");
            Console.WriteLine(@"              /\_____\ \_\ \_\ \_\  \ \_\ \_\ \_\ \_\ \__\\/`____ \ ");
            Console.WriteLine(@"              \/_____/\/_/\/_/\/_/   \/_/\/_/\/_/\/_/\/__/ `/___/> \");
            Console.WriteLine(@"                                                             /\___/");
            Console.WriteLine(@"                                                             \/__/ ");
        }
    }
}
