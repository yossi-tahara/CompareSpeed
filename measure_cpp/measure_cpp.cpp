//############################################################################
/*
        C++ベンチマーク

    © 2016 Theoride Technology (http://theolizer.com/) All Rights Reserved.

    MIT Licenseにより当プログラムの使用を許諾します。
    MIT Licenseのコピーをプロジェクト・ルートLICENSE.TXTファイルにおいてます。

*/
//############################################################################

#define _SCL_SECURE_NO_WARNINGS

#include <iostream>
#include <iomanip>
#include <algorithm>
#include <random>
#include <memory>
#include <string>
#include <sstream>
#include <list>
#include <cmath>
#include "fine_timer.h"

// ***************************************************************************
//      評価用ツール
// ***************************************************************************

//----------------------------------------------------------------------------
//      時間測定と結果表示
//----------------------------------------------------------------------------

//      ---<<< ３桁区切り >>>---

std::string separate(double iNumber, int iDigitsAfter=0)
{
    std::stringstream ss;
    ss << std::setprecision(iDigitsAfter) << std::fixed << iNumber;
    std::string ret = ss.str();
    auto pos = ret.find('.');
    if (pos == std::string::npos)
    {
        pos = ret.size();
    }
    int digit=0;
    for (int i=static_cast<int>(pos); 1 < i; --i)
    {
        ++digit;
        if ((digit % 3) == 0)
        {
            ret.insert(i-1, ",");
        }
    }
    return ret;
}

//----------------------------------------------------------------------------
//      結果記録
//----------------------------------------------------------------------------

typedef std::list<double>   TimeList;

struct Result
{
    std::string     mTitle;
    int             mSize;
    TimeList        mTimeList;
    Result(std::string iTitle, int iSize) : mTitle(iTitle), mSize(iSize) { }
};

std::list<Result>           gResultList;
std::list<Result>::iterator gNowResult;

void restart()
{
    gNowResult=gResultList.begin();
}

//----------------------------------------------------------------------------
//      時間測定
//----------------------------------------------------------------------------

int         sFactor;
FineTimer   sTimer;

void start(int iFactor=1)
{
    sFactor = iFactor;
    sTimer.restart();
}

void result(char const* iTitle, int iSize=0)
{
    double aTime = static_cast<double>(sTimer.uSec())/1000.0;
    std::string aTitle(iTitle);
    if (sFactor != 1)
    {
        aTitle.append("(1/");
        aTitle.append(std::to_string(sFactor));
        aTitle.append(")");
    }
    std::cout << std::setw(55) << aTitle << " : "
              << std::setw(11) << separate(aTime*sFactor);
    if (sFactor != 1)
    {
        std::cout << "(" << separate(aTime) << ")";
    }
    std::cout << std::endl;

    // まとめへ記録
    if (gNowResult == gResultList.end())
    {
        gResultList.emplace_back(aTitle, iSize);
        gNowResult = gResultList.end();
        --gNowResult;
    }
    gNowResult->mTimeList.push_back(aTime*sFactor);
    ++gNowResult;
}

//----------------------------------------------------------------------------
//      最終結果出力
//----------------------------------------------------------------------------

static void TotalResult(int iMeasureCount, double iTime)
{
    std::cerr << "-------- C++ --------,"
        << iMeasureCount << ","
        << std::setprecision(0) << std::fixed << iTime << ","
        << std::setprecision(0) << std::fixed << iTime/iMeasureCount/1000 << "\n";
    std::cerr << "Title,Size,Average-3Sigma,Average,Average+3Sigma,3Sigma,,Results\n";
    for (auto& aResult : gResultList)
    {
        double  sum=0;
        double  sum2=0;
        for (auto aTime : aResult.mTimeList)
        {
            sum += aTime;
            sum2 += aTime*aTime;
        }
        double average = sum/aResult.mTimeList.size();
        double sigma = sqrt(sum2/aResult.mTimeList.size() - average*average);
        std::cerr << "\"" << aResult.mTitle << "\","
                  << aResult.mSize << ", "
                  << std::setprecision(0) << std::fixed << average-sigma*3 << ","
                  << std::setprecision(0) << std::fixed << average << ","
                  << std::setprecision(0) << std::fixed << average+sigma*3 << ","
                  << std::setprecision(0) << std::fixed << sigma*3 << ",";
        for (auto aTime : aResult.mTimeList)
        {
            std::cerr << "," << std::setprecision(0) << std::fixed << aTime;
        }
        std::cerr << "\n";
    }
    std::cerr << "\n\n";
}

