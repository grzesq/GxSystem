
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum MDRejectReason
    {
        UnknownSymbol = '0',            // nieznany walor
        DuplicateRequestId = '1',       // duplikat MDReqID
        UnsupportedRequestType = '4',   // błąd w polu SubReqTyp
        UnsupportedMarketDepth = '5',   // niewspierana liczba ofert
        UnsupportedEntryType = '8'      // niewspierane notowania
    }

    public class MarketDataReqRejectMsg : FixmlMsg
	{
		public const string MsgName = "MktDataReqRej";

		public int? RequestId { get; private set; }
		public MDRejectReason RejectReason { get; private set; }
		public string RejectText { get; private set; }

		public MarketDataReqRejectMsg(FixmlMsg m) : base(m) { }

        protected override void ParseXmlMessage(string name)
        {
            MsgType = FixmlMsgType.MarketDataRejMsg;
            base.ParseXmlMessage(MsgName);
            RequestId = FixmlUtil.ReadIntO(xml, "ReqID");

            char ch = FixmlUtil.ReadChar(xml, "ReqRejResn");
            RejectReason = (MDRejectReason)ch;
            RejectText = FixmlUtil.ReadString(xml, "Txt");
        }

        public override string ToString()
		{
			return string.Format("[{0}:{1}] {2} {3}",
                "MktDataReqRej", RequestId, RejectReason,
				(RejectText != null ? "(" + RejectText + ")" : ""));
		}

	}
}
