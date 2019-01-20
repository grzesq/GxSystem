
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum BizMsgRejectReason
    {
        Other = 0,
        UnknownId = 1,
        UnknownSecurity = 2,
        UnsupportedMessageType = 3,
        ApplicationNotAvailable = 4,
        RequiredFieldMissing = 5,
        NotAuthorized = 6,
        DestinationNotAvailable = 7,
        InvalidPriceIncrement = 18
    }

    public enum BizMsgReferenceMsgType
    {
        UserRequest,                    // logowanie/wylogowanie
        NewOrderSingle,                 // nowe zlecenie
        OrderCancelRequest,             // anulata zlecenia
        OrderCancelReplaceRequest,      // modyfikacja zlecenia
        OrderStatusRequest,             // status zlecenia
        MarketDataRequest,              // notowania online
        TradingSessionStatusRequest     // status oraz faza sesji
    }


    public class BizMessageRejectException : FixmlErrorMsgException
    {
        public new BizMessageRejectMsg Msg { get { return msg as BizMessageRejectMsg; } }
        public BizMessageRejectException(string str, BizMessageRejectMsg msg) : base(str, msg) { }
        public BizMessageRejectException(BizMessageRejectMsg msg) : base(msg) { }
        protected BizMessageRejectException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class BizMessageRejectMsg : FixmlMsg
	{
		public const string MsgName = "BizMsgRej";

        private static Dictionary<string, BizMsgReferenceMsgType> dict =
            new Dictionary<string, BizMsgReferenceMsgType> {
                    { "BE", BizMsgReferenceMsgType.UserRequest },
                    { "D",  BizMsgReferenceMsgType.NewOrderSingle },
                    { "F",  BizMsgReferenceMsgType.OrderCancelRequest },
                    { "G",  BizMsgReferenceMsgType.OrderCancelReplaceRequest },
                    { "H",  BizMsgReferenceMsgType.OrderStatusRequest },
                    { "V",  BizMsgReferenceMsgType.MarketDataRequest },
                    { "g",  BizMsgReferenceMsgType.TradingSessionStatusRequest }
            };


        public BizMsgReferenceMsgType ReferenceMsgType { get; private set; }
	    public BizMsgRejectReason RejectReason { get; private set; }
	    public string RejectText { get; private set; }

        protected BizMessageRejectMsg() { MsgType = FixmlMsgType.BizMessageReject; }
        public BizMessageRejectMsg(FixmlMsg m) : base(m) { }

	    protected override void ParseXmlMessage(string name)
	    {
            MsgType = FixmlMsgType.BizMessageReject;
            base.ParseXmlMessage(MsgName);

            string msqTyp = xml.Attribute("RefMsgTyp").Value;
            if (!dict.ContainsKey(msqTyp))
                FixmlUtil.Error(xml, name, msqTyp, "- unknown BizReferenceMsgType");

            ReferenceMsgType = dict[msqTyp];

            string m = FixmlUtil.ReadString(xml, "BizRejRsn");
            if (m != null)
            {
                int res = Convert.ToInt32(m);

                if (!Enum.IsDefined(typeof(BizMsgRejectReason), (BizMsgRejectReason)res))
                    FixmlUtil.Error(xml, name, res, "- unknown BizRejectReason");
                RejectReason = (BizMsgRejectReason)res;
            }
			
		    RejectText = xml.Attribute("Txt").Value;
	    }

	    public override string ToString()
	    {
		    return string.Format("[{0}] '{1}' ({2}, {3})",
                "BizMsgRej", RejectText, ReferenceMsgType, RejectReason);
	    }

	}
}
