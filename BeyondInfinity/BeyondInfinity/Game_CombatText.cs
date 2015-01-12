using System;

namespace BeyondInfinity
{
    public class CombatText
    {
        public int Value;
        private double Duration = 1500;

        public CombatText(int value)
        {
            Value = value;
        }

        public bool Update(double ElapsedTime)
        {
            Duration -= ElapsedTime;
            return 0 < Duration ? false : true;
        }
    }
}
