using GxMTService.DTO;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GxMTService
{
    public class MTHost
    {
        private static MTHost mtHost = new MTHost();
        private IMTService chanel = null;

        #region MT DLL Command

        public static bool Init()
        {
            mtHost.GetChanel();
            bool ok = mtHost.chanel != null;
            if (ok)
            {
                try
                {
                    ok = mtHost.chanel.Init();
                }
                catch
                {
                    ok = false;
                    mtHost.chanel = null;
                }
            }
            return ok;
        }

        public static bool Deinit()
        {
            bool ok = mtHost.chanel != null;
            if (ok)
            {
                try
                {
                    ok = mtHost.chanel.Deinit();
                }
                catch
                {
                    ok = false;
                    mtHost.chanel = null;
                }
            }
            return ok;
        }

        public static bool NewWeekBar(double o, double h,
                                      double l, double c, uint time)
        {
            bool ok = mtHost.chanel != null;
            if (ok)
            {
                try
                {
                    ok = mtHost.chanel.NewWeekBar(o, h, l, c, time);
                }
                catch
                {
                    ok = false;
                    mtHost.chanel = null;
                }
            }
            return ok;
        }

        public static bool NewQuote(double ask, double bid, double lastTr, uint time)
        {
            bool ok = mtHost.chanel != null;
            if (ok)
            {
                try
                {
                    ok = mtHost.chanel.NewQuote(ask, bid, lastTr, time);
                }
                catch
                {
                    ok = false;
                    mtHost.chanel = null;
                }
            }
            return ok;
        }

        public static bool NewBar(double o, double h,
                             double l, double c, double atr, uint time)
        {
            bool ok = mtHost.chanel != null;
            if (ok)
            {
                try
                {
                    ok = mtHost.chanel.NewBar(o, h, l, c, atr, time);
                }
                catch
                {
                    ok = false;
                    mtHost.chanel = null;
                }
            }
            return ok;
        }

        #endregion

        #region Host 
        private readonly string uri = "net.pipe://GxMTService";
        private ServiceHost host = null;

        public delegate bool InitHandler();
        public static event InitHandler OnInit;

        public delegate bool DeinitHandler();
        public static event  DeinitHandler OnDeinit;

        public delegate bool NewQuoteHandler(MTQuote quote);
        public static event  NewQuoteHandler OnNewQuote;

        public delegate bool NewWeekBarHandler(MTBar bar);
        public static event NewWeekBarHandler OnNewWeekBar;

        public delegate bool NewBarHandler(MTAtrBar bar);
        public static event NewBarHandler OnNewBar;


        public static bool StartHost()
        {
            return mtHost.RunHost();
        }

        private bool RunHost()
        {
            bool ok = false;
            //DataProvider.Instance.Status = MTStaus.Offline;
            Uri[] baseAddr = new Uri[1];
            baseAddr[0] = new Uri(uri);
            host = new ServiceHost(typeof(MTService), baseAddr);

            //MEX - Meta data exchange
            ServiceMetadataBehavior behavior =
                                host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (behavior == null)
            {
                behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);
            }
            host.AddServiceEndpoint(typeof(IMTService),
                                    new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), "");
            host.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), "mex");

            host.Open();
            ok = true;

            return ok;
        }


        public static bool MTInit()
        {
            if (OnInit != null)
            {
                return OnInit();
            }
            return false;
        }

        public static bool MTDeinit()
        {
            if (OnDeinit != null)
            {
                return OnDeinit();
            }
            return false;
        }

        public static bool MTNewQuote(MTQuote quote)
        {
            if (OnNewQuote != null)
            {
                return OnNewQuote(quote);
            }
            return false;
        }

        public static bool MTNewBar(double o, double h, double l, double c,
                           double atr, uint time)
        {
            if (OnNewBar != null)
            {
                return OnNewBar(new MTAtrBar(o, h, l, c, atr, time));
            }
            return false;
        }

        public static bool MTNewWeekBar(double o, double h, double l, double c, uint time)
        {
            if (OnNewWeekBar != null)
            {
                return OnNewWeekBar(new MTBar(o, h, l, c, time));
            }
            return false;
        }



        #endregion

        #region Client 

        private ChannelFactory<IMTService> factory = null;

        /*void ConnectionFaulted(object sender, EventArgs e)
        {
            Console.WriteLine("No I dupa");
        }*/

        public static IMTService ConnectToHost()
        {
            mtHost.GetChanel();
            return mtHost.chanel;
        }

        private void GetChanel()
        {
            factory =
                new ChannelFactory<IMTService>(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                                               new EndpointAddress("net.pipe://GxMTService"));
            chanel = factory.CreateChannel();
        }


        #endregion

    }
}
