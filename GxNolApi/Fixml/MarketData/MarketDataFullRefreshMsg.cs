using System.Net.Sockets;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class MarketDataFullRefreshMsg : FixmlMsg
	{
		public const string MsgName = "MktDataFull";

		public int RequestId { get; private set; }

		public MarketDataFullRefreshMsg(Socket socket) : base(socket) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.MarketDataFullRefMsg;
            base.ParseXmlMessage(MsgName);
            RequestId = FixmlUtil.ReadInt(xml, "ReqID");
		}

		public override string ToString()
		{
			return string.Format("[{0}:{1}]", "MktDataFull", RequestId);
		}

	}
}
