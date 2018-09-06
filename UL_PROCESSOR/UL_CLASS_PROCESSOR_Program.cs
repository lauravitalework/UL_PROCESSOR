using System;
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
        public Boolean doSummary = true;
        public String fileName = "V1";

        public Boolean doTenFiles = true;
        public Boolean doSumDayFiles = true;
        public Boolean doSumAllFiles = true;
        public Boolean doAngleFiles = true;
        public Boolean getFromIts = false;
        public Boolean doGR = true;
        public Boolean doVel = true;
        public List<String> fileNames = new List<string>();

        public UL_CLASS_PROCESSOR_Program(Config c, List<DateTime> d, String fName, Boolean t, Boolean sd, Boolean s, Boolean a, Boolean its, Boolean GR, Boolean VEL)
        {
            doTenFiles = t;
            doSumDayFiles = sd;
            doSumAllFiles = s;
            doAngleFiles = a;
            configInfo = c;
            days = d;
            configInfo.readMappings();
            fileName = fName;
            configInfo.version = fileName;
            getFromIts = its;
            doGR = GR;
            doVel = VEL;
        }

        public void process(Boolean processLast)
        {
            List<ClassroomDay> cds = new List<ClassroomDay>();
            int countSets = 0;
            foreach (DateTime day in days)
            {

                List<String> subs = new List<string>();
                //subs.Add("1B");
                //subs.Add("2B");
                //subs.Add("3B");
                //subs = new List<string>();

                ClassroomDay cd = new ClassroomDay(day, configInfo);
                Console.WriteLine("PROCESSING " + configInfo.classroom + " " + day.ToShortDateString());

                Console.WriteLine("readLenaFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Console.WriteLine("getFromIts " + getFromIts);
                Dictionary<String, List<PersonInfo>> rawLena = getFromIts ? cd.readLenaItsFiles() : cd.readLenaFile();


                Console.WriteLine("readUbiFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Dictionary<String, List<PersonInfo>> rawUbi = cd.readUbiFile(new List<string>());
                /*Console.WriteLine("readUbiTagFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> rawTags = cd.readUbiTagFile();
                */
                //Dictionary<String, List<PersonInfo>> rawLenaIts = getFromIts? cd.readLenaItsFiles():new Dictionary<string, List<PersonInfo>>();
                //cd.readLenaItsFiles();
                Console.WriteLine("setUbiData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setUbiData(rawUbi, rawLena);

                DateTime trunkAt = cd.getTrunkTime();//gets first end track time from last ten minutes of tracking.


                Console.WriteLine("readUbiTagFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> rawTags = cd.readUbiTagFile(rawLena, doGR, new List<string>());//, subs);

                Console.WriteLine("setUbiTagData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setUbiTagData(rawTags.Item1, rawTags.Item2);
                Console.WriteLine("setLenaData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                //cd.setLenaData1(rawLena);
                if(getFromIts)
                cd.setLenaItsData(rawLena);
                else
                cd.setLenaData(rawLena);
                Console.WriteLine("setLenaData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                //cd.setLenaData2(rawLenaIts);
                //DateTime 
                trunkAt = cd.getTrunkTime();//gets first end track time from last ten minutes of tracking.
                Console.WriteLine("countInteractions (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");


               
                if (doAngleFiles || doTenFiles || doSumAllFiles || doSumDayFiles)
                {
                    //if (getFromIts)
                    //    cd.countInteractionsIts(doAngleFiles, doAngleFiles, true, trunkAt, rawLena); //count interactions but no need to write a file 
                    //else
                        cd.countInteractionsNew(subs,doAngleFiles, doAngleFiles, true, trunkAt, rawLena); //count interactions but no need to write a file
                                                                                                  //cd.countInteractions(subs, doAngleFiles, doAngleFiles, true, trunkAt, rawLena); //count interactions but no need to write a file
                    if (doTenFiles || doVel)
                    {
                        Console.WriteLine("write10SecTalkingCSV (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                        String fileNameTen = configInfo.root + configInfo.classroom + "/SYNC/" + configInfo.version + "10THOFSECTALKING_" + (configInfo.justFreePlay ? "freeplay_" : "") + (cd.startFromLena ? "wlena_" : "wolena_") + day.Month + "_" + day.Day + "_" + day.Year + ".CSV";
                        //subs = new List<string>();
                        //cd.write10SecCryCSV(fileNameTen, subs); //write complete data files to disc
                        cd.write10SecTalkingCSV(fileNameTen, subs, doVel); //write complete data files to disc
                    }
                    cds.Add(cd);
                }
            }
            summarize(cds, countSets, processLast);
        }
        public Boolean doTotals = false;
        public void summarize(List<ClassroomDay> cds, int c, Boolean processLast)
        {
            if (doSumAllFiles || doSumDayFiles)
            {
                foreach (ClassroomDay cd in cds)
                {

                    try
                    {
                        Console.WriteLine("Summarizing " + cd.day.Month + " " + cd.day.Year + " " + cd.day.Day + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                        List<ClassroomDay> oneDay = new List<ClassroomDay>();//sync3_3pairactivity_vo8.CSV
                        oneDay.Add(cd);
                        Console.WriteLine("Summarizing ONE");
                        if (doSumDayFiles)
                        {
                            if (doTotals)
                                writeDataSummaryTotals(false, configInfo.root + configInfo.classroom + "/SYNC/" + configInfo.version + "PAIRACTIVITY_" + c.ToString() + "_" + configInfo.classroom + "_" + (cd.justProx ? "no_" : "") + (configInfo.justFreePlay ? "freeplay_" : "") + cd.day.Month + "_" + cd.day.Day + "_" + cd.day.Year + ".csv", oneDay, true); //write summary data
                            else
                                writeDataSummary( getFromIts, false, configInfo.root + configInfo.classroom + "/SYNC/"+ configInfo.version+"PAIRACTIVITY_"+ c.ToString() + "_" + configInfo.classroom + "_" + (cd.justProx ? "no_" : "") + (configInfo.justFreePlay ? "freeplay_" : "") + cd.day.Month + "_" + cd.day.Day + "_" + cd.day.Year + ".csv", oneDay, true); //write summary data
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                Console.WriteLine("Summarizing ALL " + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");

                if (doSumAllFiles)
                {
                    String fileNameSum = configInfo.root + configInfo.classroom + "/SYNC/"+ configInfo.version+"PAIRACTIVITY_ALL_" + configInfo.classroom + (configInfo.justFreePlay ? "_freeplay" : "") + ".CSV";

                    if (doTotals)
                    {
                        fileNameSum = fileNameSum.Replace(".", "_TOTALS.");
                        writeDataSummaryTotals(c > 0, fileNameSum, cds, processLast); //write summary data
                    }
                    else
                        writeDataSummary(getFromIts, c > 0, fileNameSum, cds, processLast); //write summary data
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

        public static String[] writePairInteractionLine(ClassroomDay day, String pair, String date, Boolean nextDay)
        {
            String[] lines = new String[2];
            String subject = pair.Split('-')[0];
            String partner = pair.Split('-')[1];
            String status = "present";
            String statusp = "present";
            Boolean sAbsent = day.cf.getMapping(subject, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
            Boolean pAbsent = day.cf.getMapping(partner, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(partner)); ;
            Boolean lAbsent = (!day.startLenaTimes.ContainsKey(subject)) || (!day.startLenaTimes.ContainsKey(partner));
            Boolean dAbsent = !day.pairInteractions.ContainsKey(pair);
            status = sAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ? "No Data" : "present");
            statusp = pAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(partner)) || dAbsent ? "No Data" : "present");
            Boolean absent = sAbsent || pAbsent || dAbsent || lAbsent;
            String type = subject.IndexOf("Lab") == 0 ? "Lab" : subject.IndexOf("T") == 0 ? "Teacher" : "Child";
            String typep = partner.IndexOf("Lab") == 0 ? "Lab" : partner.IndexOf("T") == 0 ? "Teacher" : "Child";
            sAbsent = absent;
            pAbsent = absent;
            dAbsent = absent;
            lAbsent = absent;
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
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

                if (!absent)
                {
                    PairInfo pi = day.pairInteractions[pair];
                    subjectTime = pi.subject.individualTime;
                    partnerTime = pi.partner.individualTime;
                    subjectTurnCount = Math.Round(pi.subject.tc, 2);
                    subjectVocCount = Math.Round(pi.subject.vc, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocDur = Math.Round(pi.subject.vd, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdCount = Math.Round(pi.subject.ac, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectNoise = Math.Round(pi.subject.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectOln = Math.Round(pi.subject.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectCry = Math.Round(pi.subject.cry, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectInteractionTime = Math.Round(pi.subject.interactionTime, 2);
                    partnerTurnCount = Math.Round(pi.partner.tc, 2);
                    subjectJoinedCry = Math.Round(pi.closeAndOrientedCryInSecs, 2);

                    if (subject == "17A" && partner == "14A" || subject == "14A" && partner == "17A")
                    {
                        subject = subject;
                    }
                    partnerVocCount = Math.Round(pi.partner.vc, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocDur = Math.Round(pi.partner.vd, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdCount = Math.Round(pi.partner.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerNoise = Math.Round(pi.partner.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerOln = Math.Round(pi.partner.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerCry = Math.Round(pi.partner.cry, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerInteractionTime = Math.Round(pi.partner.interactionTime, 2);
                    partnerJoinedCry = Math.Round(pi.closeAndOrientedCryInSecs, 2);

                    interactionTime = pi.closeAndOrientedTalkInSecs;// pi.subject.interactionTime + pi.partner.interactionTime;
                    joinedCry = Math.Round( pi.closeAndOrientedCryInSecs, 2);
                    VD = pi.closeAndOrienteVDInSecs;// pi.subject.vd + pi.partner.vd;

                    proximityTime = Math.Round(pi.closeTimeInSecs, 2)  ;
                    proximityOrientationTime = Math.Round(pi.closeAndOrientedTimeInSecs, 2)  ;
                    sharedTime = Math.Round(pi.sharedTimeInSecs, 2); //check on this. correct. 
                    recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;
                }


                PersonInfo s = day.personTotalCounts.ContainsKey(subject) ? day.personTotalCounts[subject] : new PersonInfo();
                PersonInfo sUbi = day.personTotalCountsWUbi.ContainsKey(subject) ? day.personTotalCountsWUbi[subject] : new PersonInfo();
                PersonInfo p = day.personTotalCounts.ContainsKey(partner) ? day.personTotalCounts[partner] : new PersonInfo();
                PersonInfo pUbi = day.personTotalCountsWUbi.ContainsKey(partner) ? day.personTotalCountsWUbi[partner] : new PersonInfo();


                Tuple<String, String, String> metrics = getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total",
                    sUbi,
                    s,
                    pUbi,
                    p, absent, absent);////

                subjectLine = (
                       (status) + "," +
                       (statusp) + "," +
                       (nextDay ? "" : (type) + ",") +
                       (nextDay ? "" : (typep) + ",") +
                       (absent ? "NA" : VD.ToString()) + "," +
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
                       (sAbsent ? "NA" : subjectTime.ToString()) + "," +
                       (pAbsent ? "NA" : partnerTime.ToString()) + "," +
                       (absent ? "NA" : recordingTime.ToString()) + "," +
                        metrics.Item2);

                partnerLine = (
                    (statusp) + "," +
                    (status) + "," +
                    (nextDay ? "" : (typep) + ",") +
                    (nextDay ? "" : (type) + ",") +
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

                lines[0] = subjectLine;
                lines[1] = partnerLine;
            }
            return lines;
        }
        public static String[] writePairLineNew(ClassroomDay day, String pair, String date, Boolean nextDay)
        {
            String[] lines = new String[2];
            String subject = pair.Split('-')[0];
            String partner = pair.Split('-')[1];
            String status = "present";
            String statusp = "present";
            Boolean sAbsent = day.cf.getMapping(subject, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
            Boolean pAbsent = day.cf.getMapping(partner, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(partner)); ;
            Boolean lAbsent = (!day.startLenaTimes.ContainsKey(subject)) || (!day.startLenaTimes.ContainsKey(partner));
            Boolean dAbsent = !day.pairInteractions.ContainsKey(pair);
            status = sAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ? "No Data" : "present");
            statusp = pAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(partner)) || dAbsent ? "No Data" : "present");
            Boolean absent = sAbsent || pAbsent || dAbsent || lAbsent;
            String type = subject.IndexOf("Lab") == 0 ? "Lab" : subject.IndexOf("T") == 0 ? "Teacher" : "Child";
            String typep = partner.IndexOf("Lab") == 0 ? "Lab" : partner.IndexOf("T") == 0 ? "Teacher" : "Child";
            sAbsent = absent;
            pAbsent = absent;
            dAbsent = absent;
            lAbsent = absent;

            if (pair == "1B-2B")
            {
                int stop = 1;
            }
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
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
                    subjectTime= Math.Round(day.pairInteractions[pair].subject.individualTime, 2);
                    partnerTime = Math.Round(day.pairInteractions[pair].partner.individualTime, 2);

                    subjectTurnCount = Math.Round(day.pairInteractions[pair].subject.tc, 2); //Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    subjectVocCount = Math.Round(day.pairInteractions[pair].subject.vc, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocDur = Math.Round(day.pairInteractions[pair].subject.vd, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdCount = Math.Round(day.pairInteractions[pair].subject.ac, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectNoise = Math.Round(day.pairInteractions[pair].subject.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectOln = Math.Round(day.pairInteractions[pair].subject.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectCry = Math.Round(day.pairInteractions[pair].subject.cry, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectInteractionTime = Math.Round(day.pairInteractions[pair].subject.interactionTime, 2);
                    partnerTurnCount = Math.Round(day.pairInteractions[pair].partner.tc, 2);
                    subjectJoinedCry = Math.Round(day.pairInteractions[pair].subject.cryingTime, 2);

                    if (subject == "17A" && partner == "14A" || subject == "14A" && partner == "17A")
                    {
                        subject = subject;
                    }
                    partnerVocCount = Math.Round(day.pairInteractions[pair].partner.vc, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocDur = Math.Round(day.pairInteractions[pair].partner.vd, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdCount = Math.Round(day.pairInteractions[pair].partner.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerNoise = Math.Round(day.pairInteractions[pair].partner.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerOln = Math.Round(day.pairInteractions[pair].partner.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerCry = Math.Round(day.pairInteractions[pair].partner.cry, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerInteractionTime = Math.Round(day.pairInteractions[pair].partner.interactionTime, 2);
                    partnerJoinedCry = Math.Round(day.pairInteractions[pair].partner.cryingTime, 2);

                    interactionTime = Math.Round(day.pairInteractions[pair].partner.interactionTime, 2);
                    joinedCry = Math.Round(day.pairInteractions[pair].closeAndOrientedCryInSecs, 2) / 2;
                    VD = day.pairInteractions[pair].subject.vd + day.pairInteractions[pair].partner.vd;

                    proximityTime = Math.Round(day.pairInteractions[pair].closeTimeInSecs, 2) / 2;
                    proximityOrientationTime = Math.Round(day.pairInteractions[pair].closeAndOrientedTimeInSecs, 2) / 2;
                    sharedTime = Math.Round(day.pairInteractions[pair].sharedTimeInSecs, 2) / 2; //check on this. correct. 
                    recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;
                }


                PersonInfo s = day.personTotalCounts.ContainsKey(subject) ? day.personTotalCounts[subject] : new PersonInfo();
                PersonInfo sUbi = day.personTotalCountsWUbi.ContainsKey(subject) ? day.personTotalCountsWUbi[subject] : new PersonInfo();
                PersonInfo p = day.personTotalCounts.ContainsKey(partner) ? day.personTotalCounts[partner] : new PersonInfo();
                PersonInfo pUbi = day.personTotalCountsWUbi.ContainsKey(partner) ? day.personTotalCountsWUbi[partner] : new PersonInfo();


                Tuple<String, String, String> metrics = getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total",
                    sUbi,
                    s,
                    pUbi,
                    p, absent, absent);////

                subjectLine = (
                       (status) + "," +
                       (statusp) + "," +
                       (nextDay ? "" : (type) + ",") +
                       (nextDay ? "" : (typep) + ",") +
                       (absent ? "NA" : VD.ToString()) + "," +
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
                       (sAbsent ? "NA" : subjectTime.ToString()) + "," +
                       (pAbsent ? "NA" : partnerTime.ToString()) + "," +
                       (absent ? "NA" : recordingTime.ToString()) + "," +
                        metrics.Item2);

                partnerLine = (
                    (statusp) + "," +
                    (status) + "," +
                    (nextDay ? "" : (typep) + ",") +
                    (nextDay ? "" : (type) + ",") +
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

                lines[0] = subjectLine;
                lines[1] = partnerLine;
            }
            return lines;
        }
        public static String[] writePairLine(ClassroomDay day, String pair, String date, Boolean nextDay)
        {
            String[] lines = new String[2];
            String subject = pair.Split('-')[0];
            String partner = pair.Split('-')[1];
            String status = "present";
            String statusp = "present";
            Boolean sAbsent = day.cf.getMapping(subject, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
            Boolean pAbsent = day.cf.getMapping(partner, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(partner)); ;
            Boolean lAbsent = (!day.startLenaTimes.ContainsKey(subject)) || (!day.startLenaTimes.ContainsKey(partner));
            Boolean dAbsent = !day.pairStatsSep.ContainsKey(pair);
            status = sAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ? "No Data" : "present");
            statusp = pAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(partner)) || dAbsent ? "No Data" : "present");
            Boolean absent = sAbsent || pAbsent || dAbsent || lAbsent;
            String type = subject.IndexOf("Lab") == 0 ? "Lab" : subject.IndexOf("T") == 0 ? "Teacher" : "Child";
            String typep = partner.IndexOf("Lab") == 0 ? "Lab" : partner.IndexOf("T") == 0 ? "Teacher" : "Child";
            sAbsent = absent;
            pAbsent = absent;
            dAbsent = absent;
            lAbsent = absent;

            if(pair=="12A-13A")
            {
                int stop = 1;
            }
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", (nextDay ? 63 : 63)).ToString();
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

                if (!absent)
                {
                    if (day.individualTime.ContainsKey(subject))
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

                    }

                    subjectTurnCount = Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    subjectVocCount = Math.Round(day.pairStatsSep[pair].p1.vc, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocDur = Math.Round(day.pairStatsSep[pair].p1.vd, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdCount = Math.Round(day.pairStatsSep[pair].p1.ac, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectNoise = Math.Round(day.pairStatsSep[pair].p1.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectOln = Math.Round(day.pairStatsSep[pair].p1.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectCry = Math.Round(day.pairStatsSep[pair].p1.cry, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectInteractionTime = Math.Round(day.pairStatsSeparated[pair].Item1, 2);
                    partnerTurnCount = Math.Round(day.pairStatsSep[pair].p2.tc, 2);
                    subjectJoinedCry = Math.Round(day.pairStatsSeparated[pair].Item3, 2);  

                    if(subject=="17A" && partner=="14A"|| subject == "14A" && partner == "17A")
                    {
                        subject = subject;
                    }
                    partnerVocCount = Math.Round(day.pairStatsSep[pair].p2.vc, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocDur = Math.Round(day.pairStatsSep[pair].p2.vd, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdCount = Math.Round(day.pairStatsSep[pair].p2.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerNoise = Math.Round(day.pairStatsSep[pair].p2.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerOln = Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerCry = Math.Round(day.pairStatsSep[pair].p2.cry, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerInteractionTime = Math.Round(day.pairStatsSeparated[pair].Item2, 2);
                    partnerJoinedCry = Math.Round(day.pairStatsSeparated[pair].Item4, 2);

                    interactionTime = Math.Round(day.pairStats[pair], 2);
                    joinedCry = Math.Round(day.pairCry[pair], 2)/2;
                    VD = day.pairStatsSep[pair].p1.vd + day.pairStatsSep[pair].p2.vd;

                    proximityTime = Math.Round(day.pairClose[pair], 2) / 2;
                    proximityOrientationTime = Math.Round(day.pairCloseOrientation[pair], 2) / 2;
                    sharedTime = Math.Round(day.pairTime[pair], 2) / 2; //check on this. correct. 
                    recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;
                }


                PersonInfo s = day.personTotalCounts.ContainsKey(subject) ? day.personTotalCounts[subject] : new PersonInfo();
                PersonInfo sUbi = day.personTotalCountsWUbi.ContainsKey(subject) ? day.personTotalCountsWUbi[subject] : new PersonInfo();
                PersonInfo p = day.personTotalCounts.ContainsKey(partner) ? day.personTotalCounts[partner] : new PersonInfo();
                PersonInfo pUbi = day.personTotalCountsWUbi.ContainsKey(partner) ? day.personTotalCountsWUbi[partner] : new PersonInfo();


                Tuple<String, String, String> metrics = getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total",
                    sUbi,
                    s,
                    pUbi,
                    p, absent, absent);////

                subjectLine = (
                       (status) + "," +
                       (statusp) + "," +
                       (nextDay ? "" : (type) + ",") +
                       (nextDay ? "" : (typep) + ",") +
                       (absent ? "NA" : VD.ToString()) + "," +
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
                       (sAbsent ? "NA" : subjectTime.ToString()) + "," +
                       (pAbsent ? "NA" : partnerTime.ToString()) + "," +
                       (absent ? "NA" : recordingTime.ToString()) + "," +
                        metrics.Item2);

                partnerLine = (
                    (statusp) + "," +
                    (status) + "," +
                    (nextDay ? "" : (typep) + ",") +
                    (nextDay ? "" : (type) + ",") +
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

                lines[0] = subjectLine;
                lines[1] = partnerLine;
            }
            return lines;
        }
        public static String[] writeTotalPairLine(ClassroomDay day, String pair, String date, Boolean nextDay)
        {
            return writeTotalPairLine( day, pair, date, nextDay, true,true);
        }
        public static String[] writeTotalPairLine(ClassroomDay day, String pair, String date, Boolean nextDay, Boolean l, Boolean u)
        {
            String[] lines = new String[2];
            String subject = pair.Split('-')[0];
            String partner = pair.Split('-')[1];
            String status = "present";
            String statusp = "present";
            Boolean sAbsent = day.cf.getMapping(subject, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
            Boolean pAbsent = day.cf.getMapping(partner, day.day).isAbsent(day.day);//|| (!day.startLenaTimes.ContainsKey(partner)); ;
            Boolean lAbsent = (!day.startLenaTimes.ContainsKey(subject)) || (!day.startLenaTimes.ContainsKey(partner));
            Boolean dAbsent = !day.pairStatsSep.ContainsKey(pair);
            status = sAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(subject)) || dAbsent ? "No Data" : "present");
            statusp = pAbsent ? "absent" : ((!day.startLenaTimes.ContainsKey(partner)) || dAbsent ? "No Data" : "present");
            Boolean absent = sAbsent || pAbsent || dAbsent || lAbsent;
            String type = subject.IndexOf("Lab") == 0 ? "Lab" : subject.IndexOf("T") == 0 ? "Teacher" : "Child";
            String typep = partner.IndexOf("Lab") == 0 ? "Lab" : partner.IndexOf("T") == 0 ? "Teacher" : "Child";
            sAbsent = absent;
            pAbsent = absent;
            dAbsent = absent;
            lAbsent = absent;
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
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
                double subjectAdCount = 0;
                double partnerAdCount = 0; //Math.Round(day.pairStatsSep[pair].p2.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectNoise = 0; //Math.Round(day.pairStatsSep[pair].p1.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                double partnerNoise = 0; //Math.Round(day.pairStatsSep[pair].p2.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                double subjectOln = 0; //Math.Round(day.pairStatsSep[pair].p1.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                double partnerOln = 0; // Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
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

                if (!absent)
                {
                    if (day.individualTime.ContainsKey(subject))
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

                    }

                    subjectTurnCount = Math.Round(day.pairStatsSep[pair].p1.tc, 2);
                    subjectVocCount = Math.Round(day.pairStatsSep[pair].p1.vc, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectVocDur = Math.Round(day.pairStatsSep[pair].p1.vd, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectAdCount = Math.Round(day.pairStatsSep[pair].p1.ac, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectNoise = Math.Round(day.pairStatsSep[pair].p1.no, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectOln = Math.Round(day.pairStatsSep[pair].p1.oln, 2);//.Round(day.pairStatsSeparatedVC[pair].Item1, 2);
                    subjectInteractionTime = Math.Round(day.pairStatsSeparated[pair].Item1, 2); partnerTurnCount = Math.Round(day.pairStatsSep[pair].p2.tc, 2);

                    partnerVocCount = Math.Round(day.pairStatsSep[pair].p2.vc, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerVocDur = Math.Round(day.pairStatsSep[pair].p2.vd, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerAdCount = Math.Round(day.pairStatsSep[pair].p2.ac, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerNoise = Math.Round(day.pairStatsSep[pair].p2.no, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerOln = Math.Round(day.pairStatsSep[pair].p2.oln, 2);// Math.Round(day.pairStatsSeparatedVC[pair].Item2, 2);
                    partnerInteractionTime = Math.Round(day.pairStatsSeparated[pair].Item2, 2);

                    interactionTime = Math.Round(day.pairStats[pair], 2);
                    VD = day.pairStatsSep[pair].p1.vd + day.pairStatsSep[pair].p2.vd;

                    proximityTime = Math.Round(day.pairClose[pair], 2) / 2;
                    proximityOrientationTime = Math.Round(day.pairCloseOrientation[pair], 2) / 2;
                    sharedTime = Math.Round(day.pairTime[pair], 2) / 2; //check on this. correct. 
                    recordingTime = (day.endTime.Hour - day.startTime.Hour) * 60 * 60 + (day.endTime.Minute - day.startTime.Minute) * 60 + (day.endTime.Second - day.startTime.Second) + (day.endTime.Millisecond - day.startTime.Millisecond) / 1000;
                }


                PersonInfo s = l && day.personTotalCounts.ContainsKey(subject) ? day.personTotalCounts[subject] : new PersonInfo();
                PersonInfo sUbi = u && day.personTotalCountsWUbi.ContainsKey(subject) ? day.personTotalCountsWUbi[subject] : new PersonInfo();
                PersonInfo p = l && day.personTotalCounts.ContainsKey(partner) ? day.personTotalCounts[partner] : new PersonInfo();
                PersonInfo pUbi = u && day.personTotalCountsWUbi.ContainsKey(partner) ? day.personTotalCountsWUbi[partner] : new PersonInfo();


                Tuple<String, String, String> metrics = getMetrics("WUBI Total ", "Total", "Partner WUBI Total ", "Partner Total",
                    sUbi,
                    s,
                    pUbi,
                    p, absent, absent);////

                subjectLine = (
                       (status) + "," +
                       (statusp) + "," +
                       (nextDay ? "" : (type) + ",") +
                       (nextDay ? "" : (typep) + ",") +
                       (absent ? "NA" : recordingTime.ToString()) + "," +
                        metrics.Item2);

                partnerLine = (
                    (statusp) + "," +
                    (status) + "," +
                    (nextDay ? "" : (typep) + ",") +
                    (nextDay ? "" : (type) + ",") +
                    (absent ? "NA" : recordingTime.ToString()) + "," +
                     metrics.Item3);

                lines[0] = subjectLine;
                lines[1] = partnerLine;
            }
            return lines;
        }
        public static void writeDataSummary(Boolean getFromIts, Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
        {
            
            using (TextWriter sw = new StreamWriter(file_name, append: append))
            {
                //String title = ",Pair Talking Duration, Subject Talking Duration, Partner Talking Duration,Pair Turn Count, Subject Turn Count, Partner Turn Count, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time, Total Voc Duration, Total Voc Count, Total Turn Count, Total Partner Voc Duration, Total Partner Voc Count, Total Partner Turn Count";
                String title =
                    "Pair Block Talking," +
                    "Pair Talking Duration, " +
                    "Subject-Talking-Duration-From_Start," +
                    "Partner-Talking-Duration-From-Start, " +
                    "Subject-Talking-Duration-Evenly-Spread," +
                    "Partner-Talking-Duration-Evenly-Spread, " +
                    "Subject Turn Count, " +
                    "Partner Turn Count, " +
                    "Subject Voc Count, " +
                    "Partner Voc Count, " +
                    "Subject Adult Count, " +
                    "Partner Adult Count, " +
                    "Subject Noise, " +
                    "Partner Noise, " +
                    "Subject OLN, " +
                    "Partner OLN, " +
                    "Subject Cry, " +
                    "Partner Cry, " +
                    "Subject Joined Cry, " +
                    "Partner Joined Cry, " +
                    "Joined Cry, " +
                    "Pair Proximity Duration, " +
                    "Pair Orientation-Proximity Duration, " +
                    "Shared Time in Classroom, " +
                    "Subject Time, " +
                    "Partner Time, " +
                    "Total Recording Time,";//, "+
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
                String header1 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + title;
                String header2 = "Lead_Subject Status, Lead_Partner Status,Lead_" + title.Replace(",", ",Lead_");
                sw.Write(header1.Replace(" ", ""));
                sw.WriteLine(header2.Replace(" ", ""));
                int idx = 0;
                foreach (ClassroomDay day in days)
                {
                    String date = day.startTime.ToShortDateString();
                    idx++;
                    if (idx < days.Count || processLast)
                    {
                        foreach (String pair in day.cf.pairs) //day.pairStats.Keys)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                            {
                                int t = 0;
                            }
                            String subject = pair.Split('-')[0];
                            String partner = pair.Split('-')[1];
                            String[] lines = writePairLineNew(day, pair, date, false); ;/// getFromIts? writePairInteractionLine(day, pair, date, false) : writePairLine(day, pair, date, false);
                            if (pair == "12A-13A")
                            {
                                int stop = 1;
                            }


                            Boolean adult = false;
                            if (partner.Contains("T") || partner.Contains("Lab") || subject.Contains("T") || subject.Contains("Lab"))
                            {
                                adult = true;
                            }
                            String subjectLine = date + "," + subject + "," + partner + "," + adult + "," + lines[0];
                            String partnerLine = date + "," + partner + "," + subject + "," + adult + "," + lines[1];

                            String partnerLine2 = new StringBuilder().Insert(0, "NA,", 65).ToString();
                            String subjectLine2 = new StringBuilder().Insert(0, "NA,", 65).ToString();
                            if (idx < days.Count)
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

                                    if (!inversePair)
                                    {
                                        subjectLine2 = linesN[0];
                                        partnerLine2 = linesN[1];
                                    }
                                    else
                                    {
                                        subjectLine2 = linesN[1];
                                        partnerLine2 = linesN[0];
                                    }
                                }

                            }

                            sw.WriteLine(subjectLine + subjectLine2);
                            sw.WriteLine(partnerLine + partnerLine2);

                        }
                    }

                }
            }
        }
        public static void writeDataSummaryTotals(Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
        {
            writeDataSummaryTotals(append, file_name, days, processLast, true, false);
        }
        public static void writeDataSummaryTotals(Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast, Boolean l, Boolean u)
        {
            using (TextWriter sw = new StreamWriter(file_name, append: append))
            {
                //String title = ",Pair Talking Duration, Subject Talking Duration, Partner Talking Duration,Pair Turn Count, Subject Turn Count, Partner Turn Count, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time, Total Voc Duration, Total Voc Count, Total Turn Count, Total Partner Voc Duration, Total Partner Voc Count, Total Partner Turn Count";
                String title =
                    "Total Recording Time,";
                title += getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;//getMetrics("WUBI Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo()).Item1;
                String header1 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + title;
                String header2 = "Lead_Subject Status, Lead_Partner Status,Lead_" + title.Replace(",", ",Lead_");
                sw.Write(header1.Replace(" ", ""));
                sw.WriteLine(header2.Replace(" ", ""));
                int idx = 0;
                foreach (ClassroomDay day in days)
                {
                    String date = day.startTime.ToShortDateString();
                    idx++;
                    if (idx < days.Count || processLast)
                    {
                        foreach (String pair in day.cf.pairs) //day.pairStats.Keys)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                            {
                                int t = 0;
                            }
                            String subject = pair.Split('-')[0];
                            String partner = pair.Split('-')[1];
                            String[] lines = writeTotalPairLine(day, pair, date, false,l,u);
                            Boolean adult = false;
                            if (partner.Contains("T") || partner.Contains("Lab") || subject.Contains("T") || subject.Contains("Lab"))
                            {
                                adult = true;
                            }
                            String subjectLine = date + "," + subject + "," + partner + "," + adult + "," + lines[0];
                            String partnerLine = date + "," + partner + "," + subject + "," + adult + "," + lines[1];

                            String partnerLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            String subjectLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            if (idx < days.Count)
                            {
                                ClassroomDay nextDay = days[idx];
                                String pairN = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                Boolean inversePair = true;

                                if (nextDay.pairStatsSep.ContainsKey(pair))
                                {
                                    pairN = pair;
                                    inversePair = false;
                                }
                                else if (!nextDay.pairStatsSep.ContainsKey(pairN))
                                {
                                    pairN = "";
                                }
                                if (pairN.Trim() != "")
                                {

                                    String[] linesN = writeTotalPairLine(nextDay, pairN, nextDay.startTime.ToShortDateString(), true,l,u);

                                    if (!inversePair)
                                    {
                                        subjectLine2 = linesN[0];
                                        partnerLine2 = linesN[1];
                                    }
                                    else
                                    {
                                        subjectLine2 = linesN[1];
                                        partnerLine2 = linesN[0];
                                    }
                                }

                            }

                            sw.WriteLine(subjectLine + subjectLine2);
                            sw.WriteLine(partnerLine + partnerLine2);

                        }
                    }

                }
            }
        }
        public static void writeDataSummaryTotalsLena(Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
        {
            using (TextWriter sw = new StreamWriter(file_name, append: append))
            {
                //String title = ",Pair Talking Duration, Subject Talking Duration, Partner Talking Duration,Pair Turn Count, Subject Turn Count, Partner Turn Count, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time, Total Voc Duration, Total Voc Count, Total Turn Count, Total Partner Voc Duration, Total Partner Voc Count, Total Partner Turn Count";
                String title =
                    "Total Recording Time,";
                title += getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;//getMetrics("WUBI Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo()).Item1;
                String header1 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + title;
                String header2 = "Lead_Subject Status, Lead_Partner Status,Lead_" + title.Replace(",", ",Lead_");
                sw.Write(header1.Replace(" ", ""));
                sw.WriteLine(header2.Replace(" ", ""));
                int idx = 0;
                foreach (ClassroomDay day in days)
                {
                    String date = day.startTime.ToShortDateString();
                    idx++;
                    if (idx < days.Count || processLast)
                    {
                        foreach (String pair in day.cf.pairs) //day.pairStats.Keys)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                            {
                                int t = 0;
                            }
                            String subject = pair.Split('-')[0];
                            String partner = pair.Split('-')[1];
                            String[] lines = writeTotalPairLine(day, pair, date, false);
                            Boolean adult = false;
                            if (partner.Contains("T") || partner.Contains("Lab") || subject.Contains("T") || subject.Contains("Lab"))
                            {
                                adult = true;
                            }
                            String subjectLine = date + "," + subject + "," + partner + "," + adult + "," + lines[0];
                            String partnerLine = date + "," + partner + "," + subject + "," + adult + "," + lines[1];

                            String partnerLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            String subjectLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            if (idx < days.Count)
                            {
                                ClassroomDay nextDay = days[idx];
                                String pairN = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                Boolean inversePair = true;

                                if (nextDay.pairStatsSep.ContainsKey(pair))
                                {
                                    pairN = pair;
                                    inversePair = false;
                                }
                                else if (!nextDay.pairStatsSep.ContainsKey(pairN))
                                {
                                    pairN = "";
                                }
                                if (pairN.Trim() != "")
                                {

                                    String[] linesN = writeTotalPairLine(nextDay, pairN, nextDay.startTime.ToShortDateString(), true);

                                    if (!inversePair)
                                    {
                                        subjectLine2 = linesN[0];
                                        partnerLine2 = linesN[1];
                                    }
                                    else
                                    {
                                        subjectLine2 = linesN[1];
                                        partnerLine2 = linesN[0];
                                    }
                                }

                            }

                            sw.WriteLine(subjectLine + subjectLine2);
                            sw.WriteLine(partnerLine + partnerLine2);

                        }
                    }

                }
            }
        }
        public static void writeDataSummaryTotalsUbi(Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
        {
            using (TextWriter sw = new StreamWriter(file_name, append: append))
            {
                //String title = ",Pair Talking Duration, Subject Talking Duration, Partner Talking Duration,Pair Turn Count, Subject Turn Count, Partner Turn Count, Pair Proximity Duration, Pair Orientation-Proximity Duration, Shared Time in Classroom, Subject Time, Partner Time, Total Recording Time, Total Voc Duration, Total Voc Count, Total Turn Count, Total Partner Voc Duration, Total Partner Voc Count, Total Partner Turn Count";
                String title =
                    "Total Recording Time,";
                title += getMetrics(" WUBI Total ", " Total", "Partner WUBI Total ", " Partner Total", new PersonInfo(), new PersonInfo(), new PersonInfo(), new PersonInfo(), false, false).Item1;//getMetrics("WUBI Total ", "WUBI Partner Total", new PersonInfo(), new PersonInfo()).Item1;
                String header1 = "Date, Subject, Partner, Adult?,Subject Status, Partner Status,Subject Type,Partner Type, " + title;
                String header2 = "Lead_Subject Status, Lead_Partner Status,Lead_" + title.Replace(",", ",Lead_");
                sw.Write(header1.Replace(" ", ""));
                sw.WriteLine(header2.Replace(" ", ""));
                int idx = 0;
                foreach (ClassroomDay day in days)
                {
                    String date = day.startTime.ToShortDateString();
                    idx++;
                    if (idx < days.Count || processLast)
                    {
                        foreach (String pair in day.cf.pairs) //day.pairStats.Keys)
                        {
                            if (pair.IndexOf("T3B") >= 0)
                            {
                                int t = 0;
                            }
                            String subject = pair.Split('-')[0];
                            String partner = pair.Split('-')[1];
                            String[] lines = writeTotalPairLine(day, pair, date, false);
                            Boolean adult = false;
                            if (partner.Contains("T") || partner.Contains("Lab") || subject.Contains("T") || subject.Contains("Lab"))
                            {
                                adult = true;
                            }
                            String subjectLine = date + "," + subject + "," + partner + "," + adult + "," + lines[0];
                            String partnerLine = date + "," + partner + "," + subject + "," + adult + "," + lines[1];

                            String partnerLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            String subjectLine2 = new StringBuilder().Insert(0, "NA,", 24 * 2).ToString();
                            if (idx < days.Count)
                            {
                                ClassroomDay nextDay = days[idx];
                                String pairN = pair.Split('-')[1] + "-" + pair.Split('-')[0];
                                Boolean inversePair = true;

                                if (nextDay.pairStatsSep.ContainsKey(pair))
                                {
                                    pairN = pair;
                                    inversePair = false;
                                }
                                else if (!nextDay.pairStatsSep.ContainsKey(pairN))
                                {
                                    pairN = "";
                                }
                                if (pairN.Trim() != "")
                                {

                                    String[] linesN = writeTotalPairLine(nextDay, pairN, nextDay.startTime.ToShortDateString(), true);

                                    if (!inversePair)
                                    {
                                        subjectLine2 = linesN[0];
                                        partnerLine2 = linesN[1];
                                    }
                                    else
                                    {
                                        subjectLine2 = linesN[1];
                                        partnerLine2 = linesN[0];
                                    }
                                }

                            }

                            sw.WriteLine(subjectLine + subjectLine2);
                            sw.WriteLine(partnerLine + partnerLine2);

                        }
                    }

                }
            }
        }
    }

}
