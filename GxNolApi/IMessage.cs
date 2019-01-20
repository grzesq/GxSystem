using System;

namespace GxNolApi
{
    public enum MessagegType
    {
        LogI,
        LogD,
        LogW,
        LogE,
        InXml,
        OutXml,
        LoginOK,
        LoginFail,
        Logout,
        MarketDataStart,
        MarketDataStop,
        AsncUserRespo,
        AsncStatment,
        AsncSessionStatus,
        AsncMDUpdate,
        AsncNews
    }

    public interface IMessage
    {
        MessagegType MsgType { get; }
        String MsgTxt { get; }
        object MsgObj { get; }
    }
}
