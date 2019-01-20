using GxNolApi.NolClient.Fixml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxNolApi
{

    public enum NOLStatus
    {
        NotSet = 0,
        Closed = 1, // zamkniecie aplikacji NOL3
        Offline = 2, // NOL3 jest offline
        Online = 3, // NOL3 jest online
        NotRunning = 4, // aplikacja NOL3 nie jest uruchomiona
        InvestorOffline = 5, //Inwestor jest offline
        Connecting = 6, // łączenie
        InvestorConnecting = 7  //łączenie (Inwestor jest offline
    }

    public static class GxBossaNolAPI
    {
        private static IGxNolApi api = new GxNolApi();

        public static bool IsNolConnected { get { return api.IsConnected; } }

//        public BosAccounts Accounts { get; private set; }
//        public BosInstruments Instruments { get; private set; }

        public static BosAccounts GetAccounts()
        {
            return api.GetAccounts();
        }

        public static BosInstruments GetInstruments()
        {
            return api.GetInstruments();
        }
        

        public static void ConnectToNOL()
        {
            api.Connect();
        }

        public static void DisconnectFromNOL()
        {
            api.Disconnect();
        }
    }
}
