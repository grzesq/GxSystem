using GxMTService;
using GxMTService.DTO;
using System;
using System.Reactive.Subjects;

namespace GxFWPPSys
{
    public class MTBridge
    {
        private static Subject<bool> connect = new Subject<bool>();
        private static Subject<MTQuote> newQuote =  new Subject<MTQuote>();


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

        private bool OnMtNewQuote(MTQuote quote)
        {
            newQuote.OnNext(quote);
            return true;
        }



        public static IObservable<bool> MtConnect
        {
            get { return connect; }
        }

        

    }
}
