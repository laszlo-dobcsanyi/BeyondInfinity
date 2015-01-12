using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Equipment
    {
        public uint Slot;
        public string Name;
        public Texture Icon;

        public static Equipment GetEquipment(string Data)
        {
            uint Slot = Convert.ToUInt32(Data.Split('\t')[1]);

            if (Slot < 8) return new Item(Data);
            else return new Spell(Data);
        }
    }
}
