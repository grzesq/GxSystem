using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxMTService.DTO
{
    public class MTBar
    {
        public double Open { get; private set; }
        public double High { get; private set; }
        public double Low { get; private set; }
        public double Close { get; private set; }
        public DateTime Time { get; private set; }

        public MTBar(double o, double h, double l, double c, uint time)
        {
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            Time = Time.AddSeconds(time);
        }
    }

    public class MTAtrBar: MTBar
    {
        public double Atr { get; private set; }

        public MTAtrBar(double o, double h, double l, double c,
                         double atr, uint time) : base(o, h, l, c, time)
        {
            Atr = atr;
        }
    }

}
