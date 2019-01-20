using Microsoft.VisualStudio.TestTools.UnitTesting;
using GxNolApi.NolClient.Fixml;
using System.Xml.Linq;


namespace GxNolTest
{
    [TestClass]
    public class MarketDataMsg : MarketDataIncRefreshMsg
    {

        private string xmlM =
                @"<FIXML v = ""5.0"" r=""20080317"" s=""20080314""> " +
                @"<MktDataInc MDReqID = ""1"" > " +
                    @"<Inc Typ=""1"" Px=""6.74"" CCy=""PLN"" Sz=""7199"" NumOfOrds=""3"" MDPxLvl=""5""> " +
                        @" <Instrmt Sym = ""PGNIG"" ID=""PLPGNIG00014"" Src=""4""/> " +
                    @" </Inc> " +
                    @" <Inc Typ = ""1"" Px=""6.71"" CCy=""PLN"" Sz=""82846"" NumOfOrds=""7"" MDPxLvl=""2""> " +
                        @" <Instrmt Sym = ""PGNIG"" ID=""PLPGNIG00014"" Src=""4""/> " +
                    @" </Inc> </MktDataInc></FIXML>";

        [TestMethod]
        public void TestMethod1()
        {
            xmlDoc = XDocument.Parse(xmlM);
            ParseXmlMessage(xmlM);
            Assert.AreEqual(Entries.Length, 2);
            Assert.AreEqual(Entries[0].Instrument.Symbol, "PGNIG");
            Assert.AreEqual(Entries[0].Price, (decimal)6.74);
            Assert.AreEqual(Entries[0].PriceStr, "6.74");

        }
    }
}