//----------------------------------------------------------------------------
//      測定ループ回数
//----------------------------------------------------------------------------

const int kCount0i = 100;           // 内側のループ
const int kCount0o = 10000000;      // 外側のループ

// 巨大配列用
const int kCountLai= 4321*6789;     // 内側のループ
const int kCountLao= 100;           // 外側のループ

//----------------------------------------------------------------------------
//      測定領域
//----------------------------------------------------------------------------

volatile int sVolatileInt;
         int sNonVolatileInt;

// ***************************************************************************
//      測定対象ルーチン群
// ***************************************************************************

typedef unsigned char byte;

//----------------------------------------------------------------------------
//      site1再現テスト
//----------------------------------------------------------------------------

// site1 : 続) 気づいたら、C# が C++ の速度を凌駕している！
void test2(byte* a, int w, int h, int stride);

void site1_test2()
{
    int w = 4321;
    int stride = (w + 3) & ~3;
    int h = 6789;
    std::unique_ptr<byte[]> a(new byte[stride * h]);
    for (unsigned i=0; i < 100; ++i)
    {
        test2(a.get(), w, h, stride);
    }
}

//----------------------------------------------------------------------------
//      site1と同等な巨大配列
//----------------------------------------------------------------------------

// 巨大配列
void setLargeArray()
{
    std::unique_ptr<byte[]> aArray(new byte[kCountLai]);
    int aCount = kCountLao/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCountLai; ++i)
        {
            aArray[i] = static_cast<byte>(i ^ j);
        }
    }
}

//----------------------------------------------------------------------------
//      単純処理
//----------------------------------------------------------------------------

// 単純初期設定(volatile)
void initVolatile()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = i;
        }
    }
}

// 単純初期設定(non-volatile)
void initNonVolatile()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sNonVolatileInt = i;
        }
    }
}

// 計算処理1
void calculate1()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = i ^ j;
        }
    }
}

// 計算処理2
void calculate2()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = i % 10;
        }
    }
}

// インライン展開
int sumNormal(int x)
{
    int ret=0;
    for (int i = 0; i < x; ++i) ret += i;
    return ret;
}
void inlineExpansion()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = sumNormal(10);
        }
    }
}

//----------------------------------------------------------------------------
//      メモリ獲得と設定
//----------------------------------------------------------------------------

// 文字列操作
void modifyString()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        std::string aString = "";
        for (int i=0; i < kCount0i; ++i)
        {
            aString += "x";
        }
    }
}

// メモリ・サイズで振る
void getMemory(int iCount)
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        std::unique_ptr<byte[]> aArray(new byte[iCount]);
        for (int i=0; i < iCount; ++i)
        {
            aArray[i] = static_cast<byte>(i);
        }
    }
}

//----------------------------------------------------------------------------
//      クラス処理
//----------------------------------------------------------------------------

// テスト用のクラス
class Complex
{
    double mReal;
    double mImaginary;
public:
    Complex() : mReal(1.0), mImaginary(0.1) { }

    double getReal() { return mReal; }
    void   setReal(double value) { mReal = value; }

    double getImaginary() { return mImaginary; }
    void   setImaginary(double value) { mImaginary = value; }
};

// コンストラクト(stack)
void constuctStack()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            Complex temp;
            sVolatileInt = static_cast<int>(temp.getReal());
        }
    }
}

// コンストラクト(heap)
void constuctHeap()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            std::unique_ptr<Complex> temp(new Complex);
            sVolatileInt = static_cast<int>(temp->getReal());
        }
    }
}

//----------------------------------------------------------------------------
//      テンプレート処理
//----------------------------------------------------------------------------

// テスト用の通常関数
int maxInt(int iLhs, int iRhs)
{
    return (iLhs > iRhs)?iLhs:iRhs;
}

// テスト用の関数テンプレート
template<typename tType>
tType const& max(tType const& iLhs, tType const& iRhs)
{
    return (iLhs > iRhs)?iLhs:iRhs;
}

