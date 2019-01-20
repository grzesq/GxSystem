using System.Text;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class OrderStatusRequestMsg : FixmlMsg
	{
		private static uint nextId = 0;

		public string OrderStatusId;        // ID zapytania, definiowane przez nas
		public string ClientOrderId;        // ID zlecenia, nadane przez nas
		public string BrokerOrderId;        // główne id zlecenia nadane w DM
		public string BrokerOrderId2;       // numer zlecenia nadany w DM
		public string Account;              // numer rachunku
		public FixmlInstrument Instrument;  // nazwa papieru
		public OrderSide? Side;             // rodzaj zlecenia: kupno/sprzedaż

		public OrderStatusRequestMsg()
		{
			OrderStatusId = (++nextId).ToString();
		}

		protected override void PrepareXml()
        {
            XElement xml = new XElement("OrdStatReq",
                new XAttribute("StatReqID", OrderStatusId));

			if (ClientOrderId != null) xml.Add(new XAttribute("ID", ClientOrderId));
			if (BrokerOrderId != null) xml.Add(new XAttribute("OrdID", BrokerOrderId));
			if (BrokerOrderId2 != null) xml.Add(new XAttribute("OrdID2", BrokerOrderId2));
			if (Account != null) xml.Add(new XAttribute("Acct", Account));
			if (Instrument != null) Instrument.Write(xml, "Instrmt");
			if (Side != null) xml.Add(new XAttribute("Side", OrderSideUtil.Write(Side)));

            PrepareXmlMessage(xml);
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}:{2}] ", xml.Name, OrderStatusId, ClientOrderId));
			sb.Append(string.Format(" {0,-4} {1}", Side, Instrument));
			return sb.ToString();
		}

	}
}
