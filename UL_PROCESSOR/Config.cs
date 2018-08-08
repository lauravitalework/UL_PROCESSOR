// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.Config
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;

namespace UL_PROCESSOR
{
  internal class Config
  {
    public string version = "V18";
    public string root = "C://LVL/";
    public string classroom = "APPLETREE";
    public string freePlayTimesFile = "/FREEPLAYTIMES.CSV";
    public string extractTimesFile = "//EXTRACTTIMES.CSV";
    public string mappingFile = "/MAPPING";
    public string adjustFile = "/ADJUST";
    public string ubisenseFile = "MiamiChild.";
    public string ubisenseTagsFile = "MiamiLocation.";
    public string ubisenseFileDir = "/Ubisense_Data/FROMSUPER/";
    public string lenaFileDir = "/LENA_Data/";
    public string lenaFile = "/LENA_Data/ACTIVITYBLOCKS/LENAACTIVITYBLOCKALL.csv";
    public string syncFilePre = "/SYNC/MERGED";
    public string lenaVersion = "PRO";
    public bool justFreePlay = false;
    public string lenaDateType = "utc";
    public DateTime from = new DateTime(2017, 3, 31);
    public DateTime to = new DateTime(2017, 4, 1);
    public int mappingBIdCol = 3;
    public int mappingUbiIdCol = 4;
    public int mappingLenaIdCol = 9;
    public int mappingStartCol = 11;
    public int mappingExpiredCol = 12;
    public int mappingAbsentCol = 13;
    public int mappingAidCol = 14;
    public int mappingSexCol = 15;
    public int mappingTypeCol = 17;
    public int lenaFileIdCol = 16;
    public int lenaFileDateCol = 42;
    public int lenaFileVdCol = 22;
    public int lenaFileVcCol = 20;
    public int lenaFileTcCol = 19;
    public int lenaFileBdCol = 45;
    public int lenaFileNoCol = 33;
    public int lenaFileAcCol = 18;
    public int lenaFileOlnCol = 31;
    public int ubiFileIdCol = 1;
    public int ubiFileDateCol = 2;
    public int ubiDataFileCols = 8;
    public int ubiFileXPosCol = 3;
    public int ubiFileYPosCol = 4;
    public int ubiFileZPosCol = 5;
    public int ubiFileOriPosCol = 6;
    public Dictionary<string, List<MappingRow>> mapRows = new Dictionary<string, List<MappingRow>>();
    public Dictionary<string, List<MappingRow>> mapRowsUbiL = new Dictionary<string, List<MappingRow>>();
    public Dictionary<string, List<MappingRow>> mapRowsUbiR = new Dictionary<string, List<MappingRow>>();
    public Dictionary<string, List<MappingRow>> mapRowsUbi = new Dictionary<string, List<MappingRow>>();
    public Dictionary<string, List<MappingRow>> mapRowsLena = new Dictionary<string, List<MappingRow>>();
    public List<string> bids = new List<string>();
    public List<string> pairs = new List<string>();
    public Dictionary<string, List<string>> freePlayTimes = new Dictionary<string, List<string>>();
    public Dictionary<string, List<string>> extractTimes = new Dictionary<string, List<string>>();
    public Dictionary<string, bool> tagTest = new Dictionary<string, bool>();
    public Dictionary<DateTime, Dictionary<string, double>> adjustedTimes = new Dictionary<DateTime, Dictionary<string, double>>();

    public Config(string r, string c)
    {
      this.classroom = c;
      this.root = r;
      this.freePlayTimesFile = this.root + this.classroom + this.freePlayTimesFile;
      this.extractTimesFile = this.root + this.classroom + this.extractTimesFile;
      this.mappingFile = this.root + this.classroom + this.mappingFile + "_" + this.classroom + ".CSV";
      this.adjustFile = this.root + this.classroom + this.adjustFile + "_" + this.classroom + ".csv";
      this.ubisenseFileDir = this.root + this.classroom + this.ubisenseFileDir;
      this.lenaFileDir = this.root + this.classroom + this.lenaFileDir;
      this.lenaFile = this.root + this.classroom + this.lenaFile;
      this.syncFilePre = this.root + this.classroom + this.syncFilePre;
    }

    public Config()
    {
      this.freePlayTimesFile = this.root + this.classroom + this.freePlayTimesFile;
      this.extractTimesFile = this.root + this.classroom + this.extractTimesFile;
      this.mappingFile = this.root + this.classroom + "_" + this.classroom + ".CSV";
      this.adjustFile = this.root + this.classroom + this.adjustFile + "_" + this.classroom + ".csv";
      this.ubisenseFileDir = this.root + this.classroom + this.ubisenseFileDir;
      this.lenaFileDir = this.root + this.classroom + this.lenaFileDir;
      this.lenaFile = this.root + this.classroom + this.lenaFile;
      this.syncFilePre = this.root + this.classroom + this.syncFilePre;
    }

