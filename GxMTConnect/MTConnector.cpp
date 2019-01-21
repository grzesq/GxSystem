#include "stdafx.h"
#include "MTConnector.h"

int MTConnector::TestDLL()
{
    int x = -23;
    x = Math::Abs(x);
    return x;
}

bool MTConnector::Init()
{
    return  GxMTService::MTHost::Init();
}

bool MTConnector::Deinit()
{
    return  GxMTService::MTHost::Deinit();
}


bool MTConnector::SendWeekBar(double o, double h, double l, 
                              double c, unsigned int time)
{
    return GxMTService::MTHost::NewWeekBar(o, h, l, c, time);
    
}

bool MTConnector::NewQuote(double ask, double bid, double lastTr,
                           unsigned int time)
{
    return GxMTService::MTHost::NewQuote(ask, bid, lastTr, time);
}

bool MTConnector::NewBar(double o, double h, double l, double c, 
                         double atr, unsigned int time)
{
    return GxMTService::MTHost::NewBar(o, h, l, c, atr, time);
}