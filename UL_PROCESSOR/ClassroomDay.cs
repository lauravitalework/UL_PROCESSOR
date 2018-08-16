using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
namespace UL_PROCESSOR
{
    class ClassroomDay
    {
        public Boolean doAll10OfSecs = false;//false;//true;
        public Boolean startFromLena = true;//false;//true;
        public Boolean noLena = false;//true;//false;//true;
        private double minDistance = 1.5 * 1.5; //the squared value of g(r) cutoff in meters

        public Dictionary<String, double> pairClose = new Dictionary<string, double>();
        public Dictionary<String, double> pairCloseOrientation = new Dictionary<string, double>();
        public Dictionary<String, double> pairTime = new Dictionary<string, double>();
        public Dictionary<String, double> pairCry = new Dictionary<string, double>();
        public Dictionary<String, Tuple<double, double, double, double>> pairStatsSeparated = new Dictionary<string, Tuple<double, double, double, double>>();

        public Dictionary<String, PairInfo> pairStatsSep = new Dictionary<string, PairInfo>();
        public Dictionary<String, double> individualTime = new Dictionary<string, double>();
        public Dictionary<String, double> pairStats = new Dictionary<String, double>();
        public Dictionary<String, PersonInfo> personTotalCounts = new Dictionary<string, PersonInfo>();
        public Dictionary<String, PersonInfo> personTotalCountsWUbi = new Dictionary<string, PersonInfo>();

        public DateTime startTime; //first recorded instant of that day
        public DateTime endTime; //last recorded instant of that day

        public Dictionary<DateTime, Dictionary<String, PersonInfo>> activities = new Dictionary<DateTime, Dictionary<string, PersonInfo>>();
        public Config cf;
        public DateTime day;

        public Dictionary<String, List<Tuple<DateTime, DateTime>>> personUbiTimes = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();

        public ClassroomDay(DateTime d, Config c)
        {
            cf = c;
            day = d;
        }
        public List<DateTime> maxTimes = new List<DateTime>();
        public List<Tuple<DateTime,String>> maxPersonTimes = new List<Tuple<DateTime, String>>();
        public DateTime getTrunkTime()
        {
            DateTime end = new DateTime();
            maxTimes = maxTimes.OrderBy(x => x.TimeOfDay).ToList();
            if (maxTimes.Count > 0)
            {
                end = maxTimes[maxTimes.Count - 1];
                foreach (DateTime dt in maxTimes)
                {
                    TimeSpan span = end.Subtract(dt);
                    if (span.TotalMinutes <= 10)
                    {
                        return dt;
                    }
                }
            }
            return end;

        }

        private Tuple<double, double> linearInterpolate(DateTime t, DateTime t1, Tuple<double, double> p1, DateTime t2, Tuple<double, double> p2)
        {
            double x0 = t1.Minute * 60000 + t1.Second * 1000 + t1.Millisecond;
            double x1 = t2.Minute * 60000 + t2.Second * 1000 + t2.Millisecond;
            double x = t.Minute * 60000 + t.Second * 1000 + t.Millisecond;

            double y0x = p1.Item1;
            double y1x = p2.Item1;
            double y0y = p1.Item2;
            double y1y = p2.Item2;

            double xlerp = (y0x * (x1 - x) + y1x * (x - x0)) / (x1 - x0);
            double ylerp = (y0y * (x1 - x) + y1y * (x - x0)) / (x1 - x0);
            return new Tuple<double, double>(xlerp, ylerp);
        }
        /// <summary>
        /// Overloaded linear interpolate method for non-Tuples
        /// </summary>
        /// <param name="t">The time of interest </param>
        /// <param name="t1">first known time that is less than t </param>
        /// <param name="y0=">first known value associated with t1 </param>
        /// <param name="t2">first known time that is greater than t </param>
        /// <param name="y1">first known value associated with t2 </param>
        /// <returns>a double representing a value for DateTime t</returns>
        private double linearInterpolate(DateTime t, DateTime t1, double y0, DateTime t2, double y1)
        {
            double x0 = t1.Minute * 60000 + t1.Second * 1000 + t1.Millisecond;
            double x1 = t2.Minute * 60000 + t2.Second * 1000 + t2.Millisecond;
            double x = t.Minute * 60000 + t.Second * 1000 + t.Millisecond;
            double lerp = (y0 * (x1 - x) + y1 * (x - x0)) / (x1 - x0);
            return lerp;
        }
          
