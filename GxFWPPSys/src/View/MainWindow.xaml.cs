using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GxFWPPSys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private IDisposable newSecondSubscription;
        private IDisposable mtOnlineSubscription;

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public string Clock     { get; private set; }
        public string MTStatus  { get; private set; }
        public string NolStatus { get; private set; }


        private readonly SolidColorBrush offlineClr = 
                    new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x9C, 0x7f));

        private readonly SolidColorBrush onlineClr =
            new SolidColorBrush(Color.FromArgb(0xff, 0x19, 0xF0, 0x67));

        private MainClass Main;


        public MainWindow()
        {
            InitializeComponent();

            SetViewContex();
            Main = new MainClass();

            SysTimer.Start();
            SetSubscription();

            Main.Start();
            
        }

        private void SetViewContex()
        {
            lbClock.SetBinding(ContentProperty, "Clock");
            UpdateClock();

            lbMtStatus.SetBinding(ContentProperty, "MTStatus");
            lbNolStatus.SetBinding(ContentProperty, "NolStatus");


            MTStatus = "MT4 - offline";
            NolStatus = "NOL - offline";

            DataContext = this;
        }

        private void SetSubscription()
        {
            newSecondSubscription = SysTimer.NewSecond.Subscribe(OnNewSecond);
            mtOnlineSubscription  = MTBridge.MtConnect.Subscribe(OnMtConnect);
        }

        private void UpdateClock()
        {
            Clock = DateTime.Now.ToString("HH:mm:ss");
            OnPropertyChanged("Clock");
        }

        private void OnMtConnect(bool isOnline)
        {
            if (isOnline)
            {
                MTStatus = "MT4 - online";
                lbMtStatus.Foreground = onlineClr;
            }
            else
            {
                lbMtStatus.Foreground = offlineClr;
                MTStatus = "MT4 - offline";
            }
            OnPropertyChanged("MTStatus");
        }

        private void OnNewSecond(DateTime time)
        {
            UpdateClock();
        }

    }
}