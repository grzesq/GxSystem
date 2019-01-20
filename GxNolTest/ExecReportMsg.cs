using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GxNolApi.NolClient.Fixml;
using System.Xml.Linq;

namespace GxNolTest
{
    [TestClass]
    public class ExecReportMsg : ExecutionReportMsg
    {
        private string xml_st2 =
        @"<FIXML v=""5.0"" r=""20080317"" s=""20080314"">" +
            @" <ExecRpt Stat=""2"" ExecID=""0"" OrdID2=""107412688"" OrdID=""611346428"" Side=""2"" CumQty=""1"" " +
            @"LeavesQty=""0"" Acct=""096512"" OrdTyp=""L"" Px=""2268.00"" TmInForce=""t"" ExpireTm=""16:50"" " +
            @"TxnTm=""20190102-15:02:27"" NetMny=""45367.00"" MinQty=""0"" DefPayTyp=""N""> " +
                @" <Instrmt Sym=""FW20H1920"" ID=""PL0GF0015487"" Src=""4""/> " +
                @" <OrdQty Qty=""1"" /> " +
                @" <Comm CommTyp=""3"" Comm=""7.00""/> " +
                 @" <DsplyInstr DisplayQty=""0"" /> " +
            @"</ExecRpt ></FIXML>";

        private string xml_trd =
            @"<FIXML v = ""5.0"" r=""20080317"" s=""20080314""> " +
            @"<ExecRpt ExecTyp = ""F"" ExecID=""611221557"" OrdID2=""107404850"" Side=""2"" Acct=""096512"" Px=""2265.00"" " +
                @"TxnTm=""20190102-09:00:02"" NetMny=""45307.00"" > " +
                    @"<Instrmt Sym = ""FW20H1920"" ID=""PL0GF0015487"" Src=""4""/> " +
                    @" <OrdQty Qty = ""1"" />  " +
                    @" <Comm CommTyp=""3"" Comm=""7.00""/></ExecRpt></FIXML>  ";

        [TestMethod]
        public void ExecRepo_Filled()
        {
            xmlDoc = XDocument.Parse(xml_st2);
            ParseXmlMessage(xml_st2);

            Assert.AreEqual(Account, "096512");
            Assert.AreEqual(ExecType, null);
            Assert.AreEqual(Status, ExecReportStatus.Filled);
            Assert.AreEqual(BrokerOrderId, "611346428");
            Assert.AreEqual(BrokerOrderId2, "107412688");
            Assert.AreEqual(Side, OrderSide.Sell);
            Assert.AreEqual(CumulatedQuantity, (uint)1);
            Assert.AreEqual(LeavesQuantity, (uint)0);
            Assert.AreEqual(Type, OrderType.Limit);
            Assert.AreEqual(Price, (decimal)2268);
            Assert.AreEqual(TimeInForce, OrdTimeInForce.Time);
            Assert.AreEqual(ExpireTime, "16:50");
            Assert.AreEqual(NetMoney, (decimal)45367);
            Assert.AreEqual(DeferredPaymentType, (char)'N');
            Assert.AreEqual(Instrument.Symbol, "FW20H1920");
            Assert.AreEqual(Instrument.SecurityId, "PL0GF0015487");
            Assert.AreEqual(Quantity, (uint)1);
            Assert.AreEqual(Commission, (decimal)7);
            Assert.AreEqual(CommissionType, OrdCommissionType.Absolute);
        }

        [TestMethod]
        public void ExecRepo_Trade()
        {
            xmlDoc = XDocument.Parse(xml_trd);
            ParseXmlMessage(xml_trd);

            Assert.AreEqual(Account, "096512");
            Assert.AreEqual(ExecType, ExecReportType.Trade);
            Assert.AreEqual(Status, null);
            Assert.AreEqual(BrokerOrderId, "");
            Assert.AreEqual(BrokerOrderId2, "107404850");
            Assert.AreEqual(ExecId, "611221557");
            Assert.AreEqual(Side, OrderSide.Sell);
            Assert.AreEqual(CumulatedQuantity, null);
            Assert.AreEqual(LeavesQuantity, null);
            Assert.AreEqual(Type, null);
            Assert.AreEqual(Price, (decimal)2265);
            Assert.AreEqual(TimeInForce, null);
            Assert.AreEqual(ExpireTime, "");
            Assert.AreEqual(NetMoney, (decimal)45307);
            Assert.AreEqual(DeferredPaymentType, null);
            Assert.AreEqual(Instrument.Symbol, "FW20H1920");
            Assert.AreEqual(Instrument.SecurityId, "PL0GF0015487");
            Assert.AreEqual(Quantity, (uint)1);
            Assert.AreEqual(Commission, (decimal)7);
            Assert.AreEqual(CommissionType, OrdCommissionType.Absolute);
        }
    }
}
