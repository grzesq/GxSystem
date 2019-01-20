using GxNolApi.NolClient;
namespace GxNolApi
{
    interface IGxNolApi
    {
        bool IsConnected { get; }
        INOLClient Connection { get; }

        void Connect();
        void Disconnect();
        BosAccounts GetAccounts();
        BosInstruments GetInstruments();

    }
}
