using System;

namespace BeyondInfinity_Server
{
    public delegate void MarkEffect();
    public delegate void StackModifyEffect(int Value);

    public sealed partial class Mark
    {
        public Unit Caster;
        public Unit Target;

        public uint ID;
        public static Generator IDGenerator = new Generator(Program.CAPACITY * 16);

        public int Stack;
        public int MaxStack;
        public double Multiplier;

        public uint Effect_ID;
        public MarkEffect StartEffect;
        public MarkEffect TickEffect;
        public StackModifyEffect StackModifyEffect;
        public MarkEffect EndEffect;

        public bool Supportive;

        public Mark(Unit caster, Unit target, uint effectid)
        {
            ID = IDGenerator.Next();

            Caster = caster;
            Target = target;

            Effect_ID = effectid;

            Stack = 1;
            MaxStack = 6;
            Multiplier = 1;

            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000;
            Supportive = Spell.Support[Effect_ID / 6, Effect_ID % 6];

            StartEffect = GetStartEffect(Effect_ID);
            TickEffect = GetTickEffect(Effect_ID);
            StackModifyEffect = GetStackModifyEffect(Effect_ID);
            EndEffect = GetEndEffect(Effect_ID);
        }

        public Mark(Unit caster, Unit target, uint effectid, int stack ,double multiplier)
        {
            ID = IDGenerator.Next();

            Caster = caster;
            Target = target;

            Effect_ID = effectid;

            Stack = stack;
            MaxStack = 6;
            Multiplier = multiplier;

            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000 * Multiplier;
            Supportive = Spell.Support[Effect_ID / 6, Effect_ID % 6];

            StartEffect = GetStartEffect(Effect_ID);
            TickEffect = GetTickEffect(Effect_ID);
            StackModifyEffect = GetStackModifyEffect(Effect_ID);
            EndEffect = GetEndEffect(Effect_ID);
        }

        public Mark(Unit caster, Unit target, uint effectid, int stack)
        {
            ID = IDGenerator.Next();

            Caster = caster;
            Target = target;

            Effect_ID = effectid;

            Stack = stack;
            MaxStack = 6;
            Multiplier = 1;

            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000;
            Supportive = Spell.Support[Effect_ID / 6, Effect_ID % 6];

            StartEffect = GetStartEffect(Effect_ID);
            TickEffect = GetTickEffect(Effect_ID);
            EndEffect = GetEndEffect(Effect_ID);
        }

        public Mark(Unit caster, Unit target, uint effectid, int stack, int maxstack)
        {
            ID = IDGenerator.Next();

            Caster = caster;
            Target = target;

            Effect_ID = effectid;

            Stack = stack;
            MaxStack = maxstack;
            Multiplier = 1;

            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000;
            Supportive = Spell.Support[Effect_ID / 6, Effect_ID % 6];

            StartEffect = GetStartEffect(Effect_ID);
            TickEffect = GetTickEffect(Effect_ID);
            EndEffect = GetEndEffect(Effect_ID);
        }

        public Mark(Unit caster, Unit target, uint effectid, int stack, int maxstack,double multiplier)
        {
            ID = IDGenerator.Next();

            Caster = caster;
            Target = target;

            Effect_ID = effectid;

            Stack = stack;
            MaxStack = maxstack;
            Multiplier = multiplier;

            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000;
            Supportive = Spell.Support[Effect_ID / 6, Effect_ID % 6];

            StartEffect = GetStartEffect(Effect_ID);
            TickEffect = GetTickEffect(Effect_ID);
            EndEffect = GetEndEffect(Effect_ID);
        }

        int Period;
        private double Interval;
        public bool Update(double ElapsedTime)
        {
            if (1 <= Period)
            {
                Interval -= ElapsedTime;

                if (Interval <= 0)
                {
                    Period--;
                    Interval += Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000 * Multiplier;

                    if (TickEffect != null) TickEffect();

                    //if (Period == 0) return true;
                }
            }
            else return true;
            return false;
        }

        public void Duration_Reset()
        {
            Period = Periods[Effect_ID / 6, Effect_ID % 6];
            Interval = Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000 * Multiplier;

            Target.Broadcast_MarkDuration(this);
        }

        public void Stack_Modify(int Value)
        {
            if (Value != 0)
            {
                if (Stack + Value <= 0)
                {
                    Target.Marks_Remove(this);
                    return;
                }
                if (MaxStack < Stack + Value) Value = MaxStack - Stack;

                Stack += Value;
                if (StackModifyEffect != null) StackModifyEffect(Value);
                Target.Broadcast_MarkStack(this);
            }
        }

        public string GetData()
        {
            return ID + "\t" + Effect_ID + "\t" + Stack + "\t" + ((Period - 1) * Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000 + Interval) + "\t"
                + (Periods[Effect_ID / 6, Effect_ID % 6] * Intervals[Effect_ID / 6, Effect_ID % 6, Stack - 1] * 1000 * Multiplier);
        }
    }
}
