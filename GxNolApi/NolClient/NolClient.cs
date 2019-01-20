using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GxNolApi.NolClient.Fixml;
using GxNolApi.DTO;

namespace GxNolApi.NolClient
{
    public class NOLClient : INOLClient, IDisposable
    {
        //private bool loggedIn = false;
        private Thread thread;
        private bool loggedIn = false;

        public NOLStatus NolStatus { get; private set; }
        public BosAccounts Accounts { get; private set; }
        public BosInstruments Instruments { get; private set; }

        // blokada/synchronizacja market data, musimy zaczekaqc 
        // aż zakończymy metodę MarketDataStart(), gdzie m.in. dopiero jest ustawiany nowy mdReqId
        // (asynchroniczne MktDataInc zaczynają tu przychodzić jeszcze zanim tam odbierzemy MktDataFull)
        private readonly object mdResults = new object();
        private uint? mdReqId = null;

        public NOLClient()
        {
            Accounts = new BosAccounts(this);
            Instruments = new BosInstruments();
            NolStatus = NOLStatus.NotSet;
        }

        #region IDisposable support

        ~NOLClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (thread != null) ThreadStop();
                if (loggedIn) Logout();
            }
            catch (Exception e)
            {
                SendMessage.Send(MessagegType.LogE, "NOL Client Dispose " + e.Message);
            }
        }
        #endregion

        #region NOL3 connection sockets

        // ustalenie numeru portu, na którym nasłuchuje aplikacja NOL3
        // parametr "name" określa, który z dwóch portów nas interesuje: psync, pasync
        private static int GetRegistryPort(String name)
        {
            string regname = "Software\\COMARCH S.A.\\NOL3\\7\\Settings";
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(regname);
            if (regkey == null) throw new NolClientException("NOL3 registry key missing!");
            Object value = regkey.GetValue("ncaset_" + name);
            if (value == null) throw new NolClientException("NOL3 registry settings missing!");
            if (value.ToString() == "0") throw new NolClientException("NOL3 port '" + name + "' not ready!");
            value = regkey.GetValue("nca_" + name);
            if (value == null) throw new NolClientException("NOL3 port '" + name + "' not defined!");
            return int.Parse(value.ToString());
        }

        public static Socket GetSyncSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", GetRegistryPort("psync"));
            socket.ReceiveTimeout = 10000;  // <- nie mniej, niż max. czas odpowiedzi na request synchr.
            socket.SendTimeout = 10000;
            return socket;
        }

        public static Socket GetAsyncSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", GetRegistryPort("pasync"));
            socket.ReceiveTimeout = 10000;  // <- nie mniej, niż odstęp między Heartbeat (zwykle co 1sek?)
            socket.SendTimeout = 10000;
            return socket;
        }
        #endregion

        #region Asynchronous connection thread

        // uruchomienie wątku obsługującego odbiór komunikatów z kanału asynchronicznego
        private void ThreadStart()
        {
            thread = new Thread(ThreadProc)
            {
                Name = "NOL3 async connection",
                IsBackground = true
            };
            thread.Start();
        }

        // zakończenie wątku obsługującego kanał asynchroniczny
        private void ThreadStop()
        {
            thread.Abort();
            thread = null;
        }

        // procedura wątku z obsługą kanału asynchronicznego
        private void ThreadProc()
        {
            try
            {
                using (Socket socket = GetAsyncSocket())
                {
                    while (socket.Connected)
                    {
                        FixmlMsg msg;

                        try { msg = new FixmlMsg(socket); }
                        catch (ThreadAbortException) { throw; }
                        catch (FixmlSocketException) { throw; }
                        catch (Exception e)
                        {
                            SendMessage.Send(MessagegType.LogE, "FIX msg: " + e.Message);
                            continue;
                        }

                        msg = GetParsedMsg(msg);
                        ProcessFixMsg(msg);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                SendMessage.Send(MessagegType.LogE, "NOL Thread: " + e.Message);
            }
            SendMessage.Send(MessagegType.LogD, "NolClient.ThreadProc stop");
        }

        private static FixmlMsg GetParsedMsg(FixmlMsg msg)
        {
            if (msg.GetType() != typeof(FixmlMsg))
                throw new ArgumentException("Base 'FixmlMsg' class object required", "msg");
            switch (msg.XMLName)
            {
                case StatementMsg.MsgName:              return new StatementMsg(msg);
                case UserResponseMsg.MsgName:           return new UserResponseMsg(msg);
                case ExecutionReportMsg.MsgName:        return new ExecutionReportMsg(msg);
                case TradingSessionStatusMsg.MsgName:   return new TradingSessionStatusMsg(msg);
                case MarketDataIncRefreshMsg.MsgName:   return new MarketDataIncRefreshMsg(msg);
                case NewsMsg.MsgName:                   return new NewsMsg(msg);
                case "Heartbeat": return msg;  // <- dla tego szkoda oddzielnej klasy ;-)
/*
                case AppMessageReportMsg.MsgName: return new AppMessageReportMsg(msg);
                case NewsMsg.MsgName: return new NewsMsg(msg);
*/
                default:
                    string txt = string.Format("Unexpected async message '{0}'", msg.XMLName);
                    //SendMessage.Send(MessagegType.LogW, txt); ;
                    return msg;

            }
        }

        private void ProcessFixMsg(FixmlMsg msg)
        {
//            if (msg.GetType() != typeof(FixmlMsg))
  //              throw new ArgumentException("Base 'FixmlMsg' class object required", "msg");

            switch (msg.MsgType)
            {
                case FixmlMsg.FixmlMsgType.UserRespons:
                    new NolMsgHandler(this).AsyncUserResponseMsgHandler((UserResponseMsg)msg);
                    break;
                case FixmlMsg.FixmlMsgType.BizMessageReject:
                    break;
                case FixmlMsg.FixmlMsgType.StatementMsg:
                    new NolMsgHandler(this).AsyncStatmentHandler((StatementMsg)msg);
                    break;
                case FixmlMsg.FixmlMsgType.ExecutionReportMsg:
                    new NolMsgHandler(this).AsyncExecReportHandler((ExecutionReportMsg)msg);
                    break;
                case FixmlMsg.FixmlMsgType.SessionStatusMsg:
                    new NolMsgHandler(this).AsyncSessionStatusHandler((TradingSessionStatusMsg)msg);
                    break;
                case FixmlMsg.FixmlMsgType.NewsMsg:
                    new NolMsgHandler(this).AsyncNewsMsgHandler((NewsMsg)msg);
                    break;
                case FixmlMsg.FixmlMsgType.MarketDataIncRefMsg:
                    lock (mdResults)
                    {
                        if (((MarketDataIncRefreshMsg)msg).RequestId != mdReqId)
                            new NolMsgHandler(this).AsyncMarketDataHandler((MarketDataIncRefreshMsg)msg);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region User login, logout


        public void Login()
        {
            if ( loggedIn == true )
                return;
            SendMessage.Send(MessagegType.LogD, "Login Start....");
         StartLogin:
            using (Socket socket = GetSyncSocket())
            {
                UserRequestMsg request = new UserRequestMsg
                {
                    Type = UserRequestType.Login,
                    Username = "BOS",
                    Password = "BOS"
                };
                request.Send(socket);
                try
                {
                    UserResponseMsg response = new UserResponseMsg(socket);
                    if (response.UsrStatus == UserStatus.LoggedIn ||
                        response.NolStatus == NOLStatus.Connecting)
                    {
                        loggedIn = true;
                    }
                    else if (response.UsrStatus != UserStatus.LoggedIn)
                    {
                        throw new FixmlErrorMsgException(response);
                    }
                }
                catch (BizMessageRejectException e)
                {
                    // całe to przechwytywanie wyjątków i powtórki możnaby pominąć, gdyby NOL nie blokował 
                    // numerku ReqID, jeśli jego poprzedni klient nie zrealizował prawidłowo logowania/wylogowania
                    if (e.Msg.RejectText == "Incorrect UserReqID")
                    {
                        if (request.Id < 100)
                        {
                            SendMessage.Send(MessagegType.LoginFail, "Try relogin");
                            goto StartLogin;
                        }
                        else
                        {
                            SendMessage.Send(MessagegType.LoginFail, "UserReqID limit reached!");
                            throw new FixmlException("UserReqID limit reached!");
                        }
                    }
                    else throw;
                }
                catch (Exception e)
                {
                    SendMessage.Send(MessagegType.LogE, "Login error: " + e.Message);
                    throw;
                }
            }
            if (loggedIn)
            {
                ThreadStart();
                SendMessage.Send(MessagegType.LoginOK, "Login Done");
            }
        }

        public void Logout()
        {
            Debug.WriteLine("\nLogout...");
            loggedIn = false;  // <- nawet, jeśli niżej będzie błąd (żeby się nie powtarzał w destruktorze)
            using (Socket socket = GetSyncSocket())
            {
                UserRequestMsg request = new UserRequestMsg
                {
                    Type = UserRequestType.Logout,
                    Username = "BOS"
                };
                request.Send(socket);
                UserResponseMsg response = new UserResponseMsg(socket);
                if (response.UsrStatus != UserStatus.LoggedOut)
                    throw new FixmlErrorMsgException(response);
            }
            SendMessage.Send(MessagegType.Logout, "Logout");
        }
        #endregion

        #region TradingSessionStatus subscription

        private bool statusOn = false;

        /// <summary>
        /// Aktywacja odbioru informacji o statusie sesji (komunikat "TrdgSesStat": 
        /// informuje o zmianie fazy sesji, równoważeniu subskrybowanych instrumentów itp.).
        /// Aby na te zmiany reagować, należy się podłączyć pod zdarzenie "SessionStatusMsgEvent".
        /// </summary>
        public void TradingSessionStatusStart()
        {
            if (statusOn) return;
            Debug.WriteLine("\nTradingSessionStatusStart...");
            using (Socket socket = GetSyncSocket())
            {
                TrdSesStatusRequestMsg request = new TrdSesStatusRequestMsg
                {
                    Type = SubscriptionRequestType.StartSubscription
                };
                request.Send(socket);
                TradingSessionStatusMsg response = new TradingSessionStatusMsg(socket);
                if (response.RequestId != request.Id)
                    throw new FixmlException("Unexpected TrdgSesStat ReqID.");
            }
            Debug.WriteLine("TradingSessionStatusStart OK\n");
            statusOn = true;
        }

        /// <summary>
        /// Deaktywacja odbioru informacji o statusie sesji.
        /// </summary>
        public void TradingSessionStatusStop()
        {
            if (!statusOn) return;
            Debug.WriteLine("\nTradingSessionStatusStop...");
            using (Socket socket = GetSyncSocket())
            {
                TrdSesStatusRequestMsg request = new TrdSesStatusRequestMsg
                {
                    Type = SubscriptionRequestType.CancelSubscription
                };
                request.Send(socket);
                TradingSessionStatusMsg response = new TradingSessionStatusMsg(socket);
                if (response.RequestId != request.Id)
                    throw new FixmlException("Unexpected TrdgSesStat ReqID.");
            }
            Debug.WriteLine("TradingSessionStatusStop OK\n");
            statusOn = false;
        }

        #endregion

        #region MarketData subscription

        private HashSet<FixmlInstrument> mdInstruments = new HashSet<FixmlInstrument>();
        //private Dictionary<FixmlInstrument, MDResults> mdResults = new Dictionary<FixmlInstrument, MDResults>();

        public void MarketDataStart()
        {
            if (mdReqId != null) MarketDataStop();
            lock (mdResults)
            {
                //mdResults.Clear();
                using (Socket socket = GetSyncSocket())
                {
                    MarketDataRequestMsg request = new MarketDataRequestMsg
                    {
                        Type = SubscriptionRequestType.StartSubscription,
                        MarketDepth = 1,
                        Instruments = mdInstruments.ToArray()
                    };
                    request.Send(socket);
                    MarketDataFullRefreshMsg response = new MarketDataFullRefreshMsg(socket);
                    if (response.RequestId != request.Id)
                        throw new FixmlException("Unexpected MktDataFull ReqID.");
                    mdReqId = request.Id;
                }
                SendMessage.Send(MessagegType.MarketDataStart, "MarketDataStart OK");
            }
        }

        public void MarketDataStop()
        {
            if (mdReqId == null) return;
            lock (mdResults)
            {
                Debug.WriteLine("\nMarketDataStop...");
                using (Socket socket = GetSyncSocket())
                {
                    MarketDataRequestMsg request = new MarketDataRequestMsg
                    {
                        Type = SubscriptionRequestType.CancelSubscription
                    };
                    request.Send(socket);
                    MarketDataFullRefreshMsg response = new MarketDataFullRefreshMsg(socket);
                    if (response.RequestId != request.Id)
                        throw new FixmlException("Unexpected MktDataFull ReqID.");
                }
                mdReqId = null;
                SendMessage.Send(MessagegType.MarketDataStart, "MarketDataStop OK");
            }
        }

        public void MarketDataSubscriptionAdd(params FixmlInstrument[] instruments)
        {
            MarketDataSubscriptionChange(mdInstruments.Union(instruments));
        }

        public void MarketDataSubscriptionRemove(params FixmlInstrument[] instruments)
        {
            MarketDataSubscriptionChange(mdInstruments.Except(instruments));
        }

        private void MarketDataSubscriptionChange(IEnumerable<FixmlInstrument> instr)
        {
            bool wasActive = (mdReqId != null);
            if (wasActive) MarketDataStop();
            mdInstruments = new HashSet<FixmlInstrument>(instr);
            if (wasActive) MarketDataStart();
        }

        #endregion

        #region Orders

        // Metoda IBosClient do składania nowego zlecenia.
        public string OrderCreate(OrderData data)
        {
            string clientId = null;
            using (Socket socket = GetSyncSocket())
            {
                NewOrderSingleMsg request = new NewOrderSingleMsg();
                clientId = request.ClientOrderId;  // automatycznie przydzielone kolejne Id
                request.Account = data.AccountNumber;
                request.CreateTime = data.MainData.CreateTime;
                request.Instrument = FixmlInstrument.Find(data.MainData.Instrument);
                request.Side = (data.MainData.Side == BosOrderSide.Buy) ? OrderSide.Buy : OrderSide.Sell;
                request.Type = Order_GetType(data.MainData);
                request.Price = data.MainData.PriceLimit;
                request.StopPrice = data.MainData.ActivationPrice;
                request.Quantity = data.MainData.Quantity;
                request.MinimumQuantity = data.MainData.MinimumQuantity;
                request.DisplayQuantity = data.MainData.VisibleQuantity;
                request.TimeInForce = Order_GetTimeInForce(data.MainData);
                request.ExpireDate = data.MainData.ExpirationDate;
                request.ExpireTime = data.MainData.ExpirationTime;
                request.Send(socket);
                ExecutionReportMsg response = new ExecutionReportMsg(socket);
            }
            return clientId;
        }

        // Metoda IBosClient do modyfikacji istniejącego zlecenia.
        public void OrderReplace(OrderData data)
        {
            using (Socket socket = GetSyncSocket())
            {
                OrderReplaceRequestMsg request = new OrderReplaceRequestMsg();
                request.Account = data.AccountNumber;
                request.BrokerOrderId2 = data.BrokerId;
                request.Instrument = FixmlInstrument.Find(data.MainData.Instrument);
                request.Side = (data.MainData.Side == BosOrderSide.Buy) ? OrderSide.Buy : OrderSide.Sell;
                request.Type = Order_GetType(data.MainData);
                request.Price = data.MainData.PriceLimit;
                request.StopPrice = data.MainData.ActivationPrice;
                request.Quantity = data.MainData.Quantity;
                request.MinimumQuantity = data.MainData.MinimumQuantity;
                request.DisplayQuantity = data.MainData.VisibleQuantity;
                request.TimeInForce = Order_GetTimeInForce(data.MainData);
                request.ExpireDate = data.MainData.ExpirationDate;
                request.ExpireTime = data.MainData.ExpirationTime;
                request.Send(socket);
                ExecutionReportMsg response = new ExecutionReportMsg(socket);
            }
        }

        // Metoda IBosClient do anulowania istniejącego zlecenia.
        public void OrderCancel(OrderData data)
        {
            using (Socket socket = GetSyncSocket())
            {
                OrderCancelRequestMsg request = new OrderCancelRequestMsg();
                request.Account = data.AccountNumber;
                request.BrokerOrderId2 = data.BrokerId;
                request.Instrument = FixmlInstrument.Find(data.MainData.Instrument);
                request.Side = (data.MainData.Side == BosOrderSide.Buy) ? OrderSide.Buy : OrderSide.Sell;
                request.Quantity = data.MainData.Quantity;
                request.Send(socket);
                ExecutionReportMsg response = new ExecutionReportMsg(socket);
            }
        }

        // funkcja pomocnicza przygotowująca typ zlecenia FIXML
        private static OrderType Order_GetType(OrderMainData data)
        {
            switch (data.PriceType)
            {
                case PriceType.Limit: return (data.ActivationPrice != null) ? OrderType.StopLimit : OrderType.Limit;
                case PriceType.PKC: return (data.ActivationPrice != null) ? OrderType.StopLoss : OrderType.PKC;
                default: return OrderType.PCR_PCRO;
            }
        }

        // funkcja pomocnicza przygotowująca parametr FIXML TimeInForce 
        private static OrdTimeInForce Order_GetTimeInForce(OrderMainData data)
        {
            if (data.ImmediateOrCancel) return OrdTimeInForce.WiA;
            if (data.MinimumQuantity == data.Quantity) return OrdTimeInForce.WuA;
            if (data.PriceType == PriceType.PCRO)
                return (DateTime.Now.Hour < 12) ? OrdTimeInForce.Opening : OrdTimeInForce.Closing;
            if (data.ExpirationDate != null) return OrdTimeInForce.Date;
            return OrdTimeInForce.Day;
        }

        #endregion
    }
}
