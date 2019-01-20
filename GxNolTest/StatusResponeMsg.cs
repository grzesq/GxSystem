using System;
using System.Xml.Linq;
using GxNolApi.NolClient.Fixml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GxNolTest
{
    [TestClass]
    public class StatusResponeMsg : TradingSessionStatusMsg
    {
        private readonly string xmlM = @"<FIXML v=""5.0"" r=""20080317"" s=""20080314"">" +
                             @"<TrdgSesStat ReqID = ""1"" SesSub=""S"">" +
                             @"<Instrmt Sym=""PGNIG"" ID=""PLPGNIG00014"" Src=""4""/>" +
                             @"</TrdgSesStat></FIXML>";

        [TestMethod]
        public void TestMethod1()
        {
            xmlDoc = XDocument.Parse(xmlM);
            ParseXmlMessage(xmlM);

            Assert.AreEqual(SessionPhase, TradingSessionPhase.Trading);
            Assert.AreEqual(SessionStatus, TradingSessionStatus.Unknown);
            Assert.AreEqual(Instrument.SecurityId, "PLPGNIG00014");
            Assert.AreEqual(Instrument.Symbol, "PGNIG");
        }
    }
}