    public void readMappings()
    {
      using (StreamReader streamReader = new StreamReader(this.mappingFile))
      {
        streamReader.ReadLine();
        while (!streamReader.EndOfStream)
        {
          try
          {
            string[] strArray = streamReader.ReadLine().Split(',');
            if (strArray[7].Trim() == "00:11:CE:00:00:00:02:BE")
              ;
            if (strArray.Length > 8 && (strArray[this.mappingUbiIdCol].Trim() != "" && strArray[this.mappingLenaIdCol].Trim() != ""))
            {
              MappingRow mappingRow = new MappingRow();
              mappingRow.LenaId = strArray[this.mappingLenaIdCol].Trim();
              mappingRow.UbiID = strArray[this.mappingUbiIdCol].Trim();
              mappingRow.BID = strArray[this.mappingBIdCol].Trim();
              if (!this.bids.Contains(mappingRow.BID))
                this.bids.Add(mappingRow.BID);
              mappingRow.Expiration = Convert.ToDateTime(strArray[this.mappingExpiredCol]);
              mappingRow.Start = Convert.ToDateTime(strArray[this.mappingStartCol]);
              mappingRow.absences = this.getAbsentDays(strArray[this.mappingAbsentCol]);
              mappingRow.aid = strArray[this.mappingAidCol].Trim();
              mappingRow.sex = strArray[this.mappingSexCol].Trim();
              mappingRow.leftTag = strArray[5].Trim();
              mappingRow.rightTag = strArray[7].Trim();
              mappingRow.sex = strArray[this.mappingSexCol].Trim();
              mappingRow.aid = strArray[this.mappingAidCol].Trim();
              mappingRow.type = strArray[this.mappingTypeCol].Trim();
              if (!this.mapRowsUbi.ContainsKey(mappingRow.UbiID))
                this.mapRowsUbi.Add(mappingRow.UbiID, new List<MappingRow>());
              if (!this.mapRowsLena.ContainsKey(mappingRow.LenaId))
                this.mapRowsLena.Add(mappingRow.LenaId, new List<MappingRow>());
              if (!this.mapRowsUbiL.ContainsKey(mappingRow.leftTag))
                this.mapRowsUbiL.Add(mappingRow.leftTag, new List<MappingRow>());
              if (!this.mapRowsUbiR.ContainsKey(mappingRow.rightTag))
                this.mapRowsUbiR.Add(mappingRow.rightTag, new List<MappingRow>());
              if (!this.mapRows.ContainsKey(mappingRow.BID))
                this.mapRows.Add(mappingRow.BID, new List<MappingRow>());
              this.mapRowsUbi[mappingRow.UbiID].Add(mappingRow);
              this.mapRowsUbiL[mappingRow.leftTag].Add(mappingRow);
              this.mapRowsUbiR[mappingRow.rightTag].Add(mappingRow);
              this.mapRowsLena[mappingRow.LenaId].Add(mappingRow);
              this.mapRows[mappingRow.BID].Add(mappingRow);
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      for (int index1 = 0; index1 < this.bids.Count; ++index1)
      {
        string bid1 = this.bids[index1];
        for (int index2 = index1 + 1; index2 < this.bids.Count; ++index2)
        {
          string bid2 = this.bids[index2];
          this.pairs.Add(bid1 + "-" + bid2);
        }
      }
      using (StreamReader streamReader = new StreamReader(this.freePlayTimesFile))
      {
        streamReader.ReadLine();
        while (!streamReader.EndOfStream)
        {
          try
          {
            string[] strArray = streamReader.ReadLine().Split(',');
            if (strArray.Length > 2)
            {
              string key = Convert.ToDateTime(strArray[0]).ToShortDateString().Trim();
              string str1 = strArray[1];
              char[] chArray = new char[1]{ '|' };
              foreach (string str2 in str1.Split(chArray))
              {
                if (str2.Split('-').Length == 2)
                {
                  List<string> stringList = new List<string>();
                  if (!this.freePlayTimes.ContainsKey(key))
                  {
                    stringList.Add(str2);
                    this.freePlayTimes.Add(key, stringList);
                  }
                  else
                    this.freePlayTimes[key].Add(str2);
                }
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      if (File.Exists(this.extractTimesFile))
      {
        using (StreamReader streamReader = new StreamReader(this.extractTimesFile))
        {
          streamReader.ReadLine();
          while (!streamReader.EndOfStream)
          {
            try
            {
              string[] strArray = streamReader.ReadLine().Split(',');
              if (strArray.Length > 2)
              {
                string key = Convert.ToDateTime(strArray[0]).ToShortDateString().Trim();
                string str1 = strArray[1];
                char[] chArray = new char[1]{ '|' };
                foreach (string str2 in str1.Split(chArray))
                {
                  if (str2.Split('-').Length == 2)
                  {
                    List<string> stringList = new List<string>();
                    if (!this.extractTimes.ContainsKey(key))
                    {
                      stringList.Add(str2);
                      this.extractTimes.Add(key, stringList);
                    }
                    else
                      this.extractTimes[key].Add(str2);
                  }
                }
              }
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
      this.getAdjustedTimes();
    }

    public void getAdjustedTimes()
    {
      if (!File.Exists(this.adjustFile))
        return;
      using (StreamReader streamReader = new StreamReader(this.adjustFile))
      {
        streamReader.ReadLine();
        while (!streamReader.EndOfStream)
        {
          try
          {
            string[] strArray = streamReader.ReadLine().Split(',');
            if (strArray.Length > 5)
            {
              string key = strArray[0].Substring(17, 6);
              if (key.Substring(0, 2) == "00")
                key = key.Substring(2);
              else if (key.Substring(0, 1) == "0")
                key = key.Substring(1);
              DateTime dateTime = Convert.ToDateTime(strArray[1]);
              double num = Convert.ToDouble(strArray[4]);
              if (!this.adjustedTimes.ContainsKey(dateTime))
                this.adjustedTimes.Add(dateTime, new Dictionary<string, double>());
              this.adjustedTimes[dateTime].Add(key, num);
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
    }

    public MappingRow getMapping(string bid, DateTime d)
    {
      MappingRow mappingRow1 = new MappingRow();
      if (this.mapRows.ContainsKey(bid))
      {
        foreach (MappingRow mappingRow2 in this.mapRows[bid])
        {
          if (d.CompareTo(mappingRow2.Expiration) < 0 && d.CompareTo(mappingRow2.Start) >= 0)
          {
            mappingRow1 = mappingRow2;
            break;
          }
        }
      }
      return mappingRow1;
    }

    public MappingRow getUbiMapping(string ubiId, DateTime d)
    {
      MappingRow mappingRow1 = new MappingRow();
      if (this.mapRowsUbi.ContainsKey(ubiId))
      {
        foreach (MappingRow mappingRow2 in this.mapRowsUbi[ubiId])
        {
          if (d.CompareTo(mappingRow2.Expiration) < 0 && d.CompareTo(mappingRow2.Start) >= 0)
          {
            mappingRow1 = mappingRow2;
            break;
          }
        }
      }
      return mappingRow1;
    }

    public MappingRow getUbiMappingL(string ubiId, DateTime d)
    {
      MappingRow mappingRow1 = new MappingRow();
      if (this.mapRowsUbiL.ContainsKey(ubiId))
      {
        foreach (MappingRow mappingRow2 in this.mapRowsUbiL[ubiId])
        {
          if (d.CompareTo(mappingRow2.Expiration) < 0 && d.CompareTo(mappingRow2.Start) >= 0)
          {
            mappingRow1 = mappingRow2;
            break;
          }
        }
      }
      return mappingRow1;
    }

    public MappingRow getUbiMappingR(string ubiId, DateTime d)
    {
      MappingRow mappingRow1 = new MappingRow();
      if (this.mapRowsUbiR.ContainsKey(ubiId))
      {
        foreach (MappingRow mappingRow2 in this.mapRowsUbiR[ubiId])
        {
          if (d.CompareTo(mappingRow2.Expiration) < 0 && d.CompareTo(mappingRow2.Start) >= 0)
          {
            mappingRow1 = mappingRow2;
            break;
          }
        }
      }
      return mappingRow1;
    }

    public MappingRow getLenaMapping(string lenaId, DateTime d)
    {
      MappingRow mappingRow1 = new MappingRow();
      if (this.mapRowsLena.ContainsKey(lenaId))
      {
        foreach (MappingRow mappingRow2 in this.mapRowsLena[lenaId])
        {
          if (d.CompareTo(mappingRow2.Expiration) < 0 && d.CompareTo(mappingRow2.Start) >= 0)
          {
            mappingRow1 = mappingRow2;
            break;
          }
        }
      }
      return mappingRow1;
    }

    public List<DateTime> getAbsentDays(string absentDays)
    {
      List<DateTime> dateTimeList = new List<DateTime>();
      string str1 = absentDays;
      char[] chArray = new char[1]{ '|' };
      foreach (string str2 in str1.Split(chArray))
      {
        if (str2.Trim() != "")
          dateTimeList.Add(Convert.ToDateTime(str2));
      }
      return dateTimeList;
    }

    public bool isThisFreePlay(DateTime date)
    {
      bool flag = false;
      DateTime dateTime1 = date.AddHours((double) -date.Hour);
      dateTime1 = dateTime1.AddMinutes((double) -date.Minute);
      string key = dateTime1.AddSeconds((double) -date.Second).ToShortDateString().Trim();
      if (this.freePlayTimes.ContainsKey(key))
      {
        List<string> freePlayTime = this.freePlayTimes[key];
        foreach (string str in this.freePlayTimes[key])
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
  }
}
