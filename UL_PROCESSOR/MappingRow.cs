// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.MappingRow
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;

namespace UL_PROCESSOR
{
  public class MappingRow
  {
    public DateTime Start = new DateTime();
    public DateTime Expiration = new DateTime();
    public string BID = "";
    public string UbiID = "";
    public string LenaId = "";
    public string leftTag = "";
    public string rightTag = "";
    public string aid = "";
    public string sex = "";
    public string type = "";
    public List<DateTime> absences = new List<DateTime>();
    public DateTime day = new DateTime();

    public bool isAbsent(DateTime day)
    {
      bool flag = false;
      foreach (DateTime absence in this.absences)
      {
        if (DateTime.Compare(day, absence) == 0)
        {
          flag = true;
          break;
        }
      }
      return flag;
    }
  }
}
