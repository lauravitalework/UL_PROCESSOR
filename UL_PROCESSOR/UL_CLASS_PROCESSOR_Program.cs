﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace UL_PROCESSOR
{
    class UL_CLASS_PROCESSOR_Program
    {
        public Config configInfo;
        public List<DateTime> days;
        public List<String> fileNames = new List<string>();

        public UL_CLASS_PROCESSOR_Program(Config c, List<DateTime> d)//, String fName, Boolean t, Boolean sd, Boolean s, Boolean a, Boolean its, Boolean GR, Boolean VEL)
        {
            days = d;
            configInfo = c;
            //configInfo.readMappings();
        }

        public void process(Boolean processLast, int grp)
        {
            List<ClassroomDay> cds = new List<ClassroomDay>();
            int countSets = 0;
            int countDays = 0;

            /************ 1)READ MAPPINGS*******************/
            if (configInfo.classSettings.mappingBy == "CLASS")
                configInfo.readClassroomMappings();

            /********B)	FOR EACH DAY IN CLASSROOM: ********/
            foreach (DateTime day in days)
            {
                //configInfo.settings.subs.Add("12P");
                //configInfo.settings.subs.Add("4D");
                //configInfo.settings.subs.Add("10D");
                /*  configInfo.settings.subs.Add("9B");
                 configInfo.settings.subs.Add("1B");
                 configInfo.settings.subs.Add("3B"); 
                configInfo.settings.subs.Add("5B");
                configInfo.settings.subs.Add("7B"); */
                //  configInfo.settings.subs.Add("T2B");
                //  configInfo.settings.subs.Add("T1B");
                //  configInfo.settings.subs.Add("T3B");
                //  configInfo.settings.subs.Add("T4B");

                //    configInfo.settings.subs.Add("Lab1B");
                //  configInfo.settings.subs.Add("Lab2B");
                //  configInfo.settings.subs.Add("Lab3B");
                //  configInfo.settings.subs.Add("Lab4B");
                //  configInfo.settings.subs.Add("Lab5B");


                /*T1B
T2B
Lab1B
Lab2B
Lab4B
Lab2B
Lab3B
Lab5B
Lab3B
T4B
Lab3B
T3B
Lab2B
*/
                //DELETE TEST SUBJ PARTNER 0S
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_1");
                // configInfo.settings.subs.Add("DS_LADYBUGS_1819_T1");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_T2");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_10");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_8");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_10");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_1");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_11");
                //configInfo.settings.subs.Add("DS_LADYBUGS_1819_8");
                //configInfo.settings.subs.Add("10L");
                //configInfo.settings.subs.Add("L2");
                //L8P	DS_PANDAS_1819_1
                //configInfo.settings.subs.Add("DS_PANDAS_1819_1");
                //configInfo.settings.subs.Add("DS_PANDAS_1819_10");
                //configInfo.settings.subs.Add("L8P");

                //configInfo.settings.subs.Add("2B");
                //pairInteractions["PR_LEAP_1819_T3-PR_LEAP_AM_1819_11"].partner.ad
                /*configInfo.settings.subs.Add("PR_LEAP_AM_1819_11");
                configInfo.settings.subs.Add("PR_LEAP_AM_1819_2");
                configInfo.settings.subs.Add("PR_LEAP_AM_1819_3");
                configInfo.settings.subs.Add("PR_LEAP_AM_1819_1");
                configInfo.settings.subs.Add("PR_LEAP_1819_T3");
                configInfo.settings.subs.Add("PR_LEAP_1819_T1"); 
                configInfo.settings.subs.Add("TROP_TRTL_1920_1");
                configInfo.settings.subs.Add("TROP_TRTL_1920_11"); 

                configInfo.settings.subs.Add("DS_STARFISH_1920_1");
                configInfo.settings.subs.Add("DS_STARFISH_1920_2");
                configInfo.settings.subs.Add("DS_STARFISH_1920_3");
                //configInfo.settings.subs.Add("DS_STARFISH_1920_10");*/

                /************ 1)READ DAY MAPPINGS*******************/
                if (configInfo.classSettings.mappingBy != "CLASS")
                    configInfo.readDayMappings(day);

                ClassroomDay cd = new ClassroomDay(day, configInfo);
                Console.WriteLine("PROCESSING " + configInfo.classroom + " " + day.ToShortDateString());


                /************ 1)READ LENA FILE*******************/
                Console.WriteLine("readLenaFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " getFromIts " + configInfo.settings.getFromIts + "):");
                Dictionary<String, List<PersonInfo>> rawLena = configInfo.settings.getFromIts ? cd.readLenaItsFiles(countDays) : cd.readLenaFile();


                //OLD VERSION
                if (configInfo.settings.doUbiChildFiles)
                {
                    Console.WriteLine("readUbiFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    Dictionary<String, List<PersonInfo>> rawUbi = cd.readUbiFile(new List<string>());

                    Console.WriteLine("setUbiData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    cd.setUbiData(rawUbi, rawLena);
                }

                /************ 2)READ UBI TAG FILE*******************/
                Console.WriteLine("readUbiTagFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>, List<PersonInfo>> rawTags = cd.readUbiTagFile(rawLena, configInfo.settings.doGR);// new List<string>());//, subs);

                /************ 3)SET UBI TAG DATA*******************/
                Console.WriteLine("setUbiTagData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setUbiTagData(rawTags.Item1, rawTags.Item2);


                //SORT 
                /*List<Order> objListOrder =
                source.OrderBy(order => order.OrderDate).ToList();*/
                List<String> personKeys = new List<string>();
                foreach (String person in rawLena.Keys)
                {
                    personKeys.Add(person);

                }
                foreach (String person in personKeys)
                {
                    List<PersonInfo> newList = rawLena[person].OrderBy(order => order.dt).ToList();
                    rawLena[person] = newList;
                }

                /************ 4)WRITE GR BASE FILES OR CLEANUBI *******************/
                if (configInfo.settings.justCleanUbi)
                {
                    Console.WriteLine("CLEAN UBI (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    cd.makeCleanUbiLog(rawTags.Item3, rawLena);
                }
                else
                {
                    if (configInfo.settings.doGR)
                    {
                        Console.WriteLine("GR (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                        cd.writeGR(rawTags.Item3, rawLena);
                    }


                    /************ 5)SET LENA DATA*******************/
                    Console.WriteLine("setLenaData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    cd.setLenaData(rawLena);


                    /************ 6)GET TRUNK TIME*******************/
                    DateTime trunkAt = cd.getTrunkTime();//gets first end track time from last ten minutes of tracking.


                    /************ 8)GET REPORTS*******************/
                    if (configInfo.settings.doSocialOnsets || configInfo.settings.doAngleFiles || configInfo.settings.doApproach || configInfo.settings.doTenFiles || configInfo.settings.doSumAllFiles || configInfo.settings.doSumDayFiles)
                    {
                        /************ I)COUNT INTERACTIONS*******************/
                        if (!configInfo.justUbi)
                        {//
                            Console.WriteLine("countInteractions (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                            cd.countInteractionsNew(trunkAt, rawLena); //count interactions but no need to write a file
                        }
                        //cd.countInteractions(subs, doAngleFiles, doAngleFiles, true, trunkAt, rawLena); //count interactions but no need to write a file
                        if (configInfo.settings.doTenFiles || configInfo.settings.doVel)
                        {
                            /************ II)10TH SEC AND VEL REPORT*******************/
                            Console.WriteLine("write10SecTalkingCSV (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                            //DEBUG DELETE
                            cd.write10SecTalkingCSVDetailed(); //write complete data files to disc
                                                               //cd.write10SecTalkingCSV(  ); //write complete data files to disc
                        }
                        cd.activities.Clear();
                        cd.activities = null;
                        cds.Add(cd);

                    }
                    countDays++;
                }
                /************ C)SUMMARY REPORTS*******************/
                summarize(cds, countSets, processLast, grp);
            }
        }
        public Boolean doTotals = false;
        public void summarize(List<ClassroomDay> cds, int c, Boolean processLast, int grp)
        {
            if (configInfo.settings.doSumAllFiles || configInfo.settings.doSumDayFiles)
            {
                foreach (ClassroomDay cd in cds)
                {
                    try
                    {
                        Console.WriteLine("Summarizing " + cd.day.Month + " " + cd.day.Year + " " + cd.day.Day + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                        List<ClassroomDay> oneDay = new List<ClassroomDay>();//sync3_3pairactivity_vo8.CSV
                        oneDay.Add(cd);
                        Console.WriteLine("Summarizing ONE");
                        if (configInfo.settings.doSumDayFiles)
                        {
                            /************ 1)SUMMARY DAY REPORT*******************/
                            writeDataSummary(configInfo,configInfo.settings.getFromIts, false, configInfo.root + configInfo.classroom + "/SYNC/PAIRACTIVITY_"+ c.ToString() + "_" + configInfo.classroom + "_" + (cd.justProx ? "no_" : "") + (configInfo.justFreePlay ? "freeplay_" : "") + cd.day.Month + "_" + cd.day.Day + "_" + cd.day.Year + "_" + configInfo.settings.fileNameVersion + ".csv", oneDay, true); //write summary data
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                 

                Console.WriteLine("Summarizing ALL " + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");

                if (configInfo.settings.doSumAllFiles)
                {
                    String fileNameSum = configInfo.root + configInfo.classroom + "/SYNC/PAIRACTIVITY_ALL_"+ grp + configInfo.classroom + (configInfo.justFreePlay ? "_freeplay" : "") + "_" + configInfo.settings.fileNameVersion + ".CSV";

                    /************ 2)SUMMARY ALL REPORT*******************/
                    writeDataSummary(configInfo,configInfo.settings.getFromIts, c > 0, fileNameSum, cds, processLast); //write summary data
                    fileNames.Add(fileNameSum);
                }
            }
        }
        public static Tuple<String, String, String> getMetrics(String prefix, String prefixp, PersonInfo info, PersonInfo infop, Boolean absent, Boolean absentp)
        {
            String title = "";
            String values = "";
            String valuesInverse = "";


            title += (prefix + "Voc Dur,");
            values += (absent ? "NA" : info.vd + ",");
            title += (prefixp + "Voc Dur,");
            values += (absentp ? "NA" : infop.vd + ",");
            valuesInverse += (absentp ? "NA" : infop.vd + ",");
            valuesInverse += (absent ? "NA" : info.vd + ",");

            title += (prefix + "Voc Count,");
            values += (absent ? "NA" : info.vc + ",");
            title += (prefixp + "Voc Count,");
            values += (absentp ? "NA" : infop.vc + ",");
            valuesInverse += (absentp ? "NA" : infop.vc + ",");
            valuesInverse += (absent ? "NA" : info.vc + ",");

            title += (prefix + "Turn Count,");
            values += (absent ? "NA" : info.tc + ",");
            title += (prefixp + "Turn Count,");
            values += (absentp ? "NA" : infop.tc + ",");
            valuesInverse += (absentp ? "NA" : infop.tc + ",");
            valuesInverse += (absent ? "NA" : info.tc + ",");

            title += (prefix + "Adult Count,");
            values += (absent ? "NA" : info.ac + ",");
            title += (prefixp + "Adult Count,");
            values += (absentp ? "NA" : infop.ac + ",");
            valuesInverse += (absentp ? "NA" : infop.ac + ",");
            valuesInverse += (absent ? "NA" : info.ac + ",");

            title += (prefix + "Noise,");
            values += (absent ? "NA" : info.no + ",");
            title += (prefixp + "Noise,");
            values += (absentp ? "NA" : infop.no + ",");
            valuesInverse += (absentp ? "NA" : infop.no + ",");
            valuesInverse += (absent ? "NA" : info.no + ",");

            title += (prefix + "OLN,");
            values += (absent ? "NA" : info.oln + ",");
            title += (prefixp + "OLN,");
            values += (absentp ? "NA" : infop.oln + ",");
            valuesInverse += (absentp ? "NA" : infop.oln + ",");
            valuesInverse += (absent ? "NA" : info.oln + ",");
            return new Tuple<string, string, string>(title, values, valuesInverse);

        }
        public static void setMetrics(ref String t, ref String v, ref String i, Tuple<String, String, String> ms)
        {
            t += ms.Item1;// +",";
            v += ms.Item2;// + ",";
            i += ms.Item3;// + ",";
        }
        public static Tuple<String, String, String> getMetrics(String prefix, String prefixAll, String prefixp, String prefixpAll, PersonInfo info, PersonInfo infoAll, PersonInfo infop, PersonInfo infopAll, Boolean absent, Boolean absentp)
        {
            String title = "";
            String values = "";
            String valuesInverse = "";
            Tuple<String, String, String> ms = getMetrics(prefix + " VD", prefixAll + " VD", prefixp + " VD",
                                                          prefixpAll + " VD", info.vd, infoAll.vd, infop.vd, infopAll.vd, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);


            ms = getMetrics(prefix + " VC", prefixAll + " VC", prefixp + " VC",
                                                          prefixpAll + " VC", info.vc, infoAll.vc, infop.vc, infopAll.vc, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);


            ms = getMetrics(prefix + " TC", prefixAll + " TC", prefixp + " TC",
                                                          prefixpAll + " TC", info.tc, infoAll.tc, infop.tc, infopAll.tc, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);


            ms = getMetrics(prefix + " AC", prefixAll + " AC", prefixp + " AC",
                                                          prefixpAll + " AC", info.ac, infoAll.ac, infop.ac, infopAll.ac, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);


            ms = getMetrics(prefix + " NO", prefixAll + " NO", prefixp + " NO",
                                                          prefixpAll + " NO", info.no, infoAll.no, infop.no, infopAll.no, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);


            ms = getMetrics(prefix + " OLN", prefixAll + " OLN", prefixp + " OLN",
                                                          prefixpAll + " OLN", info.oln, infoAll.oln, infop.oln, infopAll.oln, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);

            ms = getMetrics(prefix + " CRY", prefixAll + " CRY", prefixp + " CRY",
                                                          prefixpAll + " CRY", info.cry, infoAll.cry, infop.cry, infopAll.cry, absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);

            ms = getMetrics(prefix + " AV_DB", prefixAll + " AV_DB", prefixp + " AV_DB",
                                                          prefixpAll + " AV_DB", getAvDb(info), getAvDb(infoAll), getAvDb(infop), getAvDb(infopAll), absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);

            ms = getMetrics(prefix + " AV_PEAK_DB", prefixAll + " AV_PEAK_DB", prefixp + " AV_PEAK_DB",
                                                          prefixpAll + " AV_PEAK_DB", getAvPeakvDb(info), getAvPeakvDb(infoAll), getAvPeakvDb(infop), getAvPeakvDb(infopAll), absent, absentp);
            setMetrics(ref title, ref values, ref valuesInverse, ms);

            return new Tuple<string, string, string>(title, values, valuesInverse);

        }
        public static double getAvDb(PersonInfo p)
        {
            return p.avDb != 0 && p.childSegments != 0 ? p.avDb / p.childSegments : 0; ;
        }
        public static double getAvPeakvDb(PersonInfo p)
        {
            return p.maxDb != 0 && p.childSegments != 0 ? p.maxDb / p.childSegments : 0; ;
        }
        public static Tuple<String, String, String> getMetrics(String prefix, String prefixAll, String prefixp, String prefixpAll, double info, double infoAll, double infop, double infopAll, Boolean absent, Boolean absentp)
        {
            String title = "";
            String values = "";
            String valuesInverse = "";

            title += (prefix) + ",";
            title += (prefixAll) + ",";
            title += (prefixp) + ",";
            title += (prefixpAll) + ",";

            if (!absent)
            {
                values += (info + ",");
                values += (infoAll + ",");
                values += (infop + ",");
                values += (infopAll + ",");

                valuesInverse += (infop + ",");
                valuesInverse += (infopAll + ",");
                valuesInverse += (info + ",");
                valuesInverse += (infoAll + ",");
            }
            else
            {
                values = "NA,NA,NA,NA,";
                valuesInverse = values;
            }

            return new Tuple<string, string, string>(title, values, valuesInverse);

        }

        public static String[] writePairLineNew(ClassroomDay day, String pair, String date, Boolean nextDay)
        {
            String[] lines = new String[2];
            String subject = pair.Split('-')[0];
            String partner = pair.Split('-')[1];
            String status = "present";
            String statusp = "present";
            MappingRow mrs = day.cf.getMapping(subject, day.day);
            MappingRow mrp = day.cf.getMapping(partner, day.day);

            Boolean sAbsent = mrs.isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
            Boolean pAbsent = mrp.isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(partner)); ;
            Boolean lAbsent = (!day.startLenaTimes.ContainsKey(subject)) || (!day.startLenaTimes.ContainsKey(partner));
            Boolean dAbsent = !day.pairInteractions.ContainsKey(pair);
           
            status = sAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ? "No Data" : "present");
            statusp = pAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(partner)) || dAbsent ? "No Data" : "present");
            if ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ||
               (!day.startLenaTimes.ContainsKey(partner)) || dAbsent
               )
            {
                dAbsent = dAbsent;
            }

            Boolean absent = sAbsent || pAbsent || dAbsent || lAbsent;
            String type = subject.IndexOf("Lab") == 0 ? "Lab" : subject.IndexOf("T") == 0 ? "Teacher" : "Child";
            String typep = partner.IndexOf("Lab") == 0 ? "Lab" : partner.IndexOf("T") == 0 ? "Teacher" : "Child";

           // MappingRow mr =  day.cf.getUbiMapping(subject, day.day);
            type = day.cf.getMapping(subject, day.day).type;
            typep = day.cf.getMapping(partner, day.day).type;
            type = type.Trim() == "" ? (subject.IndexOf("T") == 0 ||
                                      subject.IndexOf("_T") > 0 ?
                                      "Teacher" :
                                      (subject.IndexOf("L") == 0 ||
                                      subject.IndexOf("_L") > 0 ?
                                      "Lab" : "Child"))
                                      : type;
            typep = typep.Trim() == "" ? (partner.IndexOf("T") == 0 ||
                                      partner.IndexOf("_T") > 0 ?
                                      "Teacher" :
                                      (partner.IndexOf("L") == 0 ||
                                      partner.IndexOf("_L") > 0 ?
                                      "Lab" : "Child"))
                                      : typep;
            sAbsent = absent;
            pAbsent = absent;
            dAbsent = absent;
            lAbsent = absent;

           
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", 66).ToString();//65
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", 66).ToString();//65



            if (UL_PROCESSOR_Program.doTempLkpFpVars)
            {
                lines[0]+= (new StringBuilder().Insert(0, "NA,", 7).ToString()+"NA");
                lines[1] +=( new StringBuilder().Insert(0, "NA,", 7).ToString() + "NA");
            }

            if (!absent)
            {

                double interactionTime = 0;
                double VD = 0;
                double subjectTurnCount = 0;
                double partnerTurnCount = 0;
                double subjectVocCount = 0;
                double partnerVocCount = 0;
                double subjectVocDur = 0;
                double partnerVocDur = 0;
                double subjectAdUttLen= 0;
                double partnerAdUttLen = 0;
                double subjectAdCount = 0;
                double partnerAdCount = 0; //Math.Round(day.pairStatsSep[pair].p2.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectNoise = 0; //Math.Round(day.pairStatsSep[pair].p1.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                double partnerNoise = 0; //Math.Round(day.pairStatsSep[pair].p2.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectOln = 0; //Math.Round(day.pairStatsSep[pair].p1.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                double partnerOln = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectCry = 0; //Math.Round(day.pairStatsSep[pair].p1.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                double partnerCry = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double joinedCry = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectJoinedCry = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double partnerJoinedCry = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectInteractionTime = 0; //Math.Round(day.pairStatsSeparated[pair].Item1, 2);
                double partnerInteractionTime = 0; //Math.Round(day.pairStatsSeparated[pair].Item2, 2);
                double proximityTime = 0; //Math.Round(day.pairClose[pair], 2) / 2;
                double proximityOrientationTime = 0; // Math.Round(day.pairCloseOrientation[pair], 2) / 2;
                double sharedTime = 0; // Math.Round(day.pairTime[pair], 2) / 2; //check on this. correct. 
                double subjectTime = 0; // Math.Round(day.individualTime[subject], 2);
                double partnerTime = 0; //Math.Round(day.individualTime[partner], 2);
                double recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;

                String subjectLine = "";
                String partnerLine = "";



                //10/19/20 lkp urgent request
                double subjectVocCountFp = 0;
                double partnerVocCountFp = 0;
                double proximityOrientationTimeFp = 0; // Math.Round(day.pairCloseOrientation[pair], 2) / 2;
                double sharedTimeFp = 0; // Math.Round(day.pairTime[pair], 2) / 2; //check on this. correct. 
                double subjectVocCountNoFp = 0;
                double partnerVocCountNoFp = 0;
                double proximityOrientationTimeNoFp = 0; // Math.Round(day.pairCloseOrientation[pair], 2) / 2;
                double sharedTimeNoFp = 0; // Math.Round(day.pairTime[pair], 2) / 2; //check on this. correct. 


                if (!absent)
                {
                    /*if (day.individualTime.ContainsKey(subject))
                        subjectTime = Math.Round(day.individualTime[subject], 2);
                    else
                    {
                        subjectTime = 0;
                    }

                    if (day.individualTime.ContainsKey(partner))
                        partnerTime = Math.Round(day.individualTime[partner], 2);
                    else
                    {
                        partnerTime = 0;
                        //absent = true;

                    }*/
                    subjectTime= Math.Round(day.pairInteractions[pair].subject.individualTime, 2);///////////
                    partnerTime = Math.Round(day.pairInteractions[pair].partner.individualTime, 2);

                    subjectTurnCount = Math.Round(day.pairInteractions[pair].subject.tc, 2); //Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    subjectVocCount = Math.Round(day.pairInteractions[pair].subject.vc, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocDur = Math.Round(day.pairInteractions[pair].subject.vd, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdCount = Math.Round(day.pairInteractions[pair].subject.ac, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdUttLen = Math.Round(day.pairInteractions[pair].subject.ad, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectNoise = Math.Round(day.pairInteractions[pair].subject.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectOln = Math.Round(day.pairInteractions[pair].subject.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectCry = Math.Round(day.pairInteractions[pair].subject.cry, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectInteractionTime = Math.Round(day.pairInteractions[pair].subject.interactionTime, 2);
                    partnerTurnCount = Math.Round(day.pairInteractions[pair].partner.tc, 2);
                    subjectJoinedCry = Math.Round(day.pairInteractions[pair].subject.cryingTime, 2);
                    
                    partnerVocCount = Math.Round(day.pairInteractions[pair].partner.vc, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocDur = Math.Round(day.pairInteractions[pair].partner.vd, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdCount = Math.Round(day.pairInteractions[pair].partner.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdUttLen = Math.Round(day.pairInteractions[pair].partner.ad, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerNoise = Math.Round(day.pairInteractions[pair].partner.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerOln = Math.Round(day.pairInteractions[pair].partner.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerCry = Math.Round(day.pairInteractions[pair].partner.cry, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerInteractionTime = Math.Round(day.pairInteractions[pair].partner.interactionTime, 2);
                    partnerJoinedCry = Math.Round(day.pairInteractions[pair].partner.cryingTime, 2);

                    interactionTime = Math.Round(day.pairInteractions[pair].partner.interactionTime, 2);/////
                    joinedCry = Math.Round(day.pairInteractions[pair].closeAndOrientedCryInSecs, 2) / 2;
                    VD = day.pairInteractions[pair].subject.vd + day.pairInteractions[pair].partner.vd;

                    proximityTime = Math.Round(day.pairInteractions[pair].closeTimeInSecs, 2) / 2;
                    proximityOrientationTime = Math.Round(day.pairInteractions[pair].closeAndOrientedTimeInSecs, 2) / 2;
                    sharedTime = Math.Round(day.pairInteractions[pair].sharedTimeInSecs, 2) / 2; //check on this. correct. 
                    recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;


                    //10/19/20 lkp urgent request
                    subjectVocCountFp = Math.Round(day.pairInteractions[pair].subject.vcFp, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocCountNoFp = Math.Round(day.pairInteractions[pair].subject.vcNoFp, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    partnerVocCountFp = Math.Round(day.pairInteractions[pair].partner.vcFp, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocCountNoFp = Math.Round(day.pairInteractions[pair].partner.vcNoFp, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    proximityOrientationTimeFp = Math.Round(day.pairInteractions[pair].closeAndOrientedTimeInSecsFp, 2) / 2;
                    sharedTimeFp = Math.Round(day.pairInteractions[pair].sharedTimeInSecsFp, 2) / 2; //check on this. correct. 
                    proximityOrientationTimeNoFp = Math.Round(day.pairInteractions[pair].closeAndOrientedTimeInSecsNoFp, 2) / 2;
                    sharedTimeNoFp = Math.Round(day.pairInteractions[pair].sharedTimeInSecsNoFp, 2) / 2; //check on this. correct. 


                    if (subjectAdUttLen>0 || partnerAdUttLen > 0)
                    {
                        bool stop = true;
                    }


                }


                PersonInfo s = day.personTotalCounts.ContainsKey(subject) ? day.personTotalCounts[subject] : new PersonInfo();
                PersonInfo sUbi = day.personTotalCountsWUbi.ContainsKey(subject) ? day.personTotalCountsWUbi[subject] : new PersonInfo();
                PersonInfo p = day.personTotalCounts.ContainsKey(partner) ? day.personTotalCounts[partner] : new PersonInfo();
                PersonInfo pUbi = day.personTotalCountsWUbi.ContainsKey(partner) ? day.personTotalCountsWUbi[partner] : new PersonInfo();

                //7/2/20 chnge to recording time UPDATE DEBUG!
               // subjectTime = Math.Round(s.totalRecMSecs, 2);///////////
               // partnerTime = Math.Round(p.totalRecMSecs, 2);


                Tuple<String, String, String> metrics = getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total",
                    sUbi,
                    s,
                    pUbi,
                    p, absent, absent);////

               /* "Input2_pvc_or_stc" + "," +
                   "Pair Block Talking," +
                   "Pair Talking Duration," +
                   "Subject-Talking-Duration-From_Start," +
                   "Partner-Talking-Duration-From-Start," +
                   "Subject-Talking-Duration-Evenly-Spread," +
                   "Partner-Talking-Duration-Evenly-Spread," +
                   "Subject Turn Count," +
                   "Partner Turn Count," +*/


               subjectLine = (
                       (status) + "," +
                       (statusp) + "," +
                       (nextDay ? "" : (type) + ",") +
                       (nextDay ? "" : (typep) + ",") +/////
                       (absent ? "NA" : typep=="Child" ? partnerVocCount.ToString() : subjectAdCount.ToString()) + "," +
                       (absent ? "NA" : typep == "Child" ? partnerVocCount.ToString() : subjectTurnCount.ToString()) + "," +
                       (absent ? "NA" : typep == "Child" ? partnerVocDur.ToString() : subjectAdUttLen.ToString()) + "," +
                       (absent ? "NA" : VD.ToString()) + "," +////
                       (absent ? "NA" : interactionTime.ToString()) + "," +
                       (sAbsent ? "NA" : subjectInteractionTime.ToString()) + "," +
                       (pAbsent ? "NA" : partnerInteractionTime.ToString()) + "," +
                       (sAbsent ? "NA" : subjectVocDur.ToString()) + "," +
                       (pAbsent ? "NA" : partnerVocDur.ToString()) + "," +
                       (sAbsent ? "NA" : subjectTurnCount.ToString()) + "," +
                       (pAbsent ? "NA" : partnerTurnCount.ToString()) + "," +
                       (sAbsent ? "NA" : subjectVocCount.ToString()) + "," +
                       (pAbsent ? "NA" : partnerVocCount.ToString()) + "," +
                       (sAbsent ? "NA" : subjectAdCount.ToString()) + "," +
                       (pAbsent ? "NA" : partnerAdCount.ToString()) + "," +
                       (sAbsent ? "NA" : subjectNoise.ToString()) + "," +
                       (pAbsent ? "NA" : partnerNoise.ToString()) + "," +
                       (sAbsent ? "NA" : subjectOln.ToString()) + "," +
                       (pAbsent ? "NA" : partnerOln.ToString()) + "," +
                       (sAbsent ? "NA" : subjectCry.ToString()) + "," +
                       (pAbsent ? "NA" : partnerCry.ToString()) + "," +
                       (sAbsent ? "NA" : subjectJoinedCry.ToString()) + "," +
                       (pAbsent ? "NA" : partnerJoinedCry.ToString()) + "," +
                       (absent ? "NA" : joinedCry.ToString()) + "," +
                       (absent ? "NA" : proximityTime.ToString()) + "," +
                       (absent ? "NA" : proximityOrientationTime.ToString()) + "," +
                       (absent ? "NA" : sharedTime.ToString()) + "," +
                       (sAbsent ? "NA" : subjectTime.ToString()) + "," +/////////
                       (pAbsent ? "NA" : partnerTime.ToString()) + "," +
                       (absent ? "NA" : recordingTime.ToString()) + "," +
                        metrics.Item2 );

                partnerLine = (
                    (statusp) + "," +
                    (status) + "," +
                    (nextDay ? "" : (typep) + ",") +
                    (nextDay ? "" : (type) + ",") +
                    (absent ? "NA" : type == "Child" ? subjectVocCount.ToString() : partnerAdCount.ToString()) + "," +
                    (absent ? "NA" : type == "Child" ? subjectVocCount.ToString() : partnerTurnCount.ToString()) + "," +
                    (absent ? "NA" : type == "Child" ? subjectVocDur.ToString() : partnerAdUttLen.ToString()) + "," +
                    (absent ? "NA" : VD.ToString()) + "," +
                    (absent ? "NA" : interactionTime.ToString()) + "," +
                    (pAbsent ? "NA" : partnerInteractionTime.ToString()) + "," +
                    (sAbsent ? "NA" : subjectInteractionTime.ToString()) + "," +
                    (pAbsent ? "NA" : partnerVocDur.ToString()) + "," +
                    (sAbsent ? "NA" : subjectVocDur.ToString()) + "," +
                    (pAbsent ? "NA" : partnerTurnCount.ToString()) + "," +
                    (sAbsent ? "NA" : subjectTurnCount.ToString()) + "," +
                    (pAbsent ? "NA" : partnerVocCount.ToString()) + "," +
                    (sAbsent ? "NA" : subjectVocCount.ToString()) + "," +
                    (pAbsent ? "NA" : partnerAdCount.ToString()) + "," +
                    (sAbsent ? "NA" : subjectAdCount.ToString()) + "," +
                    (pAbsent ? "NA" : partnerNoise.ToString()) + "," +
                    (sAbsent ? "NA" : subjectNoise.ToString()) + "," +
                    (pAbsent ? "NA" : partnerOln.ToString()) + "," +
                    (sAbsent ? "NA" : subjectOln.ToString()) + "," +
                    (pAbsent ? "NA" : partnerCry.ToString()) + "," +
                    (sAbsent ? "NA" : subjectCry.ToString()) + "," +
                    (pAbsent ? "NA" : partnerJoinedCry.ToString()) + "," +
                    (sAbsent ? "NA" : subjectJoinedCry.ToString()) + "," +
                    (absent ? "NA" : joinedCry.ToString()) + "," +
                    (absent ? "NA" : proximityTime.ToString()) + "," +
                    (absent ? "NA" : proximityOrientationTime.ToString()) + "," +
                    (absent ? "NA" : sharedTime.ToString()) + "," +
                    (pAbsent ? "NA" : partnerTime.ToString()) + "," +
                    (sAbsent ? "NA" : subjectTime.ToString()) + "," +
                    (absent ? "NA" : recordingTime.ToString()) + "," +
                     metrics.Item3);

                //10/19/20 lkp urgent request
                if(UL_PROCESSOR_Program.doTempLkpFpVars)
                {
                    subjectLine += ( 
                        (absent ? "NA" : sharedTimeFp.ToString()) + "," + 
                        (sAbsent ? "NA" : subjectVocCountFp.ToString()) + "," +
                        (pAbsent ? "NA" : partnerVocCountFp.ToString()) + ","+
                        (absent ? "NA" : proximityOrientationTimeFp.ToString()) + "," +
                        (absent ? "NA" : sharedTimeNoFp.ToString()) + "," +
                        (sAbsent ? "NA" : subjectVocCountNoFp.ToString()) + "," +
                        (pAbsent ? "NA" : partnerVocCountNoFp.ToString()) + "," +
                        (absent ? "NA" : proximityOrientationTimeNoFp.ToString()));

                    partnerLine += ( 
                        (absent ? "NA" : sharedTimeFp.ToString()) + "," + 
                        (pAbsent ? "NA" : partnerVocCountFp.ToString()) + "," +
                        (sAbsent ? "NA" : subjectVocCountFp.ToString()) + ","+
                        (absent ? "NA" : proximityOrientationTimeFp.ToString()) + "," +
                        (absent ? "NA" : sharedTimeNoFp.ToString()) + "," +
                        (pAbsent ? "NA" : partnerVocCountNoFp.ToString()) + "," +
                        (sAbsent ? "NA" : subjectVocCountNoFp.ToString()) + "," +
                        (absent ? "NA" : proximityOrientationTimeNoFp.ToString()));
                }
                 

                lines[0] = subjectLine;
                lines[1] = partnerLine;
            }
            return lines;
        }
        public static void writeDataSummary(Config configInfo,Boolean getFromIts, Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
        {
            
            using (TextWriter sw = new StreamWriter(file_name, append: append))
            {
                //String title = ",Pair Talking Duration, Subject Talking Duration, Partner Talking Duration,Pair Turn Count, Subject Turn Count, Partner Turn Count, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time, Total Voc Duration, Total Voc Count, Total Turn Count, Total Partner Voc Duration, Total Partner Voc Count, Total Partner Turn Count";
                String title =
                    "Input1_pvc_or_sac" + "," +
                    "Input2_pvc_or_stc" + "," +
                    "Input3_dur_pvd_or_uttl" + "," +  //// 5/18/20TO ADD Input2_DUR_PVD_OR_UTTL
                    "Pair Block Talking," +
                    "Pair Talking Duration,"+ 
                    "Subject-Talking-Duration-From_Start," +
                    "Partner-Talking-Duration-From-Start,"+ 
                    "Subject-Talking-Duration-Evenly-Spread,"+
                    "Partner-Talking-Duration-Evenly-Spread,"+ 
                    "Subject Turn Count,"+ 
                    "Partner Turn Count,"+ 
                    "Subject Voc Count,"+ 
                    "Partner Voc Count,"+ 
                    "Subject Adult Count,"+ 
                    "Partner Adult Count,"+
                    "Subject Noise," + 
                    "Partner Noise,"+ 
                    "Subject OLN,"+ 
                    "Partner OLN,"+ 
                    "Subject Cry,"+ 
                    "Partner Cry,"+ 
                    "Subject Joined Cry,"+ 
                    "Partner Joined Cry,"+ 
                    "Joined Cry,"+ 
                    "Pair Proximity Duration,"+ 
                    "Pair Orientation-Proximity Duration,"+ 
                    "Shared Time in Classroom,"+ 
                    "Subject Time,"+ 
                    "Partner Time,"+ 
                    "Total Recording Time," ;//, "+////
                                            /* "WUBI Total VD " +","+
                                             "Total VD" +","+
                                             "Partner WUBI Total VD " +","+
                                             "Partner Total VD" +","+
                                             "WUBI Total VC " +","+
                                             "Total VC" +","+
                                             "Partner WUBI Total VC " +","+
                                             "Partner Total VC" +","+
                                             "WUBI Total TC " +","+
                                             "Total TC" +","+
                                             "Partner WUBI Total TC " +","+
                                             "Partner Total TC" +","+
                                             "WUBI Total AC " +","+
                                             "Total AC" +","+
                                             "Partner WUBI Total AC " +","+
                                             "Partner Total AC" +","+
                                             "WUBI Total NO " +","+
                                             "Total NO" +","+
                                             "Partner WUBI Total NO " +","+
                                             "Partner Total NO" +","+
                                             "WUBI Total OLN " +","+
                                             "Total OLN" +","+
                                             "Partner WUBI Total OLN " +","+
                                             "Partner Total OLN";*/


                //title += getMetrics(" Total ", "WUBI Total", "Partner Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(),false,false).Item1;//getMetrics("WUBI Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo()).Item1;
                title += getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;//getMetrics("WUBI Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo()).Item1;
                //String header1 = "Date, Subject, Partner, Subject ShortID, Partner ShortID, Subject Diagnosis, Partner Diagnosis, Subject Gender, Partner Gender, Subject Language, Partner Language, Adult,Subject Status, Partner Status,Subject Type,Partner Type, " + title;

                String header1 = "Date, Subject, Partner, Subject ShortID, Partner ShortID";//
                String szAids = "";
                String szLangs = "";
                int iAids = 0;
                int iLangs = 0;
                foreach (String aidName in configInfo.diagnosis.Keys)
                {
                    szAids += (", Subject " + aidName);// +", Partner " + aidName);
                    iAids++;
                }
                foreach (String lang in configInfo.languages.Keys)
                {
                    szLangs += (", Subject " + lang);// + ", Partner " + lang);
                    iLangs++;
                }
                header1 += (iAids>0?(szAids+szAids.Replace("Subject","Partner")):"Subject Diagnosis,Partner Diagnosis,") +
                    (iLangs > 0 ? (szLangs + szLangs.Replace("Subject", "Partner")) : "Subject Language,Partner Language") +
                    ",Subject Gender, Partner Gender, Adult,Subject Status, Partner Status,Subject Type,Partner Type, " + title;
                //Subject Diagnosis, Partner Diagnosis, Subject Gender, Partner Gender, Subject Language, Partner Language, Adult,Subject Status, Partner Status,Subject Type,Partner Type, " + title;


                String header2 = "Lead_Date,Lead_Subject Status, Lead_Partner Status,Lead_" + title.Replace(",", ",Lead_");
                sw.Write(header1.Replace(" ", ""));
                sw.WriteLine(UL_PROCESSOR_Program.doLeads?header2.Replace(" ", "")+"CLASSROOM": UL_PROCESSOR_Program.doTempLkpFpVars ? "SharedTimeinClassroomFP,SubjectVocCountFP,PartnerVocCountFP,PairOrientation_ProximityDurationFP,SharedTimeinClassroomNoFP,SubjectVocCountNoFP,PartnerVocCountNoFP,PairOrientation_ProximityDurationNoFP,CLASSROOM": "CLASSROOM");
                int idx = 0;
                foreach (ClassroomDay day in days)
                {
                    String date = day.startTime.ToShortDateString();
                    idx++;
                    if (idx < days.Count || processLast)
                    {
                        foreach (String pair in day.cf.pairs) //day.pairStats.Keys)
                        {
                            
                            String subject = pair.Split('-')[0];
                            String partner = pair.Split('-')[1];
                            String[] lines = writePairLineNew(day, pair, date, false); ;/// getFromIts? writePairInteractionLine(day, pair, date, false) : writePairLine(day, pair, date, false);
                           

                            Boolean adult = false;
                            if (partner.Contains("T") || partner.Contains("Lab") || subject.Contains("T") || subject.Contains("Lab") || subject.Contains("_L") || subject.Contains("_L"))
                            {
                                adult = true;
                            }


                            MappingRow mr = day.cf.getBaseMapping(subject, day.day);
                            MappingRow mrp = day.cf.getBaseMapping(partner, day.day);

                            //FIX HACK 
                            if(mr.shortBID=="" && mr.longBID!="" && subject.Length> mr.longBID.Length)
                            {
                                mr.shortBID = mr.longBID;
                                mr.longBID = subject;
                            }
                            if (mrp.shortBID == "" && mrp.longBID != "" && partner.Length > mrp.longBID.Length)
                            {
                                mrp.shortBID = mrp.longBID;
                                mrp.longBID = partner;
                            }
                            /*   subjectLine = (
                                      (nextDay ? "" : (mr.longBID) + ",") +
                                      (nextDay ? "" : (mrp.longBID) + ",") +
                                      (status) + "," +
                                      (statusp) + "," +
                                      (nextDay ? "" : (type) + ",") +
                                      (nextDay ? "" : (typep) + ",") +/////
                                      (nextDay ? "" : (mr.aid) + ",") +
                                      (nextDay ? "" : (mrp.aid) + ",") +
                                      (nextDay ? "" : (mr.sex) + ",") +
                                      (nextDay ? "" : (mrp.sex) + ",") +
                                      (nextDay ? "" : (mr.lang) + ",") +
                                      (nextDay ? "" : (mrp.lang) + ",") +*/


                          /*  String szAidsS = !mr.aid.Contains("|")? mr.aid : mr.aid.Split('|').Length == iAids ? String.Join(",", mr.aid.Split('|')) : String.Join(",", mr.aid.Split('|')) + new StringBuilder().Insert(0, ",",iAids - mr.aid.Split('|').Length);
                            String szAidsP = !mrp.aid.Contains("|") ? mrp.aid : mrp.aid.Split('|').Length == iAids ? String.Join(",", mrp.aid.Split('|')) : String.Join(",", mrp.aid.Split('|')) + new StringBuilder().Insert(0, ",", iAids - mrp.aid.Split('|').Length);
                            String szLangS = !mr.lang.Contains("|") ? mr.lang : mr.lang.Split('|').Length == iLangs ? String.Join(",", mr.lang.Split('|')) : String.Join(",", mr.lang.Split('|')) + new StringBuilder().Insert(0, ",", iLangs - mr.lang.Split('|').Length);
                            String szLangP = !mrp.lang.Contains("|") ? mrp.lang : mrp.lang.Split('|').Length == iLangs ? String.Join(",", mrp.lang.Split('|')) : String.Join(",", mrp.lang.Split('|')) + new StringBuilder().Insert(0, ",", iLangs - mrp.lang.Split('|').Length);
*/

                            if (mr.aid.Split('|').Length==0)
                            {
                                bool stop = true;
                            }

                            String szAidsS =  mr.aid.Split('|').Length == iAids ? String.Join(",", mr.aid.Split('|')) : (mr.aid.Split('|').Length>0?String.Join(",", mr.aid.Split('|')):"") + new StringBuilder().Insert(0, "NA,", iAids> mr.aid.Split('|').Length?iAids - mr.aid.Split('|').Length:iAids);
                            String szAidsP =  mrp.aid.Split('|').Length == iAids ? String.Join(",", mrp.aid.Split('|')) : (mrp.aid.Split('|').Length > 0 ? String.Join(",", mrp.aid.Split('|')) : "") + new StringBuilder().Insert(0, "NA,", iAids > mrp.aid.Split('|').Length ? iAids - mrp.aid.Split('|').Length : iAids);
                            String szLangS =  mr.lang.Split('|').Length == iLangs ? String.Join(",", mr.lang.Split('|')) : (mr.lang.Split('|').Length > 0 ? String.Join(",", mr.lang.Split('|')) : "") + new StringBuilder().Insert(0, "NA,", iAids > mr.lang.Split('|').Length ? iLangs - mr.lang.Split('|').Length : iLangs);
                            String szLangP =  mrp.lang.Split('|').Length == iLangs ? String.Join(",", mrp.lang.Split('|')) : (mrp.lang.Split('|').Length > 0 ? String.Join(",", mrp.lang.Split('|')) : "") + new StringBuilder().Insert(0, "NA,", iAids > mrp.lang.Split('|').Length ? iLangs - mrp.lang.Split('|').Length : iLangs);



                            String subjectLine = date + "," + subject + "," + partner + "," + mr.shortBID + "," + mrp.shortBID + "," + szAidsS + "," +szAidsP + "," + szLangS + "," + szLangP + "," + mr.sex + "," + mrp.sex + "," + adult + "," + lines[0];
                            String partnerLine = date + "," + partner + "," + subject + "," + mrp.shortBID + "," + mr.shortBID + "," + szAidsP + "," + szAidsS + "," + szLangP + "," + szLangS + "," + mrp.sex + "," + mr.sex + "," + adult + "," + lines[1];

                            String partnerLine2 = new StringBuilder().Insert(0, "NA,", 59 + (2*(iAids+iLangs))).ToString();//68
                            String subjectLine2 = new StringBuilder().Insert(0, "NA,", 59 + (2 * (iAids + iLangs))).ToString();//68
                            if (idx < days.Count && UL_PROCESSOR_Program.doLeads)
                            {
                                ClassroomDay nextDay = days[idx];
                                String pairN = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                Boolean inversePair = true;

                                if (nextDay.pairStatsSep.ContainsKey(pair) || nextDay.pairInteractions.ContainsKey(pair))
                                {
                                    pairN = pair;
                                    inversePair = false;
                                }
                                else if ((!nextDay.pairStatsSep.ContainsKey(pairN)) && (!nextDay.pairInteractions.ContainsKey(pairN)))
                                {
                                    pairN = "";
                                }
                                if (pairN.Trim() != "")
                                {

                                    String[] linesN = writePairLineNew(nextDay, pairN, nextDay.startTime.ToShortDateString(), true);
                                    ///
                                    if (!inversePair)
                                    {
                                        subjectLine2 = nextDay.startTime.ToShortDateString()+","+linesN[0];
                                        partnerLine2 = nextDay.startTime.ToShortDateString() + "," + linesN[1];
                                    }
                                    else
                                    {
                                        subjectLine2 = nextDay.startTime.ToShortDateString() + "" + linesN[1];
                                        partnerLine2 = nextDay.startTime.ToShortDateString() + "," + linesN[0];
                                    }
                                }

                            }

                             
                            sw.WriteLine(subjectLine + (UL_PROCESSOR_Program.doLeads? subjectLine2:","  ) +day.cf.classroom);
                            sw.WriteLine(partnerLine + (UL_PROCESSOR_Program.doLeads ? partnerLine2 : ",") + day.cf.classroom);

                        }
                    }

                }
            }
        }
     }

}
