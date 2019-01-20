using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum TradingSessionPhase
    {

         Unknown,
         Consultation,      // C - Konsultacja nadzoru
         PreTrading,        // P - Przed otwarciem
         IntradayAuction,   // E – Interwencja
         Opening,           //O – Otwarcie
         Auction,           //R – Dogrywka
         Trading,           //S - Sesja notowań ciągłych
         Intervetion,       //N - Interwencja nadzoru
         Consultation1,     //F - Konsultacja nadzoru 2raz(?)
         MarketClosed,      // B - Po sesji*/
    }

    public enum TradingSessionStatus
    {
        Unknown, 
        Auction,        //AR – Równoważenie
        HaltedWithOrder,//AS - Zakaz obrotu, zlecenia tak
        Trading,        //A - Papier w obrocie
        NoAuction,      //IR - Bez dogrywki - zlecenia nie
        HaltedS,        //IS - Zakaz obrotu
        Frozen,         //AG - Zamrożenie instrumentu
        Halted          //I - Zakaz obrotu
    }

    public class TradingSessionStatusMsg : FixmlMsg
	{
        private static Dictionary<string, TradingSessionPhase> dictTS =
            new Dictionary<string, TradingSessionPhase> {
                            { "C",  TradingSessionPhase.Consultation },
                            { "P",  TradingSessionPhase.PreTrading },  
                            { "E",  TradingSessionPhase.IntradayAuction },      
                            { "O",  TradingSessionPhase.Opening },      
                            { "R",  TradingSessionPhase.Auction },  
                            { "S",  TradingSessionPhase.Trading },
                            { "N",  TradingSessionPhase.Intervetion },
                            { "F",  TradingSessionPhase.Consultation1 },
                            { "B",  TradingSessionPhase.MarketClosed },
            };


        private static Dictionary<string, TradingSessionStatus> dictSS =
            new Dictionary<string, TradingSessionStatus> {
                                    { "AR", TradingSessionStatus.Auction },
                                    { "AS", TradingSessionStatus.Halted },
                                    { "A",  TradingSessionStatus.Trading },
                                    { "IR", TradingSessionStatus.NoAuction },
                                    { "IS", TradingSessionStatus.HaltedS },
                                    { "AG", TradingSessionStatus.Frozen },
                                    { "I",  TradingSessionStatus.Halted },
            };


        public const string MsgName = "TrdgSesStat";

		public uint RequestId { get; private set; }
		public string SessionId { get; private set; }
		public TradingSessionPhase SessionPhase { get; private set; }
		public TradingSessionStatus SessionStatus { get; private set; }
		public int? RejectReason { get; private set; }
		public FixmlInstrument Instrument { get; private set; }

        protected TradingSessionStatusMsg() { MsgType = FixmlMsgType.SessionStatusMsg; }
        public TradingSessionStatusMsg(Socket socket) : base(socket) { }
		public TradingSessionStatusMsg(FixmlMsg msg) : base(msg) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.SessionStatusMsg;
            base.ParseXmlMessage(MsgName);

            RequestId = FixmlUtil.ReadUInt(xml, "ReqID");
			SessionId = FixmlUtil.ReadString(xml, "SesID");

            string str = FixmlUtil.ReadString(xml, "SesSub");
            if (string.IsNullOrEmpty(str) || !dictTS.ContainsKey(str))
                SessionPhase = TradingSessionPhase.Unknown;
            else
                SessionPhase = dictTS[str];

            str = FixmlUtil.ReadString(xml, "Stat");
            if (string.IsNullOrEmpty(str) || !dictSS.ContainsKey(str))
                SessionStatus = TradingSessionStatus.Unknown;
            else
                SessionStatus = dictSS[str];

            RejectReason = FixmlUtil.ReadIntO(xml, "StatRejRsn");
			Instrument = FixmlInstrument.Read(xml, "Instrmt");
		}

        public override string ToString() => string.Format("[{0}:{1}]" + (Instrument != null ? " {2,-10}  phase = {3}, status = {4} {5}" : ""),
                "TrdgSesStat", RequestId, Instrument, SessionPhase, SessionStatus,
                (RejectReason != null) ? "(" + RejectReason + ")" : null);

    }
}
