using System;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum UserRequestType
    {
        Login = 1,
        Logout = 2,
        ChangePassword = 3,
        GetStatus = 4
    }

    public class UserRequestMsg : FixmlMsg
	{

		private static uint nextId = 0;

		public uint Id;
		public UserRequestType Type;
		public string Username;
		public string Password;
		public string NewPassword;

		public UserRequestMsg()
		{
			Id = nextId++;
		}

		protected override void PrepareXml()
		{
            XElement xml = new XElement("UserReq",
                                new XAttribute("UserReqID", Id.ToString()),
                                new XAttribute("UserReqTyp", Type.ToString("d")),
                                new XAttribute("Username", Username),
                                new XAttribute("Password", "BOS"));

            PrepareXmlMessage(xml);
		}

		public override string ToString()
		{
			return string.Format("[{0}:{1}] '{2}' {3}", "UserReq", Id, Username, Type);
		}

	}
}
