using System;
using System.Text;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class OrderCancelRequestMsg : FixmlMsg
	{
		private static uint nextId = 0;

		public string OrderCancelId;        // ID anulaty, definiowane przez nas
		public string ClientOrderId;        // ID zlecenia do anulowania, nadane przez nas
		public string BrokerOrderId;        // główne id zlecenia nadane w DM
		public string BrokerOrderId2;       // numer zlecenia nadany w DM
		public string Account;              // numer rachunku
		public FixmlInstrument Instrument;  // nazwa papieru
		public OrderSide? Side;             // rodzaj zlecenia: kupno/sprzedaż
		public DateTime? CreateTime;        // data+godz utworzenia zlecenia
		public uint? Quantity;              // ilość papierów w zleceniu
		public string Text;                 // dowolny tekst

		public OrderCancelRequestMsg()
		{
			OrderCancelId = (++nextId).ToString();
			CreateTime = DateTime.Now;
		}

        protected override void PrepareXml()
        {
            XElement xml = new XElement("OrdCxlReq",
                new XAttribute("ID", ClientOrderId));
            if (ClientOrderId != null)
                xml.Add(new XAttribute("OrigID", ClientOrderId));
            if (BrokerOrderId != null)
                xml.Add(new XAttribute("OrdID", BrokerOrderId));
            if (BrokerOrderId2 != null)
                xml.Add(new XAttribute("OrdID2", BrokerOrderId2));
            if (Account != null)
                xml.Add(new XAttribute("Acct", Account));
            if (Instrument != null)
                Instrument.Write(xml, "Instrmt");
            if (Side != null)
                xml.Add(new XAttribute("Side", OrderSideUtil.Write(Side)));
            if (CreateTime != null)
                xml.Add(new XAttribute("TxnTm", FixmlUtil.WriteDateTime((DateTime)CreateTime)));
            if (Quantity != null)
                xml.Add(new XElement("OrdQty", new XAttribute("Qty", Quantity.ToString())));
            if (Text != null)
                xml.Add(new XAttribute("Txt", Text));

            PrepareXmlMessage(xml);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}:{2}] ", xml.Name, OrderCancelId, ClientOrderId));
			sb.Append(string.Format("  {0}", Instrument));
			if (Side != null) sb.Append(string.Format("  {0,-4}", Side));
			if (Quantity != null) sb.Append(string.Format(" x {0} ", Quantity));
			return sb.ToString();
		}

	}
}
