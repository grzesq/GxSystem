using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GxNolApi.NolClient.Fixml;
using System.Xml.Linq;

namespace GxNolTest
{
    [TestClass]
    public class UserResponse : UserResponseMsg
    {
        private string header = @"<FIXML v=""5.0"" r = ""20080317"" s = ""20080314"" >";

        [TestMethod]
        public void LoginSyncMessage()
        {
            string xml = header +
             @"<UserRsp UserReqID=""0"" Username=""BOS"" MktDepth=""1"" UserStat=""1""/></FIXML>";
            xmlDoc = XDocument.Parse(xml);
            ParseXmlMessage(xml);

            uint id = 0;

            Assert.AreEqual(UserReqID, id);
            Assert.AreEqual(Username, "BOS");
            Assert.AreEqual(UsrStatus, UserStatus.LoggedIn);
            Assert.AreEqual(NolStatus, GxNolApi.NOLStatus.NotSet);
            Assert.AreEqual(StatusText, "");
        }

        [TestMethod]
        public void AlredyLoginMessage()
        {
            string xml = header +
             @"<UserRsp UserReqID = ""1"" Username=""BOS"" UserStat=""6"" UserStatText=""User is already logged""/></FIXML>";
            xmlDoc = XDocument.Parse(xml);
            ParseXmlMessage(xml);

            uint id = 1;

            Assert.AreEqual(UserReqID, id);
            Assert.AreEqual(Username, "BOS");
            Assert.AreEqual(UsrStatus, UserStatus.Other);
            Assert.AreEqual(NolStatus, GxNolApi.NOLStatus.Connecting);
            Assert.AreEqual(StatusText, "User is already logged");
        }


        [TestMethod]
        public void LoginAsyncMessage()
        {
            string xml = header +
                @"<UserRsp UserReqID=""0"" UserStat=""6"" UserStatText=""3"" /></FIXML>";
            xmlDoc = XDocument.Parse(xml);
            ParseXmlMessage(xml);

            uint id = 0;
            Assert.AreEqual(UserReqID, id);
            Assert.AreEqual(Username, "");
            Assert.AreEqual(UsrStatus, UserStatus.Other);
            Assert.AreEqual(NolStatus, GxNolApi.NOLStatus.Online);
            Assert.AreEqual(StatusText, "3");
        }

        [TestMethod]
        public void LoginAsyncStatusOffline()
        {
            string xml = header +
                @"<UserRsp UserReqID=""0"" UserStat=""6"" UserStatText=""2"" /></FIXML>";
            xmlDoc = XDocument.Parse(xml);
            ParseXmlMessage(xml);

            uint id = 0;
            Assert.AreEqual(UserReqID, id);
            Assert.AreEqual(Username, "");
            Assert.AreEqual(UsrStatus, UserStatus.Other);
            Assert.AreEqual(NolStatus, GxNolApi.NOLStatus.Offline);
            Assert.AreEqual(StatusText, "2");
        }



    }
}
