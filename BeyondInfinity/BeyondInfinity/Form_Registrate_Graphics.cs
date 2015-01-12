using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class RegistrateForm : Form
    {
        private Device Device;

        protected override void OnPaint(PaintEventArgs Event)
        {
            //base.OnPaint(e);

            Render();
        }

        private void Render()
        {
            Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            Device.BeginScene();

            Sprite Sprite = new Sprite(Device);
            Sprite.Begin(SpriteFlags.AlphaBlend);

            Draw_Background(Sprite);
            Draw_Factions(Sprite);
            Draw_HeroIcons(Sprite);
            Draw_SpellSchools(Sprite);
            Draw_SchoolSpells(Sprite);
            Draw_SpellTooltip(Sprite);

            Sprite.End();
            Sprite.Dispose();

            Device.EndScene();
            Device.Present();
        }

        private void Draw_Background(Sprite Sprite)
        {

        }

        private int Faction = -1;
        private Texture[] Factions = new Texture[3];
        private void Draw_Factions(Sprite Sprite)
        {
            for (int Current = 0; Current < 3; Current++)
                Sprite.Draw2D(Factions[Current], Point.Empty, 0f, new Point(200 + Current * 68, 16), Faction == Current ? Color.White : Color.Gray);
        }

        private int Hero = -1;
        private Texture[,] HeroIcons = new Texture[3, 24];
        private void Draw_HeroIcons(Sprite Sprite)
        {
            if (Faction != -1)
                for (int Current = 0; Current < 24; Current++)
                    Sprite.Draw2D(HeroIcons[Faction, Current], Point.Empty, 0f, new Point((Current % 6) * 68 + 96, (Current / 6) * 68 + 96), Hero == Current ? Color.White : Color.Gray);
        }

        private int School = -1;
        private Texture[] Schools = new Texture[6];
        private void Draw_SpellSchools(Sprite Sprite)
        {
            if (Hero != -1)
                for (int Current = 0; Current < 6; Current++)
                    Sprite.Draw2D(Schools[Current], Point.Empty, 0f, new Point((Current % 6) * 68 + 96, 392), School == Current ? Color.White : Color.Gray);
        }

        private int TooltipSpell = -1;
        private Texture[] Spells = new Texture[6];
        private void Draw_SchoolSpells(Sprite Sprite)
        {
            if (School != -1)
            {
                for (int Current = 0; Current < 6; Current++)
                    Sprite.Draw2D(Spells[Current], Point.Empty, 0f, new Point((Current % 6) * 68 + 96, 470), Color.White);
            }
        }

        private Microsoft.DirectX.Direct3D.Font DrawingFont;
        private Texture[] Box = new Texture[9];
        private void Draw_SpellTooltip(Sprite Sprite)
        {
            this.Text = TooltipSpell.ToString();
            if (TooltipSpell != -1)
            {
                Point Location = new Point((TooltipSpell % 6) * 68 + 32, 470 - 96 - 8);
                int Width = 6;
                int Height = 3;
                Sprite.Draw2D(Box[0], Point.Empty, 0f, Location, Color.White);
                Sprite.Draw2D(Box[1], Point.Empty, 0f, new Point(Location.X + (Width - 1) * 32, Location.Y), Color.White);
                Sprite.Draw2D(Box[2], Point.Empty, 0f, new Point(Location.X + (Width - 1) * 32, Location.Y + (Height + 1) * 16), Color.White);
                Sprite.Draw2D(Box[3], Point.Empty, 0f, new Point(Location.X, Location.Y + (Height + 1) * 16), Color.White);

                for (int Current = 1; Current < Width - 1; Current++)
                    Sprite.Draw2D(Box[4], Point.Empty, 0f, new Point(Location.X + Current * 32, Location.Y), Color.White);
                for (int Current = 2; Current < Height + 1; Current++)
                    Sprite.Draw2D(Box[5], Point.Empty, 0f, new Point(Location.X + (2 * Width - 1) * 16, Location.Y + Current * 16), Color.White);
                for (int Current = 1; Current < Width - 1; Current++)
                    Sprite.Draw2D(Box[6], Point.Empty, 0f, new Point(Location.X + Current * 32, Location.Y + (Height + 1) * 16), Color.White);
                for (int Current = 2; Current < Height + 1; Current++)
                    Sprite.Draw2D(Box[7], Point.Empty, 0f, new Point(Location.X, Location.Y + Current * 16), Color.White);

                for (int Column = 2; Column < Height + 1; Column++)
                    for (int Row = 1; Row < 2 * Width - 1; Row++)
                        Sprite.Draw2D(Box[8], Point.Empty, 0f, new Point(Location.X + Row * 16, Location.Y + Column * 16), Color.White);


                Location.X += 8;
                Location.Y += 96 + 8;
                Color[] School_Colors = new Color[6] { Color.Red, Color.Purple, Color.LightBlue, Color.Green, Color.Gray, Color.Yellow };
                DrawingFont.DrawText(Sprite, Spell.Names[TooltipSpell * 6 + School + (TooltipSpell > 3 ? 12 : 0)],
                    new Rectangle(Location.X + 8, Location.Y - 96 + 8, 5 * 32, 64), DrawTextFormat.WordBreak, School_Colors[School]);
                DrawingFont.DrawText(Sprite, Spell.Tooltips[TooltipSpell * 6 + School + (TooltipSpell > 3 ? 12 : 0)],
                    new Rectangle(Location.X + 8, Location.Y - 64 + 8, 5 * 32, 64), DrawTextFormat.WordBreak, Color.White);
            }
        }
    }
}
