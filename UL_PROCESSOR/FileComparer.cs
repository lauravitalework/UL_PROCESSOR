// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.FileComparer
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;

namespace UL_PROCESSOR
{
  internal class FileComparer
  {
    public bool compareFiles(int[] ids, string f1, string f2, bool stop, int stopCol, bool excludeNA)
    {
      bool flag = true;
      Dictionary<string, string> file1;
      Dictionary<string, string> file2;
      if (stop)
      {
        file1 = this.getFile(ids, f1, stopCol);
        file2 = this.getFile(ids, f2, stopCol);
      }
      else
      {
        file1 = this.getFile(ids, f1);
        file2 = this.getFile(ids, f2);
      }
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      foreach (string key in file1.Keys)
      {
        string l1 = file1[key];
        if (file2.ContainsKey(key))
        {
          string l2 = file2[key];
          if (!this.compareLines(l1, l2, excludeNA))
          {
            dictionary.Add(key, l1 + "|" + l2);
            Console.WriteLine("LINES DIFFER: " + key + " (" + l1 + "|" + l2 + ")");
          }
        }
        else
        {
          dictionary.Add(key, l1 + "|");
          Console.WriteLine("LINE NOT FOUND IN F2: " + key + " (" + l1 + ")");
        }
      }
      return flag;
    }

    public bool compareLines(string l1, string l2, bool excludeNA)
    {
      bool flag = false;
      string[] strArray1 = l1.Split(',');
      string[] strArray2 = l1.Split(',');
      int index = 0;
      foreach (string s1 in strArray1)
      {
        try
        {
          double result1 = 0.0;
          string s2 = strArray2[index];
          if (s1.Trim() == "NA" || s2.Trim() == "NA")
            flag = true;
          else if (double.TryParse(s1, out result1))
          {
            double result2 = 0.0;
            if (double.TryParse(s2, out result2))
              flag = Math.Round(result1, 0) == Math.Round(result2, 0);
          }
        }
        catch (Exception ex)
        {
        }
        ++index;
      }
      return flag;
    }

    public Dictionary<string, string> getFile(int[] ids, string f1, int stopCol)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (StreamReader streamReader = new StreamReader(f1))
      {
        streamReader.ReadLine();
        int id = ids[ids.Length - 1];
        while (!streamReader.EndOfStream)
        {
          try
          {
            string str1 = streamReader.ReadLine().Trim();
            string str2 = streamReader.ReadLine().Trim();
            string[] strArray = str1.Split(',');
            if (strArray.Length >= id)
            {
              string key = "";
              for (int index = 0; index < ids.Length; ++index)
                key = key + strArray[index].Trim() + "|";
              for (int index = 0; index < strArray.Length && index < stopCol; ++index)
                str2 = str2 + strArray[index].Trim() + ",";
              dictionary.Add(key, str2);
            }
          }
          catch (Exception ex)
          {
          }
        }
        return dictionary;
      }
    }

    public bool compareLines(string l1, string l2)
    {
      bool flag = false;
      string[] strArray1 = l1.Split(',');
      string[] strArray2 = l1.Split(',');
      int index = 0;
      foreach (string s in strArray1)
      {
        try
        {
          double result1 = 0.0;
          if (double.TryParse(s, out result1))
          {
            double result2 = 0.0;
            if (double.TryParse(strArray2[index], out result2))
              flag = Math.Round(result1, 0) == Math.Round(result2, 0);
          }
        }
        catch (Exception ex)
        {
        }
        ++index;
      }
      return flag;
    }

    public Dictionary<string, string> getFile(int[] ids, string f1)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (StreamReader streamReader = new StreamReader(f1))
      {
        streamReader.ReadLine();
        int id = ids[ids.Length - 1];
        while (!streamReader.EndOfStream)
        {
          try
          {
            string str = streamReader.ReadLine().Trim();
            string[] strArray = str.Split(',');
            if (strArray.Length >= id)
            {
              string key = "";
              for (int index = 0; index < ids.Length; ++index)
                key = key + strArray[index].Trim() + "|";
              dictionary.Add(key, str);
            }
          }
          catch (Exception ex)
          {
          }
        }
        return dictionary;
      }
    }
  }
}
