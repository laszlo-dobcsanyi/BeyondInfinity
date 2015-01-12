using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class RegistrateForm : Form
    {
        protected override void OnMouseUp(MouseEventArgs Event)
        {
            //base.OnMouseUp(e);

            //Faction Change
            for (int Current = 0; Current < 3; Current++)
                if ((200 + Current * 68 < Event.X) && (Event.X < 200 + Current * 68 + 64))
                    if ((16 < Event.Y) && (Event.Y < 16 + 64))
                        if (Faction != Current)
                        {
                            Faction = Current;
                            Hero = -1;
                            School = -1;

                            Render();
                            return;
                        }

            //Hero Change
            if (Faction != -1)
                for (int Current = 0; Current < 24; Current++)
                    if (((Current % 6) * 68 + 96 < Event.X) && (Event.X < (Current % 6) * 68 + 96 + 64))
                        if (((Current / 6) * 68 + 96 < Event.Y) && (Event.Y < (Current / 6) * 68 + 96 + 64))
                            if (Hero != Current)
                            {
                                Hero = Current;

                                Render();
                                return;
                            }

            //School Change
            if (Hero != -1)
                for (int Current = 0; Current < 6; Current++)
                    if (((Current % 6) * 68 + 96 < Event.X) && (Event.X < (Current % 6) * 68 + 96 + 64))
                        if ((392 < Event.Y) && (Event.Y < 392 + 64))
                            if (School != Current)
                            {
                                School = Current;
                                for (int CurrentSpell = 0; CurrentSpell < 6; CurrentSpell++)
                                    Spells[CurrentSpell] = TextureLoader.FromFile(Device, @"data\icons\" + Spell.Icon_Names[CurrentSpell * 6 + School + (CurrentSpell > 3 ? 12 : 0)] + ".png");

                                Render();
                                return;
                            }
        }

        protected override void OnMouseMove(MouseEventArgs Event)
        {
            if (School != -1)
                if (470 < Event.Y && Event.Y < 470 + 64)
                    for (int Current = 0; Current < 6; Current++)
                        if ((Current % 6) * 68 + 96 < Event.X && Event.X < (Current % 6) * 68 + 96 + 64)
                        {
                            TooltipSpell = Current;
                            Render();
                            return;
                        }

            if (TooltipSpell != -1)
            {
                TooltipSpell = -1;
                Render();
            }
        }
    }
}
