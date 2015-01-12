using System;
using System.Net;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class RegistrateForm : Form
    {
        private TextBox Name_Box;
        private Button RegistrateButton;

        public RegistrateForm()
        {
            InitializeForm();
            InitializeGraphics();
            InitializeContent();
        }

        private void InitializeForm()
        {
            Text = "Beyond Infinity - Registration";
            Size = new Size(600 + 6, 600 + 24);
            MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.WindowsDefaultLocation;
            Icon = BeyondInfinity.Properties.Resources.Icon;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

        private void InitializeGraphics()
        {
            PresentParameters PresentParams = new PresentParameters();
            PresentParams.Windowed = true;
            PresentParams.SwapEffect = SwapEffect.Discard;

            Device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, PresentParams);
        }

        private void InitializeContent()
        {
            for (int Current = 0; Current < 3; Current++)
                Factions[Current] = TextureLoader.FromFile(Device, @"data\interface\flag_" + (Current + 1) + ".png");

            for (int Faction = 0; Faction < 3; Faction++)
                for (int Current = 0; Current < 24; Current++)
                    HeroIcons[Faction, Current] = TextureLoader.FromFile(Device, @"data\icons\hero" + Faction + "_" + Current + ".png");

            for (int Current = 0; Current < 6; Current++)
                Schools[Current] = TextureLoader.FromFile(Device, @"data\icons\school_" + Current + ".png");

            for (int Current = 0; Current < 9; Current++)
                Box[Current] = TextureLoader.FromFile(Device, @"data\interface\box_" + Current + ".png");

            DrawingFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 10f, FontStyle.Regular));


            //NameBox 
            Name_Box = new TextBox();
            Name_Box.Location = new Point(300 - 64, 564);
            Name_Box.Size = new Size(128, 24);
            Name_Box.TextAlign = HorizontalAlignment.Center;

            //RegistrateButton
            RegistrateButton = new Button();
            RegistrateButton.Location = new Point(464, 562);
            RegistrateButton.Text = "Registrate Character";
            RegistrateButton.Size = new Size(128, 24);
            RegistrateButton.Click += new EventHandler(RegistrateButton_Click);

            Controls.Add(Name_Box);
            Controls.Add(RegistrateButton);
        }

        private void RegistrateButton_Click(object Sender, EventArgs Event)
        {
            if ((Faction != -1) && (Hero != -1) && (School != -1) && (Name_Box.Text != null))
            {
                UdpClient Gateway = new UdpClient();
                byte[] Data = System.Text.Encoding.Unicode.GetBytes("1:" + LoginForm.UserName_TextBox.Text + "\t" + LoginForm.Password_TextBox.Text + "\t"
                    + Name_Box.Text + "\t" + Faction + "\t" + Hero + "\t" + School);
                Gateway.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

                Gateway.Connect(new IPEndPoint(Program.ServerAddress, 1425));
                Gateway.Send(Data, Data.Length);

                try
                {
                    IPEndPoint Client = new IPEndPoint(IPAddress.Any, 0);
                    Data = Gateway.Receive(ref Client);
                    int Response = BitConverter.ToInt32(Data, 0);

                    switch (Response)
                    {
                        case 0: MessageBox.Show(Name_Box.Text + " registrated!\nNow you can log in!", "Registration Successfull", MessageBoxButtons.OK, MessageBoxIcon.Information); break;
                    }
                }
                catch (Exception E)
                {
                    MessageBox.Show("Network Error while Registrating " + Name_Box.Text + "!\n" + E.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Gateway.Close();

                Dispose();
            }
        }
    }
}
