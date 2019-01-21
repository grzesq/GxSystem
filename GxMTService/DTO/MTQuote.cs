using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxMTService.DTO
{
    public class MTQuote
    {
        public double Ask { get; private set;}
        public double Bid { get; private set; }
        public double LastPx { get; private set; }
        public DateTime Time { get; private set; }

        public MTQuote(double ask, double bid, double lastPx, uint time)
        {
            Ask = ask;
            Bid = bid;
            LastPx = lastPx;
            Time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            Time = Time.AddSeconds(time);
        }

    }
}
