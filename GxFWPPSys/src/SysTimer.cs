using System;
using System.Reactive.Subjects;
using System.Timers;

namespace GxFWPPSys.src
{
    public class SysTimer
    {
        private static Subject<DateTime> newMinute = new Subject<DateTime>();
        private static Subject<DateTime> newSecond = new Subject<DateTime>();

        private static Timer sysTimer;

        private static int lastMin = -1;
        private static int lastSec = -1;

        public static void Start()
        {
            sysTimer = new System.Timers.Timer(125);
            // Hook up the Elapsed event for the timer. 
            sysTimer.Elapsed += OnTimedEvent;
            sysTimer.AutoReset = true;
            sysTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            if (now.Second != lastSec)
            {
                lastSec = now.Second;
                newSecond.OnNext(now);
            }

            if (now.Minute != lastMin)
            {
                lastMin = now.Minute;
                newMinute.OnNext(now);
            }
        }

        public static IObservable<DateTime> NewMinute
        {
            get { return newMinute; }
        }

        public static IObservable<DateTime> NewSecond
        {
            get { return newSecond; }
        }



    }
}
