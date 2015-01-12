using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity_Editor
{
    public enum Mode
    {
        Terrain,
        Object
    }

    public class MainForm : Form
    {
        private MenuItem Menu_New;
        private MenuItem Menu_Load;

        private Device Device;
        private Vector3 Camera = new Vector3(0, 0, 0);
        private int Zoom = 500;

        private Mode Mode = Mode.Terrain;

        private Terrain Terrain;
        private Object Pointer;
        private List<Object> Objects = new List<Object>();

        public MainForm()
        {
            InitializeForm();
            InitializeGraphics();
            InitializeContent();
        }

        private void InitializeForm()
        {
            Text = "Beyond Infinity Editor";
            ClientSize = new Size(1024, 768);
            MaximumSize = ClientSize;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterScreen;

            KeyUp += new KeyEventHandler(MainForm_KeyUp);
            MouseMove += new MouseEventHandler(MainForm_MouseMove);

            Menu_New = new MenuItem("New", Menu_New_Click);
            Menu_Load = new MenuItem("Load", Menu_Load_Click);
            Menu = new MainMenu(new MenuItem[] { Menu_New, Menu_Load });
        }

        private void InitializeGraphics()
        {
            PresentParameters Parameters = new PresentParameters();
            Parameters.Windowed = true;
            Parameters.SwapEffect = SwapEffect.Discard;
            Parameters.EnableAutoDepthStencil = true;
            Parameters.AutoDepthStencilFormat = DepthFormat.D16;

            Device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, Parameters);

            Device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1f, 50000f);

            Device.RenderState.CullMode = Cull.None;

            //   Device.RenderState.SourceBlend = Blend.One;
            //   Device.RenderState.DestinationBlend = Blend.One;

            // Device.RenderState.SourceBlend = Blend.SourceAlpha;
            Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            Device.RenderState.AlphaBlendEnable = true;
          //  Device.RenderState.Ambient = Color.FromArgb(0x202020);

            Device.RenderState.ZBufferEnable = true;
            Device.RenderState.Lighting = false;

            Device.Lights[0].Type = LightType.Directional;
            Device.Lights[0].Diffuse = Color.White;
            Device.Lights[0].Ambient = Color.White;
            Device.Lights[0].Position = new Vector3(1500, 1500, 5000);
            Device.Lights[0].Direction = new Vector3(0, 0, 1);
            Device.Lights[0].Enabled = true;
           // Device.Lights[0].Update();

          /*Device.Lights[1].Type = LightType.Directional;
            Device.Lights[1].Diffuse = Color.White;
            Device.Lights[1].Direction = new Vector3(-1, -1, 1000);
            Device.Lights[1].Enabled = true;
            Device.Lights[1].Update();*/

            //Device.RenderState.FillMode = FillMode.WireFrame;

            //Device.RenderState.Ambient = Color.DarkGray;
        }

        private void InitializeContent()
        {
            Pointer = new Object(Device, "objects","grunt_animated.ms3d", Camera);
            // Terrain = new Terrain(Device);
        }

        private void MainForm_KeyUp(object Sender, KeyEventArgs Event)
        {
            switch (Event.KeyCode)
            {
                case Keys.Down: Camera.Y += 8; break;
                case Keys.Up: Camera.Y -= 8; break;
                case Keys.Right: Camera.X += 8; break;
                case Keys.Left: Camera.X -= 8; break;
                case Keys.Home: LoadData(); break;
                case Keys.PageDown: Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] += 5;
                    Terrain.Vertices[(int)(Camera.X / 64) + (int)(Camera.Y / 64) * Terrain.Size.Width].Z = Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] == 0 ? -5000 : Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] * 8;
                    break;
                case Keys.PageUp:
                    Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] -= 5;
                    Terrain.Vertices[(int)(Camera.X / 64) + (int)(Camera.Y / 64) * Terrain.Size.Width].Z = Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] == 0 ? -5000 : Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] * 8;
                    break;
            }

            if (Camera.X < 0) Camera.X = 0; if (Terrain.Size.Width * 256 < Camera.X) Camera.X = Terrain.Size.Width * 256;
            if (Camera.Y < 0) Camera.Y = 0; if (Terrain.Size.Height * 256 < Camera.Y) Camera.Y = Terrain.Size.Height * 256;

            Pointer.Location = Camera;
            Pointer.Location.Z = Terrain.GetHeight(Pointer.Location.X, Pointer.Location.Y);
            //Pointer.Location.Z = (Terrain.HeightData[(int)(Camera.X/64), (int)(Camera.Y/64)] == 0 ? -5000 : Terrain.HeightData[(int)(Camera.X/64), (int)(Camera.Y/64)] * 8)+500;
            Terrain.Vertices[(int)(Camera.X / 64) + (int)(Camera.Y / 64) * Terrain.Size.Width].Color = Color.FromArgb(Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] * Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] * Terrain.HeightData[(int)(Camera.X / 64), (int)(Camera.Y / 64)] + 20000).ToArgb();
            Device.Lights[0].Position = Camera;
            Device.Lights[0].Update();
        }

        private float Angle = 1.35f;
        private Point PreviousLocation = Cursor.Position;
        private void MainForm_MouseMove(object Sender, MouseEventArgs Event)
        {
          /*Pointer.Rotation += (float)(Cursor.Position.X - PreviousLocation.X) / 1;
            Angle += (float)(PreviousLocation.Y-Cursor.Position.Y) / 1000;
            PreviousLocation = Cursor.Position;*/

            Pointer.Location.X += (float)(Cursor.Position.X - 400) / 100;
            Pointer.Location.Y += (float)(Cursor.Position.Y - 400) / 100;

            if (Terrain != null)
            {
                if (Pointer.Location.X < 0) Pointer.Location.X = 0; if (Terrain.Size.Width * 64-128 < Pointer.Location.X) Pointer.Location.X = Terrain.Size.Width * 64-128;
                if (Pointer.Location.Y < 0) Pointer.Location.Y = 0; if (Terrain.Size.Height * 64-128 < Pointer.Location.Y) Pointer.Location.Y = Terrain.Size.Height * 64-128;
                Pointer.Location.Z = Terrain.GetHeight(Pointer.Location.X, Pointer.Location.Y);
            }
        }

        private void Menu_New_Click(object Sender, EventArgs Event)
        {

        }

        private void Menu_Load_Click(object Sender, EventArgs Event)
        {
            LoadData();
        }

        private void LoadData()
        {
          Terrain = new Terrain(Device, @"C:\Users\Lackoatya\Documents\Visual Studio 2008\Projects\BeyondInfinity\BeyondInfinity\bin\x86\Debug\data\maps\Old World");

            Objects.Add(new Object(Device, "objects","grunt_animated.ms3d", Camera));
            Objects.Add(new Object(Device, "objects","portal.ms3d", Camera));

         /*   FolderBrowserDialog Dialog = new FolderBrowserDialog();
            Dialog.RootFolder = Environment.SpecialFolder.Desktop;
              if (Dialog.ShowDialog() == DialogResult.OK)
                  Terrain = new Terrain(Device, Dialog.SelectedPath);*/
        }

        public void Render()
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            Device.BeginScene();

            if ((Terrain != null) && (Terrain.Loaded))
            {
                Device.Transform.View = Matrix.Translation(-Pointer.Location) * Matrix.RotationZ(Pointer.Rotation / 100) * Matrix.Translation(0, -Zoom, -Zoom) * Matrix.RotationX(-(float)Math.PI / Angle);

                Terrain.Render(Device);
                Pointer.Render(Device);
                foreach (Object NextObject in Objects)
                    NextObject.Render(Device);
            }

            Device.EndScene();
            Device.Present();
        }
    }
}
