using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GxNolApi.NolClient.Fixml;
using System.Xml.Linq;

namespace GxNolTest
{
    [TestClass]
    public class FixBizMsgRej : BizMessageRejectMsg
    {
        [TestMethod]
        public void RejectLoginMessga()
        {
            string xml = "<FIXML v=\"5.0\" r = \"20080317\" s = \"20080314\" >" +
                         "<BizMsgRej BizRejRsn=\"5\" RefMsgTyp=\"BE\" Txt=\"Incorrect UserReqID\"/></FIXML>";
            xmlDoc = XDocument.Parse(xml);
            ParseXmlMessage(xml);
            Assert.AreEqual(ReferenceMsgType, BizMsgReferenceMsgType.UserRequest);
            Assert.AreEqual(RejectReason, BizMsgRejectReason.RequiredFieldMissing);
            Assert.AreEqual(RejectText, "Incorrect UserReqID");
        }
    }
}