        private List<Tuple<DateTime, PersonInfo>> clean(String person, List<PersonInfo> raw, Boolean tag, Boolean addTagTimes)
        {
            if(addTagTimes)
            {
                if(raw.Last().dt.Date!=this.day.Date)
                {
                    int stop = 1;
                }
                maxPersonTimes.Add(new Tuple<DateTime, String>(raw.Last().dt, person));
                maxTimes.Add(raw.Last().dt);
            }
                 
            List<Tuple<DateTime, PersonInfo>> newList = new List<Tuple<DateTime, PersonInfo>>();
            DateTime first = raw[0].dt;//first date from merged file ordered by time
            DateTime last = raw.Last().dt;//last date from merged file ordered by time

            //targets will begin at closest 100 ms multiple of start
            int ms = first.Millisecond / 100 * 100 + 100;
            if (first.Millisecond % 100 == 0)
            {
                ms -= 100;
            }
            DateTime target = new DateTime();//will be next .1 sec
            if (ms == 1000)
            {
                if (first.Second == 59)
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute + 1, 0, 0);
                }
                else
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, first.Second + 1, 0);
                }
            }
            else
            {
                target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, first.Second, ms);
            }

            while (target.CompareTo(last) <= 0)
            {
                /******/
                //find next time row based on ms
                PersonInfo pi = new PersonInfo();
                pi.dt = target;
                int index = raw.BinarySearch(pi, new DateTimeComparer());
                if (index < 0)
                {
                    index = ~index;
                }
                if (index > 0)
                {
                    //LQ: why same hour??
                    //FIX
                    //if ((raw[index - 1].dt.Hour == raw[index].dt.Hour) && (Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    //if ((Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    TimeSpan difference = raw[index].dt.Subtract(raw[index - 1].dt); // could also write `now - otherTime`
                    if (difference.TotalSeconds <60 || doAll10OfSecs)
                    {
                        Tuple<double, double> point1 = new Tuple<double, double>(raw[index - 1].x, raw[index - 1].y);
                        Tuple<double, double> point2 = new Tuple<double, double>(raw[index].x, raw[index].y);
                        Tuple<double, double> targetpoint = linearInterpolate(target, raw[index - 1].dt, point1, raw[index].dt, point2);
                        double orientation1 = raw[index - 1].ori;
                        double orientation2 = raw[index].ori;
                        double targetorientation = linearInterpolate(target, raw[index - 1].dt, orientation1, raw[index].dt, orientation2);
                        PersonInfo pi2 = new PersonInfo();
                        if (difference.TotalSeconds < 60)
                        {
                            pi2.x = targetpoint.Item1;
                            pi2.y = targetpoint.Item2; 
                        }
                        else
                        {
                            pi2.x = -5;
                            pi2.y = -5;
                        }
                        pi2.ori = targetorientation;
                        newList.Add(new Tuple<DateTime, PersonInfo>(target, pi2));// talk, vc, tc)));
                    }

                }
                target = target.AddMilliseconds(100);

            }

            return newList;
        }

        public void setUbiData(Dictionary<String, List<PersonInfo>> rawData)
        {
            foreach (String person in rawData.Keys)
            {
                if (person == "Lab3d")
                {
                    int stop = 0;
                }
                try
                {
                    List<Tuple<DateTime, PersonInfo>> cleanedData = clean(person, rawData[person], false,true);
                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedData)
                    {
                        DateTime cur = dataLine.Item1;
                        if (!activities.ContainsKey(cur))
                        {
                            activities.Add(cur, new Dictionary<string, PersonInfo>());
                        }
                        activities[cur].Add(person, dataLine.Item2);
                    }
                    if (person == "T3A")
                    {
                        int err = 5;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION: " + e.Message);
                }
            }
        }
        public Dictionary<String, DateTime> startLenaTimes = new Dictionary<string, DateTime>();
        public void setLenaData(Dictionary<String, List<PersonInfo>> lenadata)
        {
            foreach (String person in lenadata.Keys)
            {
                 
                List<PersonInfo> dataLine = lenadata[person];
                Boolean startSet = false;
                foreach (PersonInfo data in dataLine)
                {
                    DateTime time = new DateTime(data.dt.Year, data.dt.Month, data.dt.Day, data.dt.Hour, data.dt.Minute, data.dt.Second, 0);
                    if(!startSet)
                    {
                        startLenaTimes.Add(person, time);
                        startSet = true;
                    }
                    Double vocDur = data.vd;
                    Double blockDur = data.bd;
                    Double vocCount = data.vc;
                    Double turnCount = data.tc;
                    Double a = data.ac;
                    Double n = data.no;
                    Double o = data.oln;
                    double turnCount10 = (turnCount / blockDur) / 10;
                    double vocCount10 = (vocCount / blockDur) / 10;
                    double vocDur10 = (vocDur / blockDur) / 10;
                    double adults10 = (a / blockDur) / 10;
                    double noise10 = (n / blockDur) / 10;
                    double oln10 = (o / blockDur) / 10;
                    if (personTotalCounts.ContainsKey(person))/////////////////
                    {
                        //Tuple<double, double, double, double, double> totalInfo = personTotalCounts[person];

                        personTotalCounts[person].vd = personTotalCounts[person].vd + vocDur;
                        personTotalCounts[person].vc = personTotalCounts[person].vc + vocCount;
                        personTotalCounts[person].tc = personTotalCounts[person].tc + turnCount;
                        personTotalCounts[person].ac = personTotalCounts[person].ac + a;
                        personTotalCounts[person].no = personTotalCounts[person].no + n;
                        personTotalCounts[person].oln = personTotalCounts[person].oln + o;

                        //personTotalCounts[person] = new Tuple<double, double, double, double, double>(totalInfo.Item1 + vocDur, totalInfo.Item2 + vocCount, totalInfo.Item3 + turnCount, totalInfo.Item4 + a, totalInfo.Item5 + n);
                    }
                    else
                    {
                        PersonInfo pi = new PersonInfo();
                        pi.vd = vocDur;
                        pi.vc = vocCount;
                        pi.tc = turnCount;
                        pi.ac = a;
                        pi.no = n;
                        pi.oln = o;
                        personTotalCounts.Add(person, pi);
                        personTotalCountsWUbi.Add(person, new PersonInfo());
                        //personTotalCounts.Add(person, new Tuple<double, double, double, double, double>(vocDur, vocCount, turnCount, a, n));
                    }
                    do
                    {
                        if (activities.ContainsKey(time))
                        {
                            if (activities[time].ContainsKey(person))
                            {
                                //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);
                                activities[time][person].wasTalking = vocDur > 0;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                activities[time][person].vc = vocCount10;
                                activities[time][person].tc = turnCount10;
                                activities[time][person].vd = vocDur10;
                                activities[time][person].ac = adults10;
                                activities[time][person].oln = oln10;
                                activities[time][person].no = noise10;

                                if (personTotalCountsWUbi.ContainsKey(person))/////////////////
                                {
                                    personTotalCountsWUbi[person].vc += vocCount10;
                                    personTotalCountsWUbi[person].tc += turnCount10;
                                    personTotalCountsWUbi[person].vd += vocDur10;
                                    personTotalCountsWUbi[person].ac += adults10;
                                    personTotalCountsWUbi[person].oln += oln10;
                                    personTotalCountsWUbi[person].no += noise10;
                                }

                            }
                        }

                        time = time.AddMilliseconds(100);
                        vocDur -= 0.1;
                        blockDur -= 0.1;
                    } while (blockDur > 0);
                }
            }
        }
        public Boolean isItWithUbi(DateTime dt, String p)
        {
            foreach (Tuple<DateTime, DateTime> dates in personUbiTimes[p])
            {
                if (dt >= dates.Item1 && dt <= dates.Item2)
                {
                    return true;

                }

            }
            return false;
        }
        public void setUbiTagData(Dictionary<String, List<PersonInfo>> rawData, Dictionary<String, List<PersonInfo>> rawDataR)
        {
            String l = "";
            String r = "";

            foreach (String person in rawData.Keys)
            {
                l += person + ",";
                
            }
            foreach (String person in rawDataR.Keys)
            {
                r += person + ",";
            }
            foreach (String person in rawData.Keys)
            {
                try
                {
                    List<Tuple<DateTime, PersonInfo>> cleanedData = clean(person, rawData[person], false, false);
                    List<Tuple<DateTime, PersonInfo>> cleanedDataR = clean(person, rawDataR[person], false, false);
                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedData)
                    {
                        DateTime cur = dataLine.Item1;
                        if (!activities.ContainsKey(cur))
                        {
                            activities.Add(cur, new Dictionary<string, PersonInfo>());
                        }

                        if (activities[cur].ContainsKey(person))
                        {
                            activities[cur][person].lx = dataLine.Item2.x;
                            activities[cur][person].ly = dataLine.Item2.y;
                        }
                    }
                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedDataR)
                    {
                        DateTime cur = dataLine.Item1;
                        if (!activities.ContainsKey(cur))
                        {
                            activities.Add(cur, new Dictionary<string, PersonInfo>());
                        }

                        if (activities[cur].ContainsKey(person))
                        {
                            activities[cur][person].rx = dataLine.Item2.x;
                            activities[cur][person].ry = dataLine.Item2.y;
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION: " + e.Message);
                }
            }
        }
        public void setData(Dictionary<String, List<PersonInfo>> rawData, Dictionary<String, List<PersonInfo>> rawLenaData)
        {
            setUbiData(rawData);
            setLenaData(rawLenaData);
        }
        
        public Dictionary<String, List<PersonInfo>> readUbiFile()
        {
            Dictionary<String, List<PersonInfo>> rawInfo = new Dictionary<String, List<PersonInfo>>();
            string[] folders = Directory.GetDirectories(cf.ubisenseFileDir);
            foreach (string folder in folders)
            {
                String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                DateTime folderDate;
                if (DateTime.TryParse(folderName, out folderDate) && folderDate >= day && folderDate < day.AddDays(1))
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        String fileName = Path.GetFileName(file);
                        if (fileName.StartsWith(cf.ubisenseFile) && fileName.EndsWith(".log"))
                        {
                            int lineCount = 0;
                            using (StreamReader sr = new StreamReader(file))
                            {
                                Boolean firstRow = true;
                                while ((!sr.EndOfStream))// && lineCount<10000)
                                {
                                    lineCount++;
                                    String commaLine = sr.ReadLine();
                                    String[] line = commaLine.Split(',');
                                    if (line.Length > 5 && line[0].Trim() != "")
                                    {
                                        String ubiId = line[cf.ubiFileIdCol];
                                        if(ubiId=="Child20")
                                        {
                                            ubiId = ubiId;
                                        }
                                        DateTime time = Convert.ToDateTime(line[cf.ubiFileDateCol]);
                                        

                                        try
                                        {
                                            if (firstRow)
                                            {
                                                startTime = time;
                                                firstRow = false;
                                            }
                                            endTime = time;
                                            PersonInfo info = new PersonInfo();
                                            info.ubiId = ubiId;
                                            info.x = Convert.ToDouble(line[cf.ubiFileXPosCol]);
                                            info.y = Convert.ToDouble(line[cf.ubiFileYPosCol]);
                                            info.z = Convert.ToDouble(line[cf.ubiFileZPosCol]);
                                            info.ori = Convert.ToDouble(line[cf.ubiFileOriPosCol]);
                                            info.dt = time;
                                            
                                            MappingRow mr = cf.getUbiMapping(ubiId, time);
                                            info.bid = mr.BID;
                                            if (info.bid == "20A")// && time.Day!=day.Day)
                                            {
                                                int stophere = 1;
                                            }
                                            if (mr.BID != "" && (!mr.isAbsent(day)) && time.Day == day.Day)
                                            {

                                                info.isFreePlay = isThisFreePlay(time);
                                                if (!rawInfo.ContainsKey(info.bid))
                                                {
                                                    rawInfo.Add(info.bid, new List<PersonInfo>());
                                                }
                                                rawInfo[info.bid].Add(info);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("EXCEPTION: " + e.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return rawInfo;
        }
        Dictionary<String, Boolean> tagTest = new Dictionary<string, bool>();//test delete
        List<String> tagMissing = new List<string>();//test delete
        Dictionary<String,String> tagL = new Dictionary<string, string>();//test delete
        Dictionary<String, String> tagR = new Dictionary<string, string>();//test delete
        Boolean writeChaomingFile = true;//false;
        TextWriter swc = null;
           
        public Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> readUbiTagFile()
        {
            Dictionary<String, List<PersonInfo>> rawInfo = new Dictionary<String, List<PersonInfo>>();
            Dictionary<String, List<PersonInfo>> rawInfoR = new Dictionary<String, List<PersonInfo>>();
            string[] folders = Directory.GetDirectories(cf.ubisenseFileDir);
            String tags = "";//test del

            DateTime trunk = new DateTime();
            if (writeChaomingFile)
            {
                swc = new StreamWriter(cf.root + cf.classroom + "/SYNC/chaomingtag_" + cf.classroom + "_" + day.Month + "_" + day.Day + "_" + day.Year + ".csv");

                trunk = getTrunkTime();
            }

                foreach (string folder in folders)
            {
                String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                DateTime folderDate;
                if (DateTime.TryParse(folderName, out folderDate) && folderDate >= day && folderDate < day.AddDays(1))
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        String fileName = Path.GetFileName(file);
                        if (fileName.StartsWith(cf.ubisenseTagsFile) && fileName.EndsWith(".log"))
                        {
                            using (StreamReader sr = new StreamReader(file))
                            {
                                while ((!sr.EndOfStream))// && lineCount<10000)
                                {
                                    String szLine = sr.ReadLine();
                                    String[] line = szLine.Split(',');
                                    if (line.Length > 5)
                                    {
                                        String tag = line[1].Trim();
                                        if (tag == "00:11:CE:00:00:00:02:BE")
                                        {
                                            tag = tag;
                                        }
                                            if (tags.IndexOf(tag) < 0)
                                        { 
                                            tags += tag + ",";
                                            tagTest.Add(tag, false);
                                            try
                                            {
                                                cf.tagTest[tag] = true;
                                            }
                                            catch(Exception e)
                                            {
                                                bool s = true;
                                                Console.WriteLine("EXCEPTION: " + e.Message);
                                            }
                                        }
                                        String personId = "";
                                        DateTime lineTime = Convert.ToDateTime(line[2]);
                                        Double xPos = Convert.ToDouble(line[3]);
                                        Double yPos = Convert.ToDouble(line[4]);
                                        PersonInfo i = new PersonInfo();// lineTime, 0, 0, 0, 0);
                                        i.dt = lineTime;

                                        if (cf.mapRowsUbiL.ContainsKey(tag))
                                        {
                                            personId = cf.getUbiMappingL(tag, lineTime).BID;// tagMappigLeft[tag].Trim();
                                            i.lx = xPos;
                                            i.ly = yPos;
                                            if(!tagL.ContainsKey(tag))
                                            {
                                                tagL.Add(tag, tag);
                                            }
                                            if (writeChaomingFile && (!isThisInTimes(lineTime, cf.extractTimes)))
                                            {
                                                Boolean sAbsent = cf.getMapping(personId, day).isAbsent(day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
                                                if(!sAbsent  && lineTime.CompareTo(trunk) <= 0)
                                                swc.WriteLine(szLine.Replace(tag, personId + "L"));
                                            }
                                        }
                                        else if (cf.mapRowsUbiR.ContainsKey(tag))
                                        {
                                            personId = cf.getUbiMappingR(tag, lineTime).BID;// tagMappigLeft[tag].Trim();
                                            i.rx = xPos;
                                            i.ry = yPos;
                                            if (!tagR.ContainsKey(tag))
                                            {
                                                tagR.Add(tag, tag);
                                            }
                                            if (writeChaomingFile && (!isThisInTimes(lineTime, cf.extractTimes)))
                                            {
                                                Boolean sAbsent = cf.getMapping(personId, day).isAbsent(day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
                                                if (!sAbsent && lineTime.CompareTo(trunk) <= 0)
                                                    swc.WriteLine(szLine.Replace(tag, personId + "R"));
                                            }
                                        }
                                        i.x = xPos;
                                        i.y = yPos;
                                        if (personId != "")
                                        {
                                            if (i.rx == 0)
                                            {
                                                if (!rawInfo.ContainsKey(personId))
                                                {
                                                    rawInfo.Add(personId, new List<PersonInfo>());
                                                }

                                                //rawInfo[personId].Add(new info(lineTime, xPos, yPos, orientation, vocDur, vocCount, turnCount, blockDur));


                                                rawInfo[personId].Add(i);
                                            }
                                            else
                                            {
                                                if (!rawInfoR.ContainsKey(personId))
                                                {
                                                    rawInfoR.Add(personId, new List<PersonInfo>());
                                                }
                                                //rawInfo[personId].Add(new info(lineTime, xPos, yPos, orientation, vocDur, vocCount, turnCount, blockDur));

                                                rawInfoR[personId].Add(i);
                                            }
                                        }
                                        else //if(tag!= "00:11:CE:00:00:00:01:C7" && tag!= "00:11:CE:00:00:00:01:EA" && tag!= "00:11:CE:00:00:00:02:BE" && tag != "00:11:CE:00:00:00:02:66")
                                        {
                                            bool missing = true;
                                            tagMissing.Add(tag);


                                        }
                                    }
                                }
                            }
                            //String partialName = "MiamiLocation."+ year.ToString() +"-" + (month < 10 ? "0" : "") + month.ToString() + "-" + (day < 10 ? "0" : "") + day.ToString();
                        }
                    }
                }
            }
            Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> info = new Tuple<Dictionary<string, List<PersonInfo>>, Dictionary<string, List<PersonInfo>>>(rawInfo, rawInfoR);

            return info;
        }
        public double getAdjustedSecs(String lid)
        {
            //adjustedTimes
            double adjustedSecs = 0;
            if (cf.adjustedTimes.ContainsKey(day))
            {
                if (cf.adjustedTimes[day].ContainsKey(lid))
                {
                    adjustedSecs = cf.adjustedTimes[day][lid];
                }
            }
            /*if (adjustTimes.ContainsKey(day))
            {
                if (adjustTimes[day].ContainsKey(lid))
                {
                    adjustedSecs = adjustTimes[day][lid];
                }
            }*/
            return adjustedSecs;
        }
        public Dictionary<String, List<PersonInfo>> readLenaFile()
        {

            Dictionary<String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();
            if(File.Exists(cf.lenaFile))
            using (StreamReader sr = new StreamReader(cf.lenaFile))
            {
                if (!sr.EndOfStream)
                {
                    sr.ReadLine();
                }
                int lineCount = 0;
                while ((!sr.EndOfStream))// && lineCount < 10000)
                {
                    lineCount++;
                    try
                    {
                        String commaLine = sr.ReadLine();
                        String[] line = commaLine.Split(',');
                        if (line.Length > 5 && line[0].Trim() != "")
                        {
                            String lenaId = line[cf.lenaFileIdCol].Trim();
                            DateTime time = Convert.ToDateTime(line[cf.lenaFileDateCol]);
                            double adjustedSecs = getAdjustedSecs(lenaId);
                            if(cf.lenaVersion.ToUpper()=="SP")
                            time = time.AddHours(-5);
                            time = time.AddSeconds(adjustedSecs);
                            if (time.Year == day.Year && time.Month == day.Month && time.Day == day.Day)
                            {
                                //DateTime day = time.AddHours(-time.Hour).AddMinutes(-time.Minute).AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
                                //MappingRow mr = cf.mapRows.findByLenaId(lenaId, time);//FIX????
                                MappingRow mr = cf.getLenaMapping(lenaId, time);

                                String ubiId = mr.UbiID;// ubiAndBId[0];
                                String BId = mr.BID;// ubiAndBId[1];

                                if (ubiId != "" && mr.BID != "")
                                {
                                    PersonInfo info = new PersonInfo();
                                    info.dt = time;
                                    //info.LenaData = commaLine;
                                    info.bid = BId;
                                    info.lenaId = lenaId;
                                    info.bd = Convert.ToDouble(line[cf.lenaFileBdCol].Trim());
                                    info.vd = Convert.ToDouble(line[cf.lenaFileVdCol].Trim());
                                    info.vc = Convert.ToDouble(line[cf.lenaFileVcCol].Trim());
                                    info.tc = Convert.ToDouble(line[cf.lenaFileTcCol].Trim());
                                    info.no = Convert.ToDouble(line[cf.lenaFileNoCol].Trim());
                                    info.ac = Convert.ToDouble(line[cf.lenaFileAcCol].Trim());
                                    info.oln = Convert.ToDouble(line[cf.lenaFileOlnCol].Trim());
                                    info.bid = BId;
                                    info.isFreePlay = isThisFreePlay(time);
                                    if (!rawLenaInfo.ContainsKey(info.bid))
                                    {
                                        rawLenaInfo.Add(info.bid, new List<PersonInfo>());
                                    }
                                    rawLenaInfo[info.bid].Add(info);

                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("EXCEPTION: " + e.Message);
                    }
                }
            }
            return rawLenaInfo;
        }
        public Boolean isThisFreePlay(DateTime date)
        {
            Boolean flag = false;
            DateTime dateStart = date.AddHours(-date.Hour).AddMinutes(-date.Minute).AddSeconds(-date.Second);
            String szDate = dateStart.ToShortDateString().Trim();
            if (cf.freePlayTimes.ContainsKey(szDate))
            {
                List<String> times = cf.freePlayTimes[szDate];
                foreach (String timeFrame in cf.freePlayTimes[szDate])
                {
                    String[] timeStamps = timeFrame.Split('-');
                    if (timeStamps.Length >= 2)
                    {
                        String[] timeStart = timeStamps[0].Split(':');
                        String[] timeEnd = timeStamps[1].Split(':');

                        int startHour = Convert.ToInt32(timeStart[0]);
                        int startMin = Convert.ToInt32(timeStart[1]);

                        int endHour = Convert.ToInt32(timeEnd[0]);
                        int endMin = Convert.ToInt32(timeEnd[1]);

                        DateTime startDate = new DateTime(date.Year, date.Month, date.Day, startHour, startMin, 0);
                        DateTime endDate = new DateTime(date.Year, date.Month, date.Day, endHour, endMin, 59);

                        if (date >= startDate && date <= endDate)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }
        public Boolean isThisInTimes(DateTime date,Dictionary<String, List<String>> timeContainer)
        {
            Boolean flag = false;
            DateTime dateStart = date.AddHours(-date.Hour).AddMinutes(-date.Minute).AddSeconds(-date.Second);
            String szDate = dateStart.ToShortDateString().Trim();
            if (timeContainer.ContainsKey(szDate))
            {
                List<String> times = timeContainer[szDate];
                foreach (String timeFrame in timeContainer[szDate])
                {
                    String[] timeStamps = timeFrame.Split('-');
                    if (timeStamps.Length >= 2)
                    {
                        String[] timeStart = timeStamps[0].Split(':');
                        String[] timeEnd = timeStamps[1].Split(':');

                        int startHour = Convert.ToInt32(timeStart[0]);
                        int startMin = Convert.ToInt32(timeStart[1]);

                        int endHour = Convert.ToInt32(timeEnd[0]);
                        int endMin = Convert.ToInt32(timeEnd[1]);

                        DateTime startDate = new DateTime(date.Year, date.Month, date.Day, startHour, startMin, 0);
                        DateTime endDate = new DateTime(date.Year, date.Month, date.Day, endHour, endMin, 59);

                        if (date >= startDate && date <= endDate)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }
        public void writeMergedFile(Dictionary<String, List<PersonInfo>> rawInfo, Dictionary<String, List<PersonInfo>> rawLenaInfo)
        {
            using (TextWriter sw = new StreamWriter(cf.syncFilePre + day.Month + "_" + day.Day + "_" + day.Year + ".CSV"))
            {
                sw.Write("BID, UbiID, LenaID, Is Free Play,Date String,X,Y,Z,Orientation");
                sw.WriteLine("Block Duration, Voc Duration, Voc Count, Turn Count,Noise,Adult Count,OLN");
                foreach (String bid in cf.bids)
                {
                    foreach (PersonInfo p in rawInfo[bid])
                    {
                        sw.WriteLine(p.bid + "," + p.ubiId + "," + p.lenaId + "," + p.isFreePlay + "," + p.dt + "," + p.x + "," + p.y + "," + p.z + "," + p.ori);
                    }
                    foreach (PersonInfo p in rawLenaInfo[bid])
                    {
                        sw.WriteLine(p.bid + "," + p.ubiId + "," + p.lenaId + "," + p.isFreePlay + "," + p.dt + ",,,,," + p.bd + "," + p.vd + "," + p.vc + "," + p.tc + "," + p.no + "," + p.ac + "," + p.oln);
                    }
                }
            }
        }
        public double calcSquaredDist(PersonInfo a, PersonInfo b)
        {
            Double x1 = a.x;
            Double y1 = a.y;
            Double x2 = b.x;
            Double y2 = b.y;
            return Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2);
        }
        public static Boolean withinOrientation(PersonInfo a, PersonInfo b, double angle)
        {
            Tuple<double, double> angles = withinOrientationData(a, b);
            return Math.Abs(angles.Item1) <= angle && Math.Abs(angles.Item2) <= angle;
        }
        public static Tuple<double, double> withinOrientationData(PersonInfo a, PersonInfo b)
        {
            Tuple<double, double> r = new Tuple<double, double>(180, 180);
            if (a.lx > 0 && a.ly > 0 && b.lx > 0 && b.ly > 0)
            {
                double a_center_x = getCenter(a.rx, a.lx);
                double a_center_y = getCenter(a.ry, a.ly);
                double b_center_x = getCenter(b.rx, b.lx);
                double b_center_y = getCenter(b.ry, b.ly);

                double d_ab_x = b_center_x - a_center_x;
                double d_ab_y = b_center_y - a_center_y;// getCenter(b.ry, b.ly) - getCenter(a.ry, a.ly);
                normalize(ref d_ab_x, ref d_ab_y);
                double d_ba_x = -d_ab_y;
                double d_ba_y = d_ab_x;

                double da_x = (a.lx - a.rx) / 2;
                double da_y = (a.ly - a.ry) / 2;
                double db_x = (b.lx - b.rx) / 2;
                double db_y = (b.ly - b.ry) / 2;

                normalize(ref da_x, ref da_y);
                normalize(ref db_x, ref db_y);

                double dx_a = (d_ab_x * da_x) + (d_ab_y * da_y);
                double dy_a = (d_ba_x * da_x) + (d_ba_y * da_y);
                double o_a = Math.Atan2(-dx_a, dy_a) * (180 / Math.PI);

                double dx_b = (d_ab_x * db_x) + (d_ab_y * db_y);
                double dy_b = (d_ba_x * db_x) + (d_ba_y * db_y);
                double o_b = Math.Atan2(dx_b, -dy_b) * (180 / Math.PI);
                r = new Tuple<double, double>((o_a), (o_b));
            }
            return r;
        }
        public static void normalize(ref double x, ref double y)
        {
            double r = Math.Sqrt((x * x) + (y * y));
            x = x / r;
            y = y / r;
        }
        public static double getCenter(double x, double x2)
        {
            double l = Math.Abs(x2 - x) / 2;
            return x < x2 ? x + l : x2 + l;
        }
        public Boolean justProx = false;//FIX
        public Boolean isWithLenaStart(DateTime d, String p)
        {
            return startLenaTimes.ContainsKey(p) &&
                startLenaTimes[p].CompareTo(d) <= 0;
        }
        public void countInteractions(Boolean writeFile, Boolean writeDetailFile, Boolean trunkDetailFile, DateTime trunk, Dictionary<String, List<PersonInfo>> lenaInfo)
        {
            try
            {
                TextWriter sw = null;
                if (writeDetailFile)
                    sw = new StreamWriter(cf.root + cf.classroom + "/SYNC/" + cf.version + "interaction_angles_xy_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".csv");
                {
                    if (writeDetailFile)
                        sw.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
                    //foreach (DateTime dt in activities.Keys)
                    //{
                    foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> opi in activities.OrderBy(key => key.Key))
                    {
                        DateTime dt = opi.Key;

                        if (trunkDetailFile && dt.CompareTo(trunk) <= 0 && ((!cf.justFreePlay) || (isThisFreePlay(dt))))/////
                        {

                            foreach (String person in activities[dt].Keys)
                            {

                                if (noLena || (!startFromLena) || isWithLenaStart(dt, person))
                                {
                                    if (!individualTime.ContainsKey(person))
                                    {
                                        individualTime.Add(person, 0.0);
                                    }
                                    else
                                    {
                                        individualTime[person] += 0.1;
                                    }

                                    //LQ: why 0 for ind time and pairtime.1
                                    foreach (String p in activities[dt].Keys)
                                    {
                                        if (!p.Equals(person))
                                        {
                                            String pair = p + "-" + person;
                                            if ((!cf.pairs.Contains(pair)) && cf.pairs.Contains(person + "-" + p))
                                                pair = person + "-" + p;

                                            if (!pairTime.ContainsKey(pair))
                                            {
                                                pairTime.Add(pair, 0);///////
                                                pairClose.Add(pair, 0);
                                                pairCloseOrientation.Add(pair, 0);
                                                pairStats.Add(pair, 0);
                                                pairStatsSep.Add(pair, new PairInfo());
                                                pairStatsSeparated.Add(pair, new Tuple<double, double, double, double>(0, 0, 0, 0));
                                                pairCry.Add(pair, 0);
                                            }



                                            pairTime[pair] += .1;
                                            double dist = calcSquaredDist(activities[dt][person], activities[dt][p]);
                                            if (dist <= minDistance)
                                            {
                                                pairClose[pair] += .1;
                                                Tuple<double, double> angles = withinOrientationData(activities[dt][p], activities[dt][person]);
                                                Boolean orientedCloseness = Math.Abs(angles.Item1) <= 45 && Math.Abs(angles.Item2) <= 45;
                                                if (writeDetailFile)
                                                {
                                                    sw.WriteLine(person + "," + p + "," + dt.ToLongTimeString() + ",0.1," + (orientedCloseness ? "0.1," : "0,") + (angles.Item1) + "," + (angles.Item2) + "," + activities[dt][person].lx + "," + activities[dt][person].ly + "," + activities[dt][person].rx + "," + activities[dt][person].ry + "," + activities[dt][p].lx + "," + activities[dt][p].ly + "," + activities[dt][p].rx + "," + activities[dt][p].ry);
                                                }
                                                if (orientedCloseness)
                                                {
                                                    pairCloseOrientation[pair] += .1;

                                                }
                                            }
                                        }
                                    }
                                    //if (activities[dt][person].wasTalking)
                                    Boolean wasTalking = activities[dt][person].wasTalking;
                                    Double tc = activities[dt][person].tc;
                                    Double vc = activities[dt][person].vc;
                                    Double vd = activities[dt][person].vd;
                                    Double a = activities[dt][person].ac;
                                    Double n = activities[dt][person].no;
                                    Double o = activities[dt][person].oln;
                                    Double c = activities[dt][person].cry;

                                    if (wasTalking || vd > 0 || tc > 0 || a > 0 || n > 0 || vc > 0 || o > 0 || c > 0)
                                    {
                                        foreach (String p in activities[dt].Keys)
                                        {
                                            if (!p.Equals(person))
                                            {
                                                double dist = calcSquaredDist(activities[dt][p], activities[dt][person]);
                                                if (dist <= minDistance)
                                                {
                                                    if (Math.Round(dist, 2) == 0.63 && p == "Lab2D" && person == "2D")
                                                    {
                                                        int t = 0;
                                                    }
                                                    if (justProx || withinOrientation(activities[dt][p], activities[dt][person], 45))
                                                    {
                                                        String pair = p + "-" + person;
                                                        if (!pairStats.ContainsKey(pair))
                                                        {
                                                            if (pairStats.ContainsKey(person + "-" + p))
                                                            {
                                                                pair = person + "-" + p;
                                                            }
                                                            else
                                                            {
                                                                pairStats.Add(pair, 0);
                                                                pairStatsSep.Add(pair, new PairInfo());
                                                                pairStatsSeparated.Add(pair, new Tuple<double, double, double, double>(0, 0, 0, 0));
                                                                pairCry.Add(pair, 0);
                                                            }
                                                        }
                                                        if (activities[dt][person].cry > 0 && activities[dt][p].cry > 0)
                                                        {
                                                            if (pair == "T2A-17A" || pair == "17A-T2A")
                                                            {
                                                                int stop = 1;
                                                            }

                                                            if (person.Equals(pair.Split('-')[0]))
                                                            {
                                                                pairCry[pair] += (activities[dt][person].cry);
                                                                pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3 + activities[dt][person].cry, pairStatsSeparated[pair].Item4);// + activities[dt][p].cry);
                                                            }
                                                            else
                                                            {//p is in the first part of the pair
                                                                pairCry[pair] += (activities[dt][p].cry);
                                                                pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4 + activities[dt][p].cry);
                                                            }
                                                        }


                                                        //if (tc > 0)
                                                        {
                                                            List<PersonInfo> pi = lenaInfo[person];
                                                            foreach (PersonInfo i in pi)
                                                            {
                                                                DateTime dt2 = i.dt;
                                                                DateTime dt3 = i.dt.AddSeconds(i.bd);
                                                                //////////////
                                                                int ms = dt2.Millisecond > 0 ? dt2.Millisecond / 100 * 100 : dt2.Millisecond;// + 100;
                                                                dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, dt2.Minute, dt2.Second, ms);
                                                                ms = dt3.Millisecond > 0 ? dt3.Millisecond / 100 * 100 : dt3.Millisecond;// + 100;
                                                                dt3 = new DateTime(dt3.Year, dt3.Month, dt3.Day, dt3.Hour, dt3.Minute, dt3.Second, ms);
                                                                i.bd = (dt3 - dt2).Seconds + ((dt3 - dt2).Milliseconds / 1000.00);
                                                                if (dt >= dt2 && dt <= dt3)
                                                                {
                                                                    tc = i.tc > 0 && i.bd > 0 ? (i.tc / i.bd) / 10 : 0;
                                                                    a = i.ac > 0 && i.bd > 0 ? (i.ac / i.bd) / 10 : 0;
                                                                    n = i.no > 0 && i.bd > 0 ? (i.no / i.bd) / 10 : 0;
                                                                    vc = i.vc > 0 && i.bd > 0 ? (i.vc / i.bd) / 10 : 0;
                                                                    o = i.oln > 0 && i.bd > 0 ? (i.oln / i.bd) / 10 : 0;
                                                                    c = i.cry > 0 && i.bd > 0 ? (i.cry / i.bd) / 10 : 0;
                                                                    //vd = i.vd > 0 && i.bd > 0 ? (i.vd / i.bd) / 10 : 0;
                                                                    double vd2 = i.vd > 0 && i.bd > 0 ? (i.vd / i.bd) / 10 : 0;
                                                                    PairInfo pairInfo = pairStatsSep[pair];
                                                                    if (person.Equals(pair.Split('-')[0]))
                                                                    {

                                                                        pairInfo.p1.vd += vd2;
                                                                        pairInfo.p1.vc += vc;
                                                                        pairInfo.p1.tc += tc;
                                                                        pairInfo.p1.ac += a;
                                                                        pairInfo.p1.no += n;
                                                                        pairInfo.p1.oln += o;
                                                                        pairInfo.p1.cry += c;
                                                                    }
                                                                    else //p is in the first part of the pair
                                                                    {
                                                                        pairInfo.p2.vd += vd2;
                                                                        pairInfo.p2.vc += vc;
                                                                        pairInfo.p2.tc += tc;
                                                                        pairInfo.p2.ac += a;
                                                                        pairInfo.p2.no += n;
                                                                        pairInfo.p2.oln += o;
                                                                        pairInfo.p2.cry += c;
                                                                    }///

                                                                }
                                                            }



                                                        }
                                                        if (wasTalking)
                                                        {
                                                            pairStats[pair] += 0.1;
                                                            if (person.Equals(pair.Split('-')[0]))
                                                            {
                                                                pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1 + 0.1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4);
                                                            }
                                                            else //p is in the first part of the pair
                                                            {
                                                                pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2 + 0.1, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    bool stop = true;
                                }
                            }
                        }
                    }
                }
                if (writeFile)
                {
                    using (sw = new StreamWriter(cf.version + "interaction_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".csv"))
                    {
                        sw.WriteLine("Person 1, Person2, Interaction Time, Total Time, Interaction Normalized");
                        foreach (String s in pairStats.Keys)
                        {
                            double interactionTime = Math.Round(pairStats[s], 1);
                            double totalTime = Math.Round(pairTime[s], 1);
                            double interactionNormalized = interactionTime / totalTime;
                            sw.WriteLine(s.Split('-')[0] + "," + s.Split('-')[1] + "," + interactionTime + "," + totalTime + "," + interactionNormalized);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void write10SecTalkingCSV(String file_name)
        {
            Boolean vel = true;
            Boolean doDist = true;
            Dictionary<String, double> vels1 = new Dictionary<string, double>();
            Dictionary<String, List<double>> vels = new Dictionary<string, List<double>>();
            Dictionary<String, DateTime> pos = new Dictionary<string,DateTime>();
            Dictionary<String, List<int>> velCounts = new Dictionary<string, List<int>>();
            double time = 0;
            DateTime dateTime = new DateTime();
            Dictionary<String, List<double>> distances = new Dictionary<string, List<double>>();
            using (TextWriter sw = new StreamWriter(file_name))
            {
                sw.WriteLine("BID, DateTime, X, Y, Orientation, Talking, Aid, S, Type,rx,ry,lx,ly");
                foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> pi in activities.OrderBy(key => key.Key))
                {
                    DateTime dt = pi.Key;
                    if ((!cf.justFreePlay) || (isThisFreePlay(dt)))
                    {
                        Boolean setDistance = false;
                        if(time==0)
                        {
                            time = 1;
                            dateTime = dt;
                        }
                        if (((dt.Hour*60) + dt.Minute)- ((dateTime.Hour*60) + dateTime.Minute) >= 30)
                        {
                            time = (dt.Hour * 60 )+ dt.Minute;
                            dateTime = dt;
                            setDistance = true;
                        }
                        foreach (String s in activities[dt].Keys)
                        {
                            //tagInfo ti = tags[s];
                            MappingRow mr = cf.getMapping(s, day);
                            if (((!startFromLena) || isWithLenaStart(dt, s))
                                /*&&
                                activities[dt][s].rx>0&&
                                    activities[dt][s].ry>0&&
                                    activities[dt][s].lx > 0 &&
                                    activities[dt][s].ly > 0*/)
                            {

                                if (mr.BID == "Lab3d")  
                                {
                                    int stop = 0;
                                }
                                sw.WriteLine(s + "," + dt.ToString("hh:mm:ss.fff tt") + "," + activities[dt][s].x + "," + activities[dt][s].y + "," + activities[dt][s].ori + "," + 
                                    activities[dt][s].wasTalking + "," + mr.aid + "," + mr.sex+","+mr.type+","+
                                    activities[dt][s].rx+","+
                                    activities[dt][s].ry + "," +
                                    activities[dt][s].lx + "," +
                                    activities[dt][s].ly  );
 
                                if (doDist)
                                {
                                    if (!pos.ContainsKey(s))
                                    {
                                        pos.Add(s, dt);
                                        vels.Add(s, new List<double>());
                                        velCounts.Add(s, new List<int>());
                                        vels[s].Add(0);
                                        velCounts[s].Add(0); distances.Add(s, new List<double>());
                                        distances[s].Add(0);
                                    }
                                    else
                                    {

                                        if ((dt - pos[s]).TotalSeconds <= 60)
                                        {

                                            double dist = calcSquaredDist(activities[pos[s]][s], activities[dt][s]) / ((dt - pos[s]).TotalSeconds);
                                            vels[s][vels[s].Count - 1] += dist;
                                            velCounts[s][velCounts[s].Count - 1] += 1;

                                            if (setDistance)
                                            {
                                                distances[s].Add(0);

                                            }
                                            distances[s][distances[s].Count - 1] += dist;

                                        }
                                        else

                                        {

                                            vels[s].Add(0);
                                            velCounts[s].Add(0);
                                        }



                                            pos[s] = dt;
                                    }
                                }
                            }
                        }
                        setDistance = false;
                    }
                }
            }
        
             if(doDist)
                using (TextWriter sw = new StreamWriter(cf.root+"//"+cf.classroom+"//"+ cf.classroom+"_VELOCITY.CSV",!first))
                {
                    using (TextWriter sw2 = new StreamWriter(cf.root + "//" + cf.classroom + "//" + cf.classroom + "_DISTANCES_v2.CSV", !first))
                    {
                        if (first)
                        {
                            sw2.Write("DATE, BID");
                            sw.WriteLine("DATE, BID, VEL");
                             
                        }
                        foreach (String s in distances.Keys)
                        {
                            if (first)
                            {

                                for (int sd = 0; sd < distances[s].Count; sd++)
                                {
                                    sw2.Write("," + 30 * (sd + 1));

                                }
                                sw2.WriteLine(",VEL");
                                first = false;
                            }
                            sw2.Write(day.Month + "/" + day.Day + "/" + day.Year + "," + s + ",");

                            double distanceWalked = 0;
                            for (int sd = 0; sd < distances[s].Count; sd++)
                                {
                                distanceWalked += distances[s][sd];
                                sw2.Write(distanceWalked+",");

                                }
                            double velo = 0;
                            int c = 0;
                            foreach (double v in vels[s])
                            {
                                velo += v / velCounts[s][c];
                                c++;
                            }
                            sw2.WriteLine(  velo / c);
                             

                    }
                    }

                if ((!doDist) && vel)
                    foreach (String s in vels.Keys)
                    {
                        int c = 0;
                        double velo = 0;
                        foreach (double v in vels[s])
                        {
                            velo+= v / velCounts[s][c];
                            c++;
                        }
                        sw.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + velo / c);

                    }
                }
        }
        public void write10SecCryCSV(String file_name, List<String> subjects)
        {
            Dictionary<String, Dictionary<DateTime, Tuple<bool, bool>>> secs = new Dictionary<string, Dictionary<DateTime, Tuple<bool, bool>>>();
            using (TextWriter sw = new StreamWriter(file_name.Replace(".", "SECS_ALLLENA.")))
            {
                sw.WriteLine("BID, DateTime,Talking, Cry");
                foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> pi in activities.OrderBy(key => key.Key))
                {
                    DateTime dt = pi.Key;
                    if ((!cf.justFreePlay) || (isThisFreePlay(dt)))
                    {

                       // foreach (String s in activities[dt].Keys)
                        foreach (String s in cf.bids)
                            {
                                //tagInfo ti = tags[s];

                                if (subjects.Contains(s))
                            {
                                DateTime secDate = dt.AddMilliseconds(-dt.Millisecond);//.AddSeconds(-dt.Second);
                                if (!secs.ContainsKey(s))
                                {
                                    secs.Add(s, new Dictionary<DateTime, Tuple<bool, bool>>());
                                }
                                if (!secs[s].ContainsKey(secDate))
                                {
                                    secs[s].Add(secDate, new Tuple<bool, bool>(false, false));
                                }

                                MappingRow mr = cf.getMapping(s, day);
                                if (activities[dt].ContainsKey(s) && ((!startFromLena) || isWithLenaStart(dt, s))
                                                    /*&&
                                                    activities[dt][s].rx>0&&
                                                        activities[dt][s].ry>0&&
                                                        activities[dt][s].lx > 0 &&
                                                        activities[dt][s].ly > 0*/)
                                {

                                    secs[s][secDate] = new Tuple<bool, bool>(secs[s][secDate].Item1 || activities[dt][s].wasTalking, secs[s][secDate].Item2 || activities[dt][s].isCrying);

                                }
                                else
                                {
                                    secs[s][secDate] = new Tuple<bool, bool>(secs[s][secDate].Item1 , secs[s][secDate].Item2);

                                }
                            }
                        }


                    }
                }
                 
                    foreach (String s in cf.bids)
                {
                    //tagInfo ti = tags[s];
                    int count = 1;
                    if(secs.ContainsKey(s))
                    foreach (DateTime date in secs[s].Keys)
                    {


                        sw.WriteLine(s + "," + count + "," + (secs[s][date].Item1?".":"0") + "," + (secs[s][date].Item2 ? "." : "0"));
                        count++;

                    }
                }
            }

    
        }
        public static Boolean first = true;
        public Dictionary<DateTime, Dictionary<String, double>> adjustTimes = new Dictionary<DateTime, Dictionary<String, double>>();
        public void setAdjusts()
        {
            Dictionary<String, double> times = new Dictionary<String, double>();
            times["14861"] = 0;
            times["14866"] = 2;
            times["8236"] = -31;
            times["14867"] = -2;
            times["11566"] = 0;
            times["14859"] = 0;
            times["7539"] = -103;
            times["14868"] = 0;
            times["14865"] = 4;
            times["11563"] = 9;
            times["11564"] = -23;
            times["8235"] = 4;
            times["14863"] = 1;
            /*e20170310_133854_014861.wav	0
e20170310_132949_014866.wav	2
e20170310_133201_008236.wav	-31
e20170310_133239_014867.wav	-2
e20170310_133432_011566.wav	0
e20170310_133509_014859.wav	0
e20170310_133646_007539.wav	-103
e20170310_133719_014868.wav	0
e20170310_133807_014865.wav	4
e20170310_134011_011563.wav	9
e20170310_134110_011564.wav	-23
e20170310_134147_008235.wav	4
e20170310_134226_014863.wav	1
*/
            /*e20170323_094940_011566.wav	0
 e20170323_095028_011564.wav	-6.79375
 e20170323_095107_014866.wav	21.3674375
 e20170323_095155_014859.wav	7.9963125
 e20170323_095302_014861.wav	16
 e20170323_095402_008235.wav	30.9965
 e20170323_095500_007539.wav	55.067
 e20170323_095556_014870.wav	19.307625
 e20170323_095649_008236.wav	6.8688125
 e20170323_100008_014864.wav	19
 e20170323_100142_014865.wav	29.7556875
 e20170323_100257_011563.wav	-1.3576875
 e20170323_100415_014868.wav	18.3675625
 e20170323_100553_014863.wav	21
 */
            Dictionary<String, double> times2 = new Dictionary<String, double>();
            times["14864"] = 19-1;
            times["14870"] = 19.307625 - 1;
            times["14861"] = 16 - 1;
            times["14866"] = 21.3674375 - 1;
            times["8236"] = 6.8688125 - 1;
            times["14867"] = 0 - 1;
            times["11566"] = 0 - 1;
            times["14859"] = 7.9963125 - 1;
            times["7539"] = 55.067 - 1;
            times["14868"] = 18.3675625 - 1;
            times["14865"] = 29.7556875 - 1;
            times["11563"] = -1.3576875 - 1;
            times["11564"] = -6.79375 - 1;
            times["8235"] = 30.9965 - 1;
            times["14863"] = 21 - 1;

            adjustTimes[new DateTime(2017, 03, 10)] = times;
            adjustTimes[new DateTime(2017, 03, 17)] = times2;
        }
        public Tuple<String, double> maxVD = new Tuple<string, double>("", 0);
        public Tuple<String, double> maxVD2 = new Tuple<string, double>("", 0);
        public Tuple<String, double> maxVD3 = new Tuple<string, double>("", 0);

        public Dictionary<String, List<PersonInfo>> readLenaItsFiles()
        {
            Dictionary< String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();

            string[] folders = Directory.GetDirectories(cf.lenaFileDir + "//ITS//");
            foreach (string folder in folders)
            {
                String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                DateTime folderDate;
                if (DateTime.TryParse(folderName, out folderDate) && folderDate >= day && folderDate < day.AddDays(1))
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        String fileName = Path.GetFileName(file);
                        {
                            Console.WriteLine(file);
                            String lenaId = file.Substring(file.IndexOf("\\") + 2);
                            lenaId = lenaId.Substring(16, 6);
                            if (lenaId.Substring(0, 2) == "00")
                                lenaId = lenaId.Substring(2);
                            else if (lenaId.Substring(0, 1) == "0")
                                lenaId = lenaId.Substring(1);
                             

                            XmlDocument doc = new XmlDocument();
                            doc.Load(file);
                            XmlNodeList rec = doc.ChildNodes[2].SelectNodes("ProcessingUnit/Recording");

                            /*DateTime dt2 = i.dt;
                                                            DateTime dt3 = i.dt.AddSeconds(i.bd);
                                                            //////////////
                                                            int ms = dt2.Millisecond > 0 ? dt2.Millisecond / 100 * 100 : dt2.Millisecond;// + 100;
                                                            dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, dt2.Minute, dt2.Second, ms);
                                                            ms = dt3.Millisecond > 0 ? dt3.Millisecond / 100 * 100 : dt3.Millisecond;// + 100;
                                                            dt3 = new DateTime(dt3.Year, dt3.Month, dt3.Day, dt3.Hour, dt3.Minute, dt3.Second, ms);
                                                            i.bd= (dt3 - dt2).Seconds + ((dt3 - dt2).Milliseconds / 1000.00);
                                                            */
                            foreach (XmlNode recording in rec)
                            {
                                DateTime recStartTimeOriginal = DateTime.Parse(recording.Attributes["startClockTime"].Value);
                                XmlNodeList nodes = recording.SelectNodes("Conversation|Pause");
                                XmlNodeList nodesP = recording.SelectNodes("Conversation");
                                double adjustedSecs = getAdjustedSecs(lenaId);
                                DateTime recStartTime = Config.getMsTime(recStartTimeOriginal.AddSeconds(adjustedSecs));
                                if (recStartTime.Day == day.Day)
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
                                        MappingRow mr = cf.getLenaMapping(lenaId, start);
                                        if (conv.Name == "Conversation")
                                        {
                                            double tc = Convert.ToDouble(conv.Attributes["turnTaking"].Value);
                                            if (tc > 0)
                                            {
                                                pi.dt = start;
                                                pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                pi.bid = mr.BID;// ubiAndBId[1];
                                                pi.lenaId = lenaId;
                                                pi.bd = (end - start).Seconds + ((end - start).Milliseconds / 1000.00); //endSecs - startSecs;
                                                pi.tc = tc;
                                                addToRawLena(ref rawLenaInfo, pi);
                                            }
                                        }

                                        foreach (XmlNode seg in segments)
                                        {
                                            //startClockTime
                                            startSecs = Convert.ToDouble(seg.Attributes["startTime"].Value.Substring(2, seg.Attributes["startTime"].Value.Length - 3));
                                            endSecs = Convert.ToDouble(seg.Attributes["endTime"].Value.Substring(2, seg.Attributes["endTime"].Value.Length - 3));
                                            start = Config.getMsTime(recStartTime.AddSeconds(startSecs));
                                            end = Config.getMsTime(recStartTime.AddSeconds(endSecs));
                                            pi = new PersonInfo();
                                            pi.dt = start;
                                            mr = cf.getLenaMapping(lenaId, start);
                                            pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                            pi.bid = mr.BID;// ubiAndBId[1];
                                            pi.lenaId = lenaId;
                                            String speaker = seg.Attributes["spkr"].Value;
                                            pi.bd = (end - start).Seconds + ((end - start).Milliseconds / 1000.00); //endSecs - startSecs;
                                            Boolean add = false;
                                            switch (speaker)
                                            {
                                                case "CHN":
                                                case "CHF":
                                                    pi.vd = Convert.ToDouble(seg.Attributes["childUttLen"].Value.Substring(1, seg.Attributes["childUttLen"].Value.Length - 2));
                                                    pi.vc = Convert.ToDouble(seg.Attributes["childUttCnt"].Value);
                                                    foreach(XmlAttribute atts in seg.Attributes)
                                                    {
                                                        if(atts.Name.IndexOf("startCry")==0)
                                                        {
                                                            String cryStep = atts.Name.Substring(8);
                                                            String att = atts.Name;
                                                            double cstartSecs = Convert.ToDouble(seg.Attributes[att].Value.Substring(2, seg.Attributes[att].Value.Length - 3));
                                                            double cendSecs = Convert.ToDouble(seg.Attributes["endCry"+cryStep].Value.Substring(2, seg.Attributes["endCry" + cryStep].Value.Length - 3));
                                                            DateTime cstart = Config.getMsTime(recStartTime.AddSeconds(cstartSecs));
                                                            DateTime cend = Config.getMsTime(recStartTime.AddSeconds(cendSecs));
                                                            PersonInfo cpi = new PersonInfo();
                                                            cpi.dt = cstart;
                                                            mr = cf.getLenaMapping(lenaId, start);
                                                            cpi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                            cpi.bid = mr.BID;// ubiAndBId[1];
                                                            cpi.lenaId = lenaId;
                                                            cpi.bd = (cend - cstart).Seconds + ((cend - cstart).Milliseconds / 1000.00); //cendSecs - cstartSecs;
                                                            cpi.cry = cpi.bd;
                                                            addToRawLena(ref rawLenaInfo, cpi);
                                                        }

                                                    }
                                                    add = true;
                                                    break;
                                                case "FAN":
                                                    pi.ac = Convert.ToDouble(seg.Attributes["femaleAdultWordCnt"].Value);
                                                    add = true;
                                                    break;
                                                case "MAN":
                                                    pi.ac = Convert.ToDouble(seg.Attributes["maleAdultWordCnt"].Value);
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

                                                addToRawLena(ref rawLenaInfo, pi);
                                                
                                            }
                                            
                                        }
                                      
                                    }
                                }
                            }
                        
                        }
                             
                    }
                }
            }

             return rawLenaInfo;
         }
         public void addToRawLena(ref Dictionary<String, List<PersonInfo>> rl, PersonInfo lpi)
        {
            if (!rl.ContainsKey(lpi.bid))
            {
                rl.Add(lpi.bid, new List<PersonInfo>());
            }
            rl[lpi.bid].Add(lpi);
        }
         public void setLenaData1(Dictionary<String, List<PersonInfo>> lenadata)
         {
             foreach (String person in lenadata.Keys)
             {

                 List<PersonInfo> dataLine = lenadata[person];
                 Boolean startSet = false;
                 foreach (PersonInfo data in dataLine)
                 {
                     DateTime time = new DateTime(data.dt.Year, data.dt.Month, data.dt.Day, data.dt.Hour, data.dt.Minute, data.dt.Second, 0);
                     if (!startSet)
                     {
                         startLenaTimes.Add(person, time);
                         startSet = true;
                     }
                     Double vocDur = 0;// data.vd;
                     Double blockDur = data.bd;
                     Double vocCount = data.vc;
                     Double turnCount = data.tc;
                     Double a = data.ac;
                     Double n = data.no;
                     Double o = data.oln;
                     double turnCount10 = (turnCount / blockDur) / 10;
                     double vocCount10 = (vocCount / blockDur) / 10;
                     double vocDur10 = 0;// (vocDur / blockDur) / 10;
                     double adults10 = (a / blockDur) / 10;
                     double noise10 = (n / blockDur) / 10;
                     double oln10 = (o / blockDur) / 10;
                     if (personTotalCounts.ContainsKey(person))/////////////////
                     {
                         //Tuple<double, double, double, double, double> totalInfo = personTotalCounts[person];

                         //personTotalCounts[person].vd = personTotalCounts[person].vd + vocDur;
                         personTotalCounts[person].vc = personTotalCounts[person].vc + vocCount;
                         personTotalCounts[person].tc = personTotalCounts[person].tc + turnCount;
                         personTotalCounts[person].ac = personTotalCounts[person].ac + a;
                         personTotalCounts[person].no = personTotalCounts[person].no + n;
                         personTotalCounts[person].oln = personTotalCounts[person].oln + o;

                         //personTotalCounts[person] = new Tuple<double, double, double, double, double>(totalInfo.Item1 + vocDur, totalInfo.Item2 + vocCount, totalInfo.Item3 + turnCount, totalInfo.Item4 + a, totalInfo.Item5 + n);
                     }
                     else
                     {
                         PersonInfo pi = new PersonInfo();
                         //pi.vd = vocDur;
                         pi.vc = vocCount;
                         pi.tc = turnCount;
                         pi.ac = a;
                         pi.no = n;
                         pi.oln = o;
                         personTotalCounts.Add(person, pi);
                         personTotalCountsWUbi.Add(person, new PersonInfo());
                         //personTotalCounts.Add(person, new Tuple<double, double, double, double, double>(vocDur, vocCount, turnCount, a, n));
                     }
                     do
                     {
                         if (activities.ContainsKey(time))
                         {
                             if (activities[time].ContainsKey(person))
                             {
                                 //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                 //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);
                                 activities[time][person].wasTalking = vocDur > 0;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                 activities[time][person].vc = vocCount10;
                                 activities[time][person].tc = turnCount10;
                                 //activities[time][person].vd = vocDur10;
                                 activities[time][person].ac = adults10;
                                 activities[time][person].oln = oln10;
                                 activities[time][person].no = noise10;

                                 if (personTotalCountsWUbi.ContainsKey(person))/////////////////
                                 {
                                     personTotalCountsWUbi[person].vc += vocCount10;
                                     personTotalCountsWUbi[person].tc += turnCount10;
                                     //personTotalCountsWUbi[person].vd += vocDur10;
                                     personTotalCountsWUbi[person].ac += adults10;
                                     personTotalCountsWUbi[person].oln += oln10;
                                     personTotalCountsWUbi[person].no += noise10;
                                 }

                             }
                         }
                        else if(false) 
                        {
                            ////// TO DELETE for cry utt figure test.....



                            activities.Add(time, new Dictionary<string, PersonInfo>());
                            if (!activities[time].ContainsKey(person))
                                activities[time].Add(person, new PersonInfo());
                            //if (activities[time].ContainsKey(person))
                            {
                                //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);
                                activities[time][person].wasTalking = vocDur > 0;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                activities[time][person].vc = vocCount10;
                                activities[time][person].tc = turnCount10;
                                //activities[time][person].vd = vocDur10;
                                activities[time][person].ac = adults10;
                                activities[time][person].oln = oln10;
                                activities[time][person].no = noise10;

                                 

                            }
                        }

                        time = time.AddMilliseconds(100);
                         vocDur -= 0.1;
                         blockDur -= 0.1;
                     } while (blockDur > 0);
                 }
             }
         }
         public void setLenaItsData(Dictionary<String, List<PersonInfo>> lenadata)
         {
             foreach (String person in lenadata.Keys)
             {
                Boolean startSet = false;
                List<PersonInfo> dataLine = lenadata[person];
                 foreach (PersonInfo data in dataLine)
                 {
                    DateTime time = data.dt; //new DateTime(data.dt.Year, data.dt.Month, data.dt.Day, data.dt.Hour, data.dt.Minute, data.dt.Second, 0);
                    int ms = time.Millisecond>0?time.Millisecond / 100 * 100: time.Millisecond;// + 100;
                    time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, ms);
                    DateTime timeEnd = data.dt.AddSeconds(data.bd);
                    ms = timeEnd.Millisecond > 0 ? timeEnd.Millisecond / 100 * 100 : timeEnd.Millisecond;// + 100;
                    timeEnd = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, timeEnd.Hour, timeEnd.Minute, timeEnd.Second, ms);
                    if (!startSet)
                    {
                        startLenaTimes.Add(person, time);
                        startSet = true;
                    }
                     
                     Double vocDur = data.vd;
                     Double blockDur = data.bd;
                     blockDur = (timeEnd - time).Seconds+((timeEnd - time).Milliseconds/1000.00);
                     Double vocCount = data.vc;
                     Double turnCount = data.tc;
                     Double a = data.ac;
                     Double n = data.no;
                     Double o = data.oln;
                     Double cry = data.cry;
                     double turnCount10 = (turnCount / blockDur) / 10;
                     double vocCount10 = (vocCount / blockDur) / 10;
                     double vocDur10 = (vocDur / blockDur) / 10;
                     double adults10 = (a / blockDur) / 10;
                     double noise10 = (n / blockDur) / 10;
                     double oln10 = (o / blockDur) / 10;
                     double cry10 = (cry / blockDur) / 10;
                     if (personTotalCounts.ContainsKey(person))/////////////////
                     {
                         //Tuple<double, double, double, double, double> totalInfo = personTotalCounts[person];

                         personTotalCounts[person].vd = personTotalCounts[person].vd + vocDur;
                         personTotalCounts[person].vc = personTotalCounts[person].vc + vocCount;
                         personTotalCounts[person].tc = personTotalCounts[person].tc + turnCount;
                         personTotalCounts[person].ac = personTotalCounts[person].ac + a;
                         personTotalCounts[person].no = personTotalCounts[person].no + n;
                         personTotalCounts[person].oln = personTotalCounts[person].oln + o;
                         personTotalCounts[person].cry = personTotalCounts[person].cry + cry;

                        //personTotalCounts[person] = new Tuple<double, double, double, double, double>(totalInfo.Item1 + vocDur, totalInfo.Item2 + vocCount, totalInfo.Item3 + turnCount, totalInfo.Item4 + a, totalInfo.Item5 + n);
                    }
                    else
                     {
                         PersonInfo pi = new PersonInfo();
                         pi.vd = vocDur;
                         pi.vc = vocCount;
                         pi.tc = turnCount;
                         pi.ac = a;
                         pi.no = n;
                         pi.oln = o;
                         pi.cry = cry;
                         personTotalCounts.Add(person, pi);
                         personTotalCountsWUbi.Add(person, new PersonInfo());
                         //personTotalCounts.Add(person, new Tuple<double, double, double, double, double>(vocDur, vocCount, turnCount, a, n));
                     }
                     do
                     {
                         if (activities.ContainsKey(time))
                         {
                             if (activities[time].ContainsKey(person))
                             {
                                 //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                 //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);
                                 activities[time][person].wasTalking = vocDur>0 || activities[time][person].wasTalking ;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                 activities[time][person].isCrying = cry > 0 || activities[time][person].isCrying;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                 activities[time][person].vc += vocCount10;
                                 activities[time][person].tc += turnCount10;
                                 activities[time][person].vd += vocDur10;
                                 activities[time][person].ac += adults10;
                                 activities[time][person].oln += oln10;
                                 activities[time][person].no += noise10;
                                 activities[time][person].cry += cry10;

                                if (personTotalCountsWUbi.ContainsKey(person))/////////////////
                                 {
                                     personTotalCountsWUbi[person].vc += vocCount10;
                                     personTotalCountsWUbi[person].tc += turnCount10;
                                     personTotalCountsWUbi[person].vd += vocDur10;
                                     personTotalCountsWUbi[person].ac += adults10;
                                     personTotalCountsWUbi[person].oln += oln10;
                                     personTotalCountsWUbi[person].no += noise10;
                                    personTotalCountsWUbi[person].cry += cry10;
                                }

                             }
                         }

                         time = time.AddMilliseconds(100);
                         vocDur -= 0.1;
                         blockDur -= 0.1;
                     } while (blockDur > 0);
                 }
             }
         }public void setLenaData2(Dictionary<String, List<PersonInfo>> lenadata)
         {
             foreach (String person in lenadata.Keys)
             {

                 List<PersonInfo> dataLine = lenadata[person];
                 foreach (PersonInfo data in dataLine)
                 {
                     DateTime time = new DateTime(data.dt.Year, data.dt.Month, data.dt.Day, data.dt.Hour, data.dt.Minute, data.dt.Second, 0);

                     Double vocDur = data.vd;
                     Double blockDur = data.bd;
                     Double vocCount = data.vc;
                     Double turnCount = data.tc;
                     Double a = data.ac;
                     Double n = data.no;
                     Double o = data.oln;
                     double turnCount10 = (turnCount / blockDur) / 10;
                     double vocCount10 = (vocCount / blockDur) / 10;
                     double vocDur10 = (vocDur / blockDur) / 10;
                     double adults10 = (a / blockDur) / 10;
                     double noise10 = (n / blockDur) / 10;
                     double oln10 = (o / blockDur) / 10;
                     if (personTotalCounts.ContainsKey(person))/////////////////
                     {
                         //Tuple<double, double, double, double, double> totalInfo = personTotalCounts[person];

                         personTotalCounts[person].vd = personTotalCounts[person].vd + vocDur;
                         personTotalCounts[person].vc = personTotalCounts[person].vc + vocCount;
                         personTotalCounts[person].tc = personTotalCounts[person].tc + turnCount;
                         personTotalCounts[person].ac = personTotalCounts[person].ac + a;
                         personTotalCounts[person].no = personTotalCounts[person].no + n;
                         personTotalCounts[person].oln = personTotalCounts[person].oln + o;

                         //personTotalCounts[person] = new Tuple<double, double, double, double, double>(totalInfo.Item1 + vocDur, totalInfo.Item2 + vocCount, totalInfo.Item3 + turnCount, totalInfo.Item4 + a, totalInfo.Item5 + n);
                     }
                     else
                     {
                         PersonInfo pi = new PersonInfo();
                         pi.vd = vocDur;
                         pi.vc = vocCount;
                         pi.tc = turnCount;
                         pi.ac = a;
                         pi.no = n;
                         pi.oln = o;
                         personTotalCounts.Add(person, pi);
                         personTotalCountsWUbi.Add(person, new PersonInfo());
                         //personTotalCounts.Add(person, new Tuple<double, double, double, double, double>(vocDur, vocCount, turnCount, a, n));
                     }
                     do
                     {
                         if (activities.ContainsKey(time))
                         {
                             if (activities[time].ContainsKey(person))
                             {
                                 //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                 //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);
                                 activities[time][person].wasTalking = vocDur > 0;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                 activities[time][person].vc = vocCount10;
                                 activities[time][person].tc = turnCount10;
                                 activities[time][person].vd = vocDur10;
                                 activities[time][person].ac = adults10;
                                 activities[time][person].oln = oln10;
                                 activities[time][person].no = noise10;

                                 if (personTotalCountsWUbi.ContainsKey(person))/////////////////
                                 {
                                     personTotalCountsWUbi[person].vc += vocCount10;
                                     personTotalCountsWUbi[person].tc += turnCount10;
                                     personTotalCountsWUbi[person].vd += vocDur10;
                                     personTotalCountsWUbi[person].ac += adults10;
                                     personTotalCountsWUbi[person].oln += oln10;
                                     personTotalCountsWUbi[person].no += noise10;
                                 }

                             }
                         }

                         time = time.AddMilliseconds(100);
                         vocDur -= 0.1;
                         blockDur -= 0.1;
                     } while (blockDur > 0);
                 }
             }
         }
        /********************************INTERACTIONS NEW ***************************************************/
        public Dictionary<string, PairInfo> pairInteractions = new Dictionary<string, PairInfo>();

        //public void countInteractions(Config cf, ClassroomDay day, bool writeFile, bool writeDetailFile, DateTime trunk, Dictionary<string, List<PersonInfo>> lenaInfo)
        public void countInteractionsIts(DateTime trunk)
        {
            foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> opi in this.activities.OrderBy(key => key.Key))
            {
                DateTime dt = opi.Key;
                Dictionary<DateTime, Dictionary<String, PersonInfo>> activities = this.activities;
                if (dt.CompareTo(trunk) <= 0 && ((!cf.justFreePlay) || (isThisFreePlay(dt))))/////
                {
                    foreach (String person in activities[dt].Keys)
                    {
                        if (noLena || (!startFromLena) || isWithLenaStart(dt, person))
                        {
                            PersonInfo personInfo = activities[dt][person];
                            personInfo.individualTime += 0.1;
                            personInfo.interactionTime += activities[dt][person].wasTalking?0.1:0;

                            foreach (String p in activities[dt].Keys)
                            {
                                if (!p.Equals(person))
                                {
                                    PersonInfo pInfo = activities[dt][p];

                                    PersonInfo subject = pInfo;
                                    PersonInfo partner = personInfo;
                                    subject.bid = p;
                                    partner.bid = person;
                                    if ((!cf.pairs.Contains(p + "-" + person)) && cf.pairs.Contains(person + "-" + p))
                                    {
                                        subject = personInfo;
                                        partner = pInfo;
                                    }

                                    String pair = subject.bid + "-" + partner.bid;

                                    if (!pairInteractions.ContainsKey(pair))
                                        pairInteractions.Add(pair, new PairInfo());

                                    pairInteractions[pair].sharedTimeInSecs += 0.05;
                                    pairInteractions[pair].subject.bid = subject.bid;
                                    pairInteractions[pair].partner.bid = partner.bid;
                                    pairInteractions[pair].subject.individualTime += subject.individualTime;
                                    pairInteractions[pair].partner.individualTime += partner.individualTime;
                                    pairInteractions[pair].subject.interactionTime += subject.interactionTime;
                                    pairInteractions[pair].partner.interactionTime += partner.interactionTime;


                                    double dist = calcSquaredDist(activities[dt][person], activities[dt][p]);
                                    if (dist <= minDistance)
                                    {
                                        pairInteractions[pair].closeTimeInSecs += 0.05;
                                        if (activities[dt][subject.bid].wasTalking)
                                        {
                                            pairInteractions[pair].closeAndOrientedTalkInSecs += 0.1;
                                            pairInteractions[pair].closeAndOrienteVDInSecs += activities[dt][subject.bid].vd;
                                        }

                                        Tuple<double, double> angles = withinOrientationData(activities[dt][subject.bid], activities[dt][partner.bid]);
                                        Boolean orientedCloseness = Math.Abs(angles.Item1) <= 45 && Math.Abs(angles.Item2) <= 45;
                                       
                                        if (orientedCloseness)
                                        {
                                            pairInteractions[pair].closeAndOrientedTimeInSecs += 0.05;
                                            if(activities[dt][subject.bid].cry>0 && activities[dt][partner.bid].cry>0)
                                            {
                                                pairInteractions[pair].closeAndOrientedCryInSecs += 0.05;
                                            }
                                            addPersonInfo(subject, ref pairInteractions[pair].subject,2);
                                            addPersonInfo(partner, ref pairInteractions[pair].partner,2);
                                            if (activities[dt][subject.bid].wasTalking)
                                            {
                                                pairInteractions[pair].closeAndOrientedTalkInSecs += 0.1;
                                                pairInteractions[pair].closeAndOrienteVDInSecs += activities[dt][subject.bid].vd;
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

    public void addPersonInfo(PersonInfo personInfo, ref PersonInfo target, double div)
        {
            target.vd += (personInfo.vd/div);
            target.vc += (personInfo.vc / div);
            target.tc += (personInfo.tc / div);
            target.bd += (personInfo.bd / div);
            target.no += (personInfo.no / div);
            target.oln += (personInfo.oln / div);
            target.ac += (personInfo.ac / div);
            target.cry += (personInfo.cry / div);
        }
    }
 }

