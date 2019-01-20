using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public class StatementMsg : FixmlMsg
	{
		public const string MsgName = "Statement";

		public StatementData[] Statements { get; private set; }

        protected StatementMsg() { MsgType = FixmlMsgType.StatementMsg; }
        public StatementMsg(FixmlMsg m) : base(m) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.StatementMsg;
            base.ParseXmlMessage(MsgName);
			List<StatementData> list = new List<StatementData>();
            foreach (XElement stmt in xmlDoc.Root.Descendants("Statement"))
				list.Add(new StatementData(stmt));
			Statements = list.ToArray();
		}

		public override string ToString()
		{
			var a = Statements.Select(s => s.ToString()).ToArray();
			return base.ToString() + "\n" + string.Join("\n", a);
		}

	}
}
