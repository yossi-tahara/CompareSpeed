//############################################################################
/*
        このファイルは以下のページからの引用です。
        http://espresso3389.hatenablog.com/entry/2016/05/04/122936
        従って、このファイルの著作権は上記ページの著者が保持しています。
        また、再現テストのため、test1(), test2()関数をpublicへ変更しました。
    文責：田原良則 Theoride Technology (http://theolizer.com/)
*/
//############################################################################

// Compile: csc /o /unsafe speedtest.cs
using System;
using System.Diagnostics;

class SpeedTest
{
  public static void test1(byte[] a, int w, int h, int stride)
  {
    for (int y = 0; y < h; y++)
    {
      int offset = y * stride;
      for (int x = 0; x < w; x++)
      {
        a[x + offset] = (byte)(x ^ y);
      }
    }
  }
  
  public static unsafe void test2(byte[] a, int w, int h, int stride)
  {
    fixed (byte* p0 = a)
    {
      for (int y = 0; y < h; y++)
      {
        byte* p = p0 + y * stride;
        for (int x = 0; x < w; x++)
        {
          p[x] = (byte)(x ^ y);
        }
      }
    }
  }

  static void time(Action action, int count = 100)
  {
    var tw = new Stopwatch();
    tw.Start();
    for (int i = 0; i < count; i++)
      action();
    tw.Stop();
    Console.WriteLine(tw.ElapsedMilliseconds);
  }

  static void Main(string[] args)
  {
    int w = 4321;
    int h = 6789;
    int stride = (w + 3) & ~3;
    var a = new byte[stride * h];

    time(() => test1(a, w, h, stride));
    time(() => test2(a, w, h, stride));
  }
}