// 通常関数
void normalInt()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = maxInt(i, j);
        }
    }
}

// int型
void genericsInt()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = max<int>(i, j);
        }
    }
}

// double型
void genericsDouble()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt = static_cast<int>(max<double>(i, j));
        }
    }
}

// string型
void genericsString()
{
    int aCount = kCount0o/sFactor;
    for (int j=0; j < aCount; ++j)
    {
        for (int i=0; i < kCount0i; ++i)
        {
            sVolatileInt =
                std::atoi(::max<std::string>(std::to_string(i), std::to_string(j)).c_str());
        }
    }
}

// ***************************************************************************
//      メイン・ルーチン
// ***************************************************************************

void measure()
{
    restart();

    //----------------------------------------------------------------------------
    //      site1再現テスト
    //----------------------------------------------------------------------------

    std::cout << "\n--- reproduction test for site1 ---\n";

    // site1 : 続) 気づいたら、C# が C++ の速度を凌駕している！
    start();
    site1_test2();
    result("set to large array[test2() of site1].");

    //----------------------------------------------------------------------------
    //      site1と同等な巨大配列
    //----------------------------------------------------------------------------

    std::cout << "\n--- large array like site1 ---\n";

    // 巨大配列1
    start();
    setLargeArray();
    result("set to large array.");

    //----------------------------------------------------------------------------
    //      単純処理
    //----------------------------------------------------------------------------

    std::cout << "\n--- Simple process ---\n";

    // 単純初期設定
    start();
    initVolatile();
    result("initialize volatile memory.");

    // 単純初期設定(non-volatile)
    start();
    initNonVolatile();
    result("initialize non-volatile memory.");

    // 計算処理1
    start();
    calculate1();
    result("calculation(i^j).");

    // 計算処理2
    start(10);
    calculate2();
    result("calculation(i%10).");

    // インライン展開処理
    start();
    inlineExpansion();
    result("inline expansion.");

    //----------------------------------------------------------------------------
    //      メモリ獲得と設定
    //          外側のループは全てkCount0o
    //----------------------------------------------------------------------------

    std::cout << "\n--- get memory ---\n";

    // 文字列操作
    start(50);
    modifyString();
    result("modify string.");

    // メモリ・サイズで振る
    for (int aCount=10; aCount <= 10000000; aCount *= 10)
    {
        std::string aTitle="get "+std::to_string(aCount) + "bytes memory & setup.";
        char const* aTitle2=aTitle.c_str();
        start((1000 <= aCount) ?aCount/100:1);
        getMemory(aCount);
        result(aTitle2, aCount);
    }

    //----------------------------------------------------------------------------
    //      クラス処理
    //----------------------------------------------------------------------------

    std::cout << "\n--- class ---\n";

    // コンストラクト(stack)
    start();
    constuctStack();
    result("construct on stack & read member.");

    // コンストラクト(heap)
    start(100);
    constuctHeap();
    result("construct on heap & read member.");

    //----------------------------------------------------------------------------
    //      テンプレート処理
    //----------------------------------------------------------------------------

    std::cout << "\n--- template ---\n";

    // 通常関数
    start();
    normalInt();
    result("normal(int).");

    // int型
    start();
    genericsInt();
    result("template(int).");

    // double型
    start();
    genericsDouble();
    result("template(double).");

    // string型
    start(100);
    genericsString();
    result("template(string).");
}

// ***************************************************************************
//      メイン・ルーチン
// ***************************************************************************

int main(int argc, char* argv[])
{
    int aMeasureCount = 1;
    if (1 < argc)
    {
        aMeasureCount = atoi(argv[1]);
    }

    std::cout << "-------- C++ --------\n";
    std::cout << "aMeasureCount=" << aMeasureCount << "\n";

    //----------------------------------------------------------------------------
    //      測定
    //----------------------------------------------------------------------------

    FineTimer   aTimer;
    for (int i=0; i < aMeasureCount; ++i)
    {
        std::cout << "++++++++ " << i << " ++++++++\n";

        measure();
    }
    double aTime = static_cast<double>(aTimer.uSec())/1000.0;

    //----------------------------------------------------------------------------
    //      最終結果出力
    //----------------------------------------------------------------------------

    TotalResult(aMeasureCount, aTime);
}
