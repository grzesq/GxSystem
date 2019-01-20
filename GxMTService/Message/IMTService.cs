
using System.ServiceModel;

namespace GxMTService
{

    public interface IMTQuote
    {
    }

    [ServiceContract]
    public interface IMTService
    {
        [OperationContract]
        string TestService();

        [OperationContract]
        bool Init();

        [OperationContract]
        bool Deinit();

        [OperationContract]
        bool NewQuote(double ask, double bid, double lastTr);

        [OperationContract]
        bool NewBar(double o,
                    double h,
                    double l,
                    double c,
                    double atr,
                    uint    time);

        [OperationContract]
        bool NewWeekBar(double o, double h, double l, double c, uint time);
    }
}