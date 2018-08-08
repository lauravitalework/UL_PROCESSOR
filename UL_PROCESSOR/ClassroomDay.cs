// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.ClassroomDay
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace UL_PROCESSOR
{
  internal class ClassroomDay
  {
    public static double minDistance = 2.25;
    public Interactions ints = new Interactions();
    public bool doAll10OfSecs = false;
    public bool startFromLena = true;
    public bool noLena = false;
    public Dictionary<string, double> pairClose = new Dictionary<string, double>();
    public Dictionary<string, double> pairCloseOrientation = new Dictionary<string, double>();
    public Dictionary<string, double> pairTime = new Dictionary<string, double>();
    public Dictionary<string, double> pairCry = new Dictionary<string, double>();
    public Dictionary<string, Tuple<double, double, double, double>> pairStatsSeparated = new Dictionary<string, Tuple<double, double, double, double>>();
    public Dictionary<string, PairInfo> pairStatsSep = new Dictionary<string, PairInfo>();
    public Dictionary<string, double> individualTime = new Dictionary<string, double>();
    public Dictionary<string, double> pairStats = new Dictionary<string, double>();
    public Dictionary<string, PersonInfo> personTotalCounts = new Dictionary<string, PersonInfo>();
    public Dictionary<string, PersonInfo> personTotalCountsWUbi = new Dictionary<string, PersonInfo>();
    public Dictionary<DateTime, Dictionary<string, PersonInfo>> activities = new Dictionary<DateTime, Dictionary<string, PersonInfo>>();
    public Dictionary<string, List<Tuple<DateTime, DateTime>>> personUbiTimes = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();
    public List<DateTime> maxTimes = new List<DateTime>();
    public List<Tuple<DateTime, string>> maxPersonTimes = new List<Tuple<DateTime, string>>();
    public Dictionary<string, DateTime> startLenaTimes = new Dictionary<string, DateTime>();
    private Dictionary<string, bool> tagTest = new Dictionary<string, bool>();
    private List<string> tagMissing = new List<string>();
    private Dictionary<string, string> tagL = new Dictionary<string, string>();
    private Dictionary<string, string> tagR = new Dictionary<string, string>();
    private bool writeChaomingFile = false;
    private TextWriter swc = (TextWriter) null;
    public bool justProx = false;
    public Dictionary<DateTime, Dictionary<string, double>> adjustTimes = new Dictionary<DateTime, Dictionary<string, double>>();
    public Tuple<string, double> maxVD = new Tuple<string, double>("", 0.0);
    public Tuple<string, double> maxVD2 = new Tuple<string, double>("", 0.0);
    public Tuple<string, double> maxVD3 = new Tuple<string, double>("", 0.0);
    public Dictionary<string, List<cryInfoTest>> childCries = new Dictionary<string, List<cryInfoTest>>();
    public DateTime startTime;
    public DateTime endTime;
    public Config cf;
    public DateTime day;

    public ClassroomDay(DateTime d, Config c)
    {
      this.cf = c;
      this.day = d;
    }

    public DateTime getTrunkTime()
    {
      DateTime dateTime = new DateTime();
      this.maxTimes = this.maxTimes.OrderBy<DateTime, TimeSpan>((Func<DateTime, TimeSpan>) (x => x.TimeOfDay)).ToList<DateTime>();
      if (this.maxTimes.Count > 0)
      {
        dateTime = this.maxTimes[this.maxTimes.Count - 1];
        foreach (DateTime maxTime in this.maxTimes)
        {
          if (dateTime.Subtract(maxTime).Minutes <= 10)
            return maxTime;
        }
      }
      return dateTime;
    }

    private Tuple<double, double> linearInterpolate(DateTime t, DateTime t1, Tuple<double, double> p1, DateTime t2, Tuple<double, double> p2)
    {
      double num1 = (double) (t1.Minute * 60000 + t1.Second * 1000 + t1.Millisecond);
      double num2 = (double) (t2.Minute * 60000 + t2.Second * 1000 + t2.Millisecond);
      double num3 = (double) (t.Minute * 60000 + t.Second * 1000 + t.Millisecond);
      double num4 = p1.Item1;
      double num5 = p2.Item1;
      double num6 = p1.Item2;
      double num7 = p2.Item2;
      return new Tuple<double, double>((num4 * (num2 - num3) + num5 * (num3 - num1)) / (num2 - num1), (num6 * (num2 - num3) + num7 * (num3 - num1)) / (num2 - num1));
    }

    private double linearInterpolate(DateTime t, DateTime t1, double y0, DateTime t2, double y1)
    {
      double num1 = (double) (t1.Minute * 60000 + t1.Second * 1000 + t1.Millisecond);
      double num2 = (double) (t2.Minute * 60000 + t2.Second * 1000 + t2.Millisecond);
      double num3 = (double) (t.Minute * 60000 + t.Second * 1000 + t.Millisecond);
      return (y0 * (num2 - num3) + y1 * (num3 - num1)) / (num2 - num1);
    }

    private List<Tuple<DateTime, PersonInfo>> clean(string person, List<PersonInfo> raw, bool tag, bool addTagTimes)
    {
      if (addTagTimes)
      {
        this.maxPersonTimes.Add(new Tuple<DateTime, string>(raw.Last<PersonInfo>().dt, person));
        this.maxTimes.Add(raw.Last<PersonInfo>().dt);
      }
      List<Tuple<DateTime, PersonInfo>> tupleList = new List<Tuple<DateTime, PersonInfo>>();
      DateTime dt1 = raw[0].dt;
      DateTime dt2 = raw.Last<PersonInfo>().dt;
      int millisecond = dt1.Millisecond / 100 * 100 + 100;
      if (dt1.Millisecond % 100 == 0)
        millisecond -= 100;
      DateTime t = new DateTime();
      for (t = millisecond != 1000 ? new DateTime(dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute, dt1.Second, millisecond) : (dt1.Second != 59 ? new DateTime(dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute, dt1.Second + 1, 0) : new DateTime(dt1.Year, dt1.Month, dt1.Day, dt1.Hour, dt1.Minute + 1, 0, 0)); t.CompareTo(dt2) <= 0; t = t.AddMilliseconds(100.0))
      {
        int index = raw.BinarySearch(new PersonInfo()
        {
          dt = t
        }, (IComparer<PersonInfo>) new DateTimeComparer());
        if (index < 0)
          index = ~index;
        if (index > 0)
        {
          TimeSpan timeSpan = raw[index].dt.Subtract(raw[index - 1].dt);
          if (timeSpan.TotalSeconds < 60.0 || this.doAll10OfSecs)
          {
            Tuple<double, double> p1 = new Tuple<double, double>(raw[index - 1].x, raw[index - 1].y);
            Tuple<double, double> p2 = new Tuple<double, double>(raw[index].x, raw[index].y);
            Tuple<double, double> tuple = this.linearInterpolate(t, raw[index - 1].dt, p1, raw[index].dt, p2);
            double ori1 = raw[index - 1].ori;
            double ori2 = raw[index].ori;
            double num = this.linearInterpolate(t, raw[index - 1].dt, ori1, raw[index].dt, ori2);
            PersonInfo personInfo = new PersonInfo();
            if (timeSpan.TotalSeconds < 60.0)
            {
              personInfo.x = tuple.Item1;
              personInfo.y = tuple.Item2;
            }
            else
            {
              personInfo.x = -5.0;
              personInfo.y = -5.0;
            }
            personInfo.ori = num;
            tupleList.Add(new Tuple<DateTime, PersonInfo>(t, personInfo));
          }
        }
      }
      return tupleList;
    }

    public void setUbiData(Dictionary<string, List<PersonInfo>> rawData)
    {
      foreach (string key1 in rawData.Keys)
      {
        if (key1 == "Lab3d")
          ;
        try
        {
          foreach (Tuple<DateTime, PersonInfo> tuple in this.clean(key1, rawData[key1], false, true))
          {
            DateTime key2 = tuple.Item1;
            if (!this.activities.ContainsKey(key2))
              this.activities.Add(key2, new Dictionary<string, PersonInfo>());
            this.activities[key2].Add(key1, tuple.Item2);
          }
          if (key1 == "T3A")
            ;
        }
        catch (Exception ex)
        {
          Console.WriteLine("EXCEPTION: " + ex.Message);
        }
      }
    }

    public void setLenaData(Dictionary<string, List<PersonInfo>> lenadata)
    {
      foreach (string key1 in lenadata.Keys)
      {
        List<PersonInfo> personInfoList = lenadata[key1];
        bool flag = false;
        using (List<PersonInfo>.Enumerator enumerator = personInfoList.GetEnumerator())
        {
label_14:
          if (enumerator.MoveNext())
          {
            PersonInfo current = enumerator.Current;
            DateTime key2 = new DateTime(current.dt.Year, current.dt.Month, current.dt.Day, current.dt.Hour, current.dt.Minute, current.dt.Second, 0);
            if (!flag)
            {
              this.startLenaTimes.Add(key1, key2);
              flag = true;
            }
            double vd = current.vd;
            double bd = current.bd;
            double vc = current.vc;
            double tc = current.tc;
            double ac = current.ac;
            double no = current.no;
            double oln = current.oln;
            double num1 = tc / bd / 10.0;
            double num2 = vc / bd / 10.0;
            double num3 = vd / bd / 10.0;
            double num4 = ac / bd / 10.0;
            double num5 = no / bd / 10.0;
            double num6 = oln / bd / 10.0;
            if (this.personTotalCounts.ContainsKey(key1))
            {
              this.personTotalCounts[key1].vd += vd;
              this.personTotalCounts[key1].vc += vc;
              this.personTotalCounts[key1].tc += tc;
              this.personTotalCounts[key1].ac += ac;
              this.personTotalCounts[key1].no += no;
              this.personTotalCounts[key1].oln += oln;
            }
            else
            {
              this.personTotalCounts.Add(key1, new PersonInfo()
              {
                vd = vd,
                vc = vc,
                tc = tc,
                ac = ac,
                no = no,
                oln = oln
              });
              this.personTotalCountsWUbi.Add(key1, new PersonInfo());
            }
            do
            {
              if (this.activities.ContainsKey(key2) && this.activities[key2].ContainsKey(key1))
              {
                this.activities[key2][key1].wasTalking = vd > 0.0;
                this.activities[key2][key1].vc = num2;
                this.activities[key2][key1].tc = num1;
                this.activities[key2][key1].vd = num3;
                this.activities[key2][key1].ac = num4;
                this.activities[key2][key1].oln = num6;
                this.activities[key2][key1].no = num5;
                if (this.personTotalCountsWUbi.ContainsKey(key1))
                {
                  this.personTotalCountsWUbi[key1].vc += num2;
                  this.personTotalCountsWUbi[key1].tc += num1;
                  this.personTotalCountsWUbi[key1].vd += num3;
                  this.personTotalCountsWUbi[key1].ac += num4;
                  this.personTotalCountsWUbi[key1].oln += num6;
                  this.personTotalCountsWUbi[key1].no += num5;
                }
              }
              key2 = key2.AddMilliseconds(100.0);
              vd -= 0.1;
              bd -= 0.1;
            }
            while (bd > 0.0);
            goto label_14;
          }
        }
      }
    }

    public bool isItWithUbi(DateTime dt, string p)
    {
      foreach (Tuple<DateTime, DateTime> tuple in this.personUbiTimes[p])
      {
        if (dt >= tuple.Item1 && dt <= tuple.Item2)
          return true;
      }
      return false;
    }

    public void setUbiTagData(Dictionary<string, List<PersonInfo>> rawData, Dictionary<string, List<PersonInfo>> rawDataR)
    {
      string str1 = "";
      string str2 = "";
      foreach (string key in rawData.Keys)
        str1 = str1 + key + ",";
      foreach (string key in rawDataR.Keys)
        str2 = str2 + key + ",";
      foreach (string key1 in rawData.Keys)
      {
        try
        {
          List<Tuple<DateTime, PersonInfo>> tupleList1 = this.clean(key1, rawData[key1], false, false);
          List<Tuple<DateTime, PersonInfo>> tupleList2 = this.clean(key1, rawDataR[key1], false, false);
          foreach (Tuple<DateTime, PersonInfo> tuple in tupleList1)
          {
            DateTime key2 = tuple.Item1;
            if (!this.activities.ContainsKey(key2))
              this.activities.Add(key2, new Dictionary<string, PersonInfo>());
            if (this.activities[key2].ContainsKey(key1))
            {
              this.activities[key2][key1].lx = tuple.Item2.x;
              this.activities[key2][key1].ly = tuple.Item2.y;
            }
          }
          foreach (Tuple<DateTime, PersonInfo> tuple in tupleList2)
          {
            DateTime key2 = tuple.Item1;
            if (!this.activities.ContainsKey(key2))
              this.activities.Add(key2, new Dictionary<string, PersonInfo>());
            if (this.activities[key2].ContainsKey(key1))
            {
              this.activities[key2][key1].rx = tuple.Item2.x;
              this.activities[key2][key1].ry = tuple.Item2.y;
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("EXCEPTION: " + ex.Message);
        }
      }
    }

    public void setData(Dictionary<string, List<PersonInfo>> rawData, Dictionary<string, List<PersonInfo>> rawLenaData)
    {
      this.setUbiData(rawData);
      this.setLenaData(rawLenaData);
    }

    public Dictionary<string, List<PersonInfo>> readUbiFile()
    {
      Dictionary<string, List<PersonInfo>> dictionary = new Dictionary<string, List<PersonInfo>>();
      foreach (string directory in Directory.GetDirectories(this.cf.ubisenseFileDir))
      {
        DateTime result;
        if (DateTime.TryParse(directory.Substring(directory.LastIndexOf("/") + 1), out result) && result >= this.day && result < this.day.AddDays(1.0))
        {
          foreach (string file in Directory.GetFiles(directory))
          {
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith(this.cf.ubisenseFile) && fileName.EndsWith(".log"))
            {
              int num = 0;
              using (StreamReader streamReader = new StreamReader(file))
              {
                bool flag = true;
                while (!streamReader.EndOfStream)
                {
                  ++num;
                  string[] strArray = streamReader.ReadLine().Split(',');
                  if (strArray.Length > 5 && strArray[0].Trim() != "")
                  {
                    string ubiId = strArray[this.cf.ubiFileIdCol];
                    DateTime dateTime = Convert.ToDateTime(strArray[this.cf.ubiFileDateCol]);
                    if (ubiId == "T3B")
                      ;
                    try
                    {
                      if (flag)
                      {
                        this.startTime = dateTime;
                        flag = false;
                      }
                      this.endTime = dateTime;
                      PersonInfo personInfo = new PersonInfo();
                      personInfo.ubiId = ubiId;
                      personInfo.x = Convert.ToDouble(strArray[this.cf.ubiFileXPosCol]);
                      personInfo.y = Convert.ToDouble(strArray[this.cf.ubiFileYPosCol]);
                      personInfo.z = Convert.ToDouble(strArray[this.cf.ubiFileZPosCol]);
                      personInfo.ori = Convert.ToDouble(strArray[this.cf.ubiFileOriPosCol]);
                      personInfo.dt = dateTime;
                      MappingRow ubiMapping = this.cf.getUbiMapping(ubiId, dateTime);
                      personInfo.bid = ubiMapping.BID;
                      if (ubiMapping.BID != "" && !ubiMapping.isAbsent(this.day))
                      {
                        personInfo.isFreePlay = this.cf.isThisFreePlay(dateTime);
                        if (!dictionary.ContainsKey(personInfo.bid))
                          dictionary.Add(personInfo.bid, new List<PersonInfo>());
                        dictionary[personInfo.bid].Add(personInfo);
                      }
                    }
                    catch (Exception ex)
                    {
                      Console.WriteLine("EXCEPTION: " + ex.Message);
                    }
                  }
                }
              }
            }
          }
        }
      }
      return dictionary;
    }

    public Tuple<Dictionary<string, List<PersonInfo>>, Dictionary<string, List<PersonInfo>>> readUbiTagFile()
    {
      Dictionary<string, List<PersonInfo>> dictionary1 = new Dictionary<string, List<PersonInfo>>();
      Dictionary<string, List<PersonInfo>> dictionary2 = new Dictionary<string, List<PersonInfo>>();
      string[] directories = Directory.GetDirectories(this.cf.ubisenseFileDir);
      string str1 = "";
      if (this.writeChaomingFile)
        this.swc = (TextWriter) new StreamWriter(this.cf.root + this.cf.classroom + "/SYNC/chaomingtag_" + this.cf.classroom + "_" + (object) this.day.Month + "_" + (object) this.day.Day + "_" + (object) this.day.Year + ".csv");
      foreach (string path in directories)
      {
        DateTime result;
        if (DateTime.TryParse(path.Substring(path.LastIndexOf("/") + 1), out result) && result >= this.day && result < this.day.AddDays(1.0))
        {
          foreach (string file in Directory.GetFiles(path))
          {
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith(this.cf.ubisenseTagsFile) && fileName.EndsWith(".log"))
            {
              using (StreamReader streamReader = new StreamReader(file))
              {
                while (!streamReader.EndOfStream)
                {
                  string str2 = streamReader.ReadLine();
                  string[] strArray = str2.Split(',');
                  if (strArray.Length > 5)
                  {
                    string index = strArray[1].Trim();
                    if (index == "00:11:CE:00:00:00:02:BE")
                      index = index;
                    if (str1.IndexOf(index) < 0)
                    {
                      str1 = str1 + index + ",";
                      this.tagTest.Add(index, false);
                      try
                      {
                        this.cf.tagTest[index] = true;
                      }
                      catch (Exception ex)
                      {
                        Console.WriteLine("EXCEPTION: " + ex.Message);
                      }
                    }
                    string key = "";
                    DateTime dateTime = Convert.ToDateTime(strArray[2]);
                    double num1 = Convert.ToDouble(strArray[3]);
                    double num2 = Convert.ToDouble(strArray[4]);
                    PersonInfo personInfo = new PersonInfo();
                    personInfo.dt = dateTime;
                    if (this.cf.mapRowsUbiL.ContainsKey(index))
                    {
                      key = this.cf.getUbiMappingL(index, dateTime).BID;
                      personInfo.lx = num1;
                      personInfo.ly = num2;
                      if (!this.tagL.ContainsKey(index))
                        this.tagL.Add(index, index);
                      if (this.writeChaomingFile && !this.isThisInTimes(dateTime, this.cf.extractTimes))
                        this.swc.WriteLine(str2.Replace(index, key + "L"));
                    }
                    else if (this.cf.mapRowsUbiR.ContainsKey(index))
                    {
                      key = this.cf.getUbiMappingR(index, dateTime).BID;
                      personInfo.rx = num1;
                      personInfo.ry = num2;
                      if (!this.tagR.ContainsKey(index))
                        this.tagR.Add(index, index);
                      if (this.writeChaomingFile && !this.isThisInTimes(dateTime, this.cf.extractTimes))
                        this.swc.WriteLine(str2.Replace(index, key + "R"));
                    }
                    personInfo.x = num1;
                    personInfo.y = num2;
                    if (key != "")
                    {
                      if (personInfo.rx == 0.0)
                      {
                        if (!dictionary1.ContainsKey(key))
                          dictionary1.Add(key, new List<PersonInfo>());
                        dictionary1[key].Add(personInfo);
                      }
                      else
                      {
                        if (!dictionary2.ContainsKey(key))
                          dictionary2.Add(key, new List<PersonInfo>());
                        dictionary2[key].Add(personInfo);
                      }
                    }
                    else
                      this.tagMissing.Add(index);
                  }
                }
              }
            }
          }
        }
      }
      return new Tuple<Dictionary<string, List<PersonInfo>>, Dictionary<string, List<PersonInfo>>>(dictionary1, dictionary2);
    }

    public double getAdjustedSecs(string lid)
    {
      double num = 0.0;
      if (this.cf.adjustedTimes.ContainsKey(this.day) && this.cf.adjustedTimes[this.day].ContainsKey(lid))
        num = this.cf.adjustedTimes[this.day][lid];
      return num;
    }

    public Dictionary<string, List<PersonInfo>> readLenaFile()
    {
      Dictionary<string, List<PersonInfo>> dictionary = new Dictionary<string, List<PersonInfo>>();
      if (File.Exists(this.cf.lenaFile))
      {
        using (StreamReader streamReader = new StreamReader(this.cf.lenaFile))
        {
          if (!streamReader.EndOfStream)
            streamReader.ReadLine();
          int num = 0;
          while (!streamReader.EndOfStream)
          {
            ++num;
            try
            {
              string[] strArray = streamReader.ReadLine().Split(',');
              if (strArray.Length > 5 && strArray[0].Trim() != "")
              {
                string str = strArray[this.cf.lenaFileIdCol].Trim();
                DateTime dateTime = Convert.ToDateTime(strArray[this.cf.lenaFileDateCol]);
                double adjustedSecs = this.getAdjustedSecs(str);
                if (this.cf.lenaVersion.ToUpper() == "SP")
                  dateTime = dateTime.AddHours(-5.0);
                dateTime = dateTime.AddSeconds(adjustedSecs);
                if (dateTime.Year == this.day.Year && dateTime.Month == this.day.Month && dateTime.Day == this.day.Day)
                {
                  MappingRow lenaMapping = this.cf.getLenaMapping(str, dateTime);
                  string ubiId = lenaMapping.UbiID;
                  string bid = lenaMapping.BID;
                  if (ubiId != "" && lenaMapping.BID != "")
                  {
                    PersonInfo personInfo = new PersonInfo()
                    {
                      dt = dateTime,
                      bid = bid,
                      lenaId = str,
                      bd = Convert.ToDouble(strArray[this.cf.lenaFileBdCol].Trim()),
                      vd = Convert.ToDouble(strArray[this.cf.lenaFileVdCol].Trim()),
                      vc = Convert.ToDouble(strArray[this.cf.lenaFileVcCol].Trim()),
                      tc = Convert.ToDouble(strArray[this.cf.lenaFileTcCol].Trim()),
                      no = Convert.ToDouble(strArray[this.cf.lenaFileNoCol].Trim()),
                      ac = Convert.ToDouble(strArray[this.cf.lenaFileAcCol].Trim()),
                      oln = Convert.ToDouble(strArray[this.cf.lenaFileOlnCol].Trim())
                    };
                    personInfo.bid = bid;
                    personInfo.isFreePlay = this.cf.isThisFreePlay(dateTime);
                    if (!dictionary.ContainsKey(personInfo.bid))
                      dictionary.Add(personInfo.bid, new List<PersonInfo>());
                    dictionary[personInfo.bid].Add(personInfo);
                  }
                }
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine("EXCEPTION: " + ex.Message);
            }
          }
        }
      }
      return dictionary;
    }

    public bool isThisInTimes(DateTime date, Dictionary<string, List<string>> timeContainer)
    {
      bool flag = false;
      DateTime dateTime1 = date.AddHours((double) -date.Hour);
      dateTime1 = dateTime1.AddMinutes((double) -date.Minute);
      string key = dateTime1.AddSeconds((double) -date.Second).ToShortDateString().Trim();
      if (timeContainer.ContainsKey(key))
      {
        List<string> stringList = timeContainer[key];
        foreach (string str in timeContainer[key])
        {
          char[] chArray = new char[1]{ '-' };
          string[] strArray1 = str.Split(chArray);
          if (strArray1.Length >= 2)
          {
            string[] strArray2 = strArray1[0].Split(':');
            string[] strArray3 = strArray1[1].Split(':');
            int int32_1 = Convert.ToInt32(strArray2[0]);
            int int32_2 = Convert.ToInt32(strArray2[1]);
            int int32_3 = Convert.ToInt32(strArray3[0]);
            int int32_4 = Convert.ToInt32(strArray3[1]);
            DateTime dateTime2 = new DateTime(date.Year, date.Month, date.Day, int32_1, int32_2, 0);
            DateTime dateTime3 = new DateTime(date.Year, date.Month, date.Day, int32_3, int32_4, 59);
            if (date >= dateTime2 && date <= dateTime3)
            {
              flag = true;
              break;
            }
          }
        }
      }
      return flag;
    }

    public void writeMergedFile(Dictionary<string, List<PersonInfo>> rawInfo, Dictionary<string, List<PersonInfo>> rawLenaInfo)
    {
      using (TextWriter textWriter = (TextWriter) new StreamWriter(this.cf.syncFilePre + (object) this.day.Month + "_" + (object) this.day.Day + "_" + (object) this.day.Year + ".CSV"))
      {
        textWriter.Write("BID, UbiID, LenaID, Is Free Play,Date String,X,Y,Z,Orientation");
        textWriter.WriteLine("Block Duration, Voc Duration, Voc Count, Turn Count,Noise,Adult Count,OLN");
        foreach (string bid in this.cf.bids)
        {
          foreach (PersonInfo personInfo in rawInfo[bid])
            textWriter.WriteLine(personInfo.bid + "," + personInfo.ubiId + "," + personInfo.lenaId + "," + personInfo.isFreePlay.ToString() + "," + (object) personInfo.dt + "," + (object) personInfo.x + "," + (object) personInfo.y + "," + (object) personInfo.z + "," + (object) personInfo.ori);
          foreach (PersonInfo personInfo in rawLenaInfo[bid])
            textWriter.WriteLine(personInfo.bid + "," + personInfo.ubiId + "," + personInfo.lenaId + "," + personInfo.isFreePlay.ToString() + "," + (object) personInfo.dt + ",,,,," + (object) personInfo.bd + "," + (object) personInfo.vd + "," + (object) personInfo.vc + "," + (object) personInfo.tc + "," + (object) personInfo.no + "," + (object) personInfo.ac + "," + (object) personInfo.oln);
        }
      }
    }

    public static double calcSquaredDist(PersonInfo a, PersonInfo b)
    {
      double x1 = a.x;
      double y1 = a.y;
      double x2 = b.x;
      double y2 = b.y;
      return Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0);
    }

    public static bool withinOrientation(PersonInfo a, PersonInfo b, double angle)
    {
      Tuple<double, double> tuple = ClassroomDay.withinOrientationData(a, b);
      return Math.Abs(tuple.Item1) <= angle && Math.Abs(tuple.Item2) <= angle;
    }

    public static Tuple<double, double> withinOrientationData(PersonInfo a, PersonInfo b)
    {
      Tuple<double, double> tuple = new Tuple<double, double>(180.0, 180.0);
      if (a.lx > 0.0 && a.ly > 0.0 && b.lx > 0.0 && b.ly > 0.0)
      {
        double center1 = ClassroomDay.getCenter(a.rx, a.lx);
        double center2 = ClassroomDay.getCenter(a.ry, a.ly);
        double center3 = ClassroomDay.getCenter(b.rx, b.lx);
        double center4 = ClassroomDay.getCenter(b.ry, b.ly);
        double x1 = center3 - center1;
        double y1 = center4 - center2;
        ClassroomDay.normalize(ref x1, ref y1);
        double num1 = -y1;
        double num2 = x1;
        double x2 = (a.lx - a.rx) / 2.0;
        double y2 = (a.ly - a.ry) / 2.0;
        double x3 = (b.lx - b.rx) / 2.0;
        double y3 = (b.ly - b.ry) / 2.0;
        ClassroomDay.normalize(ref x2, ref y2);
        ClassroomDay.normalize(ref x3, ref y3);
        tuple = new Tuple<double, double>(Math.Atan2(-(x1 * x2 + y1 * y2), num1 * x2 + num2 * y2) * (180.0 / Math.PI), Math.Atan2(x1 * x3 + y1 * y3, -(num1 * x3 + num2 * y3)) * (180.0 / Math.PI));
      }
      return tuple;
    }

    public static void normalize(ref double x, ref double y)
    {
      double num = Math.Sqrt(x * x + y * y);
      x /= num;
      y /= num;
    }

    public static double getCenter(double x, double x2)
    {
      double num = Math.Abs(x2 - x) / 2.0;
      return x < x2 ? x + num : x2 + num;
    }

    public bool isWithLenaStart(DateTime d, string p)
    {
      return this.startLenaTimes.ContainsKey(p) && this.startLenaTimes[p].CompareTo(d) <= 0;
    }

    public void countInteractions(bool writeFile, bool writeDetailFile, bool trunkDetailFile, DateTime trunk, Dictionary<string, List<PersonInfo>> lenaInfo)
    {
      TextWriter textWriter1 = (TextWriter) null;
      if (writeDetailFile)
        textWriter1 = (TextWriter) new StreamWriter(this.cf.root + this.cf.classroom + "/SYNC/" + this.cf.version + "interaction_angles_xy_output_" + (trunkDetailFile ? (object) "trunk_" : (object) "") + (this.cf.justFreePlay ? (object) "freeplay_" : (object) "") + (object) this.day.Month + "_" + (object) this.day.Day + "_" + (object) this.day.Year + ".csv");
      if (writeDetailFile)
        textWriter1.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
      foreach (KeyValuePair<DateTime, Dictionary<string, PersonInfo>> keyValuePair in (IEnumerable<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>>) this.activities.OrderBy<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>((Func<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>) (key => key.Key)))
      {
        DateTime key1 = keyValuePair.Key;
        if (trunkDetailFile && key1.CompareTo(trunk) <= 0 && (!this.cf.justFreePlay || this.cf.isThisFreePlay(key1)))
        {
          foreach (string key2 in this.activities[key1].Keys)
          {
            if (this.noLena || !this.startFromLena || this.isWithLenaStart(key1, key2))
            {
              if (!this.individualTime.ContainsKey(key2))
                this.individualTime.Add(key2, 0.0);
              else
                this.individualTime[key2] += 0.1;
              foreach (string key3 in this.activities[key1].Keys)
              {
                if (!key3.Equals(key2))
                {
                  string str1 = key3;
                  string str2 = key2;
                  string key4 = key3 + "-" + key2;
                  if (!this.cf.pairs.Contains(key4) && this.cf.pairs.Contains(key2 + "-" + key3))
                  {
                    key4 = key2 + "-" + key3;
                    str1 = key2;
                    str2 = key3;
                  }
                  if (!this.pairTime.ContainsKey(key4))
                  {
                    this.pairTime.Add(key4, 0.0);
                    this.pairClose.Add(key4, 0.0);
                    this.pairCloseOrientation.Add(key4, 0.0);
                    this.pairStats.Add(key4, 0.0);
                    this.pairStatsSep.Add(key4, new PairInfo());
                    this.pairStatsSeparated.Add(key4, new Tuple<double, double, double, double>(0.0, 0.0, 0.0, 0.0));
                    this.pairCry.Add(key4, 0.0);
                  }
                  this.pairTime[key4] += 0.1;
                  if (ClassroomDay.calcSquaredDist(this.activities[key1][key2], this.activities[key1][key3]) <= ClassroomDay.minDistance)
                  {
                    this.pairClose[key4] += 0.1;
                    Tuple<double, double> tuple = ClassroomDay.withinOrientationData(this.activities[key1][key3], this.activities[key1][key2]);
                    bool flag = Math.Abs(tuple.Item1) <= 45.0 && Math.Abs(tuple.Item2) <= 45.0;
                    if (writeDetailFile)
                      textWriter1.WriteLine(key2 + "," + key3 + "," + key1.ToLongTimeString() + ",0.1," + (flag ? (object) "0.1," : (object) "0,") + (object) tuple.Item1 + "," + (object) tuple.Item2 + "," + (object) this.activities[key1][key2].lx + "," + (object) this.activities[key1][key2].ly + "," + (object) this.activities[key1][key2].rx + "," + (object) this.activities[key1][key2].ry + "," + (object) this.activities[key1][key3].lx + "," + (object) this.activities[key1][key3].ly + "," + (object) this.activities[key1][key3].rx + "," + (object) this.activities[key1][key3].ry);
                    if (flag)
                      this.pairCloseOrientation[key4] += 0.1;
                  }
                }
              }
              bool wasTalking = this.activities[key1][key2].wasTalking;
              double tc = this.activities[key1][key2].tc;
              double vc = this.activities[key1][key2].vc;
              double vd = this.activities[key1][key2].vd;
              double ac = this.activities[key1][key2].ac;
              double no = this.activities[key1][key2].no;
              double oln = this.activities[key1][key2].oln;
              double cry = this.activities[key1][key2].cry;
              foreach (string key3 in this.activities[key1].Keys)
              {
                if (!key3.Equals(key2))
                {
                  string str1 = key3;
                  string str2 = key2;
                  string index = key3 + "-" + key2;
                  if (!this.cf.pairs.Contains(index) && this.cf.pairs.Contains(key2 + "-" + key3))
                  {
                    index = key2 + "-" + key3;
                    str1 = key2;
                    str2 = key3;
                  }
                  double num1 = ClassroomDay.calcSquaredDist(this.activities[key1][key3], this.activities[key1][key2]);
                  if (num1 <= ClassroomDay.minDistance)
                  {
                    if (Math.Round(num1, 2) == 0.63 && key3 == "Lab2D" && key2 == "2D")
                      ;
                    if (this.justProx || ClassroomDay.withinOrientation(this.activities[key1][key3], this.activities[key1][key2], 45.0))
                    {
                      if (this.activities[key1][key2].cry <= 0.0 || this.activities[key1][key3].cry <= 0.0)
                        ;
                      foreach (PersonInfo personInfo in lenaInfo[key2])
                      {
                        DateTime dt = personInfo.dt;
                        DateTime dateTime = personInfo.dt.AddSeconds(personInfo.bd);
                        if (key1 >= dt && key1 <= dateTime)
                        {
                          double num2 = personInfo.tc <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.tc / personInfo.bd / 10.0;
                          double num3 = personInfo.ac <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.ac / personInfo.bd / 10.0;
                          double num4 = personInfo.no <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.no / personInfo.bd / 10.0;
                          double num5 = personInfo.vc <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.vc / personInfo.bd / 10.0;
                          double num6 = personInfo.oln <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.oln / personInfo.bd / 10.0;
                          double num7 = personInfo.cry <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.cry / personInfo.bd / 10.0;
                          double num8 = personInfo.vd <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.vd / personInfo.bd / 10.0;
                          PairInfo pairInfo = this.pairStatsSep[index];
                          if (key2.Equals(index.Split('-')[0]))
                          {
                            pairInfo.p1.vd += num8;
                            pairInfo.p1.vc += num5;
                            pairInfo.p1.tc += num2;
                            pairInfo.p1.ac += num3;
                            pairInfo.p1.no += num4;
                            pairInfo.p1.oln += num6;
                            pairInfo.p1.cry += num7;
                          }
                          else
                          {
                            pairInfo.p2.vd += num8;
                            pairInfo.p2.vc += num5;
                            pairInfo.p2.tc += num2;
                            pairInfo.p2.ac += num3;
                            pairInfo.p2.no += num4;
                            pairInfo.p2.oln += num6;
                            pairInfo.p2.cry += num7;
                          }
                        }
                      }
                      if (wasTalking)
                      {
                        this.pairStats[index] += 0.1;
                        if (key2.Equals(index.Split('-')[0]))
                        {
                          if (index == "14A-T3A" && this.pairStatsSeparated[index].Item1 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[index] = new Tuple<double, double, double, double>(this.pairStatsSeparated[index].Item1 + 0.1, this.pairStatsSeparated[index].Item2, this.pairStatsSeparated[index].Item3, this.pairStatsSeparated[index].Item4);
                        }
                        else
                        {
                          if (index == "T3A-14A" && this.pairStatsSeparated[index].Item2 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[index] = new Tuple<double, double, double, double>(this.pairStatsSeparated[index].Item1, this.pairStatsSeparated[index].Item2 + 0.1, this.pairStatsSeparated[index].Item3, this.pairStatsSeparated[index].Item4);
                        }
                      }
                      if (this.activities[key1][key2].cry > 0.0)
                      {
                        if (key2.Equals(index.Split('-')[0]))
                        {
                          this.pairStatsSeparated[index] = new Tuple<double, double, double, double>(this.pairStatsSeparated[index].Item1 + 0.1, this.pairStatsSeparated[index].Item2, this.pairStatsSeparated[index].Item3, this.pairStatsSeparated[index].Item4);
                        }
                        else
                        {
                          if (index == "T3A-14A" && this.pairStatsSeparated[index].Item2 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[index] = new Tuple<double, double, double, double>(this.pairStatsSeparated[index].Item1, this.pairStatsSeparated[index].Item2 + 0.1, this.pairStatsSeparated[index].Item3, this.pairStatsSeparated[index].Item4);
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      if (!writeFile)
        return;
      object[] objArray = new object[10]
      {
        (object) this.cf.version,
        (object) "interaction_output_",
        trunkDetailFile ? (object) "trunk_" : (object) "",
        this.cf.justFreePlay ? (object) "freeplay_" : (object) "",
        (object) this.day.Month,
        (object) "_",
        (object) this.day.Day,
        (object) "_",
        (object) this.day.Year,
        (object) ".csv"
      };
      TextWriter textWriter2;
      using (textWriter2 = (TextWriter) new StreamWriter(string.Concat(objArray)))
      {
        textWriter2.WriteLine("Person 1, Person2, Interaction Time, Total Time, Interaction Normalized");
        foreach (string key in this.pairStats.Keys)
        {
          double num1 = Math.Round(this.pairStats[key], 1);
          double num2 = Math.Round(this.pairTime[key], 1);
          double num3 = num1 / num2;
          textWriter2.WriteLine(key.Split('-')[0] + "," + key.Split('-')[1] + "," + (object) num1 + "," + (object) num2 + "," + (object) num3);
        }
      }
    }

    public void countInteractionsOld(bool writeFile, bool writeDetailFile, bool trunkDetailFile, DateTime trunk, Dictionary<string, List<PersonInfo>> lenaInfo)
    {
      TextWriter textWriter1 = (TextWriter) null;
      if (writeDetailFile)
        textWriter1 = (TextWriter) new StreamWriter(this.cf.root + this.cf.classroom + "/SYNC/" + this.cf.version + "interaction_angles_xy_output_" + (trunkDetailFile ? (object) "trunk_" : (object) "") + (this.cf.justFreePlay ? (object) "freeplay_" : (object) "") + (object) this.day.Month + "_" + (object) this.day.Day + "_" + (object) this.day.Year + ".csv");
      if (writeDetailFile)
        textWriter1.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
      foreach (KeyValuePair<DateTime, Dictionary<string, PersonInfo>> keyValuePair in (IEnumerable<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>>) this.activities.OrderBy<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>((Func<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>) (key => key.Key)))
      {
        DateTime key1 = keyValuePair.Key;
        if (trunkDetailFile && key1.CompareTo(trunk) <= 0 && (!this.cf.justFreePlay || this.cf.isThisFreePlay(key1)))
        {
          foreach (string key2 in this.activities[key1].Keys)
          {
            if (this.noLena || !this.startFromLena || this.isWithLenaStart(key1, key2))
            {
              if (!this.individualTime.ContainsKey(key2))
                this.individualTime.Add(key2, 0.0);
              else
                this.individualTime[key2] += 0.1;
              foreach (string key3 in this.activities[key1].Keys)
              {
                if (!key3.Equals(key2))
                {
                  string key4 = key3 + "-" + key2;
                  if (!this.cf.pairs.Contains(key4) && this.cf.pairs.Contains(key2 + "-" + key3))
                    key4 = key2 + "-" + key3;
                  if (!this.pairTime.ContainsKey(key4))
                  {
                    this.pairTime.Add(key4, 0.0);
                    this.pairClose.Add(key4, 0.0);
                    this.pairCloseOrientation.Add(key4, 0.0);
                    this.pairStats.Add(key4, 0.0);
                    this.pairStatsSep.Add(key4, new PairInfo());
                    this.pairStatsSeparated.Add(key4, new Tuple<double, double, double, double>(0.0, 0.0, 0.0, 0.0));
                    this.pairCry.Add(key4, 0.0);
                  }
                  this.pairTime[key4] += 0.1;
                  if (ClassroomDay.calcSquaredDist(this.activities[key1][key2], this.activities[key1][key3]) <= ClassroomDay.minDistance)
                  {
                    this.pairClose[key4] += 0.1;
                    Tuple<double, double> tuple = ClassroomDay.withinOrientationData(this.activities[key1][key3], this.activities[key1][key2]);
                    bool flag = Math.Abs(tuple.Item1) <= 45.0 && Math.Abs(tuple.Item2) <= 45.0;
                    if (writeDetailFile)
                      textWriter1.WriteLine(key2 + "," + key3 + "," + key1.ToLongTimeString() + ",0.1," + (flag ? (object) "0.1," : (object) "0,") + (object) tuple.Item1 + "," + (object) tuple.Item2 + "," + (object) this.activities[key1][key2].lx + "," + (object) this.activities[key1][key2].ly + "," + (object) this.activities[key1][key2].rx + "," + (object) this.activities[key1][key2].ry + "," + (object) this.activities[key1][key3].lx + "," + (object) this.activities[key1][key3].ly + "," + (object) this.activities[key1][key3].rx + "," + (object) this.activities[key1][key3].ry);
                    if (flag)
                      this.pairCloseOrientation[key4] += 0.1;
                  }
                }
              }
              bool wasTalking = this.activities[key1][key2].wasTalking;
              double tc = this.activities[key1][key2].tc;
              double vc = this.activities[key1][key2].vc;
              double vd = this.activities[key1][key2].vd;
              double ac = this.activities[key1][key2].ac;
              double no = this.activities[key1][key2].no;
              double oln = this.activities[key1][key2].oln;
              double cry = this.activities[key1][key2].cry;
              foreach (string key3 in this.activities[key1].Keys)
              {
                if (!key3.Equals(key2))
                {
                  double num1 = ClassroomDay.calcSquaredDist(this.activities[key1][key3], this.activities[key1][key2]);
                  if (num1 <= ClassroomDay.minDistance)
                  {
                    if (Math.Round(num1, 2) == 0.63 && key3 == "Lab2D" && key2 == "2D")
                      ;
                    if (this.justProx || ClassroomDay.withinOrientation(this.activities[key1][key3], this.activities[key1][key2], 45.0))
                    {
                      string key4 = key3 + "-" + key2;
                      if (!this.pairStats.ContainsKey(key4))
                      {
                        if (this.pairStats.ContainsKey(key2 + "-" + key3))
                        {
                          key4 = key2 + "-" + key3;
                        }
                        else
                        {
                          this.pairStats.Add(key4, 0.0);
                          this.pairStatsSep.Add(key4, new PairInfo());
                          this.pairStatsSeparated.Add(key4, new Tuple<double, double, double, double>(0.0, 0.0, 0.0, 0.0));
                        }
                      }
                      if (this.activities[key1][key2].cry > 0.0 && this.activities[key1][key3].cry > 0.0)
                      {
                        if (key4 == "14A-T3A" && this.pairStatsSeparated[key4].Item1 + 0.1 == 17.0)
                          ;
                        if (key2 == "14A")
                          ;
                      }
                      foreach (PersonInfo personInfo in lenaInfo[key2])
                      {
                        DateTime dt = personInfo.dt;
                        DateTime dateTime = personInfo.dt.AddSeconds(personInfo.bd);
                        if (key1 >= dt && key1 <= dateTime)
                        {
                          double num2 = personInfo.tc <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.tc / personInfo.bd / 10.0;
                          double num3 = personInfo.ac <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.ac / personInfo.bd / 10.0;
                          double num4 = personInfo.no <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.no / personInfo.bd / 10.0;
                          double num5 = personInfo.vc <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.vc / personInfo.bd / 10.0;
                          double num6 = personInfo.oln <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.oln / personInfo.bd / 10.0;
                          double num7 = personInfo.cry <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.cry / personInfo.bd / 10.0;
                          double num8 = personInfo.vd <= 0.0 || personInfo.bd <= 0.0 ? 0.0 : personInfo.vd / personInfo.bd / 10.0;
                          PairInfo pairInfo = this.pairStatsSep[key4];
                          if (key2.Equals(key4.Split('-')[0]))
                          {
                            pairInfo.p1.vd += num8;
                            pairInfo.p1.vc += num5;
                            pairInfo.p1.tc += num2;
                            pairInfo.p1.ac += num3;
                            pairInfo.p1.no += num4;
                            pairInfo.p1.oln += num6;
                            pairInfo.p1.cry += num7;
                          }
                          else
                          {
                            pairInfo.p2.vd += num8;
                            pairInfo.p2.vc += num5;
                            pairInfo.p2.tc += num2;
                            pairInfo.p2.ac += num3;
                            pairInfo.p2.no += num4;
                            pairInfo.p2.oln += num6;
                            pairInfo.p2.cry += num7;
                          }
                        }
                      }
                      if (wasTalking)
                      {
                        this.pairStats[key4] += 0.1;
                        if (key2.Equals(key4.Split('-')[0]))
                        {
                          if (key4 == "14A-T3A" && this.pairStatsSeparated[key4].Item1 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[key4] = new Tuple<double, double, double, double>(this.pairStatsSeparated[key4].Item1 + 0.1, this.pairStatsSeparated[key4].Item2, this.pairStatsSeparated[key4].Item3, this.pairStatsSeparated[key4].Item4);
                        }
                        else
                        {
                          if (key4 == "T3A-14A" && this.pairStatsSeparated[key4].Item2 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[key4] = new Tuple<double, double, double, double>(this.pairStatsSeparated[key4].Item1, this.pairStatsSeparated[key4].Item2 + 0.1, this.pairStatsSeparated[key4].Item3, this.pairStatsSeparated[key4].Item4);
                        }
                      }
                      if (this.activities[key1][key2].cry > 0.0)
                      {
                        if (key2.Equals(key4.Split('-')[0]))
                        {
                          this.pairStatsSeparated[key4] = new Tuple<double, double, double, double>(this.pairStatsSeparated[key4].Item1 + 0.1, this.pairStatsSeparated[key4].Item2, this.pairStatsSeparated[key4].Item3, this.pairStatsSeparated[key4].Item4);
                        }
                        else
                        {
                          if (key4 == "T3A-14A" && this.pairStatsSeparated[key4].Item2 + 0.1 == 17.0)
                            ;
                          this.pairStatsSeparated[key4] = new Tuple<double, double, double, double>(this.pairStatsSeparated[key4].Item1, this.pairStatsSeparated[key4].Item2 + 0.1, this.pairStatsSeparated[key4].Item3, this.pairStatsSeparated[key4].Item4);
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      if (!writeFile)
        return;
      object[] objArray = new object[10]
      {
        (object) this.cf.version,
        (object) "interaction_output_",
        trunkDetailFile ? (object) "trunk_" : (object) "",
        this.cf.justFreePlay ? (object) "freeplay_" : (object) "",
        (object) this.day.Month,
        (object) "_",
        (object) this.day.Day,
        (object) "_",
        (object) this.day.Year,
        (object) ".csv"
      };
      TextWriter textWriter2;
      using (textWriter2 = (TextWriter) new StreamWriter(string.Concat(objArray)))
      {
        textWriter2.WriteLine("Person 1, Person2, Interaction Time, Total Time, Interaction Normalized");
        foreach (string key in this.pairStats.Keys)
        {
          double num1 = Math.Round(this.pairStats[key], 1);
          double num2 = Math.Round(this.pairTime[key], 1);
          double num3 = num1 / num2;
          textWriter2.WriteLine(key.Split('-')[0] + "," + key.Split('-')[1] + "," + (object) num1 + "," + (object) num2 + "," + (object) num3);
        }
      }
    }

    public void write10SecTalkingCSV(string file_name)
    {
      using (TextWriter textWriter = (TextWriter) new StreamWriter(file_name))
      {
        textWriter.WriteLine("BID, DateTime, X, Y, Orientation, Talking, Aid, S, Type,rx,ry,lx,ly");
        foreach (KeyValuePair<DateTime, Dictionary<string, PersonInfo>> keyValuePair in (IEnumerable<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>>) this.activities.OrderBy<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>((Func<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>) (key => key.Key)))
        {
          DateTime key1 = keyValuePair.Key;
          if (!this.cf.justFreePlay || this.cf.isThisFreePlay(key1))
          {
            foreach (string key2 in this.activities[key1].Keys)
            {
              MappingRow mapping = this.cf.getMapping(key2, this.day);
              if (!this.startFromLena || this.isWithLenaStart(key1, key2))
              {
                if (mapping.BID == "Lab3d")
                  ;
                textWriter.WriteLine(key2 + "," + key1.ToString("hh:mm:ss.fff tt") + "," + (object) this.activities[key1][key2].x + "," + (object) this.activities[key1][key2].y + "," + (object) this.activities[key1][key2].ori + "," + this.activities[key1][key2].wasTalking.ToString() + "," + mapping.aid + "," + mapping.sex + "," + mapping.type + "," + (object) this.activities[key1][key2].rx + "," + (object) this.activities[key1][key2].ry + "," + (object) this.activities[key1][key2].lx + "," + (object) this.activities[key1][key2].ly);
              }
            }
          }
        }
      }
    }

    public void setAdjusts()
    {
      Dictionary<string, double> dictionary1 = new Dictionary<string, double>();
      dictionary1["14861"] = 0.0;
      dictionary1["14866"] = 2.0;
      dictionary1["8236"] = -31.0;
      dictionary1["14867"] = -2.0;
      dictionary1["11566"] = 0.0;
      dictionary1["14859"] = 0.0;
      dictionary1["7539"] = -103.0;
      dictionary1["14868"] = 0.0;
      dictionary1["14865"] = 4.0;
      dictionary1["11563"] = 9.0;
      dictionary1["11564"] = -23.0;
      dictionary1["8235"] = 4.0;
      dictionary1["14863"] = 1.0;
      Dictionary<string, double> dictionary2 = new Dictionary<string, double>();
      dictionary1["14864"] = 18.0;
      dictionary1["14870"] = 18.307625;
      dictionary1["14861"] = 15.0;
      dictionary1["14866"] = 20.3674375;
      dictionary1["8236"] = 5.8688125;
      dictionary1["14867"] = -1.0;
      dictionary1["11566"] = -1.0;
      dictionary1["14859"] = 6.9963125;
      dictionary1["7539"] = 54.067;
      dictionary1["14868"] = 17.3675625;
      dictionary1["14865"] = 28.7556875;
      dictionary1["11563"] = -2.3576875;
      dictionary1["11564"] = -1247.0 / 160.0;
      dictionary1["8235"] = 29.9965;
      dictionary1["14863"] = 20.0;
      this.adjustTimes[new DateTime(2017, 3, 10)] = dictionary1;
      this.adjustTimes[new DateTime(2017, 3, 17)] = dictionary2;
    }

    public Dictionary<string, List<PersonInfo>> readLenaItsFiles()
    {
      Dictionary<string, List<PersonInfo>> rl = new Dictionary<string, List<PersonInfo>>();
      foreach (string directory in Directory.GetDirectories(this.cf.lenaFileDir + "//ITS//"))
      {
        DateTime result;
        if (DateTime.TryParse(directory.Substring(directory.LastIndexOf("/") + 1), out result) && result >= this.day && result < this.day.AddDays(1.0))
        {
          foreach (string file in Directory.GetFiles(directory))
          {
            Path.GetFileName(file);
            Console.WriteLine(file);
            string str1 = file.Substring(file.IndexOf("\\") + 2).Substring(16, 6);
            if (str1.Substring(0, 2) == "00")
              str1 = str1.Substring(2);
            else if (str1.Substring(0, 1) == "0")
              str1 = str1.Substring(1);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(file);
            foreach (XmlNode selectNode in xmlDocument.ChildNodes[2].SelectNodes("ProcessingUnit/Recording"))
            {
              DateTime dateTime1 = DateTime.Parse(selectNode.Attributes["startClockTime"].Value);
              XmlNodeList xmlNodeList1 = selectNode.SelectNodes("Conversation|Pause");
              selectNode.SelectNodes("Conversation");
              double adjustedSecs = this.getAdjustedSecs(str1);
              DateTime dateTime2 = dateTime1.AddSeconds(adjustedSecs);
              if (dateTime2.Day == this.day.Day)
              {
                foreach (XmlNode xmlNode1 in xmlNodeList1)
                {
                  string str2 = xmlNode1.Attributes["num"].Value;
                  XmlNodeList xmlNodeList2 = xmlNode1.SelectNodes("Segment");
                  double num1 = Convert.ToDouble(xmlNode1.Attributes["startTime"].Value.Substring(2, xmlNode1.Attributes["startTime"].Value.Length - 3));
                  double num2 = Convert.ToDouble(xmlNode1.Attributes["endTime"].Value.Substring(2, xmlNode1.Attributes["endTime"].Value.Length - 3));
                  DateTime d1 = dateTime2.AddSeconds(num1);
                  DateTime dateTime3 = dateTime2.AddSeconds(num2);
                  PersonInfo lpi1 = new PersonInfo();
                  MappingRow lenaMapping1 = this.cf.getLenaMapping(str1, d1);
                  if (xmlNode1.Name == "Conversation")
                  {
                    double num3 = Convert.ToDouble(xmlNode1.Attributes["turnTaking"].Value);
                    if (num3 > 0.0)
                    {
                      lpi1.dt = d1;
                      lpi1.ubiId = lenaMapping1.UbiID;
                      lpi1.bid = lenaMapping1.BID;
                      lpi1.lenaId = str1;
                      lpi1.bd = num2 - num1;
                      lpi1.tc = num3;
                      this.addToRawLena(ref rl, lpi1);
                    }
                  }
                  foreach (XmlNode xmlNode2 in xmlNodeList2)
                  {
                    double num3 = Convert.ToDouble(xmlNode2.Attributes["startTime"].Value.Substring(2, xmlNode2.Attributes["startTime"].Value.Length - 3));
                    double num4 = Convert.ToDouble(xmlNode2.Attributes["endTime"].Value.Substring(2, xmlNode2.Attributes["endTime"].Value.Length - 3));
                    DateTime d2 = dateTime2.AddSeconds(num3);
                    dateTime3 = dateTime2.AddSeconds(num4);
                    PersonInfo lpi2 = new PersonInfo();
                    lpi2.dt = d2;
                    MappingRow lenaMapping2 = this.cf.getLenaMapping(str1, d2);
                    lpi2.ubiId = lenaMapping2.UbiID;
                    lpi2.bid = lenaMapping2.BID;
                    lpi2.lenaId = str1;
                    string str3 = xmlNode2.Attributes["spkr"].Value;
                    lpi2.bd = num4 - num3;
                    bool flag = false;
                    string str4 = str3;
                    if (!(str4 == "CHN") && !(str4 == "CHF"))
                    {
                      if (!(str4 == "FAN"))
                      {
                        if (!(str4 == "MAN"))
                        {
                          if (!(str4 == "OLN"))
                          {
                            if (str4 == "NON")
                            {
                              lpi2.bd = num4 - num3;
                              lpi2.no = lpi2.bd;
                              flag = true;
                            }
                          }
                          else
                          {
                            lpi2.bd = num4 - num3;
                            lpi2.oln = lpi2.bd;
                            flag = true;
                          }
                        }
                        else
                        {
                          lpi2.ac = Convert.ToDouble(xmlNode2.Attributes["maleAdultWordCnt"].Value);
                          flag = true;
                        }
                      }
                      else
                      {
                        lpi2.ac = Convert.ToDouble(xmlNode2.Attributes["femaleAdultWordCnt"].Value);
                        flag = true;
                      }
                    }
                    else
                    {
                      lpi2.vd = Convert.ToDouble(xmlNode2.Attributes["childUttLen"].Value.Substring(1, xmlNode2.Attributes["childUttLen"].Value.Length - 2));
                      lpi2.vc = Convert.ToDouble(xmlNode2.Attributes["childUttCnt"].Value);
                      foreach (XmlAttribute attribute in (XmlNamedNodeMap) xmlNode2.Attributes)
                      {
                        if (attribute.Name.IndexOf("startCry") == 0)
                        {
                          string str5 = attribute.Name.Substring(8);
                          string name = attribute.Name;
                          double num5 = Convert.ToDouble(xmlNode2.Attributes[name].Value.Substring(2, xmlNode2.Attributes[name].Value.Length - 3));
                          double num6 = Convert.ToDouble(xmlNode2.Attributes["endCry" + str5].Value.Substring(2, xmlNode2.Attributes["endCry" + str5].Value.Length - 3));
                          DateTime d3 = dateTime2.AddSeconds(num5);
                          DateTime dateTime4 = dateTime2.AddSeconds(num6);
                          PersonInfo lpi3 = new PersonInfo();
                          lpi3.dt = d3;
                          MappingRow lenaMapping3 = this.cf.getLenaMapping(str1, d3);
                          lpi3.ubiId = lenaMapping3.UbiID;
                          lpi3.bid = lenaMapping3.BID;
                          lpi3.lenaId = str1;
                          lpi3.bd = num6 - num5;
                          lpi3.cry = lpi3.bd;
                          this.addToRawLena(ref rl, lpi3);
                          cryInfoTest cryInfoTest = new cryInfoTest();
                          cryInfoTest.startTime = d3;
                          cryInfoTest.endTime = dateTime4;
                          cryInfoTest.secs = lpi3.cry;
                          cryInfoTest.szstart = xmlNode2.Attributes[name].Value;
                          cryInfoTest.szend = xmlNode2.Attributes["endCry" + str5].Value;
                          cryInfoTest.start = num5;
                          cryInfoTest.end = num6;
                          if (!this.childCries.ContainsKey(lpi3.bid))
                            this.childCries.Add(lpi3.bid, new List<cryInfoTest>());
                          this.childCries[lpi3.bid].Add(cryInfoTest);
                        }
                      }
                      flag = true;
                    }
                    if (flag)
                      this.addToRawLena(ref rl, lpi2);
                  }
                }
              }
            }
          }
        }
      }
      return rl;
    }

    public void writeLenaItsOnsets(bool firstDay, string name)
    {
      using (TextWriter textWriter = (TextWriter) new StreamWriter(this.cf.syncFilePre + "_ITSONSETS_" + this.cf.classroom + "_" + name + ".CSV", !firstDay))
      {
        if (firstDay)
          textWriter.WriteLine("Date,Subject,SubjectType,segmentid,voctype,recstart,startsec,endsec,starttime,endtime,duration,uttlen");
        Dictionary<string, List<PersonInfo>> rl = new Dictionary<string, List<PersonInfo>>();
        foreach (string directory in Directory.GetDirectories(this.cf.lenaFileDir + "//ITS//"))
        {
          DateTime result;
          if (DateTime.TryParse(directory.Substring(directory.LastIndexOf("/") + 1), out result) && result >= this.day && result < this.day.AddDays(1.0))
          {
            foreach (string file in Directory.GetFiles(directory))
            {
              Path.GetFileName(file);
              Console.WriteLine(file);
              string str1 = file.Substring(file.IndexOf("\\") + 2).Substring(16, 6);
              if (str1.Substring(0, 2) == "00")
                str1 = str1.Substring(2);
              else if (str1.Substring(0, 1) == "0")
                str1 = str1.Substring(1);
              XmlDocument xmlDocument = new XmlDocument();
              xmlDocument.Load(file);
              foreach (XmlNode selectNode in xmlDocument.ChildNodes[2].SelectNodes("ProcessingUnit/Recording"))
              {
                DateTime dateTime1 = DateTime.Parse(selectNode.Attributes["startClockTime"].Value);
                XmlNodeList xmlNodeList1 = selectNode.SelectNodes("Conversation|Pause");
                selectNode.SelectNodes("Conversation");
                double adjustedSecs = this.getAdjustedSecs(str1);
                DateTime dateTime2 = dateTime1.AddSeconds(adjustedSecs);
                if (dateTime2.Day == this.day.Day)
                {
                  int num1 = 0;
                  foreach (XmlNode xmlNode1 in xmlNodeList1)
                  {
                    string str2 = xmlNode1.Attributes["num"].Value;
                    XmlNodeList xmlNodeList2 = xmlNode1.SelectNodes("Segment");
                    double num2 = Convert.ToDouble(xmlNode1.Attributes["startTime"].Value.Substring(2, xmlNode1.Attributes["startTime"].Value.Length - 3));
                    double num3 = Convert.ToDouble(xmlNode1.Attributes["endTime"].Value.Substring(2, xmlNode1.Attributes["endTime"].Value.Length - 3));
                    DateTime d1 = dateTime2.AddSeconds(num2);
                    dateTime2.AddSeconds(num3);
                    PersonInfo lpi1 = new PersonInfo();
                    MappingRow lenaMapping1 = this.cf.getLenaMapping(str1, d1);
                    if (xmlNode1.Name == "Conversation")
                    {
                      double num4 = Convert.ToDouble(xmlNode1.Attributes["turnTaking"].Value);
                      if (num4 > 0.0)
                      {
                        lpi1.dt = d1;
                        lpi1.ubiId = lenaMapping1.UbiID;
                        lpi1.bid = lenaMapping1.BID;
                        lpi1.lenaId = str1;
                        lpi1.bd = num3 - num2;
                        lpi1.tc = num4;
                        this.addToRawLena(ref rl, lpi1);
                      }
                    }
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                      double num4 = Convert.ToDouble(xmlNode2.Attributes["startTime"].Value.Substring(2, xmlNode2.Attributes["startTime"].Value.Length - 3));
                      double num5 = Convert.ToDouble(xmlNode2.Attributes["endTime"].Value.Substring(2, xmlNode2.Attributes["endTime"].Value.Length - 3));
                      DateTime d2 = dateTime2.AddSeconds(num4);
                      DateTime dateTime3 = dateTime2.AddSeconds(num5);
                      PersonInfo lpi2 = new PersonInfo();
                      lpi2.dt = d2;
                      MappingRow lenaMapping2 = this.cf.getLenaMapping(str1, d2);
                      lpi2.ubiId = lenaMapping2.UbiID;
                      lpi2.bid = lenaMapping2.BID;
                      lpi2.lenaId = str1;
                      string str3 = xmlNode2.Attributes["spkr"].Value;
                      lpi2.bd = num5 - num4;
                      bool flag = false;
                      string str4 = str3;
                      if (!(str4 == "CHN") && !(str4 == "CHF"))
                      {
                        if (!(str4 == "FAN"))
                        {
                          if (!(str4 == "MAN"))
                          {
                            if (!(str4 == "OLN"))
                            {
                              if (str4 == "NON")
                              {
                                lpi2.bd = num5 - num4;
                                lpi2.no = lpi2.bd;
                                flag = true;
                              }
                            }
                            else
                            {
                              lpi2.bd = num5 - num4;
                              lpi2.oln = lpi2.bd;
                              flag = true;
                            }
                          }
                          else
                          {
                            lpi2.ac = Convert.ToDouble(xmlNode2.Attributes["maleAdultWordCnt"].Value);
                            flag = true;
                          }
                        }
                        else
                        {
                          lpi2.ac = Convert.ToDouble(xmlNode2.Attributes["femaleAdultWordCnt"].Value);
                          flag = true;
                        }
                      }
                      else
                      {
                        ++num1;
                        double num6 = Convert.ToDouble(xmlNode2.Attributes["childUttLen"].Value.Substring(1, xmlNode2.Attributes["childUttLen"].Value.Length - 2));
                        lpi2.vc = Convert.ToDouble(xmlNode2.Attributes["childUttCnt"].Value);
                        if (num6 > 0.0)
                          textWriter.WriteLine(dateTime2.ToShortDateString() + "," + lpi2.bid + "," + lenaMapping2.type + "," + (object) num1 + ",SegmentUtt," + (object) dateTime2.Hour + ":" + (object) dateTime2.Minute + ":" + (object) dateTime2.Second + ":" + (object) dateTime2.Millisecond + "," + (object) num4 + "," + (object) num5 + "," + (object) d2.Hour + ":" + (object) d2.Minute + ":" + (object) d2.Second + ":" + (object) d2.Millisecond + "," + (object) dateTime3.Hour + ":" + (object) dateTime3.Minute + ":" + (object) dateTime3.Second + ":" + (object) dateTime3.Millisecond + "," + (object) lpi2.bd + "," + (object) num6);
                        foreach (XmlAttribute attribute in (XmlNamedNodeMap) xmlNode2.Attributes)
                        {
                          if (attribute.Name.IndexOf("startCry") == 0 || attribute.Name.IndexOf("startUtt") == 0)
                          {
                            string str5 = attribute.Name.IndexOf("startCry") == 0 ? "Cry" : "Utt";
                            string str6 = attribute.Name.Substring(8);
                            string name1 = attribute.Name;
                            double num7 = Convert.ToDouble(xmlNode2.Attributes[name1].Value.Substring(2, xmlNode2.Attributes[name1].Value.Length - 3));
                            double num8 = Convert.ToDouble(xmlNode2.Attributes["end" + str5 + str6].Value.Substring(2, xmlNode2.Attributes["end" + str5 + str6].Value.Length - 3));
                            DateTime d3 = dateTime2.AddSeconds(num7);
                            DateTime dateTime4 = dateTime2.AddSeconds(num8);
                            PersonInfo lpi3 = new PersonInfo();
                            lpi3.dt = d3;
                            MappingRow lenaMapping3 = this.cf.getLenaMapping(str1, d3);
                            lpi3.ubiId = lenaMapping3.UbiID;
                            lpi3.bid = lenaMapping3.BID;
                            lpi3.lenaId = str1;
                            lpi3.bd = num8 - num7;
                            lpi3.cry = str5 == "Cry" ? lpi3.bd : 0.0;
                            lpi3.vd = str5 == "Utt" ? lpi3.bd : 0.0;
                            this.addToRawLena(ref rl, lpi3);
                            textWriter.WriteLine(dateTime2.ToShortDateString() + "," + lpi3.bid + "," + lenaMapping3.type + "," + (object) num1 + "," + str5 + "," + (object) dateTime2.Hour + ":" + (object) dateTime2.Minute + ":" + (object) dateTime2.Second + ":" + (object) dateTime2.Millisecond + "," + (object) num7 + "," + (object) num8 + "," + (object) d3.Hour + ":" + (object) d3.Minute + ":" + (object) d3.Second + ":" + (object) d3.Millisecond + "," + (object) dateTime4.Hour + ":" + (object) dateTime4.Minute + ":" + (object) dateTime4.Second + ":" + (object) dateTime4.Millisecond + "," + (object) lpi3.bd);
                            cryInfoTest cryInfoTest = new cryInfoTest();
                            cryInfoTest.startTime = d3;
                            cryInfoTest.endTime = dateTime4;
                            cryInfoTest.secs = lpi3.cry;
                            cryInfoTest.szstart = xmlNode2.Attributes[name1].Value;
                            cryInfoTest.szend = xmlNode2.Attributes["end" + str5 + str6].Value;
                            cryInfoTest.start = num7;
                            cryInfoTest.end = num8;
                            if (!this.childCries.ContainsKey(lpi3.bid))
                              this.childCries.Add(lpi3.bid, new List<cryInfoTest>());
                            this.childCries[lpi3.bid].Add(cryInfoTest);
                          }
                        }
                        flag = true;
                      }
                      if (flag)
                        this.addToRawLena(ref rl, lpi2);
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    public void addToRawLena(ref Dictionary<string, List<PersonInfo>> rl, PersonInfo lpi)
    {
      if (!rl.ContainsKey(lpi.bid))
        rl.Add(lpi.bid, new List<PersonInfo>());
      rl[lpi.bid].Add(lpi);
    }

    public void setLenaData1(Dictionary<string, List<PersonInfo>> lenadata)
    {
      foreach (string key1 in lenadata.Keys)
      {
        List<PersonInfo> personInfoList = lenadata[key1];
        bool flag = false;
        using (List<PersonInfo>.Enumerator enumerator = personInfoList.GetEnumerator())
        {
label_14:
          if (enumerator.MoveNext())
          {
            PersonInfo current = enumerator.Current;
            DateTime key2 = new DateTime(current.dt.Year, current.dt.Month, current.dt.Day, current.dt.Hour, current.dt.Minute, current.dt.Second, 0);
            if (!flag)
            {
              this.startLenaTimes.Add(key1, key2);
              flag = true;
            }
            double num1 = 0.0;
            double bd = current.bd;
            double vc = current.vc;
            double tc = current.tc;
            double ac = current.ac;
            double no = current.no;
            double oln = current.oln;
            double num2 = tc / bd / 10.0;
            double num3 = vc / bd / 10.0;
            double num4 = ac / bd / 10.0;
            double num5 = no / bd / 10.0;
            double num6 = oln / bd / 10.0;
            if (this.personTotalCounts.ContainsKey(key1))
            {
              this.personTotalCounts[key1].vc += vc;
              this.personTotalCounts[key1].tc += tc;
              this.personTotalCounts[key1].ac += ac;
              this.personTotalCounts[key1].no += no;
              this.personTotalCounts[key1].oln += oln;
            }
            else
            {
              this.personTotalCounts.Add(key1, new PersonInfo()
              {
                vc = vc,
                tc = tc,
                ac = ac,
                no = no,
                oln = oln
              });
              this.personTotalCountsWUbi.Add(key1, new PersonInfo());
            }
            do
            {
              if (this.activities.ContainsKey(key2) && this.activities[key2].ContainsKey(key1))
              {
                this.activities[key2][key1].wasTalking = num1 > 0.0;
                this.activities[key2][key1].vc = num3;
                this.activities[key2][key1].tc = num2;
                this.activities[key2][key1].ac = num4;
                this.activities[key2][key1].oln = num6;
                this.activities[key2][key1].no = num5;
                if (this.personTotalCountsWUbi.ContainsKey(key1))
                {
                  this.personTotalCountsWUbi[key1].vc += num3;
                  this.personTotalCountsWUbi[key1].tc += num2;
                  this.personTotalCountsWUbi[key1].ac += num4;
                  this.personTotalCountsWUbi[key1].oln += num6;
                  this.personTotalCountsWUbi[key1].no += num5;
                }
              }
              key2 = key2.AddMilliseconds(100.0);
              num1 -= 0.1;
              bd -= 0.1;
            }
            while (bd > 0.0);
            goto label_14;
          }
        }
      }
    }

    public void setLenaItsData(Dictionary<string, List<PersonInfo>> lenadata)
    {
      foreach (string key1 in lenadata.Keys)
      {
        bool flag = false;
        using (List<PersonInfo>.Enumerator enumerator = lenadata[key1].GetEnumerator())
        {
label_14:
          if (enumerator.MoveNext())
          {
            PersonInfo current = enumerator.Current;
            DateTime key2 = new DateTime(current.dt.Year, current.dt.Month, current.dt.Day, current.dt.Hour, current.dt.Minute, current.dt.Second, 0);
            if (!flag)
            {
              this.startLenaTimes.Add(key1, key2);
              flag = true;
            }
            double vd = current.vd;
            double bd = current.bd;
            double vc = current.vc;
            double tc = current.tc;
            double ac = current.ac;
            double no = current.no;
            double oln = current.oln;
            double cry = current.cry;
            double num1 = tc / bd / 10.0;
            double num2 = vc / bd / 10.0;
            double num3 = vd / bd / 10.0;
            double num4 = ac / bd / 10.0;
            double num5 = no / bd / 10.0;
            double num6 = oln / bd / 10.0;
            double num7 = cry / bd / 10.0;
            if (this.personTotalCounts.ContainsKey(key1))
            {
              this.personTotalCounts[key1].vd += vd;
              this.personTotalCounts[key1].vc += vc;
              this.personTotalCounts[key1].tc += tc;
              this.personTotalCounts[key1].ac += ac;
              this.personTotalCounts[key1].no += no;
              this.personTotalCounts[key1].oln += oln;
              this.personTotalCounts[key1].cry += cry;
            }
            else
            {
              this.personTotalCounts.Add(key1, new PersonInfo()
              {
                vd = vd,
                vc = vc,
                tc = tc,
                ac = ac,
                no = no,
                oln = oln,
                cry = cry
              });
              this.personTotalCountsWUbi.Add(key1, new PersonInfo());
            }
            do
            {
              if (this.activities.ContainsKey(key2) && this.activities[key2].ContainsKey(key1))
              {
                this.activities[key2][key1].wasTalking = vd > 0.0 || this.activities[key2][key1].wasTalking;
                this.activities[key2][key1].isCrying = cry > 0.0 || this.activities[key2][key1].isCrying;
                this.activities[key2][key1].vc += num2;
                this.activities[key2][key1].tc += num1;
                this.activities[key2][key1].vd += num3;
                this.activities[key2][key1].ac += num4;
                this.activities[key2][key1].oln += num6;
                this.activities[key2][key1].no += num5;
                this.activities[key2][key1].cry += num7;
                if (this.personTotalCountsWUbi.ContainsKey(key1))
                {
                  this.personTotalCountsWUbi[key1].vc += num2;
                  this.personTotalCountsWUbi[key1].tc += num1;
                  this.personTotalCountsWUbi[key1].vd += num3;
                  this.personTotalCountsWUbi[key1].ac += num4;
                  this.personTotalCountsWUbi[key1].oln += num6;
                  this.personTotalCountsWUbi[key1].no += num5;
                  this.personTotalCountsWUbi[key1].cry += num7;
                }
              }
              key2 = key2.AddMilliseconds(100.0);
              vd -= 0.1;
              bd -= 0.1;
              cry -= 0.1;
            }
            while (bd > 0.0);
            goto label_14;
          }
        }
      }
    }

    public void setLenaData2(Dictionary<string, List<PersonInfo>> lenadata)
    {
      foreach (string key1 in lenadata.Keys)
      {
        using (List<PersonInfo>.Enumerator enumerator = lenadata[key1].GetEnumerator())
        {
label_12:
          if (enumerator.MoveNext())
          {
            PersonInfo current = enumerator.Current;
            DateTime key2 = new DateTime(current.dt.Year, current.dt.Month, current.dt.Day, current.dt.Hour, current.dt.Minute, current.dt.Second, 0);
            double vd = current.vd;
            double bd = current.bd;
            double vc = current.vc;
            double tc = current.tc;
            double ac = current.ac;
            double no = current.no;
            double oln = current.oln;
            double num1 = tc / bd / 10.0;
            double num2 = vc / bd / 10.0;
            double num3 = vd / bd / 10.0;
            double num4 = ac / bd / 10.0;
            double num5 = no / bd / 10.0;
            double num6 = oln / bd / 10.0;
            if (this.personTotalCounts.ContainsKey(key1))
            {
              this.personTotalCounts[key1].vd += vd;
              this.personTotalCounts[key1].vc += vc;
              this.personTotalCounts[key1].tc += tc;
              this.personTotalCounts[key1].ac += ac;
              this.personTotalCounts[key1].no += no;
              this.personTotalCounts[key1].oln += oln;
            }
            else
            {
              this.personTotalCounts.Add(key1, new PersonInfo()
              {
                vd = vd,
                vc = vc,
                tc = tc,
                ac = ac,
                no = no,
                oln = oln
              });
              this.personTotalCountsWUbi.Add(key1, new PersonInfo());
            }
            do
            {
              if (this.activities.ContainsKey(key2) && this.activities[key2].ContainsKey(key1))
              {
                this.activities[key2][key1].wasTalking = vd > 0.0;
                this.activities[key2][key1].vc = num2;
                this.activities[key2][key1].tc = num1;
                this.activities[key2][key1].vd = num3;
                this.activities[key2][key1].ac = num4;
                this.activities[key2][key1].oln = num6;
                this.activities[key2][key1].no = num5;
                if (this.personTotalCountsWUbi.ContainsKey(key1))
                {
                  this.personTotalCountsWUbi[key1].vc += num2;
                  this.personTotalCountsWUbi[key1].tc += num1;
                  this.personTotalCountsWUbi[key1].vd += num3;
                  this.personTotalCountsWUbi[key1].ac += num4;
                  this.personTotalCountsWUbi[key1].oln += num6;
                  this.personTotalCountsWUbi[key1].no += num5;
                }
              }
              key2 = key2.AddMilliseconds(100.0);
              vd -= 0.1;
              bd -= 0.1;
            }
            while (bd > 0.0);
            goto label_12;
          }
        }
      }
    }
  }
}
