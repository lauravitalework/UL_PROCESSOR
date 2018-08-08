// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.DateTimeComparer
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System.Collections.Generic;

namespace UL_PROCESSOR
{
  public class DateTimeComparer : IComparer<PersonInfo>
  {
    public int Compare(PersonInfo x, PersonInfo y)
    {
      int num = x.dt.CompareTo(y.dt);
      if (num != 0)
        return num;
      if (x.dt.Millisecond == y.dt.Millisecond)
        return 0;
      return x.dt.Millisecond < y.dt.Millisecond ? -1 : 1;
    }
  }
}
