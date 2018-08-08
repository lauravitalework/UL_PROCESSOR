// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.MappingLenaComparer
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System.Collections.Generic;

namespace UL_PROCESSOR
{
  public class MappingLenaComparer : IComparer<MappingRow>
  {
    public int Compare(MappingRow x, MappingRow y)
    {
      int num = x.LenaId.CompareTo(y.LenaId);
      return num == 0 ? (y.day.CompareTo(x.Expiration) > 0 || y.day.CompareTo(x.Start) < 0 ? -1 : 0) : num;
    }
  }
}
