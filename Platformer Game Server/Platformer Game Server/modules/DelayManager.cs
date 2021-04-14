using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class DelayManager {
        public void Do(int after, Action action) {
            if (after <= 0 || action == null) return;
            System.Timers.Timer timer = new System.Timers.Timer { Interval = after, Enabled = false };
            timer.Elapsed += (sender, e) => {
                timer.Stop();
                action.Invoke();
                timer.Dispose();
                GC.SuppressFinalize(timer);
            };

            timer.Start();
        }
    }
}
