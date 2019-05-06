using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
//COMMIT 2
namespace UL_PROCESSOR
{
    
     
    class UL_PROCESSOR_Program
    {
        public int chunkSize = 4;
        public void processClassroom(UL_PROCESSOR_SETTINGS settings, UL_PROCESSOR_CLASS_SETTINGS classSettings)
        {

            ClassroomDay.first = true;
            List<DateTime> dateChunks = new List<DateTime>();
            List<DateTime> lastDateChunks = new List<DateTime>();
            List<String> fileNames=new List<string>();
            int count = 0;
            int total = 0;
            Console.WriteLine(classSettings.szDates);

            Console.WriteLine("PROCESS "+classSettings.classroom);
            foreach (String szDay in classSettings.szDates.Split(','))
            {
                Console.WriteLine(szDay);
                //Console.ReadLine();

                count++;
                total++;
                if (count>chunkSize)
                {
                    if(lastDateChunks.Count > 0)
                        dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);
                    Console.WriteLine("PROCESSING LB"+total);
                    UL_CLASS_PROCESSOR_Program ul0 = new UL_CLASS_PROCESSOR_Program(new Config(settings , classSettings),dateChunks);
                    Console.WriteLine("PROCESSING LB1 1");
                    ul0.process(false,total);
                    lastDateChunks = dateChunks;
                    dateChunks = new List<DateTime>();
                    count = 1;
                    fileNames=fileNames.Concat(ul0.fileNames).ToList();
                }
                dateChunks.Add(Convert.ToDateTime(szDay));

                 
            }
            
            if (lastDateChunks.Count > 0)
                dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);


