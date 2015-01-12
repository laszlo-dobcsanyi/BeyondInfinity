using System;

namespace BeyondInfinity_Server
{
    public sealed partial class Spell
    {
        public SpellEffect GetEffect(uint ID)
        {
            switch (ID)
            {
                case 0: return Direct_Fire;
                case 1: return Direct_Arcane;
                case 2: return Direct_Frost;
                case 3: return Direct_Nature;
                case 4: return Direct_Shadow;
                case 5: return Direct_Holy;

                case 6: return Periodic_Fire;
                case 7: return Periodic_Arcane;
                case 8: return Periodic_Frost;
                case 9: return Periodic_Nature;
                case 10: return Periodic_Shadow;
                case 11: return Periodic_Holy;

                case 12: return Control_Fire;
                case 13: return Control_Arcane;
                case 14: return Control_Frost;
                case 15: return Control_Nature;
                case 16: return Control_Shadow;
                case 17: return Control_Holy;

                case 18: return Evasion_Fire;
                case 19: return Evasion_Arcane;
                case 20: return Evasion_Frost;
                case 21: return Evasion_Nature;
                case 22: return Evasion_Shadow;
                case 23: return Evasion_Holy;

                case 36: return Heal_Fire;
                case 37: return Heal_Arcane;
                case 38: return Heal_Frost;
                case 39: return Heal_Nature;
                case 40: return Heal_Shadow;
                case 41: return Heal_Holy;

                case 42: return Shield_Fire;
                case 43: return Shield_Arcane;
                case 44: return Shield_Frost;
                case 45: return Shield_Nature;
                case 46: return Shield_Shadow;
                case 47: return Shield_Holy;
            }
            return Direct_Fire;
        }

