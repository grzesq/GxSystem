using GxMTService;
using System;
using System.Reactive.Subjects;

namespace GxFWPPSys.src
{
    public class MTBridge
    {
        private static Subject<bool> connect    = new Subject<bool>();
        

        public void StartHost()
        {
            MTHost.OnInit += OnMtInit;
            MTHost.OnDeinit+= OnMtDeinit;
            MTHost.StartHost();
        }

        private bool OnMtInit()
        {
            connect.OnNext(true);
            return true;
        }

        private bool OnMtDeinit()
        {
            connect.OnNext(false);
            return true;
        }


        public static IObservable<bool> MtConnect
        {
            get { return connect; }
        }

        

    }
}
