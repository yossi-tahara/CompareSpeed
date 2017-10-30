//############################################################################
/*
        このファイルは以下のページからの引用です。
        http://espresso3389.hatenablog.com/entry/2016/05/04/122936
        従って、このファイルの著作権は上記ページの著者が保持しています。
        また、再現テストのため、
            1)test2関数のstaticをコメントアウトしました。
            2)byteのtypedefとtest2関数の定義以外を#if 0～#endifで囲いました。
    文責：田原良則 Theoride Technology (http://theolizer.com/)
*/
//############################################################################

#if 0
// Compile: cl /MD /Ox /EHsc speedtest.cpp
#include <stdio.h>
#include <windows.h>
#include <functional>
#endif
typedef unsigned char byte;

/*static*/ void test2(byte* a, int w, int h, int stride)
{
  auto p0 = a;
  for (int y = 0; y < h; y++)
  {
    auto p = p0 + y * stride;
    for (int x = 0; x < w; x++)
    {
      p[x] = (byte)(x ^ y);
    }
  }
}

#if 0
void time(std::function<void()> action, int count = 100)
{
  auto start = GetTickCount();
  for (int i = 0; i < count; i++)
    action();
  printf("%u\n", GetTickCount() - start);
}

int main()
{
  int w = 4321;
  int stride = (w + 3) & ~3;
  int h = 6789;
  auto a = new byte[stride * h];

  time([=]() { test2(a, w, h, stride); });

  delete[] a;
}
#endif
