using System;
using System.Net;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace BeyondInfinity
{
    public class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeForm();
            InitializeComponent();
        }

        private void InitializeForm()
        {
            Text = "Beyond Infinity - Login (" + Program.Received + ")";
            ClientSize = new Size(300, 140);
            MaximumSize = new Size(300 + 16, 140 + 32);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = BeyondInfinity.Properties.Resources.Icon;

            FormClosing += new FormClosingEventHandler(LoginForm_FormClosing);
        }

        private Label UserName_Label;
        public static TextBox UserName_TextBox;

        private Label Password_Label;
        public static TextBox Password_TextBox;

        private Button Login_Button;
        private Button Registrate_Button;

        private void InitializeComponent()
        {
            //UserName_Label
            UserName_Label = new Label();
            UserName_Label.Location = new Point(10, 10);
            UserName_Label.AutoSize = true;
            UserName_Label.Text = ">User Name";
            //UserName_TextBox
            UserName_TextBox = new TextBox();
            UserName_TextBox.Location = new Point(10, 30);
            UserName_TextBox.Size = new Size(280, 20);
            UserName_TextBox.CharacterCasing = CharacterCasing.Lower;
            //Password_Label
            Password_Label = new Label();
            Password_Label.Location = new Point(10, 60);
            Password_Label.AutoSize = true;
            Password_Label.Text = ">Password";
            //Password_TextBox
            Password_TextBox = new TextBox();
            Password_TextBox.Location = new Point(10, 80);
            Password_TextBox.Size = new Size(280, 20);
            Password_TextBox.CharacterCasing = CharacterCasing.Lower;
            Password_TextBox.PasswordChar = '*';

            //Login_Button
            Login_Button = new Button();
            Login_Button.Text = "Login";
            Login_Button.Size = new Size(50, 25);
            Login_Button.Location = new Point(240, 105);
            Login_Button.Click += new EventHandler(Login_Button_Click);
            //Registrate_Button
            Registrate_Button = new Button();
            Registrate_Button.Text = "Registrate";
            Registrate_Button.Size = new Size(100, 25);
            Registrate_Button.Location = new Point(10, 105);
            Registrate_Button.Click += new EventHandler(Registrate_Button_Click);

            Controls.Add(UserName_Label);
            Controls.Add(UserName_TextBox);

            Controls.Add(Password_Label);
            Controls.Add(Password_TextBox);

            Controls.Add(Login_Button);
            Controls.Add(Registrate_Button);
        }

        private void Login_Button_Click(object sender, EventArgs Event)
        {
            UdpClient Gateway = new UdpClient();
            byte[] Data = System.Text.Encoding.Unicode.GetBytes("0:" + UserName_TextBox.Text + "\t" + Password_TextBox.Text);
            Gateway.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

            Gateway.Connect(new IPEndPoint(Program.ServerAddress, 1425));
            Gateway.Send(Data, Data.Length);

            try
            {
                IPEndPoint Client = new IPEndPoint(IPAddress.Any, 0);
                Data = Gateway.Receive(ref Client);

                Program.Port = BitConverter.ToUInt16(Data, 0);
            }
            catch
            {
                Program.Port = 0;
            }

            Gateway.Close();

            Dispose();
        }

        RegistrateForm RegistrateForm;
        private void Registrate_Button_Click(object Sender, EventArgs Event)
        {
            if ((UserName_TextBox.Text != null) && (Password_TextBox.Text != null))
            {
                if (RegistrateForm == null)
                {
                    RegistrateForm = new RegistrateForm();
                    RegistrateForm.Show();
                }
                else
                {
                    RegistrateForm.Close();
                    RegistrateForm = null;
                }
            }
        }

        private void LoginForm_FormClosing(object Sender, FormClosingEventArgs Event)
        {
            Program.Terminated = true;
        }
    }
}
