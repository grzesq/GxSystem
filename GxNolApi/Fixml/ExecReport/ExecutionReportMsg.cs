using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum ExecReportType
    {
        New = '0',              // nowe zlecenie
        Trade = 'F',            // wykonane (może być częściowo)
        Canceled = '4',         // anulowane
        Replaced = '5',         // zmodyfikowane
        PendingCancel = '6',    // w trakcie anulowania
        PendingReplace = 'E',   // w trakcie modyfikacji
        Rejected = '8',         // odrzucenie zlecenia
        OrderStatus = 'I'       // status zlecenia(?)
    }

    public enum OrdRejectReason
    {
        OK = 0,
        WongID = 98,
        Other = 99
    }

    public enum OrdCommissionType
    {
        PerUnit = '1',  // na jednostkę
        Percent = '2',  // procent
        Absolute = '3'  // wartość absolutna
    }

    public enum ExecReportStatus
    {
        New,              // nowe
        Expired,          // archiwalne
        PendingReplace,   // w trakcie modyfikacji
        PartiallyFilled,  // wykonane/aktywne
        Filled,           // wykonane
        Canceled,         // anulowane
        PendingCancel,    // w trakcie anulaty
        Rejected,          // odrzucone
    }

    public class ExecutionReportMsg : FixmlMsg
	{
		public const string MsgName = "ExecRpt";

        private static Dictionary<string, ExecReportStatus> dict =
            new Dictionary<string, ExecReportStatus> {
                    { "0", ExecReportStatus.New },
                    { "C", ExecReportStatus.Expired },
                    { "E", ExecReportStatus.PendingReplace },
                    { "1", ExecReportStatus.PartiallyFilled },
                    { "2", ExecReportStatus.Filled },
                    { "4", ExecReportStatus.Canceled },
                    { "6", ExecReportStatus.PendingCancel },
                    { "8", ExecReportStatus.Rejected },
                    { "80", ExecReportStatus.Canceled },     // tymczasowo(?)
                    { "81", ExecReportStatus.Expired },       // tymczasowo(?)
            };

        public string BrokerOrderId { get; private set; }           // id zlecenia z DM, *nie* występuje w potw. transakcji
		public string BrokerOrderId2 { get; private set; }          // nr zlecenia, obecny zawsze (przynajmniej docelowo)
		public string ClientOrderId { get; private set; }           // id zlecenia nadawane przez nas
		public string StatusReqId { get; private set; }             // id zapytania o status zlecenia (OrderStatusRequest)
		public string ExecId { get; private set; }                  // id wykonania zlecenia
		public ExecReportType? ExecType { get; private set; }       // typ operacji, która spowodowała wysłanie tego raportu
		public ExecReportStatus? Status { get; private set; }       // aktualny status zlecenia
		public OrdRejectReason? RejectReason { get; private set; }  // powód odrzucenia zlecenia
		public string Account { get; private set; }                 // numer rachunku
		public FixmlInstrument Instrument { get; private set; }     // papier wartościowy, którego dotyczy raport
		public OrderSide Side { get; private set; }                 // 1=kupno, 2=sprzedaż
		public uint? Quantity { get; private set; }                 // ilość papierów w zleceniu
		public OrderType? Type { get; private set; }                // typ zlecenia (limit, PKC, stoploss itp.)
		public decimal? Price { get; private set; }                 // cena zlecenia
		public decimal? StopPrice { get; private set; }             // limit aktywacji
		public string Currency { get; private set; }                // waluta
		public OrdTimeInForce? TimeInForce { get; private set; }    // rodzaj ważności
		public DateTime? ExpireDate { get; private set; }           // data ważności zlecenia
        public string   ExpireTime { get; private set; }           // czas ważności zlecenia
        public decimal? LastPrice { get; private set; }             // cena w ostatniej transakcji
		public uint? LastQuantity { get; private set; }             // ilość w ostatniej transakcji
		public uint? LeavesQuantity { get; private set; }           // ilość pozostała w zleceniu
		public uint? CumulatedQuantity { get; private set; }        // dotychczas zrealizowana ilość
		public DateTime? TransactionTime { get; private set; }      // czas transakcji
		public decimal? Commission { get; private set; }            // wartość prowizji
		public OrdCommissionType? CommissionType { get; private set; }// typ prowizji
		public decimal? NetMoney { get; private set; }              // wartość netto transakcji
		public uint? MinimumQuantity { get; private set; }          // ilość minimalna
		public uint? DisplayQuantity { get; private set; }          // ilość ujawniona
		public string Text { get; private set; }                    // dowolny tekst
		public char? TriggerType { get; private set; }              // rodzaj triggera (4 - DDM+ po zmianie ceny)
		public char? TriggerAction { get; private set; }            // akcja triggera (1 - aktywacja zlecenia DDM+)
		public decimal? TriggerPrice { get; private set; }          // cena uaktywnienia triggera (DDM+)
		public char? TriggerPriceType { get; private set; }         // rodzaj ceny uakt. (2 = cena ost. transakcji)
		public char? DeferredPaymentType { get; private set; }      // OTP (odroczony termin płatności) = T/P


        protected ExecutionReportMsg() { MsgType = FixmlMsgType.ExecutionReportMsg; }
        public ExecutionReportMsg(Socket s) : base(s) { }
		public ExecutionReportMsg(FixmlMsg m) : base(m) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.ExecutionReportMsg;
            base.ParseXmlMessage(MsgName);

            BrokerOrderId = FixmlUtil.ReadString(xml, "OrdID");
			BrokerOrderId2 = FixmlUtil.ReadString(xml, "OrdID2");
			ClientOrderId = FixmlUtil.ReadString(xml, "ID");
			StatusReqId = FixmlUtil.ReadString(xml, "StatReqID");
			ExecId = FixmlUtil.ReadString(xml, "ExecID");
            Account = FixmlUtil.ReadString(xml, "Acct");
            Instrument = FixmlInstrument.Read(xml, "Instrmt");

            char? ch = FixmlUtil.ReadCharO(xml, "ExecTyp");
            if (ch != null)
                ExecType = (ExecReportType)ch;

            string str = FixmlUtil.ReadString(xml, "Stat");
            if (dict.ContainsKey(str))
                Status = dict[str];

            int? number = FixmlUtil.ReadIntO(xml, "RejRsn");
            if (number != null ) RejectReason = (OrdRejectReason)number;

            ch = FixmlUtil.ReadCharO(xml, "Side");
            Side = (OrderSide)ch;

            ch = FixmlUtil.ReadCharO(xml, "OrdTyp");
            if (ch!=null) Type = (OrderType)ch;

            ch = FixmlUtil.ReadCharO(xml, "TmInForce");
            if (ch != null) TimeInForce = (OrdTimeInForce)ch;

            Price = FixmlUtil.ReadDecimalO(xml, "Px");
            StopPrice = FixmlUtil.ReadDecimalO(xml, "StopPx");
            Currency = FixmlUtil.ReadString(xml, "Ccy");
            ExpireDate = FixmlUtil.ReadDateTimeO(xml, "ExpireDt");
            LastPrice = FixmlUtil.ReadDecimalO(xml, "LastPx");
            LastQuantity = FixmlUtil.ReadUIntO(xml, "LastQty");
            LeavesQuantity = FixmlUtil.ReadUIntO(xml, "LeavesQty");
            CumulatedQuantity = FixmlUtil.ReadUIntO(xml, "CumQty");
            TransactionTime = FixmlUtil.ReadDateTimeO(xml, "TxnTm");
            NetMoney = FixmlUtil.ReadDecimalO(xml, "NetMny");
            MinimumQuantity = FixmlUtil.ReadUIntO(xml, "MinQty");
            Text = FixmlUtil.ReadString(xml, "Text");
            DeferredPaymentType = FixmlUtil.ReadCharO(xml, "DefPayTyp");
            Commission = FixmlUtil.ReadDecimalO(xml.Element("Comm"), "Comm");
            ExpireTime = FixmlUtil.ReadString(xml, "ExpireTm");

            ch = FixmlUtil.ReadCharO(xml.Element("Comm"), "CommTyp");
            if (ch != null) CommissionType = (OrdCommissionType)ch;

            DisplayQuantity = FixmlUtil.ReadUIntO(xml.Element("DsplyInstr"), "DisplayQty");
            TriggerType = FixmlUtil.ReadCharO(xml.Element("TrgrInstr"),"TrgrTyp");
            TriggerAction = FixmlUtil.ReadCharO(xml.Element("TrgrInstr"), "TrgrActn");
            TriggerPrice = FixmlUtil.ReadDecimalO(xml.Element("TrgrInstr"), "TrgrPx");
            TriggerPriceType = FixmlUtil.ReadCharO(xml.Element("TrgrInstr"), "TrgrPxTyp");

            Quantity = FixmlUtil.ReadUIntO(xml.Element("OrdQty"),"Qty");
		}

		public string PriceStr
		{
			get
			{
				if ((Type == OrderType.PKC) || (Type == OrderType.StopLoss)) return "PKC";
				if (Type == OrderType.PCR_PCRO)
					switch (TimeInForce)
					{
						case OrdTimeInForce.Opening:
						case OrdTimeInForce.Closing: return "PCRO";
						default: return "PCR";
					}
				return Price.ToString();
			}
		}

		public decimal? CommissionValue
		{
			get
			{
				switch (CommissionType)
				{
					case OrdCommissionType.PerUnit: return Commission * Quantity;  // LastQuantity?
					case OrdCommissionType.Percent: return Commission * NetMoney;
					case OrdCommissionType.Absolute: return Commission;
					default: return null;
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("[{0}:{1}:{2}] {3:T}", "ExecRpt", BrokerOrderId2, ClientOrderId, TransactionTime));
            sb.Append(string.Format("  {0}  {1,-4} {2} x {3,-7}", Instrument, Side, Quantity, PriceStr));
			if (StopPrice != null) sb.Append(string.Format(" @{0}", StopPrice));
			if (Status != null) sb.Append(string.Format("  {0}", Status));
			else
				if (ExecType != null) sb.Append(string.Format("  ({0})", ExecType));
			if (Text != null) sb.Append(string.Format("'{0}'", Text));
			return sb.ToString();
		}

	}
}
