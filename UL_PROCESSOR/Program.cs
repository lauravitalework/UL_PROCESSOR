// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.UL_PROCESSOR_Program
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UL_PROCESSOR
{
  internal class UL_PROCESSOR_Program
  {
    public int chunkSize = 4;

    public void processClassroom(string version, string dir, bool doTenFiles, bool doSumDayFiles, bool doSumAllFiles, bool doAnglesFiles, string classroom, string lenaVersion, string szDates, bool fromIts)
    {
      List<DateTime> d = new List<DateTime>();
      List<DateTime> dateTimeList = new List<DateTime>();
      List<string> first = new List<string>();
      int num1 = 0;
      int num2 = 0;
      string str1 = szDates;
      char[] chArray = new char[1]{ ',' };
      foreach (string str2 in str1.Split(chArray))
      {
        ++num1;
        ++num2;
        if (num1 > this.chunkSize)
        {
          if (dateTimeList.Count > 0)
            d.Insert(0, dateTimeList[dateTimeList.Count - 1]);
          Console.WriteLine("PROCESSING LB" + (object) num2);
          UL_CLASS_PROCESSOR_Program processorProgram = new UL_CLASS_PROCESSOR_Program(new Config(dir, classroom), d, version + (object) num2 + "_", doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles, fromIts);
          processorProgram.configInfo.lenaVersion = lenaVersion;
          Console.WriteLine("PROCESSING LB1 1");
          processorProgram.process(false);
          dateTimeList = d;
          d = new List<DateTime>();
          num1 = 1;
          first = first.Concat<string>((IEnumerable<string>) processorProgram.fileNames).ToList<string>();
        }
        d.Add(Convert.ToDateTime(str2));
      }
      if (dateTimeList.Count > 0)
        d.Insert(0, dateTimeList[dateTimeList.Count - 1]);
      UL_CLASS_PROCESSOR_Program processorProgram1 = new UL_CLASS_PROCESSOR_Program(new Config(dir, classroom), d, version + (object) num2 + "_", doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles, fromIts);
      processorProgram1.configInfo.lenaVersion = lenaVersion;
      processorProgram1.process(true);
      List<string> list = first.Concat<string>((IEnumerable<string>) processorProgram1.fileNames).ToList<string>();
      if (!doSumAllFiles)
        return;
      UL_PROCESSOR_Program.MergeCsvs(list, list[list.Count - 1].Replace(".", "ALL."));
    }

    public void processClassroomOnsets(string version, string dir, bool doTenFiles, bool doSumDayFiles, bool doSumAllFiles, bool doAnglesFiles, string classroom, string lenaVersion, string szDates, bool fromIts)
    {
      int count = 0;
      string str1 = szDates;
      char[] chArray = new char[1]{ ',' };
      foreach (string str2 in str1.Split(chArray))
      {
        UL_CLASS_PROCESSOR_Program processorProgram = new UL_CLASS_PROCESSOR_Program(new Config(dir, classroom), new List<DateTime>()
        {
          Convert.ToDateTime(str2)
        });
        processorProgram.configInfo.lenaVersion = lenaVersion;
        Console.WriteLine("PROCESSING " + str2);
        processorProgram.getOnsets(version, count);
        ++count;
      }
    }

    private static void MergeCsvs(List<string> file_names, string destinationfilename)
    {
      StreamReader streamReader = new StreamReader(file_names[0]);
      StreamWriter streamWriter = new StreamWriter(destinationfilename);
      string end1 = streamReader.ReadToEnd();
      streamReader.Close();
      for (int index = 1; index < file_names.Count; ++index)
      {
        streamReader = new StreamReader(file_names[index]);
        if (index > 0)
          streamReader.ReadLine();
        string end2 = streamReader.ReadToEnd();
        end1 += end2;
      }
      streamReader.Close();
      streamReader.Dispose();
      streamWriter.Write(end1);
      streamWriter.Close();
      streamWriter.Dispose();
    }

    private static void Main(string[] args)
    {
      FileComparer fileComparer = new FileComparer();
      DateTime now = DateTime.Now;
      object[] objArray = new object[6];
      int index1 = 0;
      int num1;
      string str1;
      if (now.Month <= 10)
      {
        string str2 = "0";
        num1 = now.Month;
        string str3 = num1.ToString();
        str1 = str2 + str3;
      }
      else
        str1 = now.Month.ToString();
      objArray[index1] = (object) str1;
      int index2 = 1;
      string str4;
      if (now.Day <= 10)
      {
        string str2 = "0";
        num1 = now.Day;
        string str3 = num1.ToString();
        str4 = str2 + str3;
      }
      else
      {
        num1 = now.Day;
        str4 = num1.ToString();
      }
      objArray[index2] = (object) str4;
      objArray[2] = (object) now.Year;
      objArray[3] = (object) "_";
      objArray[4] = (object) new Random().Next();
      objArray[5] = (object) "_";
      string version = string.Concat(objArray);
      if (args.Length > 1)
      {
        string[] strArray1 = args[0].Split(' ');
        string dir = strArray1[0];
        bool doTenFiles = true;
        bool doSumDayFiles = true;
        bool doSumAllFiles = true;
        bool doAnglesFiles = true;
        bool fromIts = false;
        bool flag = false;
        for (int index3 = 1; index3 < strArray1.Length; ++index3)
        {
          string[] strArray2 = strArray1[index3].Split(':');
          if (strArray2.Length > 1)
          {
            string str2 = strArray2[0].Trim();
            if (!(str2 == "TEN"))
            {
              if (!(str2 == "SUMDAY"))
              {
                if (!(str2 == "SUMALL"))
                {
                  if (!(str2 == "ANGLES"))
                  {
                    if (!(str2 == "ONSETS"))
                    {
                      if (str2 == "ITS")
                      {
                        fromIts = strArray2[1].Trim().ToUpper() == "YES";
                        version = !fromIts ? "ADEX_" + version : "ITS_" + version;
                      }
                    }
                    else
                      flag = strArray2[1].Trim().ToUpper() == "YES";
                  }
                  else
                    doAnglesFiles = strArray2[1].Trim().ToUpper() == "YES";
                }
                else
                  doSumAllFiles = strArray2[1].Trim().ToUpper() == "YES";
              }
              else
                doSumDayFiles = strArray2[1].Trim().ToUpper() == "YES";
            }
            else
              doTenFiles = strArray2[1].Trim().ToUpper() == "YES";
          }
        }
        int num2 = 0;
        foreach (string str2 in args)
        {
          ++num2;
          if (num2 > 1)
          {
            string[] strArray2 = str2.Split(' ');
            UL_PROCESSOR_Program processorProgram1 = new UL_PROCESSOR_Program();
            UL_PROCESSOR_Program processorProgram2 = new UL_PROCESSOR_Program();
            if (!flag)
              processorProgram2.processClassroom(version, dir, doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles, strArray2[0].Trim(), strArray2[1].Trim(), strArray2[2].Trim(), fromIts);
            else
              processorProgram2.processClassroomOnsets(version, dir, doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles, strArray2[0].Trim(), strArray2[1].Trim(), strArray2[2].Trim(), fromIts);
          }
        }
      }
      Console.ReadLine();
    }
  }
}
