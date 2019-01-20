using System;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{

    public enum SubscriptionRequestType
    {
        StartSubscription = 1,
        CancelSubscription = 2
    }

    public class TrdSesStatusRequestMsg : FixmlMsg
	{

		private static uint nextId = 0;

		public uint Id;
		public SubscriptionRequestType Type;

		public TrdSesStatusRequestMsg()
		{
			Id = nextId++;
		}

        protected override void PrepareXml()
        {
            XElement xml = GetXml();
            PrepareXmlMessage(xml);
        }

        private XElement GetXml()
        {
            return new XElement("TrdgSesStatReq",
                                            new XAttribute("ReqID", Id.ToString()),
                                            new XAttribute("SubReqTyp", Type.ToString("d")));
        }

        public override string ToString()
		{
			return string.Format("[{0}:{1}] {2}", "TrdgSesStatReq", Id, Type);
		}

	}
}
