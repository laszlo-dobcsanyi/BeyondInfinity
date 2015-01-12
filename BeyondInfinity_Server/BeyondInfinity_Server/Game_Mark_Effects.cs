using System;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Mark
    {
        public MarkEffect GetStartEffect(uint ID)
        {
            switch (ID)
            {
                case 0: return DirectFire_Start;
                case 1: return DirectArcane_Start;
                case 2: return DirectFrost_Start;
                case 4: return DirectShadow_Start;
                case 5: return DirectHoly_Start;

                case 6: return PeriodicFire_Start;
                case 7: return PeriodicArcane_Start;
                case 8: return PeriodicFrost_Start;
                case 9: return PeriodicNature_Start;
                case 11: return PeriodicHoly_Start;

                case 12: return ControlFire_Start;
                case 13: return ControlArcane_Start;
                case 14: return ControlFrost_Start;
                case 15: return ControlNature_Start;
                case 16: return ControlShadow_Start;
                case 17: return ControlHoly_Start;

                case 22: return EvasionShadow_Start;
                case 23: return EvasionHoly_Start;

                case 36: return HealFire_Start;
                case 37: return HealArcane_Start;
                case 38: return HealFrost_Start;
                case 41: return HealHoly_Start;

                case 42: return ShieldFire_Start;
                case 43: return ShieldArcane_Start;
                case 44: return ShieldFrost_Start;
                case 45: return ShieldNature_Start;
                case 46: return ShieldShadow_Start;
                case 47: return ShieldHoly_Start;

                case 48: return Teleport_Start;
            }
            return null;
        }

        public MarkEffect GetTickEffect(uint ID)
        {
            switch (ID)
            {
                case 2: return DirectFrost_Tick;
                case 3: return DirectNature_Tick;

                case 6: return PeriodicFire_Tick;
                case 10: return PeriodicShadow_Tick;

                case 39: return HealNature_Tick;
                case 40: return HealShadow_Tick;

                case 48: return Teleport_Tick;
            }
            return null;
        }

        public StackModifyEffect GetStackModifyEffect(uint ID)
        {
            switch (ID)
            {
                case 2: return DirectFrost_StackModify;
                case 4: return DirectShadow_StackModify;
                case 5: return DirectHoly_StackModify;

                case 6: return PeriodicFire_StackModify;
                case 7: return PeriodicArcane_StackModify;
                case 8: return PeriodicFrost_StackModify;
                case 9: return PeriodicNature_StackModify;
                case 11: return PeriodicHoly_StackModify;

                case 36: return HealFire_StackModify;
                case 37: return HealArcane_StackModify;
                case 38: return HealFrost_StackModify;

                case 42: return ShieldFire_StackModify;
                case 43: return ShieldArcane_StackModify;
                case 45: return ShieldNature_StackModify;
                case 46: return ShieldShadow_StackModify;
                case 47: return ShieldHoly_StackModify;

            }
            return null;
        }

        public MarkEffect GetEndEffect(uint ID)
        {
            switch (ID)
            {
                case 0: return DirectFire_End;
                case 1: return DirectArcane_End;
                case 2: return DirectFrost_End;
                case 4: return DirectShadow_End;
                case 5: return DirectHoly_End;

                case 6: return PeriodicFire_End;
                case 7: return PeriodicArcane_End;
                case 8: return PeriodicFrost_End;
                case 9: return PeriodicNature_End;
                case 11: return PeriodicHoly_End;

                case 12: return ControlFire_End;
                case 13: return ControlArcane_End;
                case 14: return ControlFrost_End;
                case 15: return ControlNature_End;
                case 16: return ControlShadow_End;
                case 17: return ControlHoly_End;

                case 22: return EvasionShadow_End;
                case 23: return EvasionHoly_End;

                case 36: return HealFire_End;
                case 37: return HealArcane_End;
                case 38: return HealFrost_End;
                case 41: return HealHoly_End;

                case 42: return ShieldFire_End;
                case 43: return ShieldArcane_End;
                case 44: return ShieldFrost_End;
                case 45: return ShieldNature_End;
                case 46: return ShieldShadow_End;
                case 47: return ShieldHoly_End;

                case 48: return Teleport_End;
            }
            return null;
        }

        private double Modification = 0;

        #region DirectDamage
        //Fire
        public void DirectFire_Start()
        {
            Target.Energy_Healing += DirectFire_Healing;
        }

        public void DirectFire_Healing(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            Value = (int)(Value * (1 + ((float)Stack / 10)));
        }

        public void DirectFire_End()
        {
            Target.Energy_Healing -= DirectFire_Healing;
        }

        //Arcane
        public void DirectArcane_Start()
        {
            Target.Energy_Damaging += DirectArcane_Damaging;
        }

        public void DirectArcane_Damaging(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            Value = (int)(Value * (1 + ((float)Stack / 10)));
        }

        public void DirectArcane_End()
        {
            Target.Energy_Damaging -= DirectArcane_Damaging;
        }

        //Frost
        private void DirectFrost_Start()
        {
            Target.Global_Haste -= Stack * 50;
        }

        private void DirectFrost_StackModify(int Value)
        {
            Target.Global_Haste -= Value * 50;
        }

        private void DirectFrost_Tick()
        {
            Target.Energy_Damage(Caster, EnergyChangeType.Periodic, Stack * 50);
        }

        private void DirectFrost_End()
        {
            Target.Global_Haste += Stack * 50;
        }

        //Nature
        public void DirectNature_Tick()
        {
            Target.Energy_Damage(Caster, EnergyChangeType.Periodic, Stack * 50);
        }

        //Shadow
        public void DirectShadow_Start()
        {
            Target.Global_Resistance -= Stack * 50;
        }

        private void DirectShadow_StackModify(int Value)
        {
            Target.Global_Resistance -= Value * 50;
        }

        public void DirectShadow_End()
        {
            Target.Global_Resistance += Stack * 50;
        }

        //Holy
        public void DirectHoly_Start()
        {
            Target.Global_Accuracy -= Stack * 50;
        }

        private void DirectHoly_StackModify(int Value)
        {
            Target.Global_Accuracy -= Value * 50;
        }

        public void DirectHoly_End()
        {
            Target.Global_Accuracy += Stack * 50;
        }
        #endregion

        #region Periodic
        //Fire
        private void PeriodicFire_Start()
        {
            Modification = Stack * (50 + Caster.Global_Power / 10);
            Target.Global_ClearcastChance -= Modification;
        }

        private void PeriodicFire_StackModify(int Value)
        {
            Modification += Value * (50 + Caster.Global_Power / 10);
            Target.Global_ClearcastChance -= Value * (50 + Caster.Global_Power / 10);
        }

        private void PeriodicFire_Tick()
        {
            Target.Energy_Damage(Caster, EnergyChangeType.Periodic, Stack * (50 + Caster.Global_Power / 10));
        }

        private void PeriodicFire_End()
        {
            Target.Global_ClearcastChance += Modification;
        }

        //Arcane
        private void PeriodicArcane_Start()
        {
            Modification = Stack * (50 + Caster.Global_Power / 10);
            Target.Global_ClearcastChance += Modification;
        }

        private void PeriodicArcane_StackModify(int Value)
        {
            Modification += Value * (50 + Caster.Global_Power / 10);
            Target.Global_ClearcastChance += Value * (50 + Caster.Global_Power / 10);
        }

        private void PeriodicArcane_End()
        {
            Target.Global_ClearcastChance -= Modification;
        }

        //Frost
        public void PeriodicFrost_Start()
        {
            Modification = Stack * (3 + Caster.Global_Power / 200);
            Target.Speed_Modify(-Modification);
        }

        public void PeriodicFrost_StackModify(int Value)
        {
            Modification += Value * (3 + Caster.Global_Power / 200);
            Target.Speed_Modify(-Value * (3 + Caster.Global_Power / 200));
        }

        public void PeriodicFrost_End()
        {
            Target.Speed_Modify(Modification);
        }

        //Nature
        private void PeriodicNature_Start()
        {
            Modification = Stack * (50 + Caster.Global_Power / 10);
            Target.Global_Resistance += Modification;
        }

        private void PeriodicNature_StackModify(int Value)
        {
            Modification += Value * (50 + Caster.Global_Power / 10);
            Target.Global_Resistance += Value * (50 + Caster.Global_Power / 10);
        }

        private void PeriodicNature_End()
        {
            Target.Global_Resistance -= Modification;
        }

        //Shadow
        private void PeriodicShadow_Tick()
        {
            Target.Energy_Damage(Caster, EnergyChangeType.Periodic, Stack * (5 + Caster.Global_Power / 100));
            Caster.Energy_Heal(Caster, EnergyChangeType.Periodic, Stack * (5 + Caster.Global_Power / 100));
        }

        //Holy
        private void PeriodicHoly_Start()
        {
            Modification = Stack * (50 + Caster.Global_Power / 10);
            Target.Global_Haste += Modification;
        }

        private void PeriodicHoly_StackModify(int Value)
        {
            Modification += Value * (50 + Caster.Global_Power / 10);
            Target.Global_Haste += Value * (50 + Caster.Global_Power / 10);
        }

        private void PeriodicHoly_End()
        {
            Target.Global_Haste -= Modification;
        }
        #endregion

        #region Control
        //Fire
        private void ControlFire_Start()
        {
            Target.Status_Root();
        }

        private void ControlFire_End()
        {
            Target.Status_Mobilize();
        }

        //Arcane
        private void ControlArcane_Start()
        {
            Target.Status_Mute();
            Target.Status_Root();
        }

        private void ControlArcane_End()
        {
            Target.Status_Vocalize();
            Target.Status_Mobilize();
        }

        //Frost
        private void ControlFrost_Start()
        {
            Target.Status_Mute();
            Target.Status_Root();

            Target.Energy_DamageDone += ControlFrost_DamageDone;
        }

        private void ControlFrost_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            if (ChangeType == EnergyChangeType.Direct)
                Target.Marks_Remove(this);
        }

        private void ControlFrost_End()
        {
            Target.Energy_DamageDone -= ControlFrost_DamageDone;

            Target.Status_Vocalize();
            Target.Status_Mobilize();
        }

        //Nature
        private void ControlNature_Start()
        {
            Target.Status_Mute();

            Target.Energy_DamageDone += ControlNature_DamageDone;
        }

        private void ControlNature_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            if (ChangeType == EnergyChangeType.Direct)
                Target.Marks_Remove(this);
        }

        private void ControlNature_End()
        {
            Target.Energy_DamageDone -= ControlNature_DamageDone;

            Target.Status_Vocalize();
        }

        //Shadow
        private void ControlShadow_Start()
        {
            Target.Status_Mute();
        }

        private void ControlShadow_End()
        {
            Target.Status_Vocalize();
        }

        //Holy
        private void ControlHoly_Start()
        {
            Target.Status_Root();

            Target.Energy_DamageDone += ControlHoly_DamageDone;
        }

        private void ControlHoly_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            if (ChangeType == EnergyChangeType.Direct)
                Target.Marks_Remove(this);
        }

        private void ControlHoly_End()
        {
            Target.Energy_DamageDone -= ControlHoly_DamageDone;

            Target.Status_Mobilize();
        }
        #endregion

        #region Evasion
        //Shadow
        private void EvasionShadow_Start()
        {
            Target.Status_Hide();
            Target.Impacts_ClearUnsupportive();

            Target.Energy_DamageDone += EvasionShadow_DamageDone;
        }

        private void EvasionShadow_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            if (ChangeType == EnergyChangeType.Direct)
                Target.Marks_Remove(this);
        }

        private void EvasionShadow_End()
        {
            Target.Energy_DamageDone -= EvasionShadow_DamageDone;

            Target.Status_Show();
        }

        //Holy
        private void EvasionHoly_Start()
        {
            Target.Status_Invulnerate();
        }

        private void EvasionHoly_End()
        {
            Target.Status_Uninvulnerate();
        }
        #endregion

        #region Heal
        //Fire
        private void HealFire_Start()
        {
            Modification = Stack * (4 + (double)Caster.Global_Power / 200);
            Target.Speed_Modify(Modification);
        }

        private void HealFire_StackModify(int Value)
        {
            Modification += (Value * (4 + (double)Caster.Global_Power / 200));
            Target.Speed_Modify((Value * (4 + (double)Caster.Global_Power / 200)));
        }

        private void HealFire_End()
        {
            Target.Speed_Modify(-Modification);
        }

        //Arcane
        private void HealArcane_Start()
        {
            Modification = Stack * (100 + Caster.Global_Power / 5);
            Target.MaxEnery_Modify(Modification);

            Target.Energy_Heal(Caster, EnergyChangeType.Direct, Modification);
        }

        private void HealArcane_StackModify(int Value)
        {
            Modification += Value * (100 + Caster.Global_Power / 5);
            Target.MaxEnery_Modify(Value * (100 + Caster.Global_Power / 5));
            Target.Energy_Heal(Caster, EnergyChangeType.Direct, Value * (100 + Caster.Global_Power / 5));
        }

        private void HealArcane_End()
        {
            Target.MaxEnery_Modify(-Modification);
        }

        //Frost
        private void HealFrost_Start()
        {
            Modification = Stack * (100 + Caster.Global_Power / 10);
            Target.Global_Accuracy += Modification;
        }

        private void HealFrost_StackModify(int Value)
        {
            Modification += Value * (100 + Caster.Global_Power / 10);
            Target.Global_Accuracy += Value * (100 + Caster.Global_Power / 10);
        }

        private void HealFrost_End()
        {
            Target.Global_Accuracy -= Modification;
        }

        //Nature
        private void HealNature_Tick()
        {
            Target.Energy_Heal(Caster, EnergyChangeType.Periodic, Stack * (25 + Caster.Global_Power / 20));
        }

        //Shadow
        private void HealShadow_Tick()
        {
            Target.Energy_Heal(Caster, EnergyChangeType.Periodic, Stack * (20 + Caster.Global_Power / 20));
        }

        //Holy
        private void HealHoly_Start()
        {
            Target.Energy_Healing += HealHoly_Healing;
        }

        private void HealHoly_Healing(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            Value = (int)(Value * (1 + ((float)Stack / 10)));
        }

        private void HealHoly_End()
        {
            Target.Energy_Healing -= HealHoly_Healing;
        }
        #endregion

        #region Shield
        //Fire
        private void ShieldFire_Start()
        {
            Target.Status_Mobilize();
            Target.MaxEnery_Modify(Stack * 100);
        }

        private void ShieldFire_StackModify(int Value)
        {
            Target.MaxEnery_Modify(Value * 100);
        }

        private void ShieldFire_End()
        {
            Target.MaxEnery_Modify(-Stack * 100);
            Target.Status_Root();
        }

        //Arcane
        private void ShieldArcane_Start()
        {
            Target.Status_Vocalize();
            Target.MaxEnery_Modify(Stack * 100);
        }

        private void ShieldArcane_StackModify(int Value)
        {
            Target.MaxEnery_Modify(Value * 100);
        }

        private void ShieldArcane_End()
        {
            Target.MaxEnery_Modify(-Stack * 100);
            Target.Status_Mute();
        }

        //Frost
        private void ShieldFrost_Start()
        {
            Target.Status_Reflect();
        }

        private void ShieldFrost_End()
        {
            Target.Status_Unreflect();
        }

        //Nature
        private void ShieldNature_Start()
        {
            Caster.Energy_DamageDone += ShieldNature_DamageDone;
            Target.MaxEnery_Modify(Stack * 100);
        }

        private void ShieldNature_StackModify(int Value)
        {
            Target.MaxEnery_Modify(Value * 100);
        }

        private void ShieldNature_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            //if (Caster != null)
            if (ChangeType == EnergyChangeType.Direct)
            {
                Caster.Energy_Damage(Target, EnergyChangeType.Periodic, 100 + this.Caster.Global_Power / 5);
                Stack_Modify(-1);
            }
        }

        public void ShieldNature_End()
        {
            Caster.Energy_DamageDone -= ShieldNature_DamageDone;
            Target.MaxEnery_Modify(Stack * -100);
        }

        //Shadow
        private void ShieldShadow_Start()
        {
            Target.Status_Avoid();
            Target.MaxEnery_Modify(Stack * 100);
        }

        private void ShieldShadow_StackModify(int Value)
        {
            Target.MaxEnery_Modify(Value * 100);
        }

        private void ShieldShadow_End()
        {
            Target.MaxEnery_Modify(-Stack * 100);
            Target.Status_Unvoid();
        }

        //Holy
        private void ShieldHoly_Start()
        {
            Target.MaxEnery_Modify(Stack * 200);
        }

        private void ShieldHoly_StackModify(int Value)
        {
            Target.MaxEnery_Modify(Value * 200);
        }

        private void ShieldHoly_End()
        {
            Target.MaxEnery_Modify(Stack * -200);
        }
        #endregion

        #region Other
        public void Teleport_Start()
        {
            Target.Energy_DamageDone += Teleport_DamageDone;

            Character Character = Target as Character;
            Character.Status_Teleport();
        }

        private void Teleport_DamageDone(Unit Caster, Unit Target, EnergyChangeType ChangeType, ref double Value)
        {
            if (ChangeType == EnergyChangeType.Direct)
                Period = 0;
        }

        public void Teleport_Tick()
        {
            Character TargetCharacter = Target as Character;
            if (TargetCharacter != null)
                TargetCharacter.Status_Teleporting = true;
        }

        public void Teleport_End()
        {
            Target.Energy_DamageDone -= Teleport_DamageDone;

            Character Character = Target as Character;
            Character.Status_Teleported();
        }
        #endregion


        public static int[,] Periods = new int[9, 6]
        {
            {1,1,1,12,1,1},{1,1,1,1,12,1},{1,1,1,1,1,1},{1,1,1,1,1,1},{3,3,3,3,3,3},{3,3,3,3,3,3},{1,1,1,12,12,1},{1,1,1,1,1,1}, {1,1,1,1,1,1}
        };

        public static double[, ,] Intervals = new double[9, 6, 6]
        {
            {{12,12,12,12,12,12},{18,18,18,18,18,18},{12,12,12,12,12,12},{1,1,1,1,1,1},{24,24,24,24,24,24},{18,18,18,18,18,18}},
            {{30,30,30,30,30,30},{30,30,30,30,30,30},{10,10,10,10,10,10},{30,30,30,30,30,30},{2,2,2,2,2,2},{30,30,30,30,30,30}},
            {{3,4,5,6,7,8},{1.5f,2,2.5f,3,3.5f,4},{1,2,3,4,5,6},{4,5,6,7,8,9},{1,2,3,4,5,6},{2,4,6,8,10,12}},
            {{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{3.5f,4,4.5f,5,5.5f,6},{3.5f,4,4.5f,5,5.5f,6}},
            {{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0}},
            {{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0}},
            {{30,30,30,30,30,30},{30,30,30,30,30,30},{30,30,30,30,30,30},{1,1,1,1,1,1},{1,1,1,1,1,1},{30,30,30,30,30,30}},
            {{10,12,14,16,18,20},{10,12,14,16,18,20},{3.5f,4,4.5f,5,5.5f,6},{10,12,14,16,18,20},{4,5,6,7,8,9},{10,12,14,16,18,20}},
            {{10,10,10,10,10,10},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0},{0,0,0,0,0,0}}
        };
    }
}
