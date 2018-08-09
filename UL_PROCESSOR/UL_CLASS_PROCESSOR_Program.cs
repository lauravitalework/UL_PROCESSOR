// Decompiled with JetBrains decompiler
// Type: UL_PROCESSOR.UL_CLASS_PROCESSOR_Program
// Assembly: UL_PROCESSOR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F5FBFFAF-2271-40AA-9C8E-13AEE0DF74F3
// Assembly location: C:\Users\Psychology\Documents\Visual Studio 2015\Projects\UL_PROCESSOR\UL_PROCESSOR\bin\Debug\UL_PROCESSOR.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UL_PROCESSOR
{
    internal class UL_CLASS_PROCESSOR_Program
    {
        public bool doSummary = true;
        public string fileName = "V1";
        public bool doTenFiles = true;
        public bool doSumDayFiles = true;
        public bool doSumAllFiles = true;
        public bool doAngleFiles = true;
        public bool getFromIts = false;
        public List<string> fileNames = new List<string>();
        public bool doTotals = false;
        public Config configInfo;
        public List<DateTime> days;

        public UL_CLASS_PROCESSOR_Program(Config c, List<DateTime> d, string fName, bool t, bool sd, bool s, bool a, bool its)
        {
            this.doTenFiles = t;
            this.doSumDayFiles = sd;
            this.doSumAllFiles = s;
            this.doAngleFiles = a;
            this.configInfo = c;
            this.days = d;
            this.configInfo.readMappings();
            this.fileName = fName;
            this.configInfo.version = this.fileName;
            this.getFromIts = its;
        }

        public UL_CLASS_PROCESSOR_Program(Config c, List<DateTime> d)
        {
            this.configInfo = c;
            this.days = d;
            this.configInfo.readMappings();
        }

        public void process(bool processLast)
        {
            List<ClassroomDay> cds = new List<ClassroomDay>();
            int c = 0;
            foreach (DateTime day1 in this.days)
            {
                ClassroomDay day2 = new ClassroomDay(day1, this.configInfo);
                Console.WriteLine("PROCESSING " + this.configInfo.classroom + " " + day1.ToShortDateString());
                string[] strArray1 = new string[5]
                {
          "readUbiFile (",
          null,
          null,
          null,
          null
                };
                int index1 = 1;
                DateTime now = DateTime.Now;
                string longDateString1 = now.ToLongDateString();
                strArray1[index1] = longDateString1;
                strArray1[2] = " ";
                int index2 = 3;
                now = DateTime.Now;
                string longTimeString1 = now.ToLongTimeString();
                strArray1[index2] = longTimeString1;
                strArray1[4] = "):";
                Console.WriteLine(string.Concat(strArray1));
                Dictionary<string, List<PersonInfo>> rawData = day2.readUbiFile();
                string[] strArray2 = new string[5]
                {
          "readUbiTagFile (",
          null,
          null,
          null,
          null
                };
                int index3 = 1;
                now = DateTime.Now;
                string longDateString2 = now.ToLongDateString();
                strArray2[index3] = longDateString2;
                strArray2[2] = " ";
                int index4 = 3;
                now = DateTime.Now;
                string longTimeString2 = now.ToLongTimeString();
                strArray2[index4] = longTimeString2;
                strArray2[4] = "):";
                Console.WriteLine(string.Concat(strArray2));
                Tuple<Dictionary<string, List<PersonInfo>>, Dictionary<string, List<PersonInfo>>> tuple = day2.readUbiTagFile();
                string[] strArray3 = new string[5]
                {
          "readLenaFile (",
          null,
          null,
          null,
          null
                };
                int index5 = 1;
                now = DateTime.Now;
                string longDateString3 = now.ToLongDateString();
                strArray3[index5] = longDateString3;
                strArray3[2] = " ";
                int index6 = 3;
                now = DateTime.Now;
                string longTimeString3 = now.ToLongTimeString();
                strArray3[index6] = longTimeString3;
                strArray3[4] = "):";
                Console.WriteLine(string.Concat(strArray3));
                Console.WriteLine("getFromIts " + this.getFromIts.ToString());
                Dictionary<string, List<PersonInfo>> dictionary = this.getFromIts ? day2.readLenaItsFiles() : day2.readLenaFile();
                string[] strArray4 = new string[5]
                {
          "setUbiData (",
          null,
          null,
          null,
          null
                };
                int index7 = 1;
                now = DateTime.Now;
                string longDateString4 = now.ToLongDateString();
                strArray4[index7] = longDateString4;
                strArray4[2] = " ";
                int index8 = 3;
                now = DateTime.Now;
                string longTimeString4 = now.ToLongTimeString();
                strArray4[index8] = longTimeString4;
                strArray4[4] = "):";
                Console.WriteLine(string.Concat(strArray4));
                day2.setUbiData(rawData);
                string[] strArray5 = new string[5]
                {
          "setUbiTagData (",
          null,
          null,
          null,
          null
                };
                int index9 = 1;
                now = DateTime.Now;
                string longDateString5 = now.ToLongDateString();
                strArray5[index9] = longDateString5;
                strArray5[2] = " ";
                int index10 = 3;
                now = DateTime.Now;
                string longTimeString5 = now.ToLongTimeString();
                strArray5[index10] = longTimeString5;
                strArray5[4] = "):";
                Console.WriteLine(string.Concat(strArray5));
                day2.setUbiTagData(tuple.Item1, tuple.Item2);
                string[] strArray6 = new string[5]
                {
          "setLenaData (",
          null,
          null,
          null,
          null
                };
                int index11 = 1;
                now = DateTime.Now;
                string longDateString6 = now.ToLongDateString();
                strArray6[index11] = longDateString6;
                strArray6[2] = " ";
                int index12 = 3;
                now = DateTime.Now;
                string longTimeString6 = now.ToLongTimeString();
                strArray6[index12] = longTimeString6;
                strArray6[4] = "):";
                Console.WriteLine(string.Concat(strArray6));
                DateTime trunkTime = day2.getTrunkTime();
                if (this.getFromIts)
                {
                    day2.setLenaItsData(dictionary);
                    string[] strArray7 = new string[5]
                    {
            "setLenaData (",
            null,
            null,
            null,
            null
                    };
                    int index13 = 1;
                    now = DateTime.Now;
                    string longDateString7 = now.ToLongDateString();
                    strArray7[index13] = longDateString7;
                    strArray7[2] = " ";
                    int index14 = 3;
                    now = DateTime.Now;
                    string longTimeString7 = now.ToLongTimeString();
                    strArray7[index14] = longTimeString7;
                    strArray7[4] = "):";
                    Console.WriteLine(string.Concat(strArray7));
                }
                else
                {
                    day2.setLenaData(dictionary);
                    string[] strArray7 = new string[5]
                    {
            "setLenaData (",
            null,
            null,
            null,
            null
                    };
                    int index13 = 1;
                    now = DateTime.Now;
                    string longDateString7 = now.ToLongDateString();
                    strArray7[index13] = longDateString7;
                    strArray7[2] = " ";
                    int index14 = 3;
                    now = DateTime.Now;
                    string longTimeString7 = now.ToLongTimeString();
                    strArray7[index14] = longTimeString7;
                    strArray7[4] = "):";
                    Console.WriteLine(string.Concat(strArray7));
                }
                string[] strArray8 = new string[5]
                {
          "countInteractions  INTS (",
          null,
          null,
          null,
          null
                };
                int index15 = 1;
                now = DateTime.Now;
                string longDateString8 = now.ToLongDateString();
                strArray8[index15] = longDateString8;
                strArray8[2] = " ";
                int index16 = 3;
                now = DateTime.Now;
                string longTimeString8 = now.ToLongTimeString();
                strArray8[index16] = longTimeString8;
                strArray8[4] = "):";
                Console.WriteLine(string.Concat(strArray8));
                day2.ints.countInteractions(this.configInfo, day2, this.doAngleFiles, this.doAngleFiles, trunkTime, dictionary);
                string[] strArray9 = new string[5]
                {
          "countInteractions (",
          null,
          null,
          null,
          null
                };
                int index17 = 1;
                now = DateTime.Now;
                string longDateString9 = now.ToLongDateString();
                strArray9[index17] = longDateString9;
                strArray9[2] = " ";
                int index18 = 3;
                now = DateTime.Now;
                string longTimeString9 = now.ToLongTimeString();
                strArray9[index18] = longTimeString9;
                strArray9[4] = "):";
                Console.WriteLine(string.Concat(strArray9));
                day2.countInteractions(this.doAngleFiles, this.doAngleFiles, true, trunkTime, dictionary);
                if (this.doTenFiles)
                {
                    string[] strArray7 = new string[5]
                    {
            "write10SecTalkingCSV (",
            null,
            null,
            null,
            null
                    };
                    int index13 = 1;
                    now = DateTime.Now;
                    string longDateString7 = now.ToLongDateString();
                    strArray7[index13] = longDateString7;
                    strArray7[2] = " ";
                    int index14 = 3;
                    now = DateTime.Now;
                    string longTimeString7 = now.ToLongTimeString();
                    strArray7[index14] = longTimeString7;
                    strArray7[4] = "):";
                    Console.WriteLine(string.Concat(strArray7));
                    string file_name = this.configInfo.root + this.configInfo.classroom + "/SYNC/" + this.configInfo.version + "10THOFSECTALKING_" + (this.configInfo.justFreePlay ? (object)"freeplay_" : (object)"") + (day2.startFromLena ? (object)"wlena_" : (object)"wolena_") + (object)day1.Month + "_" + (object)day1.Day + "_" + (object)day1.Year + ".CSV";
                    day2.write10SecTalkingCSV(file_name);
                }
                cds.Add(day2);
            }
            this.summarize(cds, c, processLast);
        }

        public void getOnsets(string name, int count)
        {
            foreach (DateTime day in this.days)
            {
                ClassroomDay classroomDay = new ClassroomDay(day, this.configInfo);
                Console.WriteLine("PROCESSING " + this.configInfo.classroom + " " + day.ToShortDateString());
                classroomDay.writeLenaItsOnsets(count == 0, name);
            }
        }

        public void summarize(List<ClassroomDay> cds, int c, bool processLast)
        {
            if (!this.doSumAllFiles && !this.doSumDayFiles)
                return;
            foreach (ClassroomDay cd in cds)
            {
                try
                {
                    Console.WriteLine("Summarizing " + (object)cd.day.Month + " " + (object)cd.day.Year + " " + (object)cd.day.Day + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    List<ClassroomDay> days = new List<ClassroomDay>();
                    days.Add(cd);
                    Console.WriteLine("Summarizing ONE");
                    if (this.doSumDayFiles)
                    {
                        if (this.doTotals)
                            UL_CLASS_PROCESSOR_Program.writeDataSummaryTotals(false, this.configInfo.root + this.configInfo.classroom + "/SYNC/" + this.configInfo.version + "PAIRACTIVITY_" + c.ToString() + "_" + this.configInfo.classroom + "_" + (cd.justProx ? (object)"no_" : (object)"") + (this.configInfo.justFreePlay ? (object)"freeplay_" : (object)"") + (object)cd.day.Month + "_" + (object)cd.day.Day + "_" + (object)cd.day.Year + ".csv", days, true);
                        else
                            UL_CLASS_PROCESSOR_Program.writeDataSummary((this.getFromIts ? 1 : 0) != 0, false, this.configInfo.root + this.configInfo.classroom + "/SYNC/" + this.configInfo.version + "PAIRACTIVITY_" + c.ToString() + "_" + this.configInfo.classroom + "_" + (cd.justProx ? (object)"no_" : (object)"") + (this.configInfo.justFreePlay ? (object)"freeplay_" : (object)"") + (object)cd.day.Month + "_" + (object)cd.day.Day + "_" + (object)cd.day.Year + ".csv", days, true);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            string[] strArray = new string[5]
            {
        "Summarizing ALL  (",
        null,
        null,
        null,
        null
            };
            int index1 = 1;
            DateTime now = DateTime.Now;
            string longDateString = now.ToLongDateString();
            strArray[index1] = longDateString;
            strArray[2] = " ";
            int index2 = 3;
            now = DateTime.Now;
            string longTimeString = now.ToLongTimeString();
            strArray[index2] = longTimeString;
            strArray[4] = "):";
            Console.WriteLine(string.Concat(strArray));
            if (this.doSumAllFiles)
            {
                string file_name = this.configInfo.root + this.configInfo.classroom + "/SYNC/" + this.configInfo.version + "PAIRACTIVITY_ALL_" + this.configInfo.classroom + (this.configInfo.justFreePlay ? "_freeplay" : "") + ".CSV";
                if (this.doTotals)
                {
                    file_name = file_name.Replace(".", "_TOTALS.");
                    UL_CLASS_PROCESSOR_Program.writeDataSummaryTotals(c > 0, file_name, cds, processLast);
                }
                else
                    UL_CLASS_PROCESSOR_Program.writeDataSummary(this.getFromIts, c > 0, file_name, cds, processLast);
                this.fileNames.Add(file_name);
            }
        }

        public static Tuple<string, string, string> getMetrics(string prefix, string prefixp, PersonInfo info, PersonInfo infop, bool absent, bool absentp)
        {
            string str1 = "";
            string str2 = "";
            string str3 = "";
            string str4 = str1 + prefix + "Voc Dur,";
            string str5 = str2 + (absent ? "NA" : info.vd.ToString() + ",");
            string str6 = str4 + prefixp + "Voc Dur,";
            string str7 = str5 + (absentp ? "NA" : infop.vd.ToString() + ",");
            string str8 = str3 + (absentp ? "NA" : infop.vd.ToString() + ",") + (absent ? "NA" : info.vd.ToString() + ",");
            string str9 = str6 + prefix + "Voc Count,";
            string str10 = str7 + (absent ? "NA" : info.vc.ToString() + ",");
            string str11 = str9 + prefixp + "Voc Count,";
            string str12 = str10 + (absentp ? "NA" : infop.vc.ToString() + ",");
            string str13 = str8 + (absentp ? "NA" : infop.vc.ToString() + ",") + (absent ? "NA" : info.vc.ToString() + ",");
            string str14 = str11 + prefix + "Turn Count,";
            string str15 = str12 + (absent ? "NA" : info.tc.ToString() + ",");
            string str16 = str14 + prefixp + "Turn Count,";
            string str17 = str15 + (absentp ? "NA" : infop.tc.ToString() + ",");
            string str18 = str13 + (absentp ? "NA" : infop.tc.ToString() + ",") + (absent ? "NA" : info.tc.ToString() + ",");
            string str19 = str16 + prefix + "Adult Count,";
            string str20 = str17 + (absent ? "NA" : info.ac.ToString() + ",");
            string str21 = str19 + prefixp + "Adult Count,";
            string str22 = str20 + (absentp ? "NA" : infop.ac.ToString() + ",");
            string str23 = str18 + (absentp ? "NA" : infop.ac.ToString() + ",") + (absent ? "NA" : info.ac.ToString() + ",");
            string str24 = str21 + prefix + "Noise,";
            string str25 = str22 + (absent ? "NA" : info.no.ToString() + ",");
            string str26 = str24 + prefixp + "Noise,";
            string str27 = str25 + (absentp ? "NA" : infop.no.ToString() + ",");
            string str28 = str23 + (absentp ? "NA" : infop.no.ToString() + ",") + (absent ? "NA" : info.no.ToString() + ",");
            string str29 = str26 + prefix + "OLN,";
            string str30 = str27 + (absent ? "NA" : info.oln.ToString() + ",");
            return new Tuple<string, string, string>(str29 + prefixp + "OLN,", str30 + (absentp ? "NA" : infop.oln.ToString() + ","), str28 + (absentp ? "NA" : infop.oln.ToString() + ",") + (absent ? "NA" : info.oln.ToString() + ","));
        }

        public static void setMetrics(ref string t, ref string v, ref string i, Tuple<string, string, string> ms)
        {
            t += ms.Item1;
            v += ms.Item2;
            i += ms.Item3;
        }

        public static Tuple<string, string, string> getMetrics(string prefix, string prefixAll, string prefixp, string prefixpAll, PersonInfo info, PersonInfo infoAll, PersonInfo infop, PersonInfo infopAll, bool absent, bool absentp)
        {
            string t = "";
            string v = "";
            string i = "";
            Tuple<string, string, string> metrics1 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " VD", prefixAll + " VD", prefixp + " VD", prefixpAll + " VD", info.vd, infoAll.vd, infop.vd, infopAll.vd, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics1);
            Tuple<string, string, string> metrics2 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " VC", prefixAll + " VC", prefixp + " VC", prefixpAll + " VC", info.vc, infoAll.vc, infop.vc, infopAll.vc, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics2);
            Tuple<string, string, string> metrics3 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " TC", prefixAll + " TC", prefixp + " TC", prefixpAll + " TC", info.tc, infoAll.tc, infop.tc, infopAll.tc, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics3);
            Tuple<string, string, string> metrics4 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " AC", prefixAll + " AC", prefixp + " AC", prefixpAll + " AC", info.ac, infoAll.ac, infop.ac, infopAll.ac, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics4);
            Tuple<string, string, string> metrics5 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " NO", prefixAll + " NO", prefixp + " NO", prefixpAll + " NO", info.no, infoAll.no, infop.no, infopAll.no, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics5);
            Tuple<string, string, string> metrics6 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " OLN", prefixAll + " OLN", prefixp + " OLN", prefixpAll + " OLN", info.oln, infoAll.oln, infop.oln, infopAll.oln, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics6);
            Tuple<string, string, string> metrics7 = UL_CLASS_PROCESSOR_Program.getMetrics(prefix + " CRY", prefixAll + " CRY", prefixp + " CRY", prefixpAll + " CRY", info.cry, infoAll.cry, infop.cry, infopAll.cry, absent, absentp);
            UL_CLASS_PROCESSOR_Program.setMetrics(ref t, ref v, ref i, metrics7);
            return new Tuple<string, string, string>(t, v, i);
        }

        public static Tuple<string, string, string> getMetrics(string prefix, string prefixAll, string prefixp, string prefixpAll, double info, double infoAll, double infop, double infopAll, bool absent, bool absentp)
        {
            string str1 = "";
            string str2 = "";
            string str3 = "";
            string str4 = str1 + prefix + "," + prefixAll + "," + prefixp + "," + prefixpAll + ",";
            string str5;
            string str6;
            if (!absent)
            {
                str5 = str2 + (object)info + "," + (object)infoAll + "," + (object)infop + "," + (object)infopAll + ",";
                str6 = str3 + (object)infop + "," + (object)infopAll + "," + (object)info + "," + (object)infoAll + ",";
            }
            else
            {
                str5 = "NA,NA,NA,NA,";
                str6 = str5;
            }
            return new Tuple<string, string, string>(str4, str5, str6);
        }

        public static string[] writePairLine(bool getFromIts, ClassroomDay day, string pair, string date, bool nextDay)
        {
            return UL_CLASS_PROCESSOR_Program.writePairLineADEX(day, pair, date, nextDay);
        }

        public static string[] writePairLineADEX(ClassroomDay day, string pair, string date, bool nextDay)
        {
            string[] strArray = new string[2];
            string index1 = pair.Split('-')[0];
            string index2 = pair.Split('-')[1];
            Interactions ints = day.ints;
            PairInfo pairInfo = ints.pairInteractions.ContainsKey(pair) ? ints.pairInteractions[pair] : new PairInfo();
            bool flag1 = day.cf.getMapping(index1, day.day).isAbsent(day.day);
            bool flag2 = day.cf.getMapping(index2, day.day).isAbsent(day.day);
            bool flag3 = !day.startLenaTimes.ContainsKey(index1) || !day.startLenaTimes.ContainsKey(index2);
            bool flag4 = !day.pairStatsSep.ContainsKey(pair);
            string str1 = flag1 ? "absent" : (!day.startLenaTimes.ContainsKey(index1) | flag4 ? "No Data" : "present");
            string str2 = flag2 ? "absent" : (!day.startLenaTimes.ContainsKey(index2) | flag4 ? "No Data" : "present");
            bool flag5 = flag1 | flag2 | flag4 | flag3;
            string str3 = index1.IndexOf("Lab") == 0 ? "Lab" : (index1.IndexOf("T") == 0 ? "Teacher" : "Child");
            string str4 = index2.IndexOf("Lab") == 0 ? "Lab" : (index2.IndexOf("T") == 0 ? "Teacher" : "Child");
            bool flag6 = flag5;
            bool flag7 = flag5;
            strArray[0] = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + new StringBuilder().Insert(0, "NA,", nextDay ? 53 : 55).ToString();
            strArray[1] = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + new StringBuilder().Insert(0, "NA,", nextDay ? 53 : 55).ToString();
            if (!flag5)
            {
                double num1 = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                double num5 = 0.0;
                double num6 = 0.0;
                double num7 = 0.0;
                double num8 = 0.0;
                double num9 = 0.0;
                double num10 = 0.0;
                double num11 = 0.0;
                double num12 = 0.0;
                double num13 = 0.0;
                double num14 = 0.0;
                double num15 = 0.0;
                double num16 = 0.0;
                double num17 = 0.0;
                double num18 = 0.0;
                double num19 = 0.0;
                double num20 = 0.0;
                double num21 = 0.0;
                double num22 = 0.0;
                double num23 = 0.0;
                double num24 = 0.0;
                double num25 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                double num26 = 0.0;
                double num27 = 0.0;
                if (!flag5)
                {
                    num23 = !day.individualTime.ContainsKey(index1) ? 0.0 : Math.Round(day.individualTime[index1], 2);
                    num24 = !day.individualTime.ContainsKey(index2) ? 0.0 : Math.Round(day.individualTime[index2], 2);
                    num3 = Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    num5 = Math.Round(day.pairStatsSep[pair].p1.vc, 2);
                    num7 = Math.Round(day.pairStatsSep[pair].p1.vd, 2);
                    num9 = Math.Round(day.pairStatsSep[pair].p1.ac, 2);
                    num11 = Math.Round(day.pairStatsSep[pair].p1.no, 2);
                    num13 = Math.Round(day.pairStatsSep[pair].p1.oln, 2);
                    num15 = Math.Round(day.pairStatsSep[pair].p1.cry, 2);
                    num18 = Math.Round(day.pairStatsSeparated[pair].Item1, 2);
                    num26 = pairInfo.closeAndOrientedCryInSecs;
                    num4 = Math.Round(day.pairStatsSep[pair].p2.tc, 2);
                    if (index1 == "17A" && index2 == "14A" || index1 == "14A" && index2 == "17A")
                        index1 = index1;
                    num6 = Math.Round(day.pairStatsSep[pair].p2.vc, 2);
                    num8 = Math.Round(day.pairStatsSep[pair].p2.vd, 2);
                    num10 = Math.Round(day.pairStatsSep[pair].p2.ac, 2);
                    num12 = Math.Round(day.pairStatsSep[pair].p2.no, 2);
                    num14 = Math.Round(day.pairStatsSep[pair].p2.oln, 2);
                    num16 = Math.Round(day.pairStatsSep[pair].p2.cry, 2);
                    num19 = Math.Round(day.pairStatsSeparated[pair].Item2, 2);
                    num27 = pairInfo.closeAndOrientedCryInSecs;
                    num1 = Math.Round(day.pairStats[pair], 2);
                    num17 = pairInfo.closeAndOrientedCryInSecs;
                    num2 = day.pairStatsSep[pair].p1.vd + day.pairStatsSep[pair].p2.vd;
                    num20 = Math.Round(day.pairClose[pair], 2) / 2.0;
                    num21 = Math.Round(day.pairCloseOrientation[pair], 2) / 2.0;
                    num22 = Math.Round(day.pairTime[pair], 2) / 2.0;
                    num25 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                }
                PersonInfo infoAll = day.personTotalCounts.ContainsKey(index1) ? day.personTotalCounts[index1] : new PersonInfo();
                PersonInfo info = day.personTotalCountsWUbi.ContainsKey(index1) ? day.personTotalCountsWUbi[index1] : new PersonInfo();
                PersonInfo infopAll = day.personTotalCounts.ContainsKey(index2) ? day.personTotalCounts[index2] : new PersonInfo();
                PersonInfo infop = day.personTotalCountsWUbi.ContainsKey(index2) ? day.personTotalCountsWUbi[index2] : new PersonInfo();
                Tuple<string, string, string> metrics = UL_CLASS_PROCESSOR_Program.getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total", info, infoAll, infop, infopAll, flag5, flag5);
                string str5 = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + (flag5 ? "NA" : num2.ToString()) + "," + (flag5 ? "NA" : num1.ToString()) + "," + (flag6 ? "NA" : num18.ToString()) + "," + (flag7 ? "NA" : num19.ToString()) + "," + (flag6 ? "NA" : num7.ToString()) + "," + (flag7 ? "NA" : num8.ToString()) + "," + (flag6 ? "NA" : num3.ToString()) + "," + (flag7 ? "NA" : num4.ToString()) + "," + (flag6 ? "NA" : num5.ToString()) + "," + (flag7 ? "NA" : num6.ToString()) + "," + (flag6 ? "NA" : num9.ToString()) + "," + (flag7 ? "NA" : num10.ToString()) + "," + (flag6 ? "NA" : num11.ToString()) + "," + (flag7 ? "NA" : num12.ToString()) + "," + (flag6 ? "NA" : num13.ToString()) + "," + (flag7 ? "NA" : num14.ToString()) + "," + (flag6 ? "NA" : num15.ToString()) + "," + (flag7 ? "NA" : num16.ToString()) + "," + (flag6 ? "NA" : num26.ToString()) + "," + (flag7 ? "NA" : num27.ToString()) + "," + (flag5 ? "NA" : num17.ToString()) + "," + (flag5 ? "NA" : num20.ToString()) + "," + (flag5 ? "NA" : num21.ToString()) + "," + (flag5 ? "NA" : num22.ToString()) + "," + (flag6 ? "NA" : num23.ToString()) + "," + (flag7 ? "NA" : num24.ToString()) + "," + (flag5 ? "NA" : num25.ToString()) + "," + metrics.Item2;
                string str6 = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + (flag5 ? "NA" : num2.ToString()) + "," + (flag5 ? "NA" : num1.ToString()) + "," + (flag7 ? "NA" : num19.ToString()) + "," + (flag6 ? "NA" : num18.ToString()) + "," + (flag7 ? "NA" : num8.ToString()) + "," + (flag6 ? "NA" : num7.ToString()) + "," + (flag7 ? "NA" : num4.ToString()) + "," + (flag6 ? "NA" : num3.ToString()) + "," + (flag7 ? "NA" : num6.ToString()) + "," + (flag6 ? "NA" : num5.ToString()) + "," + (flag7 ? "NA" : num10.ToString()) + "," + (flag6 ? "NA" : num9.ToString()) + "," + (flag7 ? "NA" : num12.ToString()) + "," + (flag6 ? "NA" : num11.ToString()) + "," + (flag7 ? "NA" : num14.ToString()) + "," + (flag6 ? "NA" : num13.ToString()) + "," + (flag7 ? "NA" : num16.ToString()) + "," + (flag6 ? "NA" : num15.ToString()) + "," + (flag7 ? "NA" : num27.ToString()) + "," + (flag6 ? "NA" : num26.ToString()) + "," + (flag5 ? "NA" : num17.ToString()) + "," + (flag5 ? "NA" : num20.ToString()) + "," + (flag5 ? "NA" : num21.ToString()) + "," + (flag5 ? "NA" : num22.ToString()) + "," + (flag7 ? "NA" : num24.ToString()) + "," + (flag6 ? "NA" : num23.ToString()) + "," + (flag5 ? "NA" : num25.ToString()) + "," + metrics.Item3;
                strArray[0] = str5;
                strArray[1] = str6;
            }
            return strArray;
        }

        public static string[] writePairLineITS(ClassroomDay day, string pair, string date, bool nextDay)
        {
            PairInfo pairInteraction = day.ints.pairInteractions[pair];
            string[] strArray = new string[2];
            string index1 = pair.Split('-')[0];
            string index2 = pair.Split('-')[1];
            bool flag1 = day.cf.getMapping(index1, day.day).isAbsent(day.day);
            bool flag2 = day.cf.getMapping(index2, day.day).isAbsent(day.day);
            bool flag3 = !day.startLenaTimes.ContainsKey(index1) || !day.startLenaTimes.ContainsKey(index2);
            bool flag4 = !day.pairStatsSep.ContainsKey(pair);
            string str1 = flag1 ? "absent" : (!day.startLenaTimes.ContainsKey(index1) | flag4 ? "No Data" : "present");
            string str2 = flag2 ? "absent" : (!day.startLenaTimes.ContainsKey(index2) | flag4 ? "No Data" : "present");
            bool flag5 = flag1 | flag2 | flag4 | flag3;
            string str3 = index1.IndexOf("Lab") == 0 ? "Lab" : (index1.IndexOf("T") == 0 ? "Teacher" : "Child");
            string str4 = index2.IndexOf("Lab") == 0 ? "Lab" : (index2.IndexOf("T") == 0 ? "Teacher" : "Child");
            bool flag6 = flag5;
            bool flag7 = flag5;
            strArray[0] = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + new StringBuilder().Insert(0, "NA,", nextDay ? 53 : 55).ToString();
            strArray[1] = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + new StringBuilder().Insert(0, "NA,", nextDay ? 53 : 55).ToString();
            if (!flag5)
            {
                double num1 = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                double num5 = 0.0;
                double num6 = 0.0;
                double num7 = 0.0;
                double num8 = 0.0;
                double num9 = 0.0;
                double num10 = 0.0;
                double num11 = 0.0;
                double num12 = 0.0;
                double num13 = 0.0;
                double num14 = 0.0;
                double num15 = 0.0;
                double num16 = 0.0;
                double num17 = 0.0;
                double num18 = 0.0;
                double num19 = 0.0;
                double num20 = 0.0;
                double num21 = 0.0;
                double num22 = 0.0;
                double num23 = 0.0;
                double num24 = 0.0;
                double num25 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                double num26 = 0.0;
                double num27 = 0.0;
                if (!flag5)
                {
                    num23 = !day.individualTime.ContainsKey(index1) ? 0.0 : Math.Round(day.individualTime[index1], 2);
                    num24 = !day.individualTime.ContainsKey(index2) ? 0.0 : Math.Round(day.individualTime[index2], 2);
                    num3 = Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    num5 = Math.Round(day.pairStatsSep[pair].p1.vc, 2);
                    num7 = Math.Round(day.pairStatsSep[pair].p1.vd, 2);
                    num9 = Math.Round(day.pairStatsSep[pair].p1.ac, 2);
                    num11 = Math.Round(day.pairStatsSep[pair].p1.no, 2);
                    num13 = Math.Round(day.pairStatsSep[pair].p1.oln, 2);
                    num15 = Math.Round(day.pairStatsSep[pair].p1.cry, 2);
                    num18 = Math.Round(day.pairStatsSeparated[pair].Item1, 2);
                    num26 = Math.Round(day.pairStatsSeparated[pair].Item3, 2);
                    num4 = Math.Round(day.pairStatsSep[pair].p2.tc, 2);
                    if (index1 == "17A" && index2 == "14A" || index1 == "14A" && index2 == "17A")
                        index1 = index1;
                    num6 = Math.Round(day.pairStatsSep[pair].p2.vc, 2);
                    num8 = Math.Round(day.pairStatsSep[pair].p2.vd, 2);
                    num10 = Math.Round(day.pairStatsSep[pair].p2.ac, 2);
                    num12 = Math.Round(day.pairStatsSep[pair].p2.no, 2);
                    num14 = Math.Round(day.pairStatsSep[pair].p2.oln, 2);
                    num16 = Math.Round(day.pairStatsSep[pair].p2.cry, 2);
                    num19 = Math.Round(day.pairStatsSeparated[pair].Item2, 2);
                    num27 = Math.Round(day.pairStatsSeparated[pair].Item4, 2);
                    num1 = Math.Round(day.pairStats[pair], 2);
                    num17 = Math.Round(day.pairCry[pair], 2);
                    num2 = day.pairStatsSep[pair].p1.vd + day.pairStatsSep[pair].p2.vd;
                    num20 = Math.Round(day.pairClose[pair], 2) / 2.0;
                    num21 = Math.Round(day.pairCloseOrientation[pair], 2) / 2.0;
                    num22 = Math.Round(day.pairTime[pair], 2) / 2.0;
                    num25 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                }
                PersonInfo infoAll = day.personTotalCounts.ContainsKey(index1) ? day.personTotalCounts[index1] : new PersonInfo();
                PersonInfo info = day.personTotalCountsWUbi.ContainsKey(index1) ? day.personTotalCountsWUbi[index1] : new PersonInfo();
                PersonInfo infopAll = day.personTotalCounts.ContainsKey(index2) ? day.personTotalCounts[index2] : new PersonInfo();
                PersonInfo infop = day.personTotalCountsWUbi.ContainsKey(index2) ? day.personTotalCountsWUbi[index2] : new PersonInfo();
                Tuple<string, string, string> metrics = UL_CLASS_PROCESSOR_Program.getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total", info, infoAll, infop, infopAll, flag5, flag5);
                string str5 = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + (flag5 ? "NA" : num2.ToString()) + "," + (flag5 ? "NA" : num1.ToString()) + "," + (flag6 ? "NA" : num18.ToString()) + "," + (flag7 ? "NA" : num19.ToString()) + "," + (flag6 ? "NA" : num7.ToString()) + "," + (flag7 ? "NA" : num8.ToString()) + "," + (flag6 ? "NA" : num3.ToString()) + "," + (flag7 ? "NA" : num4.ToString()) + "," + (flag6 ? "NA" : num5.ToString()) + "," + (flag7 ? "NA" : num6.ToString()) + "," + (flag6 ? "NA" : num9.ToString()) + "," + (flag7 ? "NA" : num10.ToString()) + "," + (flag6 ? "NA" : num11.ToString()) + "," + (flag7 ? "NA" : num12.ToString()) + "," + (flag6 ? "NA" : num13.ToString()) + "," + (flag7 ? "NA" : num14.ToString()) + "," + (flag6 ? "NA" : num15.ToString()) + "," + (flag7 ? "NA" : num16.ToString()) + "," + (flag6 ? "NA" : num26.ToString()) + "," + (flag7 ? "NA" : num27.ToString()) + "," + (flag5 ? "NA" : num17.ToString()) + "," + (flag5 ? "NA" : num20.ToString()) + "," + (flag5 ? "NA" : num21.ToString()) + "," + (flag5 ? "NA" : num22.ToString()) + "," + (flag6 ? "NA" : num23.ToString()) + "," + (flag7 ? "NA" : num24.ToString()) + "," + (flag5 ? "NA" : num25.ToString()) + "," + metrics.Item2;
                string str6 = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + (flag5 ? "NA" : num2.ToString()) + "," + (flag5 ? "NA" : num1.ToString()) + "," + (flag7 ? "NA" : num19.ToString()) + "," + (flag6 ? "NA" : num18.ToString()) + "," + (flag7 ? "NA" : num8.ToString()) + "," + (flag6 ? "NA" : num7.ToString()) + "," + (flag7 ? "NA" : num4.ToString()) + "," + (flag6 ? "NA" : num3.ToString()) + "," + (flag7 ? "NA" : num6.ToString()) + "," + (flag6 ? "NA" : num5.ToString()) + "," + (flag7 ? "NA" : num10.ToString()) + "," + (flag6 ? "NA" : num9.ToString()) + "," + (flag7 ? "NA" : num12.ToString()) + "," + (flag6 ? "NA" : num11.ToString()) + "," + (flag7 ? "NA" : num14.ToString()) + "," + (flag6 ? "NA" : num13.ToString()) + "," + (flag7 ? "NA" : num16.ToString()) + "," + (flag6 ? "NA" : num15.ToString()) + "," + (flag7 ? "NA" : num27.ToString()) + "," + (flag6 ? "NA" : num26.ToString()) + "," + (flag5 ? "NA" : num17.ToString()) + "," + (flag5 ? "NA" : num20.ToString()) + "," + (flag5 ? "NA" : num21.ToString()) + "," + (flag5 ? "NA" : num22.ToString()) + "," + (flag7 ? "NA" : num24.ToString()) + "," + (flag6 ? "NA" : num23.ToString()) + "," + (flag5 ? "NA" : num25.ToString()) + "," + metrics.Item3;
                strArray[0] = str5;
                strArray[1] = str6;
            }
            return strArray;
        }

        public static string[] writeTotalPairLine(ClassroomDay day, string pair, string date, bool nextDay)
        {
            return UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day, pair, date, nextDay, true, true);
        }

        public static string[] writeTotalPairLine(ClassroomDay day, string pair, string date, bool nextDay, bool l, bool u)
        {
            string[] strArray = new string[2];
            string index1 = pair.Split('-')[0];
            string index2 = pair.Split('-')[1];
            bool flag1 = day.cf.getMapping(index1, day.day).isAbsent(day.day);
            bool flag2 = day.cf.getMapping(index2, day.day).isAbsent(day.day);
            bool flag3 = !day.startLenaTimes.ContainsKey(index1) || !day.startLenaTimes.ContainsKey(index2);
            bool flag4 = !day.pairStatsSep.ContainsKey(pair);
            string str1 = flag1 ? "absent" : (!day.startLenaTimes.ContainsKey(index1) | flag4 ? "No Data" : "present");
            string str2 = flag2 ? "absent" : (!day.startLenaTimes.ContainsKey(index2) | flag4 ? "No Data" : "present");
            bool flag5 = flag1 | flag2 | flag4 | flag3;
            string str3 = index1.IndexOf("Lab") == 0 ? "Lab" : (index1.IndexOf("T") == 0 ? "Teacher" : "Child");
            string str4 = index2.IndexOf("Lab") == 0 ? "Lab" : (index2.IndexOf("T") == 0 ? "Teacher" : "Child");
            strArray[0] = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + new StringBuilder().Insert(0, "NA,", 48).ToString();
            strArray[1] = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + new StringBuilder().Insert(0, "NA,", 48).ToString();
            if (!flag5)
            {
                double num1 = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                double num5 = 0.0;
                double num6 = 0.0;
                double num7 = 0.0;
                double num8 = 0.0;
                double num9 = 0.0;
                double num10 = 0.0;
                double num11 = 0.0;
                double num12 = 0.0;
                double num13 = 0.0;
                double num14 = 0.0;
                double num15 = 0.0;
                double num16 = 0.0;
                double num17 = 0.0;
                double num18 = 0.0;
                double num19 = 0.0;
                double num20 = 0.0;
                double num21 = 0.0;
                double num22 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                if (!flag5)
                {
                    num20 = !day.individualTime.ContainsKey(index1) ? 0.0 : Math.Round(day.individualTime[index1], 2);
                    num21 = !day.individualTime.ContainsKey(index2) ? 0.0 : Math.Round(day.individualTime[index2], 2);
                    num3 = Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    num5 = Math.Round(day.pairStatsSep[pair].p1.vc, 2);
                    num7 = Math.Round(day.pairStatsSep[pair].p1.vd, 2);
                    num9 = Math.Round(day.pairStatsSep[pair].p1.ac, 2);
                    num11 = Math.Round(day.pairStatsSep[pair].p1.no, 2);
                    num13 = Math.Round(day.pairStatsSep[pair].p1.oln, 2);
                    num15 = Math.Round(day.pairStatsSeparated[pair].Item1, 2);
                    num4 = Math.Round(day.pairStatsSep[pair].p2.tc, 2);
                    num6 = Math.Round(day.pairStatsSep[pair].p2.vc, 2);
                    num8 = Math.Round(day.pairStatsSep[pair].p2.vd, 2);
                    num10 = Math.Round(day.pairStatsSep[pair].p2.ac, 2);
                    num12 = Math.Round(day.pairStatsSep[pair].p2.no, 2);
                    num14 = Math.Round(day.pairStatsSep[pair].p2.oln, 2);
                    num16 = Math.Round(day.pairStatsSeparated[pair].Item2, 2);
                    num1 = Math.Round(day.pairStats[pair], 2);
                    num2 = day.pairStatsSep[pair].p1.vd + day.pairStatsSep[pair].p2.vd;
                    num17 = Math.Round(day.pairClose[pair], 2) / 2.0;
                    num18 = Math.Round(day.pairCloseOrientation[pair], 2) / 2.0;
                    num19 = Math.Round(day.pairTime[pair], 2) / 2.0;
                    num22 = (double)((day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000);
                }
                PersonInfo infoAll = !l || !day.personTotalCounts.ContainsKey(index1) ? new PersonInfo() : day.personTotalCounts[index1];
                PersonInfo info = !u || !day.personTotalCountsWUbi.ContainsKey(index1) ? new PersonInfo() : day.personTotalCountsWUbi[index1];
                PersonInfo infopAll = !l || !day.personTotalCounts.ContainsKey(index2) ? new PersonInfo() : day.personTotalCounts[index2];
                PersonInfo infop = !u || !day.personTotalCountsWUbi.ContainsKey(index2) ? new PersonInfo() : day.personTotalCountsWUbi[index2];
                Tuple<string, string, string> metrics = UL_CLASS_PROCESSOR_Program.getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total", info, infoAll, infop, infopAll, flag5, flag5);
                string str5 = str1 + "," + str2 + "," + (nextDay ? "" : str3 + ",") + (nextDay ? "" : str4 + ",") + (flag5 ? "NA" : num22.ToString()) + "," + metrics.Item2;
                string str6 = str2 + "," + str1 + "," + (nextDay ? "" : str4 + ",") + (nextDay ? "" : str3 + ",") + (flag5 ? "NA" : num22.ToString()) + "," + metrics.Item3;
                strArray[0] = str5;
                strArray[1] = str6;
            }
            return strArray;
        }

        public static void testCry(ClassroomDay day)
        {
            double num = 0.0;
            foreach (cryInfoTest cryInfoTest in day.childCries["14A"])
                num += cryInfoTest.secs;
        }

        public static void writeDataSummary(bool getFromIts, bool append, string file_name, List<ClassroomDay> days, bool processLast)
        {
            using (TextWriter textWriter = (TextWriter)new StreamWriter(file_name, append))
            {
                string str1 = "Pair Block Talking,Pair Talking Duration, Subject-Talking-Duration-From_Start,Partner-Talking-Duration-From-Start, Subject-Talking-Duration-Evenly-Spread,Partner-Talking-Duration-Evenly-Spread, Subject Turn Count, Partner Turn Count, Subject Voc Count, Partner Voc Count, Subject Adult Count, Partner Adult Count, Subject Noise, Partner Noise, Subject OLN, Partner OLN, Subject Cry, Partner Cry, Subject jCry, Partner jCry, Joined Cry, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time," + UL_CLASS_PROCESSOR_Program.getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;
                string str2 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + str1;
                string str3 = "Lead_Subject Status, Lead_Partner Status,Lead_" + str1.Replace(",", ",Lead_");
                textWriter.Write(str2.Replace(" ", ""));
                textWriter.WriteLine(str3.Replace(" ", ""));
                int index = 0;
                foreach (ClassroomDay day1 in days)
                {
                    UL_CLASS_PROCESSOR_Program.testCry(day1);
                    string shortDateString = day1.startTime.ToShortDateString();
                    ++index;
                    if (index < days.Count | processLast)
                    {
                        foreach (string pair in day1.cf.pairs)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                                ;
                            string str4 = pair.Split('-')[0];
                            string str5 = pair.Split('-')[1];
                            string[] strArray1 = UL_CLASS_PROCESSOR_Program.writePairLine(getFromIts, day1, pair, shortDateString, false);
                            bool flag1 = false;
                            if (str5.Contains("T") || str5.Contains("Lab") || str4.Contains("T") || str4.Contains("Lab"))
                                flag1 = true;
                            string str6 = shortDateString + "," + str4 + "," + str5 + "," + flag1.ToString() + "," + strArray1[0];
                            string str7 = shortDateString + "," + str5 + "," + str4 + "," + flag1.ToString() + "," + strArray1[1];
                            string str8 = new StringBuilder().Insert(0, "NA,", 57).ToString();
                            string str9 = new StringBuilder().Insert(0, "NA,", 57).ToString();
                            if (index < days.Count)
                            {
                                ClassroomDay day2 = days[index];
                                string str10 = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                bool flag2 = true;
                                if (day2.pairStatsSep.ContainsKey(pair))
                                {
                                    str10 = pair;
                                    flag2 = false;
                                }
                                else if (!day2.pairStatsSep.ContainsKey(str10))
                                    str10 = "";
                                if (str10.Trim() != "")
                                {
                                    string[] strArray2 = UL_CLASS_PROCESSOR_Program.writePairLine(getFromIts, day2, str10, day2.startTime.ToShortDateString(), true);
                                    if (!flag2)
                                    {
                                        str9 = strArray2[0];
                                        str8 = strArray2[1];
                                    }
                                    else
                                    {
                                        str9 = strArray2[1];
                                        str8 = strArray2[0];
                                    }
                                }
                            }
                            textWriter.WriteLine(str6 + str9);
                            textWriter.WriteLine(str7 + str8);
                        }
                    }
                }
            }
        }

        public static void writeDataSummaryTotals(bool append, string file_name, List<ClassroomDay> days, bool processLast)
        {
            UL_CLASS_PROCESSOR_Program.writeDataSummaryTotals(append, file_name, days, processLast, true, false);
        }

        public static void writeDataSummaryTotals(bool append, string file_name, List<ClassroomDay> days, bool processLast, bool l, bool u)
        {
            using (TextWriter textWriter = (TextWriter)new StreamWriter(file_name, append))
            {
                string str1 = "Total Recording Time," + UL_CLASS_PROCESSOR_Program.getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;
                string str2 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + str1;
                string str3 = "Lead_Subject Status, Lead_Partner Status,Lead_" + str1.Replace(",", ",Lead_");
                textWriter.Write(str2.Replace(" ", ""));
                textWriter.WriteLine(str3.Replace(" ", ""));
                int index = 0;
                foreach (ClassroomDay day1 in days)
                {
                    string shortDateString = day1.startTime.ToShortDateString();
                    ++index;
                    if (index < days.Count | processLast)
                    {
                        foreach (string pair in day1.cf.pairs)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                                ;
                            string str4 = pair.Split('-')[0];
                            string str5 = pair.Split('-')[1];
                            string[] strArray1 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day1, pair, shortDateString, false, l, u);
                            bool flag1 = false;
                            if (str5.Contains("T") || str5.Contains("Lab") || str4.Contains("T") || str4.Contains("Lab"))
                                flag1 = true;
                            string str6 = shortDateString + "," + str4 + "," + str5 + "," + flag1.ToString() + "," + strArray1[0];
                            string str7 = shortDateString + "," + str5 + "," + str4 + "," + flag1.ToString() + "," + strArray1[1];
                            string str8 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            string str9 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            if (index < days.Count)
                            {
                                ClassroomDay day2 = days[index];
                                string str10 = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                bool flag2 = true;
                                if (day2.pairStatsSep.ContainsKey(pair))
                                {
                                    str10 = pair;
                                    flag2 = false;
                                }
                                else if (!day2.pairStatsSep.ContainsKey(str10))
                                    str10 = "";
                                if (str10.Trim() != "")
                                {
                                    string[] strArray2 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day2, str10, day2.startTime.ToShortDateString(), true, l, u);
                                    if (!flag2)
                                    {
                                        str9 = strArray2[0];
                                        str8 = strArray2[1];
                                    }
                                    else
                                    {
                                        str9 = strArray2[1];
                                        str8 = strArray2[0];
                                    }
                                }
                            }
                            textWriter.WriteLine(str6 + str9);
                            textWriter.WriteLine(str7 + str8);
                        }
                    }
                }
            }
        }

        public static void writeDataSummaryTotalsLena(bool append, string file_name, List<ClassroomDay> days, bool processLast)
        {
            using (TextWriter textWriter = (TextWriter)new StreamWriter(file_name, append))
            {
                string str1 = "Total Recording Time," + UL_CLASS_PROCESSOR_Program.getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;
                string str2 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + str1;
                string str3 = "Lead_Subject Status, Lead_Partner Status,Lead_" + str1.Replace(",", ",Lead_");
                textWriter.Write(str2.Replace(" ", ""));
                textWriter.WriteLine(str3.Replace(" ", ""));
                int index = 0;
                foreach (ClassroomDay day1 in days)
                {
                    string shortDateString = day1.startTime.ToShortDateString();
                    ++index;
                    if (index < days.Count | processLast)
                    {
                        foreach (string pair in day1.cf.pairs)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                                ;
                            string str4 = pair.Split('-')[0];
                            string str5 = pair.Split('-')[1];
                            string[] strArray1 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day1, pair, shortDateString, false);
                            bool flag1 = false;
                            if (str5.Contains("T") || str5.Contains("Lab") || str4.Contains("T") || str4.Contains("Lab"))
                                flag1 = true;
                            string str6 = shortDateString + "," + str4 + "," + str5 + "," + flag1.ToString() + "," + strArray1[0];
                            string str7 = shortDateString + "," + str5 + "," + str4 + "," + flag1.ToString() + "," + strArray1[1];
                            string str8 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            string str9 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            if (index < days.Count)
                            {
                                ClassroomDay day2 = days[index];
                                string str10 = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                bool flag2 = true;
                                if (day2.pairStatsSep.ContainsKey(pair))
                                {
                                    str10 = pair;
                                    flag2 = false;
                                }
                                else if (!day2.pairStatsSep.ContainsKey(str10))
                                    str10 = "";
                                if (str10.Trim() != "")
                                {
                                    string[] strArray2 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day2, str10, day2.startTime.ToShortDateString(), true);
                                    if (!flag2)
                                    {
                                        str9 = strArray2[0];
                                        str8 = strArray2[1];
                                    }
                                    else
                                    {
                                        str9 = strArray2[1];
                                        str8 = strArray2[0];
                                    }
                                }
                            }
                            textWriter.WriteLine(str6 + str9);
                            textWriter.WriteLine(str7 + str8);
                        }
                    }
                }
            }
        }

        public static void writeDataSummaryTotalsUbi(bool append, string file_name, List<ClassroomDay> days, bool processLast)
        {
            using (TextWriter textWriter = (TextWriter)new StreamWriter(file_name, append))
            {
                string str1 = "Total Recording Time," + UL_CLASS_PROCESSOR_Program.getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;
                string str2 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + str1;
                string str3 = "Lead_Subject Status, Lead_Partner Status,Lead_" + str1.Replace(",", ",Lead_");
                textWriter.Write(str2.Replace(" ", ""));
                textWriter.WriteLine(str3.Replace(" ", ""));
                int index = 0;
                foreach (ClassroomDay day1 in days)
                {
                    string shortDateString = day1.startTime.ToShortDateString();
                    ++index;
                    if (index < days.Count | processLast)
                    {
                        foreach (string pair in day1.cf.pairs)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                                ;
                            string str4 = pair.Split('-')[0];
                            string str5 = pair.Split('-')[1];
                            string[] strArray1 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day1, pair, shortDateString, false);
                            bool flag1 = false;
                            if (str5.Contains("T") || str5.Contains("Lab") || str4.Contains("T") || str4.Contains("Lab"))
                                flag1 = true;
                            string str6 = shortDateString + "," + str4 + "," + str5 + "," + flag1.ToString() + "," + strArray1[0];
                            string str7 = shortDateString + "," + str5 + "," + str4 + "," + flag1.ToString() + "," + strArray1[1];
                            string str8 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            string str9 = new StringBuilder().Insert(0, "NA,", 48).ToString();
                            if (index < days.Count)
                            {
                                ClassroomDay day2 = days[index];
                                string str10 = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                bool flag2 = true;
                                if (day2.pairStatsSep.ContainsKey(pair))
                                {
                                    str10 = pair;
                                    flag2 = false;
                                }
                                else if (!day2.pairStatsSep.ContainsKey(str10))
                                    str10 = "";
                                if (str10.Trim() != "")
                                {
                                    string[] strArray2 = UL_CLASS_PROCESSOR_Program.writeTotalPairLine(day2, str10, day2.startTime.ToShortDateString(), true);
                                    if (!flag2)
                                    {
                                        str9 = strArray2[0];
                                        str8 = strArray2[1];
                                    }
                                    else
                                    {
                                        str9 = strArray2[1];
                                        str8 = strArray2[0];
                                    }
                                }
                            }
                            textWriter.WriteLine(str6 + str9);
                            textWriter.WriteLine(str7 + str8);
                        }
                    }
                }
            }
        }
    }
}
