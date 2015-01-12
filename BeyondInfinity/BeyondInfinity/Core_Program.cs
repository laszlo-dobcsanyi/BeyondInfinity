using System;
using System.IO;
using System.Windows.Forms;

namespace BeyondInfinity
{
    public static partial class Program
    {
        public const int CALLTOARMS_START = 1 * 60 * 1000;
        public const int CALLTOARMS_END = 5 * 60 * 1000;

        public static GameForm GameForm;
        public static bool Terminated = false;

        public static void Main()
        {
            //try
            {
                StreamReader ConfigFile = new StreamReader(@"beyondinfinity.config");
                ServerAddress = System.Net.IPAddress.Parse(ConfigFile.ReadLine());
                ConfigFile.Close();

                while (!Terminated)
                {
                    using (GameForm = new GameForm())
                    {
                        GC.SuppressFinalize(GameForm);

                        Application.Run(new LoginForm());

                        Cursor.Hide();

                        if (Port != 0)
                        {
                            System.Threading.Thread.Sleep(500);

                            Listen();

                            Send(1, "!");

                            GameForm.Show();
                            while ((GameForm.Created) && (!GameForm.Shutdown))
                            {
                                Game.Update();
                                GameForm.Render();
                                Application.DoEvents();

                                if (GameForm.ContainsFocus) { }
                                else System.Threading.Thread.Sleep(20);
                            }
                            GameForm.Shutdown = true;

                            Send(0, ".");

                            Disconnect();
                            System.Threading.Thread.Sleep(250);

                            Game.Dispose(true);
                            GameForm.Dispose();
                            GameForm.Device.Dispose();
                        }
                    }

                    Port = 0;

                    Cursor.Show();
                }
            }
            /*catch (Exception E)
            {
                MessageBox.Show("Global Error: " + E.Message + "\n" + E.Source + "\n" + E.StackTrace + "\n" + E.TargetSite + "\n" + E.Data + "\n" + E.InnerException + "\nReceived:" + Received, "Szólj Lacikának!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Send(0, ".");
                Disconnect();
            }*/
        }
    }
}
