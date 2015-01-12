using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public partial class Unit
    {
        public List<Mark> Marks = new List<Mark>();
        public ReaderWriterLockSlim Marks_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<Impact> Impacts = new List<Impact>();
        public ReaderWriterLockSlim Impacts_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<CombatText> CombatTexts = new List<CombatText>();
        public ReaderWriterLockSlim CombatTexts_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public virtual void Marks_Add(Mark Mark)
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                Marks.Add(Mark);
            }
            finally { Marks_Locker.ExitWriteLock(); }
        }

        public Mark Marks_Get(uint EffectID)
        {
            Marks_Locker.EnterReadLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (NextMark.EffectID == EffectID)
                        return NextMark;
                return null;
            }
            finally { Marks_Locker.ExitReadLock(); }
        }

        public virtual void Marks_Remove(uint ID)
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                foreach (Mark NextMark in Marks)
                    if (NextMark.ID == ID)
                    {
                        Marks.Remove(NextMark);
                        return;
                    }
            }
            finally { Marks_Locker.ExitWriteLock(); }
        }

        public void Marks_Clear()
        {
            Marks_Locker.EnterWriteLock();
            try
            {
                Marks.Clear();
            }
            finally { Marks_Locker.ExitWriteLock(); }
        }

        public void Impacts_Add(Impact Impact)
        {
            Impacts_Locker.EnterWriteLock();
            try
            {
                Impacts.Add(Impact);
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void Impacts_Remove(uint ID)
        {
            Impacts_Locker.EnterWriteLock();
            try
            {
                foreach (Impact NextImpact in Impacts)
                    if (NextImpact.ID == ID)
                    {
                        Impacts.Remove(NextImpact);
                        return;
                    }
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void Impacts_Clear()
        {
            Impacts_Locker.EnterWriteLock();
            try
            {
                Impacts.Clear();
            }
            finally { Impacts_Locker.ExitWriteLock(); }
        }

        public void CombatTexts_Add(CombatText CombatText)
        {
            CombatTexts_Locker.EnterWriteLock();
            try
            {
                CombatTexts.Add(CombatText);
            }
            finally { CombatTexts_Locker.ExitWriteLock(); }
        }

        public void CombatTexts_Remove(CombatText CombatText)
        {
            CombatTexts_Locker.EnterWriteLock();
            try
            {
                CombatTexts.Remove(CombatText);
            }
            finally { CombatTexts_Locker.ExitWriteLock(); }
        }

        public void CombatTexts_Clear()
        {
            CombatTexts_Locker.EnterWriteLock();
            try
            {
                CombatTexts.Clear();
            }
            finally { CombatTexts_Locker.ExitWriteLock(); }
        }
    }
}
