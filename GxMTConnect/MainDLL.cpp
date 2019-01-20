#include "stdafx.h"
#include "MTConnector.h"
#include "Windows.h"

#define _DLLAPI extern "C" __declspec(dllexport)

_DLLAPI int _stdcall TestDll()
{
    return MTConnector::TestDLL();
}

_DLLAPI bool _stdcall Init()
{
    return MTConnector::Init();
}

_DLLAPI bool _stdcall Deinit()
{
    return MTConnector::Deinit();
}

_DLLAPI bool _stdcall NewQuote(double ask, double bid, double lastTr)
{
    return true;
}

_DLLAPI bool _stdcall NewBar(double o, double h,
    double l, double c, double atr, unsigned  int time)
{
    return true;
}

_DLLAPI bool _stdcall NewWeekBar(double o, double h, double l,
    double c, unsigned  int time)
{
    return MTConnector::SendWeekBar(o, h, l, c, time);
}