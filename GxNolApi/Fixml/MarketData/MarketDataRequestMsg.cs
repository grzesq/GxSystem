using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum MDEntryType
    {
        Buy = '0',      // oferta kupna
        Sell = '1',     // oferta sprzedaży
        Trade = '2',    // kurs/wolumen/czas transakcji
        Vol = 'B',      // dotychczasowy obrót sesji
        Lop = 'C',      // liczba otwartych pozycji
        Index = '3',    // wartość/obrót indeksu
        Open = '4',     // kurs/obrót na otwarciu
        Close = '5',    // kurs/obrót na zamknięciu
        High = '7',     // kurs maksymalny
        Low = '8',      // kurs minimalny
        Ref = 'r'       // kurs odniesienia
    }

    public class MarketDataRequestMsg : FixmlMsg
	{
        private static uint nextId = 0;

		public uint Id;
		public SubscriptionRequestType Type;
		public byte MarketDepth;
		public FixmlInstrument[] Instruments;

        private readonly MDEntryType[] All = {
            MDEntryType.Buy, MDEntryType.Sell,
            MDEntryType.Trade, MDEntryType.Vol, MDEntryType.Lop,
            MDEntryType.Index,
            MDEntryType.Open, MDEntryType.Close,
            MDEntryType.High, MDEntryType.Low,
            MDEntryType.Ref
        };

        public MarketDataRequestMsg()
		{
			Id = nextId++;
		}

		protected override void PrepareXml()
		{

            XElement xml = new XElement("TrdgSesStatReq",
                               new XAttribute("ReqID", Id.ToString()),
                               new XAttribute("SubReqTyp", Type.ToString("d")));

            
			if (Type != SubscriptionRequestType.StartSubscription)
            {
                xml.Add(new XAttribute("MktDepth", MarketDepth.ToString()));
            }

			foreach (MDEntryType type in All)
                xml.Add(new XAttribute("Typ", MarketDepth.ToString()));

            if ((Instruments != null) && (Instruments.Length > 0))
			{
                XElement elem = new XElement("InstReq");
				foreach (FixmlInstrument instr in Instruments)
					instr.Write(elem, "Instrmt");
			}

            PrepareXmlMessage(xml);
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}] {2}", "TrdgSesStatReq", Id, Type));
            sb.Append(string.Format(" (EntryTypes: {0})", "All"));
			if (Instruments != null)
				foreach (FixmlInstrument instr in Instruments)
					sb.Append("\n " + instr);
			return sb.ToString();
		}

	}
}
