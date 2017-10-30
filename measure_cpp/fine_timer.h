//############################################################################
/*
        高精度タイマ

    © 2016 Theoride Technology (http://theolizer.com/) All Rights Reserved.

    MIT Licenseにより当プログラムの使用を許諾します。
    MIT Licenseのコピーをプロジェクト・ルートLICENSE.TXTファイルにおいてます。

*/
//############################################################################


#ifndef FINE_TIMER_H
#define FINE_TIMER_H

#include <chrono>

class FineTimer
{
    std::chrono::system_clock::time_point   mStart;
public:
    FineTimer() : mStart(std::chrono::system_clock::now())
    { }
    void restart()
    {
        mStart=std::chrono::system_clock::now();
    }
    std::chrono::milliseconds::rep mSec()
    {
        auto now=std::chrono::system_clock::now();
        auto ret=std::chrono::duration_cast<std::chrono::milliseconds>(now-mStart).count();
        mStart=now;
        return ret;
    }
    std::chrono::microseconds::rep uSec()
    {
        auto now=std::chrono::system_clock::now();
        auto ret=std::chrono::duration_cast<std::chrono::microseconds>(now-mStart).count();
        mStart=now;
        return ret;
    }
};

#endif  // FINE_TIMER_H
