#pragma once

using namespace System;

ref class MTConnector
{
private:

public:
    static int  TestDLL();
    static bool Init();
    static bool Deinit();

    static bool SendWeekBar(double o,
        double h,
        double l,
        double c,
        unsigned int time);

    static bool NewQuote(double ask,
        double bid,
        double lastTr,
        unsigned int time);

    static bool NewBar(double o,
        double h,
        double l,
        double c,
        double atr,
        unsigned int time);
};
