using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace BeyondInfinity_Server
{
    public sealed class NetworkManager
    {
        public static Socket Gateway;
        public static IPAddress ServerAddress;
        public static Generator PortGenerator = new Generator(Program.CHARACTERS_NUMBER);

        public static void OpenGateway()
        {
            //ServerAddress = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[2];
            //ServerAddress = IPAddress.Parse("192.168.1.102");
            ServerAddress = IPAddress.Loopback;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" ->Opening Gateway @ [{0}:{1}]..", ServerAddress, 1425);

            try
            {
                Gateway = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Gateway.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                Gateway.Bind(new IPEndPoint(ServerAddress, 1425));
            }
            catch (Exception E)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" ! Fatal error while setting up Gateway :\n" + E.Message);

                Console.ReadLine();
                Environment.Exit(1);
            }

            Listener = new Thread(new ThreadStart(Listen));
            Listener.Start();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("<- Gateway Opened!");
        }

        private static Thread Listener;
        private static void Listen()
        {
            ThreadPool.SetMinThreads(Program.THREADPOOL_MIN, 4);
            ThreadPool.SetMaxThreads(Program.THREADPOOL_MAX, 8);

            try
            {
                while (true)
                {
                    EndPoint Client = new IPEndPoint(IPAddress.Any, 0);
                    byte[] Data = new byte[256];

                    int ReceivedBytes = Gateway.ReceiveFrom(Data, ref Client);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(Process), new LoginData(Client, ReceivedBytes, Data));
                }
            }
            catch (Exception E)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" ! Fatal Gateway Error: " + E.Message);
            }
        }

        private static void Process(object data)
        {
            LoginData LoginData = (LoginData)data;

            string[] Command = Encoding.Unicode.GetString(LoginData.Data, 0, LoginData.Received).Split(new char[] { ':' }, 2);

            if (Command[0] == "0")
            {
                string[] Arguments = Command[1].Split('\t');
                try
                {
                    StreamReader AccountFile = new StreamReader(@"data\accounts\" + Arguments[0] + ".data");
                    if (AccountFile.ReadLine() == Arguments[1])
                    {

                        int NextPort = (int)(1425 + 1 + PortGenerator.Next() * 2);

                        LoginData.Data = BitConverter.GetBytes(NextPort);

                        System.Threading.Thread.Sleep(1000);
                        Gateway.SendTo(LoginData.Data, LoginData.Client);

                        Connection Connection = new Connection(((IPEndPoint)LoginData.Client).Address, NextPort, AccountFile.ReadLine());
                        AccountFile.Close();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\tConnection ! Error while Authenticating : Wrong Password?");
                    }

                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\tConnection ! Error while Authenticating : Wrong User Name?");
                }
            }
            else
            {
                string[] Arguments = Command[1].Split('\t');
                Console.WriteLine("\tRegistrating {0}..", Arguments[2]);
                int Faction = Convert.ToInt32(Arguments[3]);
                int School = Convert.ToInt32(Arguments[5]);

                try
                {
                    StreamWriter HeroFile = new StreamWriter(@"data\heroes\" + Arguments[2] + ".data");
                    HeroFile.WriteLine(Arguments[3]);
                    HeroFile.WriteLine(Arguments[4]);
                    HeroFile.WriteLine("5000\n0");

                    for (int Current = 0; Current < 6; Current++)
                        HeroFile.WriteLine("200\t200\t200\t" + ((Current < 4 ? Current : Current + 2) * 6 + School) + "\t200\t200\t200");

                    HeroFile.WriteLine("8");
                    for (int Current = 0; Current < 8; Current++)
                        HeroFile.WriteLine(new Item(Current, School, 30).GetData());

                    HeroFile.WriteLine("0");
                    HeroFile.Close();
                }
                catch (Exception E)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\tConnection ! Error while Registrating Hero : " + E.Message);
                    return;
                }

                try
                {
                    StreamWriter AccountFile = new StreamWriter(@"data\accounts\" + Arguments[0] + ".data");
                    AccountFile.WriteLine(Arguments[1]);
                    AccountFile.WriteLine(Arguments[2]);
                    AccountFile.Close();
                }
                catch (Exception E)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\tConnection ! Error while Registrating Account : " + E.Message);
                    return;
                }

                LoginData.Data = BitConverter.GetBytes(0);
                Gateway.SendTo(LoginData.Data, LoginData.Client);
            }
        }

        private sealed class LoginData
        {
            public EndPoint Client;
            public int Received;
            public byte[] Data;

            public LoginData(EndPoint client,int received, byte[] data)
            {
                Client = client;
                Received = received;
                Data = data;
            }
        }
    }
}