            UL_CLASS_PROCESSOR_Program ull = new UL_CLASS_PROCESSOR_Program(new Config(settings, classSettings), dateChunks);
            ull.process(true,total);
            fileNames = fileNames.Concat(ull.fileNames).ToList(); 
            if(settings.doSumAllFiles)
            MergeCsvs(fileNames, fileNames[fileNames.Count - 1].Replace(".", "ALL."));


        }
        static void MergeCsvs(List<String> file_names, String destinationfilename)
        {
            StreamReader rdr = new StreamReader(file_names[0]);
            rdr = new StreamReader(file_names[0]);
            StreamWriter wtr = new StreamWriter(destinationfilename);

            string master = rdr.ReadToEnd();
            rdr.Close();

            for (int i = 1; i < file_names.Count; i++)
            {
                rdr = new StreamReader(file_names[i]);
                if (i > 0)
                    rdr.ReadLine();
                string newdata = rdr.ReadToEnd();
                master += newdata;
            }

            rdr.Close();
            rdr.Dispose();

            wtr.Write(master);
            wtr.Close();
            wtr.Dispose();

        }
        public static Dictionary<String, List<PersonInfo>> readLenaItsFiles(String folder)
        {
            Dictionary<String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();



            //Date	Subject	SubjectType	segmentid	voctype	recstart	startsec	endsec	starttime	endtime	duration	uttlen
              TextWriter sw = null;
            {
                TextWriter swd = null;
                sw = new StreamWriter("D://ADOS//_ONSETSV2.CSV", true);// countDays > 0);

                sw.WriteLine("File,Date,Subject,SubjectType,segmentid,voctype,recstart,startsec,endsec,starttime,endtime,duration,seg_duration,wordcount,avg_db,avg_peak");
                
                //foreach (string folder in folders)
                {
                    String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                    
                    {
                        string[] files = Directory.GetFiles(folder);
                        foreach (string file in files)
                        {
                            String fileName = Path.GetFileName(file);
                            if( fileName.IndexOf(".its")>=0)
                            { 
                                Console.WriteLine(file);
                                String lenaId = file.Substring(file.LastIndexOf("//") + 2);
                                lenaId = lenaId.Substring(32, 6);// cf.lenaVersion == "SP" ? 15 : 16, 6);
                                if (lenaId.Substring(0, 2) == "00")
                                    lenaId = lenaId.Substring(2);
                                else if (lenaId.Substring(0, 1) == "0")
                                    lenaId = lenaId.Substring(1);


                                XmlDocument doc = new XmlDocument();
                                doc.Load(file);
                                XmlNodeList rec = doc.ChildNodes[2].SelectNodes("ProcessingUnit/Recording");// cf.lenaVersion == "SP" ? doc.ChildNodes[0].SelectNodes("ProcessingUnit/Recording") : doc.ChildNodes[2].SelectNodes("ProcessingUnit/Recording");


                                /*DateTime dt2 = i.dt;
                                                                DateTime dt3 = i.dt.AddSeconds(i.bd);
                                                                //////////////
                                                                int ms = dt2.Millisecond > 0 ? dt2.Millisecond / 100 * 100 : dt2.Millisecond;// + 100;
                                                                dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, dt2.Minute, dt2.Second, ms);
                                                                ms = dt3.Millisecond > 0 ? dt3.Millisecond / 100 * 100 : dt3.Millisecond;// + 100;
                                                                dt3 = new DateTime(dt3.Year, dt3.Month, dt3.Day, dt3.Hour, dt3.Minute, dt3.Second, ms);
                                                                i.bd= (dt3 - dt2).Seconds + ((dt3 - dt2).Milliseconds / 1000.00);
                                                                */
                                int segmentNumber = 0;
                                int childSegmentNumber = 0;
                                foreach (XmlNode recording in rec)
                                {
                                    DateTime recStartTimeOriginal = DateTime.Parse(recording.Attributes["startClockTime"].Value);
                                    XmlNodeList nodes = recording.SelectNodes("Conversation|Pause");
                                    XmlNodeList nodesP = recording.SelectNodes("Conversation");
                                    double adjustedSecs =0;// getAdjustedSecs(lenaId);
                                    DateTime recStartTime = Config.getMsTime(recStartTimeOriginal.AddSeconds(adjustedSecs));


                                    MappingRow mr = new MappingRow();// cf.getLenaMapping(lenaId, recStartTime);
                                    mr.BID = lenaId;
                                    String pibid = mr.BID;
                                    String pitype = mr.type;
                                    double subjectAvgDb = 1;
                                    double subjectMaxDb = 1;
                                    double subjectConvAvgDb = 1;
                                    double subjectConvMaxDb = 1;

                                    //if (recStartTime.Day == day.Day)
                                    {
                                        foreach (XmlNode conv in nodes)
                                        {
                                            String num = conv.Attributes["num"].Value;
                                            XmlNodeList segments = conv.SelectNodes("Segment");
                                            double startSecs = Convert.ToDouble(conv.Attributes["startTime"].Value.Substring(2, conv.Attributes["startTime"].Value.Length - 3));
                                            double endSecs = Convert.ToDouble(conv.Attributes["endTime"].Value.Substring(2, conv.Attributes["endTime"].Value.Length - 3));
                                            DateTime start = Config.getMsTime(recStartTime.AddSeconds(startSecs));
                                            DateTime end = Config.getMsTime(recStartTime.AddSeconds(endSecs));
                                            PersonInfo pi = new PersonInfo();
                                            //mr = cf.getLenaMapping(lenaId, start);
                                            if (conv.Name == "Conversation")
                                            {
                                                double tc = Convert.ToDouble(conv.Attributes["turnTaking"].Value);
                                                double db = Convert.ToDouble(conv.Attributes["average_dB"].Value);
                                                double mdb = Convert.ToDouble(conv.Attributes["peak_dB"].Value);
                                                subjectConvAvgDb = subjectConvAvgDb == 1 ? db : (subjectConvAvgDb + db) != 0 ? (subjectConvAvgDb + db) / 2 : 0;
                                                subjectConvMaxDb = subjectConvMaxDb == 1 ? mdb : (subjectConvMaxDb + mdb) != 0 ? (subjectConvMaxDb + mdb) / 2 : 0;

                                                if (tc > 0)
                                                {
                                                    pi.dt = start;
                                                    pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                    pi.bid = mr.BID;// ubiAndBId[1];
                                                    pi.lenaId = lenaId;
                                                    pi.bd = (end - start).Seconds + ((end - start).Milliseconds > 0 ? ((end - start).Milliseconds / 1000.00) : 0.00); //endSecs - startSecs;
                                                    pi.tc = tc;

                                                    //addToRawLena(ref rawLenaInfo, pi);
                                                }

                                            }

                                            foreach (XmlNode seg in segments)
                                            {
                                                //startClockTime
                                                segmentNumber++;
                                                startSecs = Convert.ToDouble(seg.Attributes["startTime"].Value.Substring(2, seg.Attributes["startTime"].Value.Length - 3));
                                                endSecs = Convert.ToDouble(seg.Attributes["endTime"].Value.Substring(2, seg.Attributes["endTime"].Value.Length - 3));
                                                start = Config.getMsTime(recStartTime.AddMilliseconds(startSecs * 1000));
                                                end = Config.getMsTime(recStartTime.AddMilliseconds(endSecs * 1000));
                                                pi = new PersonInfo();
                                                pi.dt = start;
                                                //mr = cf.getLenaMapping(lenaId, start);
                                                pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                pi.bid = mr.BID;// ubiAndBId[1];
                                                pi.lenaId = lenaId;
                                                String speaker = seg.Attributes["spkr"].Value;
                                                pi.bd = (end - start).Seconds + ((end - start).Milliseconds > 0 ? ((end - start).Milliseconds / 1000.00) : 0); //endSecs - startSecs;
                                                Boolean add = false;
                                                switch (speaker)
                                                {
                                                    case "CHN":
                                                    case "CHF":
                                                        childSegmentNumber++;
                                                        //if (mr.type == "Child")
                                                        {
                                                            pi.vd = Convert.ToDouble(seg.Attributes["childUttLen"].Value.Substring(1, seg.Attributes["childUttLen"].Value.Length - 2));
                                                            pi.vc = Convert.ToDouble(seg.Attributes["childUttCnt"].Value);
                                                            pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                            pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                            pi.childSegments = childSegmentNumber;
                                                            //if (cf.settings.doOnsets)
                                                            {
                                                                sw.WriteLine(file+","+recStartTime.Day + "," + pi.bid + "," + mr.type + "," + segmentNumber + ",CHN_CHF SegmentUtt," + recStartTime + "," + startSecs + "," + endSecs + "," + start + "," + end + "," + pi.vd + "," + pi.bd + ",," + pi.avDb + "," + pi.maxDb);
                                                            }
                                                            pi.vd = 0;
                                                            //pi.vc = 0;
                                                            if (mr.type == "Child")
                                                            {
                                                                subjectAvgDb = subjectAvgDb == 1 ? pi.avDb : (subjectAvgDb + pi.avDb) != 0 ? (subjectAvgDb + pi.avDb) / 2 : 0;
                                                                subjectMaxDb = subjectMaxDb == 1 ? pi.maxDb : (subjectMaxDb + pi.maxDb) != 0 ? (subjectMaxDb + pi.maxDb) / 2 : 0;
                                                            }

                                                            foreach (XmlAttribute atts in seg.Attributes)
                                                            {

                                                                String newSwLine = "";
                                                                if (atts.Name.IndexOf("startCry") == 0)
                                                                {
                                                                    String cryStep = atts.Name.Substring(8);
                                                                    String att = atts.Name;
                                                                    double cstartSecs = Convert.ToDouble(seg.Attributes[att].Value.Substring(2, seg.Attributes[att].Value.Length - 3));
                                                                    double cendSecs = Convert.ToDouble(seg.Attributes["endCry" + cryStep].Value.Substring(2, seg.Attributes["endCry" + cryStep].Value.Length - 3));
                                                                    DateTime cstart = Config.getMsTime(recStartTime.AddMilliseconds(cstartSecs * 1000));
                                                                    DateTime cend = Config.getMsTime(recStartTime.AddMilliseconds(cendSecs * 1000));
                                                                    PersonInfo cpi = new PersonInfo();
                                                                    cpi.dt = cstart;
                                                                    //mr = cf.getLenaMapping(lenaId, start);
                                                                    cpi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                                    cpi.bid = mr.BID;// ubiAndBId[1];
                                                                    cpi.lenaId = lenaId;
                                                                    cpi.bd = (cend - cstart).Seconds + ((cend - cstart).Milliseconds > 0 ? (cend - cstart).Milliseconds / 1000.00 : 0); //cendSecs - cstartSecs;
                                                                    cpi.cry = cpi.bd;
                                                                    newSwLine = (file + "," + recStartTime.Day + "," + pi.bid + "," + mr.type + "," + segmentNumber + ",CHN_CHF Cry," + recStartTime + "," + cstartSecs + "," + cendSecs + "," + cstart + "," + cend + "," + cpi.cry + "," + pi.bd);
                                                                   // if (mr.type == "Child")
                                                                    //    addToRawLena(ref rawLenaInfo, cpi);

                                                                }
                                                                else if (atts.Name.IndexOf("startUtt") == 0)
                                                                {
                                                                    String cryStep = atts.Name.Substring(8);
                                                                    String att = atts.Name;
                                                                    double cstartSecs = Convert.ToDouble(seg.Attributes[att].Value.Substring(2, seg.Attributes[att].Value.Length - 3));
                                                                    double cendSecs = Convert.ToDouble(seg.Attributes["endUtt" + cryStep].Value.Substring(2, seg.Attributes["endUtt" + cryStep].Value.Length - 3));
                                                                    DateTime cstart = Config.getMsTime(recStartTime.AddMilliseconds(cstartSecs * 1000));
                                                                    DateTime cend = Config.getMsTime(recStartTime.AddMilliseconds(cendSecs * 1000));
                                                                    PersonInfo cpi = new PersonInfo();
                                                                    cpi.dt = cstart;
                                                                   // mr = cf.getLenaMapping(lenaId, start);
                                                                    cpi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                                    cpi.bid = mr.BID;// ubiAndBId[1];
                                                                    cpi.lenaId = lenaId;
                                                                    cpi.bd = (cend - cstart).Seconds + ((cend - cstart).Milliseconds > 0 ? (cend - cstart).Milliseconds / 1000.00 : 0); //cendSecs - cstartSecs;
                                                                    cpi.vd = cpi.bd;
                                                                    //pi.vc = Convert.ToDouble(seg.Attributes["childUttCnt"].Value);
                                                                    //cpi.cry = cpi.bd;
                                                                    newSwLine = (file + "," + recStartTime.Day + "," + pi.bid + "," + mr.type + "," + segmentNumber + ",CHN_CHF Utt," + recStartTime + "," + cstartSecs + "," + cendSecs + "," + cstart + "," + cend + "," + cpi.vd + "," + pi.bd);
                                                                   // if (mr.type == "Child")
                                                                   //     addToRawLena(ref rawLenaInfo, cpi);
                                                                }
                                                               if (newSwLine != ""  )
                                                                    sw.WriteLine(newSwLine);
                                                            }
                                                            add = true;
                                                        }
                                                        break;
                                                    case "FAN":
                                                        pi.ac = Convert.ToDouble(seg.Attributes["femaleAdultWordCnt"].Value);
                                                        //if (mr.type == "Lab" || mr.type == "Teacher")
                                                        {
                                                            pi.vd = Convert.ToDouble(seg.Attributes["femaleAdultUttLen"].Value.Substring(1, seg.Attributes["femaleAdultUttLen"].Value.Length - 2));
                                                            pi.vc = Convert.ToDouble(seg.Attributes["femaleAdultWordCnt"].Value);
                                                            pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                            pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                            pi.childSegments = childSegmentNumber;
                                                           // if (cf.settings.doOnsets)
                                                            {
                                                                sw.WriteLine(file + "," + recStartTime.Day + "," + pi.bid + "," + mr.type + "," + segmentNumber + ",FAN SegmentUtt," + recStartTime + "," + startSecs + "," + endSecs + "," + start + "," + end + "," + pi.vd + "," + pi.bd + "," + pi.vc + "," + pi.avDb + "," + pi.maxDb);
                                                            }
                                                        }
                                                        if (mr.type == "Lab" || mr.type == "Teacher")
                                                            add = true;
                                                        break;
                                                    case "MAN":
                                                        pi.ac = Convert.ToDouble(seg.Attributes["maleAdultWordCnt"].Value);
                                                        // if (mr.type == "Lab" || mr.type == "Teacher")
                                                        {
                                                            pi.vd = Convert.ToDouble(seg.Attributes["maleAdultUttLen"].Value.Substring(1, seg.Attributes["maleAdultUttLen"].Value.Length - 2));
                                                            pi.vc = Convert.ToDouble(seg.Attributes["maleAdultWordCnt"].Value);
                                                            pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                            pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                            pi.childSegments = childSegmentNumber;
                                                            //if (cf.settings.doOnsets)
                                                            {
                                                                sw.WriteLine(file + "," + recStartTime.Day + "," + pi.bid + "," + mr.type + "," + segmentNumber + ",MAN SegmentUtt," + recStartTime + "," + startSecs + "," + endSecs + "," + start + "," + end + "," + pi.vd + "," + pi.bd + "," + pi.vc + "," + pi.avDb + "," + pi.maxDb);
                                                            }
                                                        }
                                                        if (mr.type == "Lab" || mr.type == "Teacher")
                                                            add = true;
                                                        break;
                                                    case "OLN":
                                                        pi.bd = endSecs - startSecs;
                                                        pi.oln = pi.bd;
                                                        add = true;
                                                        break;
                                                    case "NON":
                                                        pi.bd = endSecs - startSecs;
                                                        pi.no = pi.bd;
                                                        add = true;
                                                        break;
                                                }

                                                if (add)
                                                {

                                                   // ..addToRawLena(ref rawLenaInfo, pi);
                                                }

                                            }

                                        }
                                    }

                                   // if (cf.settings.doDbs)
                                    //    swd.WriteLine(this.day + "," + pibid + "," + pitype + "," + (subjectConvAvgDb != 1 ? subjectConvAvgDb : 0) + "," + (subjectConvMaxDb != 1 ? subjectConvMaxDb : 0) + "," + (subjectAvgDb != 1 ? subjectAvgDb : 0) + "," + (subjectMaxDb != 1 ? subjectMaxDb : 0));
                                }
                                // 

                            }

                        }
                    }
                }
                 
                    sw.Close();
                
            }

            return rawLenaInfo;
        }
        public static void napOnset()
        {
            Dictionary<DateTime, Dictionary<String, Tuple<String, String>>> naps = new Dictionary<DateTime, Dictionary<string, Tuple<string, string>>>();
            using (StreamReader sr = new StreamReader("C://LVL//APPLETREE//NAP.CSV"))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    String[] line = sr.ReadLine().Split(',');
                    DateTime dt = Convert.ToDateTime(line[0]);
                    String bid = line[2].Trim();
                    String st = line[3].Trim();
                    String et = line[4].Trim();
                    if(!naps.ContainsKey(dt))
                    {
                        naps.Add(dt, new Dictionary<string, Tuple<string, string>>());

                    }
                    naps[dt].Add(bid, new Tuple<string, string>(st, et!=""?et:"4:11"));
                }
            }
            using (TextWriter sw = new StreamWriter("C://LVL//APPLETREE//CRYNAPS.CSV"))//"
            {
                using (StreamReader sr = new StreamReader("C://LVL//APPLETREE//CRY.CSV"))
                {
                    String szline = sr.ReadLine();
                    sw.WriteLine(szline);
                    while (!sr.EndOfStream)
                    {
                        szline = sr.ReadLine();
                        String[] line = szline.Split(',');
                        DateTime dt = Convert.ToDateTime(line[1]);
                        String bid = line[2].Trim();
                        String st = line[9].Trim();
                        String et = line[10].Trim();

                        int sth = Convert.ToInt32(line[9].Trim().Split(':')[0]);
                        int stm = Convert.ToInt32(line[9].Trim().Split(':')[1]);
                        int eth = Convert.ToInt32(line[10].Trim().Split(':')[0]);
                        int etm = Convert.ToInt32(line[10].Trim().Split(':')[1]);

                        if(dt.Month==10&&dt.Day==17&&bid=="11A"&&sth==11&&stm==5)
                        {
                            dt = dt;
                        }


                        if (naps.ContainsKey(dt) && naps[dt].ContainsKey(bid))
                        {
                            String ns = naps[dt][bid].Item1;
                            String ne = naps[dt][bid].Item2;

                            int nsth = Convert.ToInt32(naps[dt][bid].Item1.Split(':')[0]);
                            int nstm = Convert.ToInt32(naps[dt][bid].Item1.Split(':')[1]);
                            int neth = Convert.ToInt32(naps[dt][bid].Item2.Split(':')[0]);
                            int netm = Convert.ToInt32(naps[dt][bid].Item2.Split(':')[1]);

                            DateTime nsdt = new DateTime(dt.Year, dt.Month, dt.Day, nsth<8?nsth+12:nsth, nstm, 0);
                            DateTime nedt = new DateTime(dt.Year, dt.Month, dt.Day, neth < 8 ? neth + 12 : neth, netm, 0);
                            if(sth>12)
                            {
                                sth = sth;
                            }
                            DateTime sdt = new DateTime(dt.Year, dt.Month, dt.Day, sth < 8 ? sth + 0 : sth, stm, 0);
                            DateTime edt = new DateTime(dt.Year, dt.Month, dt.Day, eth < 8 ? eth + 0 : eth, etm, 0);

                            
                                if ((sdt >= nsdt && sdt <= nedt) ||
                                (edt >= nsdt && edt <= nedt))
                                {

                                szline += ",NAP,"+ nsdt+","+ nedt;
                            }
                            sw.WriteLine(szline);
                        }
                    }
                }
            }

                }
        static void Main(string[] args)
        {
            //readLenaItsFiles("D://ADOS//");
            //PythonRunner pr = new PythonRunner();
            //pr.runScript();
            //ItsReader r = new ItsReader();
            // r.read();
            //FileComparer fc = new FileComparer();
            //fc.compareFiles(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
            //    "C://LVL//LADYBUGS1//SYNC//ADEX_07242018_267290963_2_PAIRACTIVITY_ALL_LADYBUGS1_TOTALSALL.CSV", "C://LVL//LADYBUGS1//SYNC//ITS_07242018_900772189_2_PAIRACTIVITY_ALL_LADYBUGS1_TOTALSALL.CSV", false,80,false);
         //   napOnset();


            if (args.Length>0)
            {

                Console.WriteLine(args[0]);
               // Console.ReadLine();
                UL_PROCESSOR_SETTINGS settingParams = new UL_PROCESSOR_SETTINGS();
                settingParams.from(args);

                String[] settings = args[0].Split(' ');
               
                for (int a=1;a< settings.Length;a++)
                {
                    if(settings[a].Split('|').Length>1)
                    {
                        /******** A)FOR EACH CLASSROOM:********/
                        UL_PROCESSOR_CLASS_SETTINGS classSettings = new UL_PROCESSOR_CLASS_SETTINGS();
                        classSettings.from(settings[a].Split('|'));
                        UL_PROCESSOR_Program pc = new UL_PROCESSOR_Program();
                        pc = new UL_PROCESSOR_Program();

                        pc.processClassroom(settingParams, classSettings);
                    }

                }

                int argCount = 0;

                /*String[] settings = args[0].Split(' ');
            dir = settings[0];

            UL_PROCESSOR_SETTINGS settingParams = new UL_PROCESSOR_SETTINGS();
            for (int a = 1; a < settings.Length; a++)
            {
                String[] setting = settings[a].Split(':');*/
                /******** A)FOR EACH CLASSROOM:********/


               /* foreach (String arg in args)
                {
                    argCount++;
                    if (argCount > 1)
                    {
                        UL_PROCESSOR_CLASS_SETTINGS classSettings = new UL_PROCESSOR_CLASS_SETTINGS();
                        classSettings.from(arg.Split(' '));
                        UL_PROCESSOR_Program pc = new UL_PROCESSOR_Program();
                        pc = new UL_PROCESSOR_Program();
                        pc.processClassroom(settingParams, classSettings);
                         
                    }
                }*/

            }
            Console.ReadLine();
        }

    }
}
