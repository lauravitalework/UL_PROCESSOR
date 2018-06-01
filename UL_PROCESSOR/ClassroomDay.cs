using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace UL_PROCESSOR
{
    class ClassroomDay
    {
        public Boolean startFromLena = true;//false;//true;
        private double minDistance = 1.5 * 1.5; //the squared value of g(r) cutoff in meters
        public Dictionary<String, double> pairClose = new Dictionary<string, double>();
        public Dictionary<String, double> pairCloseOrientation = new Dictionary<string, double>();
        public Dictionary<String, double> pairTime = new Dictionary<string, double>();
        public Dictionary<String, Tuple<double, double>> pairStatsSeparated = new Dictionary<string, Tuple<double, double>>();
        public Dictionary<String, Tuple<double, double>> pairStatsSeparatedTC = new Dictionary<string, Tuple<double, double>>();
        public Dictionary<String, Tuple<double, double>> pairStatsSeparatedVD = new Dictionary<string, Tuple<double, double>>();
        public Dictionary<String, PairInfo> pairStatsSep = new Dictionary<string, PairInfo>();
        public Dictionary<String, double> individualTime = new Dictionary<string, double>();
        public Dictionary<String, double> pairStats = new Dictionary<String, double>();
        //public Dictionary<String, Tuple<double, double, double, double, double>> personTotalCounts = new Dictionary<string, Tuple<double, double, double, double, double>>();
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
                    if (span.Minutes <= 10)
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
                    if ((raw[index - 1].dt.Hour == raw[index].dt.Hour) && (Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    //if ((Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    {
                        Tuple<double, double> point1 = new Tuple<double, double>(raw[index - 1].x, raw[index - 1].y);
                        Tuple<double, double> point2 = new Tuple<double, double>(raw[index].x, raw[index].y);
                        Tuple<double, double> targetpoint = linearInterpolate(target, raw[index - 1].dt, point1, raw[index].dt, point2);
                        double orientation1 = raw[index - 1].ori;
                        double orientation2 = raw[index].ori;
                        double targetorientation = linearInterpolate(target, raw[index - 1].dt, orientation1, raw[index].dt, orientation2);
                        PersonInfo pi2 = new PersonInfo();
                        pi2.x = targetpoint.Item1;
                        pi2.y = targetpoint.Item2;
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
                                        DateTime time = Convert.ToDateTime(line[cf.ubiFileDateCol]);
                                        if(ubiId=="T3B")
                                        {
                                            int stophere = 1;
                                        }
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
                                            if (mr.BID != "" && (!mr.isAbsent(day)))
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
        public Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>> readUbiTagFile()
        {
            Dictionary<String, List<PersonInfo>> rawInfo = new Dictionary<String, List<PersonInfo>>();
            Dictionary<String, List<PersonInfo>> rawInfoR = new Dictionary<String, List<PersonInfo>>();
            string[] folders = Directory.GetDirectories(cf.ubisenseFileDir);
            String tags = "";//test del
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
                                    String[] line = sr.ReadLine().Split(',');
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
        public Dictionary<String, List<PersonInfo>> readLenaFile()
        {

            Dictionary<String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();

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
            using (TextWriter sw = new StreamWriter(cf.root + cf.classroom + "/SYNC/" + "interaction_angles_xy_output" + (trunkDetailFile ? "_trunk" : "") + (cf.justFreePlay ? "_freeplay" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".csv"))
            {
                sw.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
                //foreach (DateTime dt in activities.Keys)
                //{
                foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> opi in activities.OrderBy(key => key.Key))
                {
                    DateTime dt = opi.Key;
                    if (dt.Hour > 9)
                    {
                        bool flagb = true;
                    }
                     
                    if (trunkDetailFile && dt.CompareTo(trunk) <= 0 && ((!cf.justFreePlay)||(isThisFreePlay(dt))))/////
                    {
                         
                        foreach (String person in activities[dt].Keys)
                        {
                            //delete
                            if(person=="T3B" && dt.Hour>9)
                            {
                                bool flagb = true;
                            }
                            if ((!startFromLena) || isWithLenaStart(dt, person))
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
                                        if((!cf.pairs.Contains(pair)) && cf.pairs.Contains(person + "-" + p))
                                            pair = person + "-" + p;

                                        if (!pairTime.ContainsKey(pair))
                                        {
                                            pairTime.Add(pair, 0);///////
                                            pairClose.Add(pair, 0);
                                            pairCloseOrientation.Add(pair, 0);
                                            pairStats.Add(pair, 0);
                                            pairStatsSep.Add(pair, new PairInfo());
                                            pairStatsSeparated.Add(pair, new Tuple<double, double>(0, 0));
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

                                if (wasTalking || vd > 0 || tc > 0 || a > 0 || n > 0 || vc > 0 || o > 0)
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
                                                            pairStatsSeparated.Add(pair, new Tuple<double, double>(0, 0));

                                                        }
                                                    }

                                                    //if (tc > 0)
                                                    {
                                                        List<PersonInfo> pi = lenaInfo[person];
                                                        foreach (PersonInfo i in pi)
                                                        {
                                                            DateTime dt2 = i.dt;
                                                            DateTime dt3 = i.dt.AddSeconds(i.bd);

                                                            if (dt >= dt2 && dt <= dt3)
                                                            {
                                                                tc = i.tc > 0 && i.bd > 0 ? (i.tc / i.bd) / 10 : 0;
                                                                a = i.ac > 0 && i.bd > 0 ? (i.ac / i.bd) / 10 : 0;
                                                                n = i.no > 0 && i.bd > 0 ? (i.no / i.bd) / 10 : 0;
                                                                vc = i.vc > 0 && i.bd > 0 ? (i.vc / i.bd) / 10 : 0;
                                                                o = i.oln > 0 && i.bd > 0 ? (i.oln / i.bd) / 10 : 0;
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
                                                                    //pairStatsSeparatedTC[pair] = new Tuple<double, double>(pairStatsSeparatedTC[pair].Item1 + tc, pairStatsSeparatedTC[pair].Item2);
                                                                    //pairStatsSeparatedVC[pair] = new Tuple<double, double>(pairStatsSeparatedVC[pair].Item1 + vc, pairStatsSeparatedVC[pair].Item2);
                                                                    //pairStatsSeparatedVD[pair] = new Tuple<double, double>(pairStatsSeparatedVD[pair].Item1 + vd2, pairStatsSeparatedVD[pair].Item2);
                                                                }
                                                                else //p is in the first part of the pair
                                                                {
                                                                    pairInfo.p2.vd += vd2;
                                                                    pairInfo.p2.vc += vc;
                                                                    pairInfo.p2.tc += tc;
                                                                    pairInfo.p2.ac += a;
                                                                    pairInfo.p2.no += n;
                                                                    pairInfo.p2.oln += o;
                                                                    //pairStatsSeparatedTC[pair] = new Tuple<double, double>(pairStatsSeparatedTC[pair].Item1, pairStatsSeparatedTC[pair].Item2 + tc);
                                                                    //pairStatsSeparatedVC[pair] = new Tuple<double, double>(pairStatsSeparatedVC[pair].Item1, pairStatsSeparatedVC[pair].Item2 + vc);
                                                                    //pairStatsSeparatedVD[pair] = new Tuple<double, double>(pairStatsSeparatedVD[pair].Item1, pairStatsSeparatedVD[pair].Item2 + vd2);
                                                                }

                                                            }
                                                        }



                                                    }
                                                    if (wasTalking)
                                                    {
                                                        pairStats[pair] += 0.1;
                                                        if (person.Equals(pair.Split('-')[0]))
                                                        {
                                                            pairStatsSeparated[pair] = new Tuple<double, double>(pairStatsSeparated[pair].Item1 + 0.1, pairStatsSeparated[pair].Item2);
                                                        }
                                                        else //p is in the first part of the pair
                                                        {
                                                            pairStatsSeparated[pair] = new Tuple<double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2 + 0.1);
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
                using (TextWriter sw = new StreamWriter("interaction_output" + (trunkDetailFile ? "_trunk" : "") + (cf.justFreePlay ? "_freeplay" : "") + ".csv"))
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
        public void write10SecTalkingCSV(String file_name)
        {
            using (TextWriter sw = new StreamWriter(file_name))
            {
                sw.WriteLine("BID, DateTime, X, Y, Orientation, Talking, Aid, S, Type");
                foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> pi in activities.OrderBy(key => key.Key))
                {
                    DateTime dt = pi.Key;
                    if ((!cf.justFreePlay) || (isThisFreePlay(dt)))
                    {
                        foreach (String s in activities[dt].Keys)
                        {
                            //tagInfo ti = tags[s];
                            MappingRow mr = cf.getMapping(s, day);
                            if ((!startFromLena) || isWithLenaStart(dt, s))
                            {
                                sw.WriteLine(s + "," + dt.ToString("hh:mm:ss.fff tt") + "," + activities[dt][s].x + "," + activities[dt][s].y + "," + activities[dt][s].ori + "," + activities[dt][s].wasTalking + "," + mr.aid + "," + mr.sex+","+mr.type);
                            }
                        }
                    }
                }
            }
        }

    }
}

