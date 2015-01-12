using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace BeyondInfinity_Server
{
    public sealed partial class Mind
    {
        private void GetEventHandlers()
        {
            switch (Unit.Name)
            {
                case "Gangnam":
                    Combat_Enter += Gangnam_Combat_Enter;
                    Combat_Leave += Gangnam_Combat_Leave;
                    break;
            }
        }

        #region Gangnam World
        private System.Timers.Timer Gangnam_CastTimer;

        private void Gangnam_Combat_Enter()
        {
            Gangnam_CastTimer = new System.Timers.Timer(10 * 1000);
            Gangnam_CastTimer.Elapsed += new System.Timers.ElapsedEventHandler(Gangnam_CastTimer_Elapsed);
            Gangnam_CastTimer.Start();
        }

        private void Gangnam_CastTimer_Elapsed(object Sender, System.Timers.ElapsedEventArgs Event)
        {

        }

        private void Gangnam_Combat_Leave()
        {
            Gangnam_CastTimer.Stop();
            Gangnam_CastTimer.Dispose();
        }
        #endregion
    }
}
