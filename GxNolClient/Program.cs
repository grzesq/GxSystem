using GxMTService;
using GxNolApi;
using GxNolApi.NolClient;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace GxNolClient 
{
    class Program: IDisposable
    {

        static IMTService s = null;

        static void Main(string[] args)
        {
            MTHost.StartHost();
            
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();

                if (keyinfo.Key == ConsoleKey.S)
                {
                    s = MTHost.ConnectToHost();
                }
                else if (keyinfo.Key == ConsoleKey.Q)
                {
                    try
                    {
                        Console.WriteLine(s.TestService());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }


            }
            while (keyinfo.Key != ConsoleKey.X);


        }



        private IDisposable messageSubscription;
        private void Start()
        {
            messageSubscription = SendMessage.Message.Subscribe(OnMessage);

            Console.WriteLine("Polaczenie do nol ");

            try
            {
                GxBossaNolAPI.ConnectToNOL();
            }
            catch (NolClientException e)
            {
                Console.WriteLine("Nol Sie wykrzeczyl " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("...." + e.Message);
            }


            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();

                if (keyinfo.Key == ConsoleKey.S)
                {
                    Statment();
                }

            }
            while (keyinfo.Key != ConsoleKey.X);
        }


        private void Statment()
        {
            foreach (BosAccount ac in GxBossaNolAPI.GetAccounts())
            {
                Console.WriteLine("\n\n");
                Console.WriteLine(ac.Number);
                Console.WriteLine("Zlecenia: {0}", ac.Orders.Count);
                int x = 1;
                foreach (BosOrder or in ac.Orders)
                {
                    Console.WriteLine("{0} Utworzone: {1} Status{2}",
                        x, or.CreateTime, or.StatusReport.Status.ToString());
                    x++;
                }
            }
        }

        private void OnMessage(IMessage message)
        {
            string typ = "";
            switch (message.MsgType)
            {
                case MessagegType.LogD:
                    typ = "Debug: ";
                    break;
                case MessagegType.LogE:
                    typ = "Error: ";
                    break;
                case MessagegType.InXml:
                    typ = "In Xml: ";
                    break;
                case MessagegType.LoginOK:
                    typ = "LoginOK ";
                    break;
                case MessagegType.LoginFail:
                    typ = "LoginOK ";
                    break;
                case MessagegType.AsncUserRespo:
                    typ = "AsyncUserRespo: ";
                    if (((NOLStatus)message.MsgObj) == NOLStatus.Online)
                    {
                        typ += "--------NOL ONLINE --------------- \n";
                    }
                    else
                    {
                        typ += "--------NOL ZLY STATUS --------------- \n";
                    }
                    break;
                case MessagegType.AsncNews:
                    typ += (string)message.MsgObj +"\n";
                    break;
                case MessagegType.AsncStatment:
                    typ = "Statment";
                    break;
            }

            string x = message.MsgTxt.ToString();

            if (!x.Contains("Heartbeat"))
                Console.WriteLine(typ + x);
        }



        public void Dispose()
        {
            
        }
    }

    }
