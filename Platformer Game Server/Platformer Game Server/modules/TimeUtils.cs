using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class TimeUtils {
        private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeInMillis() {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}
