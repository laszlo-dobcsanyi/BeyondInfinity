using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public delegate void MouseMoveProcess(MouseEventArgs Event);
    public delegate void MouseUpProcess(MouseEventArgs Event);
    public delegate void KeyDownProcess(KeyEventArgs Event);
    public delegate void KeyUpProcess(KeyEventArgs Event);

    public partial class GameForm : Form
    {
        public static MouseMoveProcess MouseMoveProcessor = null;
        public static MouseUpProcess MouseUpProcessor = null;
        public static KeyDownProcess KeyDownProcessor = null;
        public static KeyUpProcess KeyUpProcessor = null;

        protected override void OnMouseMove(MouseEventArgs Event)
        {
            if (MouseMoveProcessor != null)
                MouseMoveProcessor(Event);
        }

        protected override void OnMouseUp(MouseEventArgs Event)
        {
            if (MouseUpProcessor != null)
                MouseUpProcessor(Event);
        }

        private bool ChatMode = false;
        protected override void OnKeyDown(KeyEventArgs Event)
        {
            if (!ChatMode)
                if (KeyDownProcessor != null)
                    KeyDownProcessor(Event);
        }

        protected override void OnKeyUp(KeyEventArgs Event)
        {
            if (ChatMode)
                Chat_KeyUp(Event);
            else
                if (KeyUpProcessor != null)
                    KeyUpProcessor(Event);
        }


        #region KeyDown
        public void KeyDown_CastModifier(KeyEventArgs Event)
        {
            if (Event.KeyCode == Keys.ShiftKey)
            {
                Target = true;
                return;
            }

            if (Event.KeyCode == Keys.ControlKey)
                Splash = true;
        }
        #endregion

        #region KeyUp
        public void KeyUp_Spell_ID(KeyEventArgs Event)
        {
            if (Casting_Spell == -1)
                for (int Current = 0; Current < 6; Current++)
                    if (Game.Character != null)
                        if (Game.Character.Spells[Current].Cooldown == 0)
                            if (Game.Character.Spells[Current].Key == Event.KeyValue)
                            {
                                Casting_Spell = Current;
                                return;
                            }

            if (Event.KeyCode == Keys.F5)
                Program.Send(160, "!");
        }

        public void KeyUp_Spell_Rank(KeyEventArgs Event)
        {
            if (Casting_Spell != -1)
                if (Casting_Rank == -1)
                    switch (Event.KeyCode)
                    {
                        case Keys.D1: Casting_Rank = 0; break;
                        case Keys.D2: Casting_Rank = 1; break;
                        case Keys.D3: Casting_Rank = 2; break;
                        case Keys.D4: Casting_Rank = 3; break;
                        case Keys.D5: Casting_Rank = 4; break;
                        case Keys.D6: Casting_Rank = 5; break;
                    }
        }

        public void KeyUp_CastModifier(KeyEventArgs Event)
        {
            if (Event.KeyCode == Keys.ShiftKey)
                Target = false;

            if (Event.KeyCode == Keys.ControlKey)
                Splash = false;
        }

        public void KeyUp_Panel_Control(KeyEventArgs Event)
        {
            switch (Event.KeyCode)
            {
                case Keys.F1:
                    if (CurrentPanel != Panels.Character)
                    {
                        CurrentPanel = Panels.Character;
                        Offset = new Point(192, 0);
                    }
                    else
                    {
                        CurrentPanel = Panels.None;
                        Offset = new Point(0, 0);
                    }
                    break;

                case Keys.F2:
                    if (CurrentPanel != Panels.Attributes)
                    {
                        CurrentPanel = Panels.Attributes;
                        Offset = new Point(192, 0);
                    }
                    else
                    {
                        CurrentPanel = Panels.None;
                        Offset = new Point(0, 0);
                    }
                    break;

                case Keys.F3:
                    if (CurrentPanel != Panels.Group)
                    {
                        CurrentPanel = Panels.Group;
                        Offset = new Point(-192, 0);
                    }
                    else
                    {
                        CurrentPanel = Panels.None;
                        Offset = new Point(0, 0);
                    }
                    break;


                case Keys.F4:
                    if (CurrentPanel != Panels.Quest)
                    {
                        CurrentPanel = Panels.Quest;
                        Offset = new Point(-192, 0);
                    }
                    else
                    {
                        CurrentPanel = Panels.None;
                        Offset = new Point(0, 0);
                    }
                    break;
            }
        }

        public void KeyUp_Objects(KeyEventArgs Event)
        {
            switch (Event.KeyCode)
            {
                case Keys.I:
                    Hero TargetHero = GetClickedHero(CursorLocation);
                    if (TargetHero != null)
                        Program.Send(20, TargetHero.Name);
                    return;

                case Keys.U:
                    TargetHero = GetClickedHero(CursorLocation);
                    if (TargetHero != null)
                        Program.Send(21, TargetHero.Name);
                    return;
            }
        }

        public void KeyUp_CharacterPanel(KeyEventArgs Event)
        {
            if (CurrentPanel == Panels.Character)
            {
                if (Event.KeyCode == Keys.D)
                {
                    for (uint Column = 0; Column < 2; Column++)
                        for (uint Row = 0; Row < 5; Row++)
                            if (Collide(new RectangleF(Row * 64 + 14, Column * 64 + 640 - 14, 64, 64), CursorLocation))
                            {
                                Program.Send(36, (Column * 5 + Row).ToString());
                                return;
                            }
                }
            }
        }

        public void KeyUp_Chat(KeyEventArgs Event)
        {
            if (Event.KeyCode == Keys.Enter)
                ChatMode = true;
        }

        private char[] big = new char[26] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private char[] small = new char[26] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private char[] number = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private char[] special = new char[10] { '§', '\'', '"', '+', '!', '%', '/', '=', '(', ')' };

        private char[] mark0 = new char[3] { ',', '-', '.' };
        private char[] mark1 = new char[3] { '?', '_', ':' };

        private int pointer = 0;
        private char[] chars = new char[256];
        private string Token = "/s";
        private void Chat_KeyUp(KeyEventArgs Event)
        {
            if (!Event.Shift)
            {
                if (64 < Event.KeyValue && Event.KeyValue < 92)
                {
                    chars[pointer] = small[Event.KeyValue - 65];
                    pointer++;
                }

                if (47 < Event.KeyValue && Event.KeyValue < 58)
                {
                    chars[pointer] = number[Event.KeyValue - 48];
                    pointer++;
                }

                if (187 < Event.KeyValue && Event.KeyValue < 191)
                {
                    chars[pointer] = mark0[Event.KeyValue - 188];
                    pointer++;
                }
            }
            else
            {
                if (64 < Event.KeyValue && Event.KeyValue < 92)
                {
                    chars[pointer] = big[Event.KeyValue - 65];
                    pointer++;
                }

                if (47 < Event.KeyValue && Event.KeyValue < 58)
                {
                    chars[pointer] = special[Event.KeyValue - 48];
                    pointer++;
                }

                if (187 < Event.KeyValue && Event.KeyValue < 191)
                {
                    chars[pointer] = mark1[Event.KeyValue - 188];
                    pointer++;
                }
            }

            switch (Event.KeyValue)
            {
                case 8: pointer = (pointer == 0 ? 0 : pointer - 1); break;
                case 13:
                    string Text = new string(chars, 0, pointer);
                    string[] Arguments = Text.Split(' ');
                    if (0 < Text.Length)
                        if (Arguments[0][0] == '/')
                        {
                            Token = Arguments[0];

                            if (1 < Arguments.Length)
                                Program.Send(17, Text);
                        }
                        else if (0 < Arguments.Length) Program.Send(17, Token + " " + Text);

                    ChatMode = false;
                    pointer = 0;
                    break;
                case 27: ChatMode = false; break;
                case 32: chars[pointer] = ' '; pointer++; break;
            }
        }
        #endregion

        #region MouseUp
        public void MouseUp_Character_Move(MouseEventArgs Event)
        {
            if ((CursorLocation.X < ClientSize.Width + 2 * Offset.X) && (0 + 2 * Offset.X < CursorLocation.X))
            {
                if (!(CurrentPanel == Panels.Dialog && Collide(new RectangleF(128, 72, 768, 512), CursorLocation)))
                    if (Event.Button == MouseButtons.Left)
                    {
                        if ((ClientSize.Width - 64) / 2 + Offset.X < CursorLocation.X)
                            if (CursorLocation.X < (ClientSize.Width + 64) / 2 + Offset.X)
                                if ((ClientSize.Height - 64) / 2 + Offset.Y < CursorLocation.Y)
                                    if (CursorLocation.Y < (ClientSize.Height + 64) / 2 + Offset.Y)
                                    {
                                        Program.Send(103, ".");
                                        return;
                                    }

                        double Degree = 0;

                        if (CursorLocation.X < ClientSize.Width / 2 + Offset.X)
                            Degree = (Math.PI - Math.Atan((double)(CursorLocation.Y - ClientSize.Height / 2 - Offset.Y) / (double)(CursorLocation.X - ClientSize.Width / 2 - Offset.X))) / Math.PI * 180;
                        if (ClientSize.Width / 2 + Offset.X < CursorLocation.X)
                            Degree = (-Math.Atan((double)(CursorLocation.Y - ClientSize.Height / 2 - Offset.Y) / (double)(CursorLocation.X - ClientSize.Width / 2 - Offset.X))) / Math.PI * 180;

                        if (Degree < 0)
                            Degree += 360;

                        if (CursorLocation.X == ClientSize.Width / 2 + Offset.X)
                            if (CursorLocation.Y < ClientSize.Height / 2 + Offset.Y)
                                Degree = 90;
                            else Degree = 270;

                        Program.Send(102, ((int)Degree).ToString());
                        if (CurrentPanel == Panels.Dialog) CurrentPanel = Panels.None;
                    }
            }
        }

        public void MouseUp_Character_Cast(MouseEventArgs Event)
        {
            if ((CursorLocation.X < ClientSize.Width + 2 * Offset.X) && (0 + 2 * Offset.X < CursorLocation.X))
            {
                if (!(CurrentPanel == Panels.Dialog && Collide(new RectangleF(128, 72, 768, 512), CursorLocation)))

                    if (Event.Button == MouseButtons.Right)
                    {
                        if (Casting_Rank != -1)
                        {
                            if (Target)
                            {
                                Hero TargetHero = GetClickedHero(CursorLocation);
                                if (TargetHero != null)
                                    if (Math.Sqrt(Math.Pow(Game.Character.Location.X - TargetHero.Location.X, 2) + Math.Pow(Game.Character.Location.Y - TargetHero.Location.Y, 2))
                                        < 200 + Game.Character.Spells[Casting_Spell].Parameters[0] / 2)
                                        Program.Send(13, Casting_Spell + "\t" + Casting_Rank + "\t" + TargetHero.Name);
                                    else Game.Messages_Add(0);
                                else
                                {
                                    Person TargetPerson = GetClickedPerson(CursorLocation);
                                    if (TargetPerson != null)
                                        if (Math.Sqrt(Math.Pow(Game.Character.Location.X - TargetPerson.Location.X, 2) + Math.Pow(Game.Character.Location.Y - TargetPerson.Location.Y, 2))
                                            < 200 + Game.Character.Spells[Casting_Spell].Parameters[0] / 2)
                                            Program.Send(14, Casting_Spell + "\t" + Casting_Rank + "\t" + TargetPerson.ID);
                                        else Game.Messages_Add(0);
                                    else
                                    {
                                        Creature TargetCreature = GetClickedCreature(CursorLocation);
                                        if (TargetCreature != null)
                                            if (Math.Sqrt(Math.Pow(Game.Character.Location.X - TargetCreature.Location.X, 2) + Math.Pow(Game.Character.Location.Y - TargetCreature.Location.Y, 2))
                                                < 200 + Game.Character.Spells[Casting_Spell].Parameters[0] / 2)
                                                Program.Send(15, Casting_Spell + "\t" + Casting_Rank + "\t" + TargetCreature.ID);
                                            else Game.Messages_Add(0);
                                    }
                                }

                                Casting_Spell = -1;
                                Casting_Rank = -1;
                                return;
                            }

                            if (Splash)
                            {
                                if (Math.Sqrt(Math.Pow(ClientSize.Width / 2 - CursorLocation.X - Offset.X, 2) + Math.Pow(ClientSize.Height / 2 - CursorLocation.Y - Offset.Y, 2))
                                    < 150 + Game.Character.Spells[Casting_Spell].Parameters[0] / 2)
                                    Program.Send(16, Casting_Spell + "\t" + Casting_Rank + "\t" + (Game.Character.Location.X - ClientSize.Width / 2 + CursorLocation.X - Offset.X) + "\t"
                                        + (Game.Character.Location.Y - ClientSize.Height / 2 + CursorLocation.Y - Offset.Y));
                                else Game.Messages_Add(0);

                                Casting_Spell = -1;
                                Casting_Rank = -1;
                                return;
                            }

                            double Degree = 0;

                            if (CursorLocation.X < ClientSize.Width / 2 + Offset.X)
                                Degree = (Math.PI - Math.Atan((double)(CursorLocation.Y - ClientSize.Height / 2 - Offset.Y) / (double)(CursorLocation.X - ClientSize.Width / 2 - Offset.X))) / Math.PI * 180;
                            if (ClientSize.Width / 2 + Offset.X < CursorLocation.X)
                                Degree = (-Math.Atan((double)(CursorLocation.Y - ClientSize.Height / 2 - Offset.Y) / (double)(CursorLocation.X - ClientSize.Width / 2 - Offset.X))) / Math.PI * 180;

                            if (Degree < 0)
                                Degree += 360;

                            if (CursorLocation.X == ClientSize.Width / 2 + Offset.X)
                                if (CursorLocation.Y < ClientSize.Height / 2 + Offset.Y)
                                    Degree = 90;
                                else Degree = 270;

                            Program.Send(12, Casting_Spell + "\t" + Casting_Rank + "\t" + Degree);

                            Casting_Spell = -1;
                            Casting_Rank = -1;
                        }
                        else
                        {
                            if (CurrentPanel == Panels.Dialog) CurrentPanel = Panels.None;

                            Flag TargetFlag = GetClickedFlag(CursorLocation);
                            if (TargetFlag != null)
                                if (TargetFlag.Owner != Game.Character.Faction)
                                    if (Math.Sqrt(Math.Pow(TargetFlag.Location.X - Game.Character.Location.X, 2) + Math.Pow(TargetFlag.Location.Y - Game.Character.Location.Y, 2)) < 64)
                                        Program.Send(155, TargetFlag.ID.ToString());

                            Corpse TargetCorpse = GetClickedCorpse(CursorLocation);
                            if (TargetCorpse != null)
                                if (Game.Character.Speed == 0)
                                    if (Math.Sqrt(Math.Pow(TargetCorpse.Location.X - Game.Character.Location.X, 2) + Math.Pow(TargetCorpse.Location.Y - Game.Character.Location.Y, 2)) < 64)
                                    {
                                        Program.Send(32, TargetCorpse.ID.ToString());
                                        Game.Corpse_Looting = TargetCorpse;
                                        TargetCorpse.Looting = true;

                                        CurrentPanel = Panels.Character;
                                        Offset = new Point(192, 0);
                                    }

                            if (Game.Character.Speed == 0)
                            {
                                TargetPerson = GetClickedPerson(CursorLocation);
                                if (TargetPerson != null)
                                    if (Math.Sqrt(Math.Pow(TargetPerson.Location.X - Game.Character.Location.X, 2) + Math.Pow(TargetPerson.Location.Y - Game.Character.Location.Y, 2)) < 128)
                                        Program.Send(18, TargetPerson.ID + "\t" + (byte)0);
                            }


                            Game.Target_Locker.EnterWriteLock();
                            try
                            {
                                Game.Target = GetClickedHero(CursorLocation);
                                if (Game.Target == null)
                                {
                                    Game.Target = GetClickedPerson(CursorLocation);
                                    if (Game.Target == null) Game.Target = GetClickedCreature(CursorLocation);
                                }
                            }
                            finally { Game.Target_Locker.ExitWriteLock(); }
                        }
                    }
            }
        }

        public void MouseUp_CharacterPanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Character)
                if (Event.Button == MouseButtons.Right)
                {
                    //Loot
                    if (Game.Character.Speed == 0)
                        for (uint Current = 0; Current < 5; Current++)
                            if (Collide(new RectangleF(Current * 64 + 14, 576 - 30, 64, 64), CursorLocation))
                                if (Game.Character.Equipment_Loot[Current] != null)
                                {
                                    Program.Send(30, Current + "\t" + Game.Corpse_Looting.ID);
                                    return;
                                }
                    //Equip
                    for (uint Column = 0; Column < 2; Column++)
                        for (uint Row = 0; Row < 5; Row++)
                            if (Collide(new RectangleF(Row * 64 + 14, Column * 64 + 640 - 14, 64, 64), CursorLocation))
                                if (Game.Character.Equipment_Backpack[Column * 5 + Row] != null)
                                {
                                    Program.Send(31, (Column * 5 + Row).ToString());
                                    return;
                                }
                }
        }

        public void MouseUp_AttributePanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Attributes)
            {
                if (Event.Button == MouseButtons.Left)
                {
                    if (Collide(new RectangleF(16, 256 - 8 + 0 * 24, 300, 24), CursorLocation)) { School = School == 5 ? 0 : School + 1; return; }
                    if (Collide(new RectangleF(16, 256 - 8 + 1 * 24, 300, 24), CursorLocation)) { Slot = Slot == 7 ? 0 : Slot + 1; return; }
                    if (Collide(new RectangleF(16, 256 - 8 + 2 * 24, 300, 24), CursorLocation)) { Attributes_Number = Attributes_Number == 3 ? 1 : Attributes_Number + 1; return; }

                    if (Collide(new RectangleF(16, 256 - 8 + 4 * 24, 300, 24), CursorLocation)) { Attributes[0] = Attributes[0] == 6 ? 0 : Attributes[0] + 1; return; }
                    if (1 < Attributes_Number) if (Collide(new RectangleF(16, 256 - 8 + 5 * 24, 300, 24), CursorLocation)) { Attributes[1] = Attributes[1] == 6 ? 0 : Attributes[1] + 1; return; }
                    if (2 < Attributes_Number) if (Collide(new RectangleF(16, 256 - 8 + 6 * 24, 300, 24), CursorLocation)) { Attributes[2] = Attributes[2] == 6 ? 0 : Attributes[2] + 1; return; }
                }

                if (Event.Button == MouseButtons.Right)
                {
                    if (Collide(new RectangleF(16, 256 - 8 + 0 * 24, 300, 24), CursorLocation)) { School = School == 0 ? 5 : School - 1; return; }
                    if (Collide(new RectangleF(16, 256 - 8 + 1 * 24, 300, 24), CursorLocation)) { Slot = Slot == 0 ? 7 : Slot - 1; return; }
                    if (Collide(new RectangleF(16, 256 - 8 + 2 * 24, 300, 24), CursorLocation)) { Attributes_Number = Attributes_Number == 1 ? 3 : Attributes_Number - 1; return; }

                    if (Collide(new RectangleF(16, 256 - 8 + 4 * 24, 300, 24), CursorLocation)) { Attributes[0] = Attributes[0] == 0 ? 6 : Attributes[0] - 1; return; }
                    if (1 < Attributes_Number) if (Collide(new RectangleF(16, 256 - 8 + 5 * 24, 300, 24), CursorLocation)) { Attributes[1] = Attributes[1] == 0 ? 6 : Attributes[1] - 1; return; }
                    if (2 < Attributes_Number) if (Collide(new RectangleF(16, 256 - 8 + 6 * 24, 300, 24), CursorLocation)) { Attributes[2] = Attributes[2] == 0 ? 6 : Attributes[2] - 1; return; }
                }

                if (Collide(new RectangleF(96, 256 + 8 * 24, 160, 32), CursorLocation))
                {
                    string Attributes_Data = Attributes_Number.ToString();
                    for (int Current = 0; Current < Attributes_Number; Current++)
                        Attributes_Data += "\t" + Attributes[Current];
                    Program.Send(33, Slot + "\t" + School + "\t" + Attributes_Data);
                    return;
                }
            }
        }

        public void MouseUp_GroupPanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Group)
                if (!InArenaQueue)
                    if (Collide(new Rectangle(ClientSize.Width - 256, 448 + 4, 448 - 256 - 32, 64), CursorLocation))
                        Program.Send(22, "!");
        }

        public void MouseUp_DialogPanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Dialog)
                if (CurrentAnswer != -1)
                    Program.Send(18, TargetPerson.ID + "\t" + (byte)Answers[CurrentAnswer]);
        }
        #endregion

        #region MouseMove
        public void MouseMove_CursorMove(MouseEventArgs Event)
        {
            CursorLocation = Event.Location;
        }

        public void MouseMove_CharacterPanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Character)
            {
                //Equipped Items
                foreach (Slot NextSlot in Slots)
                    if (Collide(new RectangleF(NextSlot.Location, new SizeF(64, 64)), CursorLocation))
                    {
                        Showing_Item = Game.Character.Equipped[NextSlot.SlotID];
                        return;
                    }
                //Spells
                for (int Column = 0; Column < 2; Column++)
                    for (int Row = 0; Row < 3; Row++)
                        if (Collide(new RectangleF(Row * 72 + 6 + 64, Column * 72 + 448 - 38 - 32, 64, 64), CursorLocation))
                        {
                            Showing_Spell = Game.Character.Spells[Row * 2 + Column];
                            return;
                        }
                Showing_Spell = null;


                //Loot Items
                for (int Current = 0; Current < 5; Current++)
                    if (Collide(new RectangleF(Current * 64 + 14, 576 - 30, 64, 64), CursorLocation))
                    {
                        Showing_Item = Game.Character.Equipment_Loot[Current] as Item;
                        Showing_Spell = Game.Character.Equipment_Loot[Current] as Spell;
                        return;
                    }

                //Backpack Items
                for (int Column = 0; Column < 2; Column++)
                    for (int Row = 0; Row < 5; Row++)
                        if (Collide(new RectangleF(Row * 64 + 14, Column * 64 + 640 - 14, 64, 64), CursorLocation))
                        {
                            Showing_Item = Game.Character.Equipment_Backpack[Column * 5 + Row] as Item;
                            Showing_Spell = Game.Character.Equipment_Backpack[Column * 5 + Row] as Spell;
                            return;
                        }

                Showing_Item = null;
                Showing_Spell = null;
            }
        }

        public void MouseMove_DialogPanel(MouseEventArgs Event)
        {
            if (CurrentPanel == Panels.Dialog)
                for (int Current = 0; Current < Answers.Length; Current++)
                    if (Collide(new Rectangle(512 + 16, 72 + 16 + Current * 64, 384 - 32, 64), CursorLocation))
                    {
                        CurrentAnswer = Current;
                        return;
                    }
            CurrentAnswer = -1;
        }
        #endregion

        private bool Collide(RectangleF Rectangle, PointF Point)
        {
            if (Rectangle.X <= Point.X)
                if (Rectangle.Y <= Point.Y)
                    if (Point.X <= Rectangle.X + Rectangle.Width)
                        if (Point.Y <= Rectangle.Y + Rectangle.Height)
                            return true;
            return false;
        }
       
        public Hero GetClickedHero(Point Location)
        {
            if (Collide(new RectangleF(Offset.X - 32, Offset.Y - 32, 64, 64), new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                return Game.Character;

            Game.Heroes_Locker.EnterReadLock();
            try
            {
                foreach (Hero NextHero in Game.Heroes)
                    if (Collide(new RectangleF(NextHero.Location.X - Game.Character.Location.X + Offset.X - 32,
                        NextHero.Location.Y - Game.Character.Location.Y + Offset.Y - 32, 64, 64),
                        new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                        return NextHero;
            }
            finally { Game.Heroes_Locker.ExitReadLock(); }
            return null;
        }

        public Person GetClickedPerson(Point Location)
        {
            Game.Persons_Locker.EnterReadLock();
            try
            {
                foreach (Person NextPerson in Game.Persons)
                    if (Collide(new RectangleF(NextPerson.Location.X - Game.Character.Location.X + Offset.X - 32,
                        NextPerson.Location.Y - Game.Character.Location.Y + Offset.Y - 32, 64, 64),
                        new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                        return NextPerson;
            }
            finally { Game.Persons_Locker.ExitReadLock(); }
            return null;
        }

        public Creature GetClickedCreature(Point Location)
        {
            Game.Creatures_Locker.EnterReadLock();
            try
            {
                foreach (Creature NextCreature in Game.Creatures)
                    if (Collide(new RectangleF(NextCreature.Location.X - Game.Character.Location.X + Offset.X - 32,
                        NextCreature.Location.Y - Game.Character.Location.Y + Offset.Y - 32, 64, 64),
                        new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                        return NextCreature;
            }
            finally { Game.Creatures_Locker.ExitReadLock(); }
            return null;
        }

        public Corpse GetClickedCorpse(Point Location)
        {
            Game.Corpses_Locker.EnterReadLock();
            try
            {
                foreach (Corpse NextCorpse in Game.Corpses)
                    if (Collide(new RectangleF(NextCorpse.Location.X - Game.Character.Location.X + Offset.X - 32,
                        NextCorpse.Location.Y - Game.Character.Location.Y + Offset.Y - 32, 64, 64),
                        new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                        return NextCorpse;
            }
            finally { Game.Corpses_Locker.ExitReadLock(); }
            return null;
        }

        public Flag GetClickedFlag(Point Location)
        {
            if (Game.Battlefield != -1)
            {
                foreach (Flag NextFlag in Game.Battlefields[Game.Battlefield].Flags)
                    if (Collide(new RectangleF(NextFlag.Location.X - Game.Character.Location.X + Offset.X - 32,
                        NextFlag.Location.Y - Game.Character.Location.Y + Offset.Y - 32, 64, 64),
                        new PointF(Location.X - ClientSize.Width / 2, Location.Y - ClientSize.Height / 2)))
                        return NextFlag;
            }
            return null;
        }
    }
}