        #region Direct Damage
        public void Direct_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 0));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }

        public void Direct_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 1));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }

        public void Direct_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 2));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }

        public void Direct_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 3));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }

        public void Direct_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 4));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }

        public void Direct_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 5));
            Target.Energy_Damage(Caster, EnergyChangeType.Direct, (Rank + 1) * (150 + Caster.Global_Power / 3) * Multiplier);
        }
        #endregion

        #region Periodic
        public void Periodic_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //DOT
            //-Crit Chance
            Target.Marks_Add(new Mark(Caster, Target, 6, Rank + 1, Multiplier));
        }

        public void Periodic_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //+Crit Chance
            Target.Marks_Add(new Mark(Caster, Target, 7, Rank + 1, Multiplier));
        }

        public void Periodic_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //DOT
            //-Haste
            Target.Marks_Add(new Mark(Caster, Target, 8, Rank + 1, Multiplier));
        }

        public void Periodic_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //+Resistance
            Target.Marks_Add(new Mark(Caster, Target, 9, Rank + 1, Multiplier));
        }

        public void Periodic_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //Heals damage to Caster
            Target.Marks_Add(new Mark(Caster, Target, 10, Rank + 1, Multiplier));
        }

        public void Periodic_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //+Haste
            Target.Marks_Add(new Mark(Caster, Target, 11, Rank + 1, Multiplier));
        }
        #endregion

        #region Control
        public void Control_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 12, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        public void Control_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 13, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        public void Control_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 14, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        public void Control_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 15, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        public void Control_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            //Silence
            Target.Marks_Add(new Mark(Caster, Target, 16, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        public void Control_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 17, Rank + 1, 1 + Caster.Global_Power / 1000));
        }

        #endregion

        #region Evasion
        public void Evasion_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Location_Set(Target.Area.GetCollisionPoint(Target.Location, Target.Rotation, 100 + Rank * (50 + Caster.Global_Power) / 10));
        }

        //May clear more marks! 6x1 stacked mark?
        public void Evasion_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            int Charges = Rank + 1;
            Mark NextMark = Target.Marks_GetSupportive();
            while ((0 < Charges) && (NextMark != null))
            {
                if (NextMark.Stack < Charges)
                {
                    Charges -= NextMark.Stack;
                    Caster.Marks_Add(new Mark(NextMark.Caster, Caster, NextMark.Effect_ID, NextMark.Stack, NextMark.MaxStack, NextMark.Multiplier));
                    NextMark.Stack_Modify(-NextMark.Stack);
                }
                else
                {
                    Caster.Marks_Add(new Mark(NextMark.Caster, Caster, NextMark.Effect_ID, Charges, NextMark.MaxStack, NextMark.Multiplier));
                    NextMark.Stack_Modify(-Charges);
                    Charges = 0;
                }

                NextMark = Target.Marks_GetSupportive();
            }

            /*if (NextMark != null)
              {
                  if (Rank + 1 <= NextMark.Stack)
                  {
                      NextMark.Stack_Modify(-Rank - 1);
                      Caster.Marks_Add(new Mark(NextMark.Caster, Caster, NextMark.Effect_ID, Rank + 1, NextMark.MaxStack));
                  }
                  else
                  {
                      int MaxStack = NextMark.Stack;
                      NextMark.Stack_Modify(-MaxStack);
                      Caster.Marks_Add(new Mark(NextMark.Caster, Caster, NextMark.Effect_ID, MaxStack, NextMark.MaxStack));
                  }
              }*/
        }

        public void Evasion_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            int Charges = Rank + 1;
            Mark NextMark = Target.Marks_GetSupportive();
            while (0 < Charges && NextMark != null)
            {
                if (NextMark.Stack < Charges)
                {
                    Charges -= NextMark.Stack;
                    NextMark.Stack_Modify(-NextMark.Stack);
                }
                else
                {
                    NextMark.Stack_Modify(-Charges);
                    Charges = 0;
                }

                NextMark = Target.Marks_GetSupportive();
            }

            /*if (NextMark != null)
                  NextMark.Stack_Modify(-Rank - 1);*/
        }

        public void Evasion_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            int Charges = Rank + 1;
            Mark NextMark = Target.Marks_GetUnsupportive();
            while (0 < Charges && NextMark != null)
            {
                if (NextMark.Stack < Charges)
                {
                    Charges -= NextMark.Stack;
                    NextMark.Stack_Modify(-NextMark.Stack);
                }
                else
                {
                    NextMark.Stack_Modify(-Charges);
                    Charges = 0;
                }

                NextMark = Target.Marks_GetUnsupportive();
            }
        }

        public void Evasion_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 22, Rank + 1, 1 + (Caster.Global_Power / 1000)));
        }

        public void Evasion_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 23, Rank + 1, 1 + (Caster.Global_Power / 1000)));
        }
        #endregion

        #region Summon

        #endregion

        #region Command

        #endregion

        #region Heal
        public void Heal_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 36, 1, Multiplier));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, (Rank + 1) * (100 + Caster.Global_Power / 9));
        }

        public void Heal_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 37, 1, Multiplier));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, (Rank + 1) * (100 + Caster.Global_Power / 9));
        }

        public void Heal_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 38, 1, Multiplier));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, (Rank + 1) * (100 + Caster.Global_Power / 9));
        }

        public void Heal_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 39, Rank + 1, Multiplier));
        }

        public void Heal_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 40, 1, Multiplier));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, (Rank + 1) * (40 + Caster.Global_Power / 18));
        }

        public void Heal_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 41, 1, Multiplier));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, (Rank + 1) * (100 + Caster.Global_Power / 9));
        }

        #endregion

        #region Shield
        public void Shield_Fire(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 42, 1, Multiplier));
        }

        public void Shield_Arcane(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 43, 1, Multiplier));
        }

        public void Shield_Frost(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 44, Rank + 1, Multiplier));
        }

        public void Shield_Nature(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 45, 3, 3, Multiplier));
        }

        public void Shield_Shadow(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 46, 1, Multiplier));
        }

        public void Shield_Holy(Unit Caster, Unit Target, int Rank, double Multiplier)
        {
            Target.Marks_Add(new Mark(Caster, Target, 47, 1, Multiplier));
        }
        #endregion


        public static float[,] Cooldowns = new float[8*6, 6] 
        {
            {1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},
            {2,3,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},
            {14,18,22,26,30,34},{24,28,32,36,40,44},{14,18,22,26,30,34},{9,13,17,21,25,29},{19,23,27,31,35,39},{9,13,17,21,25,29},
            {24,28,32,36,40,44},{6,10,14,18,22,26},{4,6,8,10,12,14},{1,2,3,4,5,6},{14,18,22,26,30,34},{24,28,32,36,40,44},
            {5,6,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},
            {6,7,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},
            {1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},{1,2,3,4,5,6},
            {1,2,3,4,5,6},{1,2,3,4,5,6},{14,18,22,26,30,34},{1,2,3,4,5,6},{24,28,32,36,40,44},{1,2,3,4,5,6}
        };

        public static double[] Intervals = new double[8*6]
        {
            5,5,5,5,5,5,
            4,4,4,4,4,4,
            3,3,3,3,3,3,
            2,2,2,2,2,2,
            3,3,3,3,3,3,
            3,3,3,3,3,3,
            2,2,2,2,2,2,
            2,2,2,2,2,2
        };

        public static int[,] EnergyCost = new int[8, 6]
        {
            {100,100,100,100,100,100},{50,50,50,50,50,50},{100,100,100,100,100,100},{100,100,100,100,100,100},
            {100,100,100,100,100,100},{100,100,100,100,100,100},{0,0,0,0,0,0},{100,100,100,100,100,100}
        };

        public static bool[,] Support = new bool[9, 6]
        {
            {false,false,false,false,false,false},{false,true,false,true,false,true},{false,false,false,false,false,false},{true,false,false,false,true,true},
            {true,true,true,true,true,true},{true,true,true,true,true,true},{true,true,true,true,true,true},{true,true,true,true,true,true},{true,true,true,true,true,true}
        };
    }
}
