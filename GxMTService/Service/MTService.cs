using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GxMTService
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MTService : IMTService
    {
        private static readonly List<IClientCallback> CallbackChannels = 
                                                            new List<IClientCallback>();


        public string TestService()
        {
            return "OK";
        }

        public bool Init()
        {
            return MTHost.MTInit();
        }

        public bool Deinit()
        {
            return MTHost.MTDeinit();
        }

        public bool NewQuote(double ask, double bid, double lastTr)
        {
            return true;
        }

        public bool NewBar(double o, double h, double l, double c,
                           double atr, uint time)
        {
            return true;
        }

        public bool NewWeekBar(double o, double h, double l, double c, uint time)
        {
            return true;
        }
    }
}
