using System.Text;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class NewsMsg : FixmlMsg
	{
		public const string MsgName = "News";

		public string OrigTime { get; private set; }
		public string Headline { get; private set; }
		public string Text { get; private set; }

        protected NewsMsg() { MsgType = FixmlMsgType.NewsMsg; }
        public NewsMsg(FixmlMsg m) : base(m) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.NewsMsg;
            base.ParseXmlMessage(MsgName);

            OrigTime = FixmlUtil.ReadString(xml, "OrigTm");
			Headline = FixmlUtil.ReadString(xml, "Headline");

            XCData elem = (XCData)xml.FirstNode;
            if (elem != null)
            {
                Text = elem.Value;
            }
		}

		public override string ToString()
		{
			return string.Format("[{0}] '{1}'", xml.Name, Text);
		}

	}
}
