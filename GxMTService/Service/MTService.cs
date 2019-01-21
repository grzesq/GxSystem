using GxMTService.DTO;
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

        public bool NewQuote(double ask, double bid, double lastTr, uint time)
        {
                            
            return MTHost.MTNewQuote(new MTQuote(ask, bid, lastTr, time));
        }

        public bool NewBar(double o, double h, double l, double c,
                           double atr, uint time)
        {
            return MTHost.MTNewBar(o, h, l, c, atr, time);
        }

        public bool NewWeekBar(double o, double h, double l, double c, uint time)
        {
            return MTHost.MTNewWeekBar(o, h, l, c, time);
        }
    }
}
