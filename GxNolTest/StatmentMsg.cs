using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GxNolApi.NolClient.Fixml;
using System.Xml.Linq;

namespace GxNolTest
{
    [TestClass]
    public class StatmentMessage : StatementMsg
    {
        private string emptyxml  = 
        @"<FIXML v=""5.0"" r =""20080317"" s =""20080314"">" + 
        @"<Statement Acct = ""00-22-096496"" type=""P"" ike=""N"">"+
            @"<Fund name = ""SecValueSum"" value=""0.00""/>"+
            @"<Fund name = ""Cash"" value=""1800.53"" />"+
            @"<Fund name = ""Deposit"" value=""0.00"" />"+
            @"<Fund name = ""CashBlocked"" value=""0.00"" />"+
            @"<Fund name = ""BlockedDeposit"" value=""0.00"" />"+
            @"<Fund name = ""SecSafeties"" value=""0.00"" />"+
            @"<Fund name = ""FreeDeposit"" value=""0.00"" />"+
            @"<Fund name = ""SecSafetiesUsed"" value=""0.00"" />"+
            @"<Fund name = ""OptionBonus"" value=""0.00"" />"+
            @"<Fund name = ""PortfolioValue"" value=""1800.53"" />"+
        @"</Statement>"+
        @"<Statement Acct = ""00-55-086820"" type=""M"" ike=""N"" >"+
                    @"<Fund name = ""SecValueSum"" value=""0.00""/>"+
                    @"<Fund name = ""Cash"" value=""0.00"" />"+
                    @"<Fund name = ""Liabilities"" value=""0.00"" />"+
                    @"<Fund name = ""Recivables"" value=""0.00"" />"+
                    @"<Fund name = ""MaxBuy"" value=""0.00"" />"+
                    @"<Fund name = ""CashRecivables"" value=""0.00"" />"+
                    @"<Fund name = ""MaxOtpBuy"" value=""0.00"" />"+
                    @"<Fund name = ""CashBlocked"" value=""0.00"" />"+
                    @"<Fund name = ""LiabilitiesLimitMax"" value=""0.00"" />"+
                    @"<Fund name = ""RecivablesBlocked"" value=""0.00"" />"+
                    @"<Fund name = ""PortfolioValue"" value=""0.00"" />"+
        @"</Statement>"+
        @"<Statement Acct = ""096512"" type=""P"" ike=""N"" >"+
                    @"<Fund name = ""SecValueSum"" value=""0.00""/>"+
                    @"<Fund name = ""Cash"" value=""5652.00"" />"+
                    @"<Fund name = ""Deposit"" value=""0.00"" />"+
                    @"<Fund name = ""CashBlocked"" value=""0.00"" />"+
                    @"<Fund name = ""BlockedDeposit"" value=""0.00"" />"+
                    @"<Fund name = ""FreeDeposit"" value=""0.00"" />"+
                    @"<Fund name = ""PortfolioValue"" value=""5652.00"" />"+
        @"</Statement></FIXML>";



