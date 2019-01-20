using System;
using System.Net.Sockets;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum UserStatus
    {
        LoggedIn = 1,
        LoggedOut = 2,
        UserUnknown = 3,
        PasswordIncorrect = 4,
        PasswordChanged = 5,
        Other = 6,
        ForcedLogout = 7,
        SessionShutdown = 8
    }

    public class UserResponseMsg : FixmlMsg
	{
		public const string MsgName = "UserRsp";

		public uint UserReqID { get; private set; }
		public string Username { get; private set; }
		public UserStatus UsrStatus { get; private set; }
        public NOLStatus NolStatus { get; private set; }
        public string StatusText { get; private set; }

        protected UserResponseMsg() { MsgType = FixmlMsgType.UserRespons; }
        public UserResponseMsg(Socket socket) : base(socket) { }
		public UserResponseMsg(FixmlMsg m) : base(m) { }

		protected override void ParseXmlMessage(string name)
		{
            MsgType = FixmlMsgType.UserRespons;
            base.ParseXmlMessage(MsgName);

            UserReqID = Convert.ToUInt32(xml.Attribute("UserReqID").Value);
			Username = FixmlUtil.ReadString(xml, "Username");

            int i = Convert.ToInt32(xml.Attribute("UserStat").Value);
            if (!Enum.IsDefined(typeof(UserStatus), (UserStatus)i))
                FixmlUtil.Error(xml, name, i, "- unknown UserStatus");

            UsrStatus = (UserStatus)i;
            StatusText = FixmlUtil.ReadString(xml, "UserStatText"); 

            if  (UsrStatus == UserStatus.Other)
            {
                if (StatusText.Equals("User is already logged"))
                {
                    NolStatus = NOLStatus.Connecting;
                }
                else if (StatusText.Length == 1)
                {
                    try
                    {
                        i = Convert.ToInt32(StatusText);
                        if (i > 0)
                        {
                            if (!Enum.IsDefined(typeof(NOLStatus), (NOLStatus)i))
                                FixmlUtil.Error(xml, name, i, "- unknown NOLStatus");
                            NolStatus = (NOLStatus)i;
                        }
                    }
                    catch { }
                }
            }
		}

		public override string ToString()
		{
			return string.Format("[{0}:{1}] '{2}' {3} {4}",
                "UserRsp", UserReqID, Username, UsrStatus,
				(StatusText != null ? "(" + StatusText + ")" : ""));
		}

	}
}
