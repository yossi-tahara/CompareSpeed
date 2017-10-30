//############################################################################
/*
        C#ベンチマーク

    © 2016 Theoride Technology (http://theolizer.com/) All Rights Reserved.

    MIT Licenseにより当プログラムの使用を許諾します。
    MIT Licenseのコピーをプロジェクト・ルートLICENSE.TXTファイルにおいてます。

*/
//############################################################################

#define TOTAL_TIMER
#define FUNCTION_DIVISION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace measure_cs
{
    using TimeList =List<double>;

    class Program
    {
        // ***************************************************************************
        //      測定ツール
        // ***************************************************************************

        //----------------------------------------------------------------------------
        //      結果記録
        //----------------------------------------------------------------------------

        struct Result
        {
            public String       mTitle;
            public int          mSize;
            public TimeList     mTimeList;
            public Result(String iTitle, int iSize)
            {
                mTitle = iTitle;
                mSize = iSize;
                mTimeList = new TimeList();
            }
        };

        static List<Result>     sResultList = new List<Result>();
        static int              sNowResult;

        static void restart()
        {
            sNowResult=0;
        }

        //----------------------------------------------------------------------------
        //      時間測定と結果表示
        //----------------------------------------------------------------------------

        static int          sFactor;
        static Stopwatch    sTimer = new Stopwatch();

        static void start(int iFactor=1)
        {
            sFactor = iFactor;
            sTimer.Restart();
        }

        static void result(string iTitle, int iSize=0)
        {
            sTimer.Stop();
            var aTime = sTimer.Elapsed.TotalMilliseconds;
            string aTitle = iTitle;
            if (sFactor != 1)
            {
                aTitle += "(1/" + sFactor.ToString() + ")";
            }
            if (sFactor == 1)
            {
                Console.WriteLine("{0,55} : {1,11:N0}", aTitle, aTime*sFactor);
            }
            else
            {
                Console.WriteLine("{0,55} : {1,11:N0}({2,0:N0})", aTitle, aTime*sFactor, aTime);
            }

            // まとめへ記録
            if (sNowResult == sResultList.Count)
            {
                sResultList.Add(new Result(aTitle, iSize));
            }
            sResultList[sNowResult].mTimeList.Add(aTime*sFactor);
            ++sNowResult;
        }

        //----------------------------------------------------------------------------
        //      最終結果出力
        //----------------------------------------------------------------------------

        static void TotalResult(int iMeasureCount, double iTime)
        {
            Console.Error.WriteLine("-------- C# --------,{0},{1,0:F0},{2,0:F0}",
                iMeasureCount, iTime, iTime/iMeasureCount/1000);
            Console.Error.WriteLine("Title,Size,Average-3Sigma,Average,Average+3Sigma,3Sigma,,Results");
            foreach (var aResult in sResultList)
            {
                double  sum=0;
                double  sum2=0;
                foreach (var aTime in aResult.mTimeList)
                {
                    sum += aTime;
                    sum2 += aTime*aTime;
                }
                double average = sum/aResult.mTimeList.Count;
                double sigma = Math.Sqrt(sum2/aResult.mTimeList.Count - average*average);
                Console.Error.Write("\"{0}\",{1},{2,0:F0},{3,0:F0},{4,0:F0},{5,0:F0},",
                    aResult.mTitle,
                    aResult.mSize,
                    average-sigma*3,
                    average,
                    average+sigma*3,
                    sigma*3);
                foreach (var aTime in aResult.mTimeList)
                {
                    Console.Error.Write(",{0,0:F0}", aTime);
                }
                Console.Error.WriteLine();
            }
            Console.Error.WriteLine();
            Console.Error.WriteLine();
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

        static volatile int sVolatileInt;
        static          int sNonVolatileInt;

        // ***************************************************************************
        //      測定対象ルーチン群
        // ***************************************************************************

        //----------------------------------------------------------------------------
        //      site1再現テスト(managed/unsafe)
        //          続) 気づいたら、C# が C++ の速度を凌駕している！
        //----------------------------------------------------------------------------

        // managed
        static void site1_test1()
        {
            int w = 4321;
            int stride = (w + 3) & ~3;
            int h = 6789;
            var a = new byte[stride * h];
            for (int i=0; i < 100; ++i)
            {
                SpeedTest.test1(a, w, h, stride);
            }
        }

        // unsafe
        static void site1_test2()
        {
            int w = 4321;
            int stride = (w + 3) & ~3;
            int h = 6789;
            var a = new byte[stride * h];
            for (int i=0; i < 100; ++i)
            {
                SpeedTest.test2(a, w, h, stride);
            }
        }

        //----------------------------------------------------------------------------
        //      site1と同等な巨大配列
        //----------------------------------------------------------------------------

        // 巨大配列1(managed)
        static void setLargeArrayManaged1()
        {
            var aArray = new byte[kCountLai];
            int aCount = kCountLao/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                for (int i=0; i < kCountLai; ++i)
                {
                    aArray[i] = (byte)(i ^ j);
                }
            }
        }

        // 巨大配列2(managed)
        static void setLargeArrayManaged2()
        {
            var aArray = new byte[kCountLai];
            int aCount = kCountLao/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                setLargeArrayManaged2Body(aArray, j);
            }
        }
        static void setLargeArrayManaged2Body(byte[] iArray, int j)
        {
            for (int i=0; i < kCountLai; ++i)
            {
                iArray[i] = (byte)(i ^ j);
            }
        }

        // 巨大配列3(managed)
        static void setLargeArrayManaged3()
        {
            var aArray = new byte[kCountLai];
            int aCount = kCountLao/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                setLargeArrayManaged3Body(aArray, j);
            }
        }
        static void setLargeArrayManaged3Body(byte[] iArray, int j)
        {
            for (int i=0; i < kCountLai; i+=2)
            {
                iArray[i] = (byte)(i ^ j);
            }
        }

        // 巨大配列1(unsafe)
        unsafe static void setLargeArrayUnsafe1()
        {
            var aArray = new byte[kCountLai];
            fixed (byte* aArrayFixed = aArray)
            {
                int aCount = kCountLao/sFactor;
                for (int j=0; j < aCount; ++j)
                {
                    for (int i=0; i < kCountLai; ++i)
                    {
                        aArrayFixed[i] = (byte)(i ^ j);
                    }
                }
            }
        }

        // 巨大配列2(unsafe)
        unsafe static void setLargeArrayUnsafe2()
        {
            var aArray = new byte[kCountLai];
            fixed (byte* aArrayFixed = aArray)
            {
                byte* aArrayFixed2 = aArrayFixed;
                int aCount = kCountLao/sFactor;
                for (int j=0; j < aCount; ++j)
                {
                    for (int i=0; i < kCountLai; ++i)
                    {
                        aArrayFixed2[i] = (byte)(i ^ j);
                    }
                }
            }
        }

        //----------------------------------------------------------------------------
        //      単純処理
        //----------------------------------------------------------------------------

        // 単純初期設定(volatile)
        static void initVolatile()
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
        static void initNonVolatile()
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
        static void calculate1()
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
        static void calculate2()
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
        static int sumNormal(int x)
        {
            int ret=0;
            for (int i = 0; i < x; ++i) ret += i;
            return ret;
        }
        static void inlineExpansion()
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
        //      メモリ獲得と設定(managed)
        //----------------------------------------------------------------------------

        // 文字列操作
        static void modifyString()
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                String aString = "";
                for (int i=0; i < kCount0i; ++i)
                {
                    aString += "x";
                }
            }
        }

        // メモリ・サイズで振る
        static void getMemoryManaged(int iCount)
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                byte[] aArray = new byte[iCount];
                for (int i=0; i < iCount; ++i)
                {
                    aArray[i] = (byte)i;
                }
            }
        }

        //----------------------------------------------------------------------------
        //      メモリ獲得と設定(unsafe)
        //----------------------------------------------------------------------------

        // 可変長メモリ
        unsafe static void getMemoryUnsafe(int iCount)
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                byte[] aArray = new byte[iCount];
                fixed (byte* aArrayFixed = aArray)
                {
                    byte* aArrayFixed2 = aArrayFixed;
                    for (int i=0; i < iCount; ++i)
                    {
                        aArrayFixed2[i] = (byte)i;
                    }
                }
            }
        }

        //----------------------------------------------------------------------------
        //      クラス処理
        //----------------------------------------------------------------------------

        // テスト用のクラス
        class Complex
        {
            private double mReal;
            private double mImaginary;
            public Complex() { mReal = 1.0; mImaginary = 0.1; }
            public double Real
            {
                get { return mReal; }
                set { mReal = value; }
            }
            public double Imaginary
            {
                get { return mImaginary; }
                set { mImaginary = value; }
            }
        }

        // コンストラクト
        static void constuct()
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                for (int i=0; i < kCount0i; ++i)
                {
                    var temp = new Complex();
                    sVolatileInt = (int)temp.Real;
                }
            }
        }

        //----------------------------------------------------------------------------
        //      ジェネリック処理
        //----------------------------------------------------------------------------

        // テスト用の通常メソッド
        static int maxInt(int iLhs, int iRhs)
        {
            return (iLhs > iRhs)?iLhs:iRhs;
        }

        // テスト用のジェネリック・メソッド
        static Type max<Type>(Type iLhs, Type iRhs) where Type : IComparable
        {
            return (iLhs.CompareTo(iRhs) > 0)?iLhs:iRhs;
        }

        // 通常関数
        static void normalInt()
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
        static void genericsInt()
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
        static void genericsDouble()
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                for (int i=0; i < kCount0i; ++i)
                {
                    sVolatileInt = (int)max<double>(i, j);
                }
            }
        }

        // string型
        static void genericsString()
        {
            int aCount = kCount0o/sFactor;
            for (int j=0; j < aCount; ++j)
            {
                for (int i=0; i < kCount0i; ++i)
                {
                    sVolatileInt =
                        int.Parse(max<String>(i.ToString(), j.ToString()));
                }
            }
        }

        // ***************************************************************************
        //      メイン・ルーチン
        //          １つの関数が大きくなると処理速度が不意に変わってしまう。
        //          最適化が適切に機能しなくなっているような印象を受ける。
        // ***************************************************************************

        static void measure()
        {
            restart();

#if FUNCTION_DIVISION
            test1();
            test2();
            test3();
            test4();
        }

        static void test1()
        {
#endif
            //----------------------------------------------------------------------------
            //      site1再現テスト(managed/unsafe)
            //          続) 気づいたら、C# が C++ の速度を凌駕している！
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- reproduction test for site1 (managed/unsafe) ---");

            // managed
            start();
            site1_test1();
            result("set to large array[test1() of site1].");

            // unsafe
            start();
            site1_test2();
            result("set to large array[test2() of site1].");

            //----------------------------------------------------------------------------
            //      site1と同等な巨大配列
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- large array like site1 ---");

            // 巨大配列1(managed)
            start();
            setLargeArrayManaged1();
            result("set to large array by managed-1.");

            // 巨大配列2(managed)
            start();
            setLargeArrayManaged2();
            result("set to large array by managed-2.");

            // 巨大配列3(managed)
            start();
            setLargeArrayManaged3();
            result("set to large array by managed-3.");

            // 巨大配列1(unsafe)
            start();
            setLargeArrayUnsafe1();
            result("set to large array by unsafe-1.");

            // 巨大配列2(unsafe)
            start();
            setLargeArrayUnsafe2();
            result("set to large array by unsafe-2.");
#if FUNCTION_DIVISION
        }

        static void test2()
        {
#endif
            //----------------------------------------------------------------------------
            //      単純処理
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- Simple process ---");

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
            start(10);
            inlineExpansion();
            result("inline expansion.");
#if FUNCTION_DIVISION
        }

        static void test3()
        {
#endif
            //----------------------------------------------------------------------------
            //      メモリ獲得と設定(managed)
            //          外側のループは全てkCount0o
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- get memory & setup(managed) ---");

            // 文字列操作
            start(50);
            modifyString();
            result("modify string.");

            // メモリ・サイズで振る
            for (int aCount=10; aCount <= 10000000; aCount *= 10)
            {
                String  aTitle=String.Format("get {0}bytes memory & setup.", aCount);
                start((1000 <= aCount) ?aCount/100:1);
                getMemoryManaged(aCount);
                result(aTitle, aCount);
            }

            //----------------------------------------------------------------------------
            //      メモリ獲得と設定(unsafe)
            //          外側のループは全てkCount0o
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- get memory & setup(unsafe) ---");

            // メモリ・サイズで振る
            for (int aCount=10; aCount <= 10000000; aCount *= 10)
            {
                String  aTitle=String.Format("get {0}bytes memory & setup.", aCount);
                start((1000 <= aCount) ?aCount/100:1);
                getMemoryUnsafe(aCount);
                result(aTitle, aCount);
            }
#if FUNCTION_DIVISION
        }

        static void test4()
        {
#endif
            //----------------------------------------------------------------------------
            //      クラス処理
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- class ---");

            // コンストラクト
            start(10);
            constuct();
            result("construct & read member.");

            //----------------------------------------------------------------------------
            //      ジェネリック処理
            //----------------------------------------------------------------------------

            Console.WriteLine("\n--- generics ---");

            // 通常関数
            start();
            normalInt();
            result("normal(int).");

            // int型
            start(10);
            genericsInt();
            result("generics(int).");

            // double型
            start(10);
            genericsDouble();
            result("generics(double).");

            // string型
            start(1000);
            genericsString();
            result("generics(string).");
        }

// ***************************************************************************
//      メイン・ルーチン
// ***************************************************************************

        static void Main(string[] args)
        {
            int aMeasureCount=1;
            if (0 < args.Length)
            {
                aMeasureCount=int.Parse(args[0]);
            }

            Console.WriteLine("-------- C# --------");
            Console.WriteLine("aMeasureCount=" + aMeasureCount);

            //----------------------------------------------------------------------------
            //      測定
            //----------------------------------------------------------------------------

            Stopwatch   aTimer = new Stopwatch();
#if TOTAL_TIMER
            aTimer.Start();
#endif
            for (int i=0; i < aMeasureCount; ++i)
            {
                Console.WriteLine("++++++++ {0} ++++++++", i);
                measure();
            }
#if TOTAL_TIMER
            aTimer.Stop();
#endif

            //----------------------------------------------------------------------------
            //      最終結果出力
            //----------------------------------------------------------------------------

            TotalResult(aMeasureCount, aTimer.Elapsed.TotalMilliseconds);
        }
    }
}
