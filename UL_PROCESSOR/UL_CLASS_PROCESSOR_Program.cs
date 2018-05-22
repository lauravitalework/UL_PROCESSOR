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
        public UL_CLASS_PROCESSOR_Program(Config c, List<DateTime> d, String fName)
        {
            configInfo = c;
            days = d;
            configInfo.readMappings();
            fileName = fName;
        }

        public void process(Boolean processLast)
        {
            List<ClassroomDay> cds = new List<ClassroomDay>();
            int countSets = 0;
            foreach (DateTime day in days)
            {

                ClassroomDay cd = new ClassroomDay(day, configInfo);
                Console.WriteLine("PROCESSING " + configInfo.classroom + " " + day.ToShortDateString());
                Console.WriteLine("readUbiFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Dictionary<String, List<PersonInfo>> rawUbi = cd.readUbiFile();
                Console.WriteLine("readUbiTagFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> rawTags = cd.readUbiTagFile();
                Console.WriteLine("readLenaFile (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                Dictionary<String, List<PersonInfo>> rawLena = cd.readLenaFile();
                Console.WriteLine("setUbiData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setUbiData(rawUbi);
                Console.WriteLine("setUbiTagData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setUbiTagData(rawTags.Item1, rawTags.Item2);
                Console.WriteLine("setLenaData (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.setLenaData(rawLena);
                DateTime trunkAt = cd.getTrunkTime();
                Console.WriteLine("countInteractions (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.countInteractions(false, true, true, trunkAt, rawLena); //count interactions but no need to write a file
                ///////reviiew time order
                Console.WriteLine("write10SecTalkingCSV (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                cd.write10SecTalkingCSV(configInfo.root + configInfo.classroom + "/SYNC/" + "10THOFSECTALKING_" + (configInfo.justFreePlay ? "_freeplay" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".CSV"); //write complete data files to disc
                cds.Add(cd);
            }
            summarize(cds, countSets, processLast);


        }
        public void summarize(List<ClassroomDay> cds, int c, Boolean processLast)
        {

            foreach (ClassroomDay cd in cds)
            {

                try
                {
                    Console.WriteLine("Summarizing " + cd.day.Month + " " + cd.day.Year + " " + cd.day.Day + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");
                    List<ClassroomDay> oneDay = new List<ClassroomDay>();//sync3_3pairactivity_vo8.CSV
                    oneDay.Add(cd);
                    Console.WriteLine("Summarizing ONE");
                    writeDataSummary(false, configInfo.root + configInfo.classroom + "/SYNC/PAIRACTIVITY_V41918" + c.ToString() + "_" + configInfo.classroom + "_" + (cd.justProx ? "no" : "") + (configInfo.justFreePlay ? "_freeplay" : "") + "o_" + cd.day.Month + "_" + cd.day.Day + "_" + cd.day.Year + ".csv", oneDay, true); //write summary data
                }
                catch (Exception e)
                {

                }
            }
            Console.WriteLine("Summarizing ALL " + " (" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "):");

            writeDataSummary(c > 0, configInfo.root + configInfo.classroom + "/SYNC/PAIRACTIVITY__ALL_" + fileName + configInfo.classroom + (configInfo.justFreePlay ? "_freeplay" : "") + (cds.Count > 0 ? cds[0].day.Year.ToString() : "") + ".CSV", cds, processLast); //write summary data
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

            return new Tuple<string, string, string>(title, values, valuesInverse);

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
            lines[0] = (status) + "," +
                   (statusp) + "," +
                   (nextDay ? "" : (type) + ",") +
                   (nextDay ? "" : (typep) + ",")
                   + new StringBuilder().Insert(0, "NA,", 23 * 2).ToString();
            lines[1] = (statusp) + "," +
                   (status) + "," +
                   (nextDay ? "" : (typep) + ",") +
                   (nextDay ? "" : (type) + ",")
                   + new StringBuilder().Insert(0, "NA,", 23 * 2).ToString();
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
        public static void writeDataSummary(Boolean append, String file_name, List<ClassroomDay> days, Boolean processLast)
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
                            String[] lines = writePairLine(day, pair, date, false);
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

                                    String[] linesN = writePairLine(nextDay, pairN, nextDay.startTime.ToShortDateString(), true);

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
