using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public abstract partial class Unit
    {
        public List<Mark> Marks = new List<Mark>();
        public ReaderWriterLockSlim Marks_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<Impact> Impacts = new List<Impact>();
        public ReaderWriterLockSlim Impacts_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Marks_Add(Mark Mark)
        {
            if (((!Mark.Supportive) && (Status_Void <= 0)) || (Mark.Supportive))
            {
                Marks_Locker.EnterReadLock();
                try
                {
                    foreach (Mark NextMark in Marks)
                        if (NextMark.Effect_ID == Mark.Effect_ID)
                        {
                            NextMark.Stack_Modify(Mark.Stack);
                            NextMark.Duration_Reset();

                            Mark.IDGenerator.Free(Mark.ID);
                            return;
                        }
                }
                finally { Marks_Locker.ExitReadLock(); }

                Broadcast_MarksAdd(Mark);

                Marks_Locker.EnterWriteLock();
                try
                {
                    Marks.Add(Mark);
                    if (Mark.StartEffect != null) Mark.StartEffect();
                }
                finally { Marks_Locker.ExitWriteLock(); }
            }
        }

        public Mark Marks_GetSupportive()
        {
            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (NextMark.Supportive)
                        return NextMark;
            }
            finally { Marks_Locker.ExitReadLock(); }
            return null;
        }

        public Mark Marks_GetUnsupportive()
        {
            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (!NextMark.Supportive)
                        return NextMark;
            }
            finally { Marks_Locker.ExitReadLock(); }
            return null;
        }

        public void Marks_Remove(Mark Mark)
        {
            Broadcast_MarksRemove(Mark);

            Marks_Locker.EnterWriteLock();
            try
            {
                if (Mark.EndEffect != null) Mark.EndEffect();
                Marks.Remove(Mark);
            }
            finally { Marks_Locker.ExitWriteLock(); }

            Mark.IDGenerator.Free(Mark.ID);
        }


        public void Impacts_Add(Impact Impact)
        {
            Broadcast_ImpactsAdd(Impact);

            Impacts_Locker.EnterWriteLock();
            try
            {
                Impacts.Add(Impact);
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void Impacts_Remove(Impact Impact)
        {
            Broadcast_ImpactsRemove(Impact);

            Impacts_Locker.EnterWriteLock();
            try
            {
                Impacts.Remove(Impact);
            }
            finally { Impacts_Locker.ExitWriteLock(); }

            Impact.IDGenerator.Free(Impact.ID);
        }

        public void Impacts_Clear()
        {
            Impacts_Locker.EnterWriteLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                {
                    Broadcast_ImpactsRemove(NextImpact);
                    Impact.IDGenerator.Free(NextImpact.ID);
                }

                Impacts.Clear();
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void Impacts_ClearUnsupportive()
        {
            List<Impact> RemovableImpacts = new List<Impact>();

            Impacts_Locker.EnterWriteLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    if (!Spell.Support[NextImpact.Spell.Effect_ID / 6, NextImpact.Spell.Effect_ID % 6])
                        RemovableImpacts.Add(NextImpact);

                foreach (Impact NextImpact in RemovableImpacts)
                {
                    Broadcast_ImpactsRemove(NextImpact);
                    Impacts.Remove(NextImpact);
                    Impact.IDGenerator.Free(NextImpact.ID);
                }
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void Clear()
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                Mark[] Marks_Copy = new Mark[Marks.Count];
                Marks.CopyTo(Marks_Copy);

                foreach (Mark NextMark in Marks_Copy)
                {
                    if (NextMark.EndEffect != null) NextMark.EndEffect();

                    Mark.IDGenerator.Free(NextMark.ID);
                }

                Marks.Clear();

            }
            finally { Marks_Locker.ExitWriteLock(); }

            Impacts_Locker.EnterWriteLock();
            try
            {
                Impact[] Impacts_Copy = new Impact[Impacts.Count];
                Impacts.CopyTo(Impacts_Copy);

                foreach (Impact NextImpact in Impacts_Copy)
                    Impact.IDGenerator.Free(NextImpact.ID);

                Impacts.Clear();
            }
            finally { Impacts_Locker.ExitWriteLock(); }

            Energy_Damaging = null;
            Energy_DamageDone = null;

            Energy_Healing = null;
            Energy_HealDone = null;

            Broadcast_Clear();
        }
    }
}
