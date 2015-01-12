using System;
using System.Threading;

namespace BeyondInfinity_Server
{
    public sealed class Generator
    {
        private uint[] IDs;
        private uint LastID;
        private ReaderWriterLockSlim ID_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public Generator(uint MaxID)
        {
            IDs = new uint[MaxID];
            for (uint Current = 0; Current < MaxID; Current++)
                IDs[Current] = Current;

            LastID = MaxID;
        }

        public uint Next()
        {
            ID_Locker.EnterWriteLock();
            try
            {
                LastID--;
                return IDs[LastID];
            }
            finally { ID_Locker.ExitWriteLock(); }
        }

        public void Free(uint ID)
        {
            ID_Locker.EnterWriteLock();
            try
            {
                IDs[LastID] = ID;
                LastID++;
            }
            finally { ID_Locker.ExitWriteLock(); }
        }
    }
}
