// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.Interactions
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UL_PROCESSOR
{
  internal class Interactions
  {
    public Dictionary<string, PairInfo> pairInteractions = new Dictionary<string, PairInfo>();

    public void countInteractions(Config cf, ClassroomDay day, bool writeFile, bool writeDetailFile, DateTime trunk, Dictionary<string, List<PersonInfo>> lenaInfo)
    {
      Dictionary<DateTime, Dictionary<string, PersonInfo>> activities = day.activities;
      TextWriter textWriter = (TextWriter) null;
      if (writeDetailFile)
        textWriter = (TextWriter) new StreamWriter(cf.root + cf.classroom + "/SYNC/" + cf.version + "interaction_angles_xy_output_trunk_" + (cf.justFreePlay ? (object) "freeplay_" : (object) "") + (object) day.day.Month + "_" + (object) day.day.Day + "_" + (object) day.day.Year + ".csv");
      if (writeDetailFile)
        textWriter.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
      foreach (KeyValuePair<DateTime, Dictionary<string, PersonInfo>> keyValuePair in (IEnumerable<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>>) activities.OrderBy<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>((Func<KeyValuePair<DateTime, Dictionary<string, PersonInfo>>, DateTime>) (key => key.Key)))
      {
        DateTime key1 = keyValuePair.Key;
        if (key1.CompareTo(trunk) <= 0 && (!cf.justFreePlay || cf.isThisFreePlay(key1)))
        {
          foreach (string key2 in activities[key1].Keys)
          {
            foreach (string key3 in activities[key1].Keys)
            {
              if (key2 != key3)
              {
                string key4 = "";
                PairInfo pairInfo = new PairInfo();
                if (cf.pairs.Contains(key4))
                {
                  key4 = key2 + "-" + key3;
                  pairInfo.szSubject = key2;
                  pairInfo.szPartner = key3;
                }
                else if (cf.pairs.Contains(key3 + "-" + key2))
                {
                  key4 = key3 + "-" + key2;
                  pairInfo.szSubject = key3;
                  pairInfo.szPartner = key2;
                }
                if (key4 != "")
                {
                  if (!this.pairInteractions.ContainsKey(key4))
                    this.pairInteractions.Add(key4, pairInfo);
                  this.pairInteractions[key4].sharedTimeInSecs += 0.1;
                  if (ClassroomDay.calcSquaredDist(activities[key1][key2], activities[key1][key3]) <= ClassroomDay.minDistance)
                  {
                    this.pairInteractions[key4].closeTimeInSecs += 0.1;
                    Tuple<double, double> tuple = ClassroomDay.withinOrientationData(activities[key1][key3], activities[key1][key2]);
                    bool flag = Math.Abs(tuple.Item1) <= 45.0 && Math.Abs(tuple.Item2) <= 45.0;
                    if (writeDetailFile)
                      textWriter.WriteLine(key2 + "," + key3 + "," + key1.ToLongTimeString() + ",0.1," + (flag ? (object) "0.1," : (object) "0,") + (object) tuple.Item1 + "," + (object) tuple.Item2 + "," + (object) activities[key1][key2].lx + "," + (object) activities[key1][key2].ly + "," + (object) activities[key1][key2].rx + "," + (object) activities[key1][key2].ry + "," + (object) activities[key1][key3].lx + "," + (object) activities[key1][key3].ly + "," + (object) activities[key1][key3].rx + "," + (object) activities[key1][key3].ry);
                    if (flag)
                    {
                      this.pairInteractions[key4].closeAndOrientedTimeInSecs += 0.1;
                      this.addPersonInfo(activities[key1][pairInfo.szSubject], ref this.pairInteractions[key4].subject);
                      this.addPersonInfo(activities[key1][pairInfo.szPartner], ref this.pairInteractions[key4].partner);
                      if (activities[key1][pairInfo.szSubject].cry > 0.0 && activities[key1][pairInfo.szPartner].cry > 0.0)
                        this.pairInteractions[key4].closeAndOrientedCryInSecs += 0.1;
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    public void addPersonInfo(PersonInfo personInfo, ref PersonInfo target)
    {
      target.vd += personInfo.vd;
      target.vc += personInfo.vc;
      target.tc += personInfo.tc;
      target.bd += personInfo.bd;
      target.no += personInfo.no;
      target.oln += personInfo.oln;
      target.ac += personInfo.ac;
      target.cry += personInfo.cry;
      if (personInfo.cry < 0.2)
        return;
      personInfo = personInfo;
    }
  }
}
