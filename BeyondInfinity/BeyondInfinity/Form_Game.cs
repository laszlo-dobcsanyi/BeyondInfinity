using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public delegate void GraphicalProcess(Device Device);

    public partial class GameForm : Form
    {
        public bool Shutdown = false;

        public GameForm()
        {
            InitializeForm();
            InitializeGraphics();
            InitializeContent();

            Phase_Loading();
        }

        private void InitializeForm()
        {
            Text = "Beyond Infinity";
            Size = new Size(SystemInformation.VirtualScreen.Width - 6, SystemInformation.VirtualScreen.Height - 24);
            MaximumSize = Size;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterScreen;
            Icon = BeyondInfinity.Properties.Resources.Icon;
            //TopMost = true;
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

        private void InitializeGraphics()
        {
            PresentParameters PresentParams = new PresentParameters();
            PresentParams.Windowed = true;
            PresentParams.SwapEffect = SwapEffect.Discard;

            Device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, PresentParams);
        }

        public bool ChatBox_Visible = false;
        private Color[] ColorPalette;
        private void InitializeContent()
        {
            for (int Faction = 0; Faction < 3; Faction++)
                for (int Current = 0; Current < 24; Current++)
                    Hero.Icons[Faction * 24 + Current] = TextureLoader.FromFile(Device, @"data\icons\hero" + Faction + "_" + Current + ".png");

            for (int Current = 0; Current < 8 * 6 + 1; Current++)
                Spell.Icons[Current] = TextureLoader.FromFile(Device, @"data\icons\" + Spell.Icon_Names[Current] + ".png");

            for (int Current = 0; Current < 8 * 6 + 1; Current++)
                Mark.Icons[Current] = TextureLoader.FromFile(Device, @"data\icons\" + Mark.Icon_Names[Current] + ".png");

                CooldownFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 24f, FontStyle.Regular));
            RankFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 28f, FontStyle.Bold));
            KeyFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 16f, FontStyle.Regular));
            MarkFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 40f, FontStyle.Regular));

            DrawingFont = new Microsoft.DirectX.Direct3D.Font(Device, new System.Drawing.Font("Arial Black", 10f, FontStyle.Regular));

            Cursor_Hand = TextureLoader.FromFile(Device, @"data\interface\cursor.png");
            Cursor_Casting = TextureLoader.FromFile(Device, @"data\interface\cursor_casting.png");

            for (int Current = 0; Current < 4; Current++)
                Splashes[Current] = TextureLoader.FromFile(Device, @"data\interface\splash" + Current + ".png");

            for (int Current = 0; Current < 9; Current++)
                Box[Current] = TextureLoader.FromFile(Device, @"data\interface\box_" + Current + ".png");

            Panel_Left = TextureLoader.FromFile(Device, @"data\interface\panel_left.png");
            Panel_Right = TextureLoader.FromFile(Device, @"data\interface\panel_right.png");
            Panel_Dialog = TextureLoader.FromFile(Device, @"data\interface\panel_dialog.png");

            Buttons[0] = TextureLoader.FromFile(Device, @"data\interface\button.png");
            Buttons[1] = TextureLoader.FromFile(Device, @"data\interface\button_disabled.png");

            for (int Current = 0; Current < 9; Current++)
                EmptySlots[Current] = TextureLoader.FromFile(Device, @"data\interface\slot_" + Current + ".png");

            Spell_Selected = TextureLoader.FromFile(Device, @"data\interface\spell_selected.png");
            Spell_Casting = TextureLoader.FromFile(Device, @"data\interface\spell_casting.png");


            Border_Enemy = TextureLoader.FromFile(Device, @"data\interface\border_enemy.png");
            Border_Friend = TextureLoader.FromFile(Device, @"data\interface\border_friend.png");
            Border_Group = TextureLoader.FromFile(Device, @"data\interface\border_group.png");
            Border_Creature = TextureLoader.FromFile(Device, @"data\interface\border_creature.png");
            Border_Supportive = TextureLoader.FromFile(Device, @"data\interface\border_supportive.png");
            Border_Unsupportive = TextureLoader.FromFile(Device, @"data\interface\border_unsupportive.png");

            Corpse = TextureLoader.FromFile(Device, @"data\interface\corpse.png");
            Corpse_Looting = TextureLoader.FromFile(Device, @"data\interface\corpse_looting.png");

            Portal = TextureLoader.FromFile(Device, @"data\interface\portal.png");

            for (int Current = 0; Current < 4; Current++)
                Bases[Current] = TextureLoader.FromFile(Device, @"data\interface\bases_" + Current + ".png"); ;

            for (int Current = 0; Current < 4; Current++)
                Flags[Current] = TextureLoader.FromFile(Device, @"data\interface\flag_" + Current + ".png"); ;

            ColorPalette = new Color[6 * 256];

            int line = 2;
            int[] values = new int[3];

            line = 2;
            values[0] = 0;
            values[1] = 255;
            values[2] = 255;
            for (int Fade = 0; Fade < 6; Fade++)
            {
                if (0 < line) line--;
                else line = 2;

                for (int Row = 0; Row < 256; Row++)
                {
                    ColorPalette[Fade * 256 + Row] = Color.FromArgb(values[0], values[1], values[2]);

                    if (Row != 255)
                    {
                        if (Fade % 2 == 1) values[line]++;
                        else values[line]--;
                    }
                }
            }
        }

        public void Phase_Loading()
        {
            GraphicalProcessor = DrawLoading;

            KeyDownProcessor = null;
            KeyUpProcessor = null;
            MouseMoveProcessor = null;
            MouseUpProcessor = null;

            Target = false;
            Splash = false;
        }

        public void Phase_Game()
        {
            GraphicalProcessor = DrawGame;

            KeyDownProcessor = null;
            KeyDownProcessor += KeyDown_CastModifier;

            KeyUpProcessor = null;
            KeyUpProcessor += KeyUp_Spell_Rank;
            KeyUpProcessor += KeyUp_Spell_ID;
            KeyUpProcessor += KeyUp_CastModifier;
            KeyUpProcessor += KeyUp_Objects;
            KeyUpProcessor += KeyUp_Panel_Control;
            KeyUpProcessor += KeyUp_CharacterPanel;
            KeyUpProcessor += KeyUp_Chat;

            MouseUpProcessor = null;
            MouseUpProcessor += MouseUp_Character_Move;
            MouseUpProcessor += MouseUp_Character_Cast;
            MouseUpProcessor += MouseUp_CharacterPanel;
            MouseUpProcessor += MouseUp_AttributePanel;
            MouseUpProcessor += MouseUp_GroupPanel;
            MouseUpProcessor += MouseUp_DialogPanel;

            MouseMoveProcessor = null;
            MouseMoveProcessor += MouseMove_CursorMove;
            MouseMoveProcessor += MouseMove_CharacterPanel;
            MouseMoveProcessor += MouseMove_DialogPanel;
        }

        public void OpenDialog(string Data)
        {
            string[] Arguments = Data.Split('\t');
            int DialogID = Convert.ToInt32(Arguments[0]);

            if (DialogID != -1)
            {
                Dialog = Person.Dialogs[DialogID];
                Arguments = Arguments[1].Split(',');
                Answers = new int[Arguments.Length];
                for (int Current = 0; Current < Arguments.Length; Current++)
                    Answers[Current] = Convert.ToInt32(Arguments[Current]);

                CurrentPanel = Panels.Dialog;
                Offset = new Point(0, 0);
            }
            else
            {
                CurrentPanel = Panels.None;
                Offset = new Point(0, 0);
            }
        }

        public void Render()
        {
            Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            Device.BeginScene();

            if (GraphicalProcessor != null)
                GraphicalProcessor(Device);

            Device.EndScene();
            Device.Present();
        }
    }
}
