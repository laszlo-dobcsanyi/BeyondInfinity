using System;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Flag
    {
        public uint ID;
        public Point Location;
        public Battlefield Battlefield;
        public int Owner;

        public Flag(uint id, Point location)
        {
            ID = id;
            Location = location;

            Owner = -1;
        }
    }
}
