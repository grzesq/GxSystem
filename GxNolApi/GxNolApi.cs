using GxNolApi.NolClient;
using System;
namespace GxNolApi
{
    public class GxNolApi : IGxNolApi, IDisposable
    {
        private INOLClient connection = new NOLClient();

        public GxNolApi()
        {
        }

        public bool IsConnected { get { return (Connection != null); } }

        internal INOLClient Connection { get => connection; set => connection = value; }

        INOLClient IGxNolApi.Connection
        {
            get
            {
                return connection;
            }
        }

        public void Connect()
        {
           connection.Login();
        }

        public void Disconnect()
        {
            connection.Logout();
        }

        public BosAccounts GetAccounts()
        {
            return connection.Accounts;
        }

        public BosInstruments GetInstruments()
        {
            return connection.Instruments;
        }


        public void Dispose()
        {
        }
    }
}
