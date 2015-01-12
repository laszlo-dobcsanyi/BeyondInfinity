using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public delegate void EnergyChange(Unit Caster, Unit Target,EnergyChangeType ChangeType, ref double Value);

    public enum EnergyChangeType
    {
        Direct,
        Periodic,
        SpellCost
    }

    public abstract partial class Unit
    {
        public Area Area;
        public Region Region;
        public ReaderWriterLockSlim Location_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public uint ID;
        public uint FactionID;
        public uint IconID;
        public uint ItemLevel;
        public string Name;

        public PointF Location;
        public double Rotation;
        public double Speed;
        public bool Moving;

        public double Energy;
        public double MaxEnergy;

        public double Global_Accuracy = 1000;
        public double Global_ClearcastChance = 100;
        public double Global_Resistance = 1000;
        public double Global_Haste = 1000;
        public double Global_Power = 0; 
        public int[] Global_SchoolPowers = new int[6];

        public Spell[] Spells;

        public static Random Random = new Random();

        public bool Region_Moving = false;
        public void Update(double ElapsedTime)
        {
            if (Status_Rooted <= 0)
                if (Moving)
                {
                    PointF NextLocation = new PointF(Location.X + (float)(ElapsedTime / 1000 * Speed * Math.Cos((double)Rotation / 180 * Math.PI)),
                        Location.Y - (float)(ElapsedTime / 1000 * Speed * Math.Sin((double)Rotation / 180 * Math.PI)));

                    if (Area.IsValidGroundLocation(NextLocation)) Location = NextLocation;
                    else Stuck();
                }

            List<Mark> RemovableMarks = new List<Mark>();

            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (NextMark.Update(ElapsedTime) == true)
                        RemovableMarks.Add(NextMark);
            }
            finally { Marks_Locker.ExitReadLock(); }

            foreach (Mark NextMark in RemovableMarks)
                Marks_Remove(NextMark);

            List<Impact> RemovableImpacts = new List<Impact>();

            Impacts_Locker.EnterReadLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    if (NextImpact.Update(ElapsedTime) == true)
                        RemovableImpacts.Add(NextImpact);
            }
            finally { Impacts_Locker.ExitReadLock(); }

            foreach (Impact NextImpact in RemovableImpacts)
            {
                NextImpact.Spell.Effect(NextImpact.Spell.Caster, this, NextImpact.Rank, NextImpact.BonusRanked);
                Impacts_Remove(NextImpact);
            }

            foreach (Spell NextSpell in Spells)
                if (NextSpell != null) //Because Creature's Spell Slot maybe Empty...!!..
                    NextSpell.Update(ElapsedTime);

            if (!Region.Collide(new RectangleF((Region.Index.X - 1) * Area.Regions_Size, (Region.Index.Y - 1) * Area.Regions_Size, 2 * Area.Regions_Size, 2 * Area.Regions_Size), Location)) Region_Moving = true;
        }

        //public ReaderWriterLockSlim EnergyChange_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public EnergyChange Energy_Damaging;
        public EnergyChange Energy_DamageDone;
        public void Energy_Damage(Unit Caster, EnergyChangeType ChangeType, double Value)
        {
            if (Status_Invulnerable <= 0)
            {
                if (Energy_Damaging != null) Energy_Damaging(Caster, this, ChangeType, ref Value);

                Energy -= (int)(Value * (1000 / (float)Global_Resistance));

                if (0 < Value)
                    if (Energy <= 0)
                    {
                        Status_Dead = true;
                        Killer = Caster;
                    }
                    else
                        if (ChangeType != EnergyChangeType.SpellCost) Broadcast_EnergyModify(Caster);
                        else Broadcast_Energy();

                if (Energy_DamageDone != null) Energy_DamageDone(Caster, this, ChangeType, ref Value);
            }
        }

        public EnergyChange Energy_Healing;
        public EnergyChange Energy_HealDone;
        public void Energy_Heal(Unit Caster, EnergyChangeType ChangeType, double Value)
        {
            if (Energy_Healing != null) Energy_Healing(Caster, this, ChangeType, ref Value);

            if (0 < Value)
                if (Energy != MaxEnergy)
                {
                    Energy += Value;
                    if (MaxEnergy < Energy) Energy = MaxEnergy;
                    Broadcast_EnergyModify(Caster);
                }

            if (Energy_HealDone != null) Energy_HealDone(Caster, this, ChangeType, ref Value);
        }

        public void Energy_Set(double Value)
        {
            Energy = Value;
            Broadcast_Energy();
        }

        /// <summary>
        /// Does not sets Killer, only Status_Death, so don't use with spells/effects!
        /// </summary>
        public void Energy_Modify(double Value)
        {
            Energy += Value;

            if (MaxEnergy < Energy)
            {
                Energy = MaxEnergy;
                Broadcast_Energy();
            }

            if (Energy <= 0) Status_Dead = true;
            else Broadcast_Energy();
        }

        public void MaxEnery_Modify(double Value)
        {
            MaxEnergy += Value;

            if (MaxEnergy < Energy)
            {
                Energy = MaxEnergy;
                Broadcast_Energy();
            }

            Broadcast_MaxEnergy();
        }


        public void Speed_Modify(double Value)
        {
            Speed += Value;

            if (Moving) Broadcast_Location();
        }

        public void Rotate(int rotation)
        {
            Rotation = rotation;
            Moving = true;

            Broadcast_Location();
        }

        public void Stop()
        {
            Moving = false;
            Broadcast_Location();
        }

        public virtual void Stuck()
        {
            Moving = false;
            Broadcast_Location();
        }

        public virtual void Location_Set(PointF location)
        {
            if (Status_Rooted <= 0)
            {
                Location = location;

                Broadcast_Location();
            }
        }

        public virtual void GlobalCooldown_Set(float Value)
        {
            foreach (Spell NextSpell in Spells)
                if (NextSpell.Cooldown < Value) NextSpell.Cooldown = Value;
        }


        public bool Status_Dead = false;
        public Unit Killer;

        public abstract void Status_Death(Unit Killer);

        public int Status_Void = 0;
        public int Status_Muted = 0;
        public int Status_Rooted = 0;
        public int Status_Invisible = 0;
        public int Status_Invulnerable = 0;
        public int Status_Reflection = 0;

        public void Status_Avoid()
        {
            Status_Void++;
        }

        public void Status_Unvoid()
        {
            Status_Void--;
        }

        public void Status_Mute()
        {
            Status_Muted++;
            if (Status_Muted == 1)
            {
                Character Character = this as Character;
                if (Character != null) Character.Connection.Send(Connection.Command.Character_Mute, ".");
            }
        }

        public void Status_Vocalize()
        {
            Status_Muted--;
            if (Status_Muted == 0)
            {
                Character Character = this as Character;
                if (Character != null) Character.Connection.Send(Connection.Command.Character_UnMute, ".");
            }
        }

        public void Status_Root()
        {
            Status_Rooted++;
            Broadcast_Location();
        }

        public void Status_Mobilize()
        {
            Status_Rooted--;
            Broadcast_Location();
        }

        public void Status_Hide()
        {
            Status_Invisible++;
            Broadcast_Leave();
        }

        public void Status_Show()
        {
            Status_Invisible--;
            Broadcast_Enter();
        }

        public void Status_Invulnerate()
        {
            Status_Invulnerable++;
        }

        public void Status_Uninvulnerate()
        {
            Status_Invulnerable--;
        }

        public void Status_Reflect()
        {
            Status_Reflection++;
        }

        public void Status_Unreflect()
        {
            Status_Reflection--;
        }


        public abstract WayPoint Schedule_Next();

        public abstract void Broadcast_Enter();
        public abstract void Broadcast_Enter(Region Region);
        public abstract void Broadcast_Enter(Connection Connection);

        public abstract void Broadcast_Leave();

        public abstract void Broadcast_Energy();

        public abstract void Broadcast_EnergyModify(Unit Caster);

        public abstract void Broadcast_MaxEnergy();

        public abstract void Broadcast_Location();

        public abstract void Broadcast_ItemLevel();

        public abstract void Broadcast_MarksAdd(Mark Mark);

        public abstract void Broadcast_MarkStack(Mark Mark);

        public abstract void Broadcast_MarkDuration(Mark Mark);

        public abstract void Broadcast_MarksRemove(Mark Mark);

        public abstract void Broadcast_ImpactsAdd(Impact Impact);

        public abstract void Broadcast_ImpactsRemove(Impact Impact);

        public abstract void Broadcast_Clear();
    }
}
