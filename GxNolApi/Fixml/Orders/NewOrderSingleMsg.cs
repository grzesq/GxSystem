using System;
using System.Xml;
using System.Text;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class NewOrderSingleMsg : FixmlMsg
	{
		private static uint nextId = 0;

		public string ClientOrderId;        // ID zlecenia, definiowane przez nas
		public DateTime TradeDate;          // data sesji, na którą składamy zlecenie
		public string Account;              // numer rachunku
		public uint? MinimumQuantity;       // ilość minimalna
		public uint? DisplayQuantity;       // ilość ujawniona
		public FixmlInstrument Instrument;  // interesujący nas papier wartościowy
		public OrderSide? Side;             // rodzaj zlecenia: kupno/sprzedaż
		public DateTime CreateTime;         // data+godz utworzenia zlecenia przez klienta
		public uint Quantity;               // ilość papierów w zleceniu
		public OrderType Type;              // typ zlecenia (limit, PKC, stoploss itp.)
		public decimal? Price;              // cena zlecenia
		public decimal? StopPrice;          // limit aktywacji
		public string Currency;             // waluta
		public OrdTimeInForce TimeInForce;  // rodzaj ważności zlecenia
		public DateTime? ExpireDate;        // data ważności zlecenia
        public string  ExpireTime;          // czas ważności zlecenia
        public char? TriggerType;           // rodzaj triggera (4 - DDM+ po zmianie ceny)
		public char? TriggerAction;         // akcja triggera (1 - aktywacja zlecenia DDM+)
		public decimal? TriggerPrice;       // cena uaktywnienia triggera (DDM+)
		public char? TriggerPriceType;      // rodzaj ceny uakt. (2 = cena ost. transakcji)
		public char? DeferredPaymentType;   // OTP (odroczony termin płatności) = T/P

		public NewOrderSingleMsg()
		{
			ClientOrderId = (++nextId).ToString();
			TradeDate = DateTime.Now;
			CreateTime = DateTime.Now;
			Type = OrderType.Limit;
			Currency = "PLN";
			TimeInForce = OrdTimeInForce.Day;
		}

		protected override void PrepareXml()
        {
            XElement xml = new XElement("Order",
                            new XAttribute("ID", ClientOrderId));

            if (TradeDate != null) xml.Add(new XAttribute("TrdDt", FixmlUtil.WriteDate((DateTime)TradeDate)));
            if (Account != null) xml.Add(new XAttribute("Acct", Account));
            if (MinimumQuantity != null)
                xml.Add(new XAttribute("MinQty", FixmlUtil.WriteDecimal(MinimumQuantity)));
            if (DisplayQuantity != null)
                xml.Add(new XElement("DsplyInstr", 
                                  new XAttribute("DisplayQty", FixmlUtil.WriteDecimal(DisplayQuantity))));
            if (Instrument != null) Instrument.Write(xml, "Instrmt");
            if (Side != null) xml.Add(new XAttribute("Side", OrderSideUtil.Write(Side)));
            xml.Add(new XAttribute("TxnTm", FixmlUtil.WriteDateTime(CreateTime)));
            xml.Add(new XElement("OrdQty", new XAttribute("Qty", Quantity.ToString())));
            xml.Add(new XAttribute("OrdTyp", OrderTypeUtil.Write(Type)));
            if (Price != null) xml.Add(new XAttribute("Px", FixmlUtil.WriteDecimal(Price)));
            if (StopPrice != null) xml.Add(new XAttribute("StopPx", FixmlUtil.WriteDecimal(StopPrice)));
            if (Currency != null) xml.Add(new XAttribute("Ccy", Currency));
            xml.Add(new XAttribute("TmInForce", OrdTmInForceUtil.Write(TimeInForce)));
            if (ExpireDate != null) xml.Add(new XAttribute("ExpireDt", FixmlUtil.WriteDate((DateTime)ExpireDate)));
            if (!string.IsNullOrEmpty(ExpireTime)) xml.Add(new XAttribute("ExpireTm", ExpireTime));
            if (TriggerPrice != null)
            {
                xml.Add(new XElement("TrgrInstr"),
                                new XAttribute("TrgrTyp", TriggerType.ToString()),
                                new XAttribute("TrgrActn", TriggerAction.ToString()),
                                new XAttribute("TrgrPx", FixmlUtil.WriteDecimal(TriggerPrice)),
                                new XAttribute("TrgrPxTyp", TriggerPriceType.ToString()));
            }
            if (DeferredPaymentType != null)
                xml.Add(new XAttribute("DefPayTyp", DeferredPaymentType.ToString()));
            PrepareXmlMessage(xml);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}] ", xml.Name, ClientOrderId));
			sb.Append(string.Format("  {0}  {1,-4} {2} x ", Instrument, Side, Quantity));
			if (Price != null) sb.Append(Price);
			else
				if ((Type == OrderType.PKC) || (Type == OrderType.StopLoss)) sb.Append("PKC");
				else sb.Append("PCR/PCRO");
			if (StopPrice != null) sb.Append(string.Format(" @{0}", StopPrice));
			return sb.ToString();
		}

	}
}
