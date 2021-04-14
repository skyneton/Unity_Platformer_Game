using System;
using System.Collections;
using UnityEngine;

public class TimeUtils : MonoBehaviour {
    private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long CurrentTimeInMillis() {
        return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }
}