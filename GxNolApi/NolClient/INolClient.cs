using GxNolApi.DTO;
using GxNolApi.NolClient.Fixml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxNolApi.NolClient
{
    interface INOLClient
    {
        NOLStatus       NolStatus { get; }
        BosAccounts     Accounts { get;}
        BosInstruments  Instruments { get;  }

        void Login();
        void Logout();
        void TradingSessionStatusStart();
        void TradingSessionStatusStop();
        void MarketDataStart();
        void MarketDataStop();
        void MarketDataSubscriptionAdd(params FixmlInstrument[] instruments);
        void MarketDataSubscriptionRemove(params FixmlInstrument[] instruments);
        string OrderCreate(OrderData data);
        void OrderReplace(OrderData data);
        void OrderCancel(OrderData data);
    }
}