        private string xmlM =
            @"<FIXML v = ""5.0"" r=""20080317"" s=""20080314"">" +
            @" <Statement Acct = ""00-22-234200"" type=""P"" ike=""N"" ><Fund name = ""SecValueSum"" value=""0.00""/>" +
                @"<Fund name = ""Cash"" value=""1.00""/>" +
                @" <Fund name = ""Deposit"" value=""0.00"" />" +
                @" <Fund name = ""CashBlocked"" value=""0.00""/>" +
                @" <Fund name = ""BlockedDeposit"" value=""0.00"" />" +
                @" <Fund name = ""SecSafeties"" value=""0.00"" />" +
                @" <Fund name = ""FreeDeposit"" value=""0.00"" />" +
                @" <Fund name = ""SecSafetiesUsed"" value=""0.00"" />" +
                @" <Fund name = ""OptionBonus"" value=""0.00""/>" +
                @" <Fund name = ""PortfolioValue"" value=""1.00"" />" +
             @" </Statement>" +
             @" <Statement Acct = ""00-55-006039"" type=""M"" ike=""N"" >" +
                @" <Position Acc110 = ""1"" Acc120=""0""><Instrmt ID = ""PLPGER000010"" Src=""4"" Sym=""PGE""/></Position>" +
                @" <Position Acc110 = ""2"" Acc120=""0""><Instrmt ID = ""PLPGNIG00014"" Src=""4"" Sym=""PGNIG""/></Position>" +
                @" <Position Acc110 = ""1"" Acc120=""0""><Instrmt ID = ""PLTAURN00011"" Src=""4"" Sym=""TAURONPE""/></Position>" +
                @" <Fund name = ""SecValueSum"" value=""29.84""/>" +
                @" <Fund name = ""Cash"" value=""8.10"" />" +
                @" <Fund name = ""Liabilities"" value=""0.00"" />" +
                @" <Fund name = ""Recivables"" value=""25.46"" />" +
                @" <Fund name = ""MaxBuy"" value=""111.87"" />" +
                @" <Fund name = ""CashRecivables"" value=""33.56"" />" +
                @" <Fund name = ""MaxOtpBuy"" value=""111.87""/>" +
                @" <Fund name = ""CashBlocked"" value=""0.00"" />" +
                @" <Fund name = ""LiabilitiesLimitMax"" value=""0.00"" />" +
                @" <Fund name = ""RecivablesBlocked"" value=""0.00"" />" +
                @" <Fund name = ""PortfolioValue"" value=""63.40"" />" +
            @" </Statement>" + 
            @" <Statement Acct = ""076490"" type=""P"" ike=""N"" >" +
                @" <Fund name = ""SecValueSum"" value=""0.00""/>" +
                @" <Fund name = ""Cash"" value=""0.00"" />" +
                @" <Fund name = ""Deposit"" value=""0.00"" />" +
                @" <Fund name = ""CashBlocked"" value=""0.00"" />" +
                @" <Fund name = ""BlockedDeposit"" value=""0.00"" />" +
                @" <Fund name = ""FreeDeposit"" value=""0.00"" />" +
                @" <Fund name = ""PortfolioValue"" value=""0.00""/>" +
            @" </Statement>" +
            @" <Statement Acct = ""600988"" type=""M"" ike=""Z"" >" +
                @" <Fund name = ""SecValueSum"" value=""0.00""/>" +
                @" <Fund name = ""Cash"" value=""0.00"" />" +
                @" <Fund name = ""Liabilities"" value=""0.00"" />" +
                @" <Fund name = ""Recivables"" value=""0.00"" />" +
                @" <Fund name = ""MaxBuy"" value=""0.00"" />" +
                @" <Fund name = ""CashRecivables"" value=""0.00"" />" +
                @" <Fund name = ""MaxOtpBuy"" value=""0.00"" />" +
                @" <Fund name = ""CashBlocked"" value=""0.00"" />" +
                @" <Fund name = ""LiabilitiesLimitMax"" value=""0.00"" />" +
                @" <Fund name = ""RecivablesBlocked"" value=""0.00"" />" +
                @" <Fund name = ""PortfolioValue"" value=""0.00"" />" +
            @" </Statement>" +
            @" <Statement Acct=""800594"" type=""M"" ike=""T"" >" +
                @" <Fund name = ""SecValueSum"" value=""0.00""/>" +
                @" <Fund name = ""Cash"" value=""0.00"" />" +
                @" <Fund name = ""Liabilities"" value=""0.00"" />" +
                @" <Fund name = ""Recivables"" value=""0.00"" />" +
                @" <Fund name = ""MaxBuy"" value=""0.00"" />" +
                @" <Fund name = ""CashRecivables"" value=""0.00"" />" +
                @" <Fund name = ""MaxOtpBuy"" value=""0.00"" />" +
                @" <Fund name = ""CashBlocked"" value=""0.00""/>" +
                @" <Fund name = ""LiabilitiesLimitMax"" value=""0.00""/>" +
                @" <Fund name = ""RecivablesBlocked"" value=""0.00"" />" +
                @" <Fund name = ""PortfolioValue"" value=""0.00"" />" +
                @" </Statement>" +
            "</FIXML>";

        [TestMethod]
        public void EmptyStatment()
        {
            xmlDoc = XDocument.Parse(emptyxml);
            ParseXmlMessage(emptyxml);

            Assert.AreEqual(Statements.Length, 3);
            Assert.AreEqual(Statements[0].Positions.Count, 0);
            Assert.AreEqual(Statements[1].Positions.Count, 0);
            Assert.AreEqual(Statements[2].Positions.Count, 0);
        }

        [TestMethod]
        public void RespStatmentMessage()
        {
            xmlDoc = XDocument.Parse(xmlM);
            ParseXmlMessage(xmlM);

            Assert.AreEqual(Statements.Length, 5);
            Assert.AreEqual(Statements[0].Positions.Count, 0);
            Assert.AreEqual(Statements[1].AccountNumber, "00-55-006039");
            Assert.AreEqual(Statements[1].Positions.Count, 3);
            Assert.AreEqual(Statements[2].Positions.Count, 0);
            Assert.AreEqual(Statements[3].Positions.Count, 0);
            Assert.AreEqual(Statements[4].Positions.Count, 0);

        }


    }
}
