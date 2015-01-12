using System;
using System.Threading;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity
{
    public class Group
    {
        public List<Unknown> Members = new List<Unknown>();
        public ReaderWriterLockSlim Members_Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Add(string Data)
        {
            Unknown NewMember = new Unknown(Data);
            Members_Locker.EnterWriteLock();
            try
            {
                Members.Add(NewMember);
            }
            finally { Members_Locker.ExitWriteLock(); }


            Hero Target = Game.Heroes_Get(NewMember.Name);
            if (Target != null)
                Target.InGroup = true;
        }

        public void Remove(string Name)
        {
            Members_Locker.EnterWriteLock();
            try
            {
                foreach (Unknown NextUnknown in Members)
                    if (NextUnknown.Name == Name)
                    {
                        Members.Remove(NextUnknown);
                        break;
                    }
            }
            finally { Members_Locker.ExitWriteLock(); }

            Hero Target = Game.Heroes_Get(Name);
            if (Target != null)
                Target.InGroup = false;
        }

        public void Clear()
        {
            Members_Locker.EnterWriteLock();
            try
            {
                Hero Target;
                foreach (Unknown NextUnknown in Members)
                {
                    Target = Game.Heroes_Get(NextUnknown.Name);
                    if (Target != null)
                        Target.InGroup = false;
                }

                Members.Clear();
            }
            finally { Members_Locker.ExitWriteLock(); }
        }
    }

    public class Unknown
    {
        public string Name;
        public Texture Icon;

        public Unknown(string Data)
        {
            string[] Arguments = Data.Split('\t');

            Name = Arguments[0];
            Icon = TextureLoader.FromFile(Program.GameForm.Device, @"data\icons\hero" + Game.Character.Faction + "_" + Arguments[1] + ".png");
        }
    }
}
