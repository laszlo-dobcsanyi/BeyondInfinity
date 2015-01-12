using System;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class Unit
    {
        public uint Faction;
        public string Name;
        public Texture Icon;
        public uint ItemLevel;

        public double Energy;
        public double MaxEnergy;

        public PointF Location;
        public double Rotation;
        public double Speed;

        public void Update(double ElapsedTime)
        {
            Location.X += (float)(ElapsedTime / 1000 * Speed * Math.Cos((double)Rotation / 180 * Math.PI));
            Location.Y -= (float)(ElapsedTime / 1000 * Speed * Math.Sin((double)Rotation / 180 * Math.PI));

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    NextMark.Update(ElapsedTime);
            }
            finally { Marks_Locker.ExitReadLock(); }

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    NextImpact.Update(ElapsedTime);
            }
            finally { Impacts_Locker.ExitReadLock(); }

            List<CombatText> RemovableCombatTexts = new List<CombatText>();
            CombatTexts_Locker.EnterReadLock();
            try
            {
                foreach (CombatText NextCombatText in CombatTexts)
                    if (NextCombatText.Update(ElapsedTime) == true)
                        RemovableCombatTexts.Add(NextCombatText);
            }
            finally { CombatTexts_Locker.ExitReadLock(); }

            foreach (CombatText NextCombatText in RemovableCombatTexts)
                CombatTexts_Remove(NextCombatText);
        }

        /// <summary>
        /// Only if add CombatText!
        /// </summary>
        public void Energy_Set(int Value)
        {
            int Difference = Value - (int)Energy;

            Energy = Value;
            CombatTexts_Add(new CombatText(Difference));
        }
    }
}
