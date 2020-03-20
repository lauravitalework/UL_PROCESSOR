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
        public Dictionary<DateTime, Dictionary<String, PersonInfo>> activities = new Dictionary<DateTime, Dictionary<string, PersonInfo>>();
        public Dictionary<String, PairInfo> pairStatsSep = new Dictionary<string, PairInfo>();
        public Dictionary<String, double> pairStats = new Dictionary<String, double>();
        public Dictionary<String, PersonInfo> personTotalCounts = new Dictionary<string, PersonInfo>();
        public Dictionary<String, PersonInfo> personTotalCountsWUbi = new Dictionary<string, PersonInfo>();

        public DateTime startTime; //first recorded instant of that day
        public DateTime endTime; //last recorded instant of that day


        public Config cf;
        public DateTime day;
        public String szDay = "";
        public Dictionary<String, List<Tuple<DateTime, DateTime>>> personUbiTimes = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();

        public ClassroomDay(DateTime d, Config c)
        {
            cf = c;
            day = d;
            szDay = d.Month + "_" + day.Day + "_" + day.Year;
        }
        public List<DateTime> maxTimes = new List<DateTime>();
        public List<Tuple<DateTime, String>> maxPersonTimes = new List<Tuple<DateTime, String>>();
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
            /**** got ms totLA***/

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
            if (addTagTimes)
            {
                if (!cf.getMapping(person, this.day).isAbsent(day))
                {
                    maxPersonTimes.Add(new Tuple<DateTime, String>(raw.Last().dt, person));
                    maxTimes.Add(raw.Last().dt);
                }
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
                else
                {
                    Boolean stop100 = true;
                }

                if(index==228)
                {
                    Boolean stop228 = true;
                }

                if (index > 0)
                {
                    //LQ: why same hour??
                    //FIX
                    //if ((raw[index - 1].dt.Hour == raw[index].dt.Hour) && (Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    //if ((Math.Abs(raw[index - 1].dt.Minute - raw[index].dt.Minute) < 2))
                    TimeSpan difference = raw[index].dt.Subtract(raw[index - 1].dt); // could also write `now - otherTime`
                    if (difference.TotalSeconds < 60 || cf.settings.doAll10OfSecs)
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

        public void setUbiData(Dictionary<String, List<PersonInfo>> rawData, Dictionary<String, List<PersonInfo>> rawLena)
        {
            foreach (String person in rawData.Keys)
            {
                try
                {
                    List<Tuple<DateTime, PersonInfo>> cleanedData = clean(person, rawData[person], false, cf.settings.doUbiChildFiles);
                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedData)
                    {
                        DateTime cur = dataLine.Item1;
                        if ((cf.justUbi) || (rawLena.ContainsKey(person) &&
                                                cur >= rawLena[person][0].dt &&
                                                cur <= rawLena[person][rawLena[person].Count - 1].dt))
                        {
                            if (!activities.ContainsKey(cur))
                            {
                                activities.Add(cur, new Dictionary<string, PersonInfo>());
                            }
                            activities[cur].Add(person, dataLine.Item2);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION: " + e.Message);
                }
            }
        }
        public Dictionary<String, DateTime> startLenaTimes = new Dictionary<string, DateTime>();
        public DateTime maxLenaTimes = new DateTime(1900, 1, 1);
        public void setLenaData(Dictionary<String, List<PersonInfo>> lenadata)
        {
            if (cf.settings.getFromIts)
                setLenaItsData(lenadata);
            else
                setLenaADEXData(lenadata);
        }

        public void setLenaADEXData(Dictionary<String, List<PersonInfo>> lenadata)
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

            int c = 0; int c2 = 0;
            foreach (String person in rawData.Keys)
            {

                try
                {
                    List<Tuple<DateTime, PersonInfo>> cleanedData = new List<Tuple<DateTime, PersonInfo>>();
                    List<Tuple<DateTime, PersonInfo>> cleanedDataR = new List<Tuple<DateTime, PersonInfo>>();

                    try
                    {
                        cleanedData = clean(person, rawData[person], false, !cf.settings.doUbiChildFiles);
                    }
                    catch (Exception e)
                    {

                    }
                    try
                    {
                        cleanedDataR = clean(person, rawDataR[person], false, false);
                    }
                    catch (Exception e)
                    {

                    }

                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedData)
                    {
                        DateTime cur = dataLine.Item1;
                        if (!activities.ContainsKey(cur))
                        {
                            activities.Add(cur, new Dictionary<string, PersonInfo>());
                        }

                        if (!activities[cur].ContainsKey(person))
                        {
                            dataLine.Item2.lx = dataLine.Item2.x;
                            dataLine.Item2.ly = dataLine.Item2.y;

                            /*****T1 HACK right tag stopped working on 2/12/19 at 10:28:38.644 left 00:11:CE:00:00:00:02:CE right 00:11:CE:00:00:00:02:F2*****/
                            //Boolean hackT1 = false;// cf.getMapping(person, cur).leftTag.Trim()== "00:11:CE:00:00:00:02:CE" && cur >= new DateTime(2019, 02, 12, 10, 28, 38, 644) && cur <= new DateTime(2019, 06, 3);
                            Boolean hackThisT1 = cf.settings.hackT1 ? cf.getMapping(person, cur).leftTag.Trim() == "00:11:CE:00:00:00:02:CE" && cur >= new DateTime(2019, 02, 12, 10, 28, 38, 644) && cur <= new DateTime(2019, 06, 3) : false;
                            /*****T1 HACK right tag *****/

                            if (cf.makeRL || hackThisT1)
                            {
                                dataLine.Item2.rx = dataLine.Item2.lx + 0.15;
                                dataLine.Item2.ry = dataLine.Item2.ly;
                            }
                            dataLine.Item2.x = 0;
                            dataLine.Item2.y = 0;
                            activities[cur].Add(person, dataLine.Item2);
                        }
                        else//if (activities[cur].ContainsKey(person))
                        {
                            activities[cur][person].lx = dataLine.Item2.x;
                            activities[cur][person].ly = dataLine.Item2.y;
                        }
                        c++;
                        if (c == 139582)
                        {
                            c = c;
                        }
                    }

                    foreach (Tuple<DateTime, PersonInfo> dataLine in cleanedDataR)
                    {

                        DateTime cur = dataLine.Item1;

                        if (cur.Minute == 44)
                        {
                            c = c;
                        }
                        if (!activities.ContainsKey(cur))
                        {
                            activities.Add(cur, new Dictionary<string, PersonInfo>());
                        }
                        if (!activities[cur].ContainsKey(person))
                        {
                            dataLine.Item2.rx = dataLine.Item2.x;
                            dataLine.Item2.ry = dataLine.Item2.y;
                            dataLine.Item2.x = 0;
                            dataLine.Item2.y = 0;
                            if (cf.makeRL)
                            {
                                dataLine.Item2.lx = dataLine.Item2.rx + 0.15;
                                dataLine.Item2.ly = dataLine.Item2.ry;
                            }
                            activities[cur].Add(person, dataLine.Item2);
                        }
                        else// if (activities[cur].ContainsKey(person))
                        {
                            activities[cur][person].rx = dataLine.Item2.x;
                            activities[cur][person].ry = dataLine.Item2.y;
                            if (activities[cur][person].lx != 0 && activities[cur][person].ly != 0)
                            {
                                //getCenter
                                // activities[cur][person].y = getCenter(activities[cur][person].ly, activities[cur][person].ry);
                                // activities[cur][person].x = getCenter(activities[cur][person].lx, activities[cur][person].rx);
                            }



                            /* if (activities[cur][person].x == 0 && activities[cur][person].y == 0)
                             {

                                 double x2 = Math.Abs(activities[cur][person].rx - activities[cur][person].lx) / 2.00;
                                 x2 = activities[cur][person].rx < activities[cur][person].lx ? activities[cur][person].rx + x2 : activities[cur][person].lx + x2;

                                 double y2 = Math.Abs(activities[cur][person].ry - activities[cur][person].ly) / 2.00;
                                 y2 = activities[cur][person].ry < activities[cur][person].ly ? activities[cur][person].ry + y2 : activities[cur][person].ly + y2;

                                 activities[cur][person].x = x2;
                                 activities[cur][person].y = y2;
                             }*/
                        }

                        c2++;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION: " + e.Message);
                }
            }
        }

        public Dictionary<String, List<PersonInfo>> readUbiFile(List<String> subs)
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

                                            if ((subs.Count == 0 || subs.Contains(info.bid)) && (mr.BID != "" && (!mr.isAbsent(day)) && time.Day == day.Day))
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


        public Boolean isValidTagTime(String personId, ref Dictionary<String, DateTime> last, Dictionary<String, DateTime> lastPairTag, ref Dictionary<String, String> firstLines, ref Dictionary<String, String> firstLinesPair, DateTime lineTime, Boolean validTime, String line, ref TextWriter swc)
        {
            if (lastPairTag.ContainsKey(personId) &&
                (lineTime - lastPairTag[personId]).TotalSeconds <= 1)
            {
                validTime = true;
                swc.WriteLine(firstLinesPair[personId]);
            }
            else
            {
                if (last.ContainsKey(personId))
                {
                    last[personId] = lineTime;
                    firstLines[personId] = line;
                }
                else
                {
                    last.Add(personId, lineTime);
                    firstLines.Add(personId, line);
                }

            }
            return validTime;
        }
        public void writeGR(List<PersonInfo> rawData, Dictionary<String, List<PersonInfo>> rawLena)
        {
            DateTime trunk = getTrunkTime();/////
            Dictionary<String, DateTime> lastR = new Dictionary<string, DateTime>();
            Dictionary<String, DateTime> lastL = new Dictionary<string, DateTime>();
            Dictionary<String, String> firstLinesL = new Dictionary<string, string>();
            Dictionary<String, String> firstLinesR = new Dictionary<string, string>();
            TextWriter swc = new StreamWriter(cf.root + cf.classroom + "/SYNC/chaomingtagGR_" + cf.classroom + "_" + szDay + "_" + cf.settings.fileNameVersion + ".CSV");

            foreach (PersonInfo i in rawData)
            {
                Boolean validTime = false;
                String personId = i.bid;
                DateTime lineTime = i.dt;
                String szLine = i.szLineData;
                if (i.bid == "L2P")
                {
                    bool stop = true;
                }


                if ((cf.settings.subs.Count == 0 || cf.settings.subs.Contains(personId)) &&
                         personId.Trim() != "" &&
                         rawLena.ContainsKey(personId) &&
                         lineTime >= rawLena[personId][0].dt &&
                         lineTime <= rawLena[personId][rawLena[personId].Count - 1].dt &&
                         (!isThisInTimes(lineTime, cf.extractTimes)))
                {
                    if (i.bid == "L2P")
                    {
                        bool stop = true;
                    }
                    Boolean sAbsent = cf.getMapping(personId, day).isAbsent(day);//|| (!day.startLenaTimes.ContainsKey(subject));// false;
                    if ((!sAbsent) && lineTime.CompareTo(trunk) <= 0)
                    {
                        if (i.tagType == "L")
                            validTime = validTime || isValidTagTime(personId, ref lastL, lastR, ref firstLinesL, ref firstLinesR, lineTime, validTime, (szLine.Replace(i.tag, personId + i.tagType) + "," + i.tag), ref swc);
                        else
                            validTime = validTime || isValidTagTime(personId, ref lastR, lastL, ref firstLinesR, ref firstLinesL, lineTime, validTime, (szLine.Replace(i.tag, personId + i.tagType) + "," + i.tag), ref swc);

                        if (validTime)
                            swc.WriteLine(szLine.Replace(i.tag, personId + i.tagType) + "," + i.tag);
                    }
                }
            }
            swc.Close();
            swc.Dispose();
        }
        public Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>, List<PersonInfo>> readUbiTagFile(Dictionary<String, List<PersonInfo>> rawLena, Boolean writeChaomingFile)
        {
            Dictionary<String, List<PersonInfo>> rawInfo = new Dictionary<String, List<PersonInfo>>();
            Dictionary<String, List<PersonInfo>> rawInfoR = new Dictionary<String, List<PersonInfo>>();
            List<PersonInfo> rawInfoAll = new List<PersonInfo>();
            string[] folders = { cf.ubisenseFileDir };//

            if (cf.classSettings.mappingBy == "CLASS")
                folders = Directory.GetDirectories(cf.ubisenseFileDir);



            foreach (string folder in folders)
            {
                String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                DateTime folderDate;
                Boolean firstRow = true;

                if (cf.classSettings.mappingBy != "CLASS" ||
                    (DateTime.TryParse(folderName, out folderDate) && folderDate >= day && folderDate < day.AddDays(1)))
                {
                    string[] files = Directory.GetFiles(folder);
                    files = Directory.GetFiles(folder);
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
                                    String tagType = "";
                                    //rowCount++;

                                    if (line.Length > 5)
                                    {
                                        String tag = line[1].Trim();
                                        if (tag == "00:11:CE:00:00:00:02:66")
                                            tag = tag;
                                        String personId = "";
                                        DateTime lineTime = Convert.ToDateTime(line[2]);
                                        Double xPos = Convert.ToDouble(line[3]);
                                        Double yPos = Convert.ToDouble(line[4]);
                                        PersonInfo i = new PersonInfo();// lineTime, 0, 0, 0, 0);
                                        i.dt = lineTime;
                                        if (firstRow)
                                        {
                                            startTime = lineTime;
                                            firstRow = false;
                                        }
                                        endTime = lineTime;
                                        if (cf.mapRowsUbiL.ContainsKey(tag))
                                        {
                                            MappingRow mr = cf.getUbiMappingL(tag, lineTime);
                                            personId = mr.BID;// tagMappigLeft[tag].Trim();
                                            i.bid = personId;
                                            i.longBid = mr.longBID;
                                            i.gender = mr.sex;
                                            i.lang = mr.lang;
                                            i.diagnosis = mr.aid;
                                            tagType = "L";
                                            if (cf.settings.subs.Count == 0 || cf.settings.subs.Contains(personId))
                                            {
                                                i.lx = xPos;
                                                i.ly = yPos;
                                            }
                                        }
                                        else if (cf.mapRowsUbiR.ContainsKey(tag))
                                        {
                                            MappingRow mr = cf.getUbiMappingR(tag, lineTime);
                                            personId = mr.BID;// tagMappigLeft[tag].Trim();
                                            i.bid = personId;
                                            i.longBid = mr.longBID;
                                            i.gender = mr.sex;
                                            i.lang = mr.lang;
                                            i.diagnosis = mr.aid;

                                           
                                            tagType = "R";
                                            if (cf.settings.subs.Count == 0 || cf.settings.subs.Contains(personId))
                                            {
                                                i.rx = xPos;
                                                i.ry = yPos;
                                            }
                                        }
                                        i.x = xPos;
                                        i.y = yPos;
                                        if ((cf.settings.subs.Count == 0 || cf.settings.subs.Contains(personId)) && (personId != ""))
                                        {
                                            if (tagType == "L")
                                            {
                                                if (!rawInfo.ContainsKey(personId))
                                                {
                                                    rawInfo.Add(personId, new List<PersonInfo>());
                                                }
                                                rawInfo[personId].Add(i);
                                            }
                                            else if (tagType == "R")
                                            {
                                                if (!rawInfoR.ContainsKey(personId))
                                                {
                                                    rawInfoR.Add(personId, new List<PersonInfo>());
                                                }
                                                rawInfoR[personId].Add(i);
                                                //rawInfoAll
                                            }
                                            if (writeChaomingFile && tagType != "")
                                            {
                                                i.szLineData = szLine.Replace(tag, personId + tagType) + "," + tag;
                                                i.tagType = tagType;
                                                i.tag = tag;
                                                rawInfoAll.Add(i);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<String, List<PersonInfo>>, List<PersonInfo>> info = new Tuple<Dictionary<String, List<PersonInfo>>, Dictionary<string, List<PersonInfo>>, List<PersonInfo>>(rawInfo, rawInfoR, rawInfoAll);

            return info;
        }
        public double getAdjustedSecs(String lid)
        {
            //adjustedTimes
            double adjustedSecs = 0;
            String msg = "ADJUST TIME NOT FOUND FOR " + lid + " ( " + day.ToShortDateString() + " ) ";
            if (cf.adjustedTimes.ContainsKey(day))
            {
                if (cf.adjustedTimes[day].ContainsKey(lid))
                {
                    adjustedSecs = cf.adjustedTimes[day][lid];
                    msg = adjustedSecs + " Adjust secs " + lid + " ( " + day.ToShortDateString() + " ) ";
                }
            }
            /*if (adjustTimes.ContainsKey(day))
            {
                if (adjustTimes[day].ContainsKey(lid))
                {
                    adjustedSecs = adjustTimes[day][lid];
                }
            }*/
            Console.WriteLine(msg);
            return adjustedSecs;
        }
        public Dictionary<String, List<PersonInfo>> readLenaFile()
        {

            Dictionary<String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();
            if (File.Exists(cf.lenaFile))
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
                                if (cf.lenaVersion.ToUpper() == "SP")
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
        public Boolean isThisInTimes(DateTime date, Dictionary<String, List<String>> timeContainer)
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
            using (TextWriter sw = new StreamWriter(cf.syncFilePre + cf.settings.fileNameVersion + ".CSV"))
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
        public static double getAngle(PersonInfo a)
        {
            double r = 180;
            if (a.lx > 0 && a.ly > 0)
            {
                double a_center_x = getCenter(a.rx, a.lx);
                double a_center_y = getCenter(a.ry, a.ly);
                r = Math.Atan2(-a_center_x, a_center_y) * (180 / Math.PI);

            }
            return r;
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
        public void write10SecTalkingCSV()
        {
            String file_name = cf.root + cf.classroom + "/SYNC/" + (cf.justUbi ? "JUSTUBI" : "") + (cf.makeRL ? "MAKERL" : "") + "10THOFSECTALKING_" + szDay + "_" + cf.settings.fileNameVersion + ".CSV";

            Dictionary<String, PersonInfo> lastPersonInfo = new Dictionary<string, PersonInfo>();
            Dictionary<String, Tuple<double, double>> personVel = new Dictionary<string, Tuple<double, double>>();
            using (TextWriter sw = new StreamWriter(file_name))//"
            {
                using (TextWriter swvd = new StreamWriter(cf.root + cf.classroom + "/SYNC/DISTDETAILS_" + cf.settings.fileNameVersion + "_" + szDay + " .CSV"))//"
                {
                    sw.WriteLine("BID, DateTime, X, Y, Chaoming_Orientation, Talking, Aid, S, Type,rx,ry,lx,ly,Crying,Meters,Seconds,INFLOW,PART_FLOW_METERS,PART_FLOW_SECS,FLOW_METERS,FLOW_SECS,Velocity,Distance,Angle Velocity,Ubi_Orientation,UL_INFO");
                    foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> pi in activities.OrderBy(key => key.Key))
                    {
                        DateTime dt = pi.Key;

                        if ((!cf.justFreePlay) || (isThisFreePlay(dt)))
                        {

                            foreach (String s in activities[dt].Keys)
                            {
                                if (cf.settings.subs.Count == 0 || cf.settings.subs.Contains(s))
                                {
                                    MappingRow mr = cf.getMapping(s, day);
                                    if ((!double.IsNaN(activities[dt][s].x)) &&
                                                                    (!double.IsNaN(activities[dt][s].y)) &&
                                                                    (!double.IsNaN(activities[dt][s].lx)) &&
                                                                    (!double.IsNaN(activities[dt][s].ly)) &&
                                                                    (!double.IsNaN(activities[dt][s].rx)) &&
                                                                    (!double.IsNaN(activities[dt][s].ry)) &&
                                                                    ((cf.justUbi && activities[dt][s].lx != 0 &&
                                                                     activities[dt][s].ly != 0 && activities[dt][s].rx != 0 &&
                                                                     activities[dt][s].ry != 0) ||
                                                                    (((!cf.settings.startFromLena) || isWithLenaStart(dt, s))
                                                                     &&
                                                                     activities[dt][s].x != 0 &&
                                                                     activities[dt][s].y != 0)))
                                    {

                                        bool timeFlowFlag = false;
                                        double distInMeters = 0;
                                        double timeInSecs = 0;
                                        if (!lastPersonInfo.ContainsKey(s))
                                        {
                                            lastPersonInfo.Add(s, activities[dt][s]);
                                        }
                                        else if ((dt - lastPersonInfo[s].dt).TotalSeconds < 60)
                                        {
                                            timeFlowFlag = true;
                                            timeInSecs = (dt - lastPersonInfo[s].dt).TotalSeconds;
                                            if (double.IsNaN(Math.Sqrt(calcSquaredDist(lastPersonInfo[s], activities[dt][s]))))
                                            {
                                                distInMeters = distInMeters;
                                            }

                                            distInMeters += Math.Sqrt(calcSquaredDist(lastPersonInfo[s], activities[dt][s]));
                                            if (double.IsNaN(distInMeters))
                                            {
                                                distInMeters = distInMeters;
                                            }
                                            distInMeters += lastPersonInfo[s].meters;
                                            if (double.IsNaN(distInMeters))
                                            {
                                                distInMeters = distInMeters;
                                            }
                                            timeInSecs += lastPersonInfo[s].secs;
                                        }
                                        if (!timeFlowFlag)
                                        {
                                            if (!personVel.ContainsKey(s))
                                            {
                                                personVel.Add(s, new Tuple<double, double>(lastPersonInfo[s].meters, lastPersonInfo[s].secs));
                                            }
                                            else
                                            {
                                                personVel[s] = new Tuple<double, double>(personVel[s].Item1 + lastPersonInfo[s].meters, personVel[s].Item2 + lastPersonInfo[s].secs);
                                            }

                                        }


                                        sw.WriteLine(s + "," + dt.ToString("hh:mm:ss.fff tt") + "," +
                                            activities[dt][s].x + "," +
                                            activities[dt][s].y + "," +
                                            activities[dt][s].ori_chaoming + "," +
                                                activities[dt][s].wasTalking + "," +
                                                mr.aid + "," + mr.sex + "," + mr.type + "," +
                                                activities[dt][s].rx + "," +
                                                activities[dt][s].ry + "," +
                                                activities[dt][s].lx + "," +
                                                activities[dt][s].ly + "," +
                                                activities[dt][s].isCrying + "," +
                                                distInMeters + "," +
                                                timeInSecs + "," +
                                                timeFlowFlag + "," +
                                                lastPersonInfo[s].meters + "," +
                                                lastPersonInfo[s].secs + "," +
                                                (timeFlowFlag ? 0 : lastPersonInfo[s].meters) + "," +
                                                (timeFlowFlag ? 0 : lastPersonInfo[s].secs) + "," + activities[dt][s].ori+"," + (activities[dt][s].hasULData?1:0)
                                            /* + "," +
                                        pointToPointVel + "," +
                                        dist + "," +
                                        angleVel*/
                                            );


                                        lastPersonInfo[s] = activities[dt][s];
                                        lastPersonInfo[s].meters = distInMeters;
                                        lastPersonInfo[s].secs = timeInSecs;
                                        lastPersonInfo[s].dt = dt;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (cf.settings.doVel)
                using (TextWriter sw = new StreamWriter(cf.root + "//" + cf.classroom + "//SYNC//" + cf.classroom + "_VELOCITY_" + cf.settings.fileNameVersion + ".CSV", !first))
                {
                    if (first)
                    {
                        sw.WriteLine("DATE, BID, DIST, TIME, VEL");

                    }
                    foreach (String s in personVel.Keys)
                    {
                        double vel = personVel[s].Item1 != 0 && personVel[s].Item2 != 0 ? personVel[s].Item1 / personVel[s].Item2 : 0;
                        sw.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + personVel[s].Item1 + "," + personVel[s].Item2 + "," + vel);
                    }

                }
        }

        public void write10SecTalkingCSVOld()
        {
            //Dictionary<String, List<double>> vels = new Dictionary<string, List<double>>();
            // 
            //Dictionary<String, List<int>> velCounts = new Dictionary<string, List<int>>();
            double time = 0;
            DateTime dateTime = new DateTime();
            //Dictionary<String, List<double>> distances = new Dictionary<string, List<double>>();
            String file_name = cf.root + cf.classroom + "/SYNC/10THOFSECTALKING_" + szDay + "_" + cf.settings.fileNameVersion + ".CSV";

            Dictionary<String, DateTime> lastTimeStamp = new Dictionary<string, DateTime>();
            Dictionary<String, DateTime> firstTimeStamp = new Dictionary<string, DateTime>();
            Dictionary<String, List<double>> distInMeters = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> timeInSecs = new Dictionary<string, List<double>>();
            Dictionary<String, List<double>> vels = new Dictionary<string, List<double>>();

            using (TextWriter sw = new StreamWriter(file_name))//"
            {
                using (TextWriter swvd = new StreamWriter(cf.root + cf.classroom + "/SYNC/DISTDETAILS_" + szDay + "_" + cf.settings.fileNameVersion + ".CSV"))//"
                {
                    sw.WriteLine("BID, DateTime, X, Y, Orientation, Chaoming_Orientation,Talking, Aid, S, Type,rx,ry,lx,ly,Crying,Velocity,Distance,Angle Velocity");
                    foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> pi in activities.OrderBy(key => key.Key))
                    {
                        DateTime dt = pi.Key;
                        if ((!cf.justFreePlay) || (isThisFreePlay(dt)))
                        {
                            Boolean setDistance = false;
                            if (time == 0)
                            {
                                time = 1;
                                dateTime = dt;
                            }
                            if (((dt.Hour * 60) + dt.Minute) - ((dateTime.Hour * 60) + dateTime.Minute) >= 30)
                            {
                                time = (dt.Hour * 60) + dt.Minute;
                                dateTime = dt;
                                setDistance = true;
                            }
                            foreach (String s in activities[dt].Keys)
                            {
                                if (cf.settings.subs.Count == 0 || cf.settings.subs.Contains(s))
                                {
                                    //tagInfo ti = tags[s];
                                    MappingRow mr = cf.getMapping(s, day);
                                    if (((!cf.settings.startFromLena) || isWithLenaStart(dt, s))
                                                                    /*&&
                                                                    activities[dt][s].rx>0&&
                                                                        activities[dt][s].ry>0&&
                                                                        activities[dt][s].lx > 0 &&
                                                                        activities[dt][s].ly > 0*/)
                                    {

                                        double pointToPointVel = 0;
                                        double pointToPointSecs = 0;
                                        double angleVel = 0;
                                        double dist = 0;
                                        double chaoming_ori = 0;
                                        try
                                        {
                                            chaoming_ori = Math.Atan2(activities[dt][s].rx - activities[dt][s].lx, activities[dt][s].ry - activities[dt][s].ly) / (Math.PI * 180 + 90);
                                            chaoming_ori = chaoming_ori > 360 ? chaoming_ori - 360 : chaoming_ori;
                                            //activities[dt][s].ori = chaoming_ori;
                                        }
                                        catch (Exception e)
                                        {

                                        }


                                        // 
                                        /*assuming we have (XL, YL), (XR, YR).

double theta = atan2(XR-XL, YR-YL) /M_PI*180 + 90;
if (theta > 360) theta -= 360;*/
                                        if (cf.settings.doVel)
                                        {
                                            if (!lastTimeStamp.ContainsKey(s))
                                            {
                                                lastTimeStamp.Add(s, dt);
                                                firstTimeStamp.Add(s, dt);
                                                timeInSecs.Add(s, new List<double>());
                                                distInMeters.Add(s, new List<double>());
                                                vels.Add(s, new List<double>());
                                                timeInSecs[s].Add(0);
                                                distInMeters[s].Add(0);
                                                vels[s].Add(0);
                                            }
                                            else
                                            {
                                                if ((dt - lastTimeStamp[s]).TotalSeconds >= 60)
                                                {
                                                    swvd.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + firstTimeStamp[s] + "," + lastTimeStamp[s] + "," + distInMeters[s][distInMeters[s].Count - 1] + "," + (firstTimeStamp[s] - lastTimeStamp[s]).TotalSeconds);


                                                    timeInSecs[s].Add(0);
                                                    distInMeters[s].Add(0);
                                                    vels[s].Add(0);
                                                    firstTimeStamp[s] = dt;
                                                }
                                                else
                                                {
                                                    dist = Math.Sqrt(calcSquaredDist(activities[lastTimeStamp[s]][s], activities[dt][s]));// / ((dt - pos[s]).TotalSeconds);
                                                    pointToPointSecs = (dt - lastTimeStamp[s]).TotalSeconds;
                                                    pointToPointVel = dist != 0 && pointToPointSecs != 0 ? dist / pointToPointSecs : 0;
                                                    timeInSecs[s][timeInSecs[s].Count - 1] += pointToPointSecs;
                                                    distInMeters[s][distInMeters[s].Count - 1] += dist;
                                                    vels[s][vels[s].Count - 1] += pointToPointVel;
                                                    if (pointToPointVel > 2)
                                                        pointToPointVel = pointToPointVel;
                                                }
                                                lastTimeStamp[s] = dt;
                                            }
                                            /* if (!pos.ContainsKey(s))
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

                                                   dist = Math.Sqrt(calcSquaredDist(activities[pos[s]][s], activities[dt][s]));// / ((dt - pos[s]).TotalSeconds);
                                                   pointToPointVel = (dist / (dt - pos[s]).TotalSeconds);
                                                   angleVel = (Math.Abs(activities[pos[s]][s].ori - activities[dt][s].ori) / (dt - pos[s]).TotalSeconds);
                                                   vels[s][vels[s].Count - 1] += (dist/(dt-pos[s]).TotalSeconds);
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


                                               if (s == "8P" && dist > 2 && pointToPointVel > 25)
                                               {
                                                   dist = calcSquaredDist(activities[pos[s]][s], activities[dt][s]);
                                               }


                                               pos[s] = dt;
                                           }*/
                                        }



                                        //So the angle is arc tan(1.0) = 45°. 
                                        sw.WriteLine(s + "," + dt.ToString("hh:mm:ss.fff tt") + "," + activities[dt][s].x + "," + activities[dt][s].y + "," + activities[dt][s].ori + "," + chaoming_ori + "," +
                                                activities[dt][s].wasTalking + "," +
                                                mr.aid + "," + mr.sex + "," + mr.type + "," +
                                                activities[dt][s].rx + "," +
                                                activities[dt][s].ry + "," +
                                                activities[dt][s].lx + "," +
                                                activities[dt][s].ly + "," +
                                                activities[dt][s].isCrying + "," +
                                                pointToPointVel + "," +
                                                dist + "," +
                                                angleVel
                                                );

                                    }
                                }
                                setDistance = false;
                            }
                        }
                    }
                }
            }

            if (cf.settings.doVel)
                using (TextWriter sw = new StreamWriter(cf.root + "//" + cf.classroom + "//SYNC//" + cf.classroom + "_VELOCITY_" + cf.settings.fileNameVersion + ".CSV", !first))
                {
                    using (TextWriter sw2 = new StreamWriter(cf.root + "//" + cf.classroom + "//SYNC//" + cf.classroom + "_DISTANCES" + cf.settings.fileNameVersion + ".CSV", !first))
                    {
                        if (first)
                        {
                            sw2.Write("DATE, BID, DIST, TIME, VEL");
                            sw.WriteLine("DATE, BID, VEL");

                        }
                        foreach (String s in distInMeters.Keys)
                        {
                            double personVel = 0;
                            bool set = false;
                            for (int pos = 0; pos < distInMeters[s].Count; pos++)
                            {
                                double vel = distInMeters[s][pos] != 0 && timeInSecs[s][pos] != 0 ? distInMeters[s][pos] / timeInSecs[s][pos] : 0;
                                personVel += (vel / (distInMeters[s][pos] != 0 && set ? 2 : 1));
                                sw2.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + distInMeters[s][pos] + "," + timeInSecs[s][pos] + "," + vel + "," + personVel);
                                set = true;
                            }
                            sw.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + personVel);
                        }
                        /* foreach (String s in distances.Keys)
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


                     }*/
                    }

                    /*foreach (String s in vels.Keys)
                    {
                        int c = 0;
                        double velo = 0;
                        foreach (double v in vels[s])
                        {
                            velo+= v / velCounts[s][c];
                            c++;
                        }
                        sw.WriteLine(day.Month + "/" + day.Day + "/" + day.Year + "," + s + "," + velo / c);

                    }*/
                }
        }
        public static Boolean first = true;
        public Dictionary<DateTime, Dictionary<String, double>> adjustTimes = new Dictionary<DateTime, Dictionary<String, double>>();
        public void setMinData(ref Dictionary<DateTime, Tuple<double, double, double, double>> minDate, DateTime start, DateTime end, String type, double count)
        {

            if (start.Minute == 14)
            {
                Boolean stp = true;
            }
            double bdSecs = (end - start).Seconds + ((end - start).Milliseconds > 0 ? ((end - start).Milliseconds / 1000.00) : 0.00); //endSecs - startSecs;
            if (count > 0 && bdSecs > 0)
            {
                double minCount = count;
                DateTime minStartTime = start;// new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0);

                double minBdSecs = bdSecs;

                while (minStartTime <= end)
                {
                    DateTime minEndTime = end;
                    DateTime minTime = new DateTime(minStartTime.Year, minStartTime.Month, minStartTime.Day, minStartTime.Hour, minStartTime.Minute, 0);
                    if (minStartTime.Hour != end.Hour || minStartTime.Minute != end.Minute)
                    {
                        minEndTime = minTime.AddMinutes(1);// new DateTime(minStartTime.Year, minStartTime.Month, minStartTime.Day, minStartTime.Hour, minStartTime.Minute, 59);
                                                           // minEndTime=minEndTime.AddMilliseconds(900);

                    }
                    minBdSecs = (minEndTime - minStartTime).Seconds + ((minEndTime - minStartTime).Milliseconds > 0 ? ((minEndTime - minStartTime).Milliseconds / 1000.00) : 0.00); //endSecs - startSecs;
                    minCount = (minBdSecs * count) / bdSecs;
                    Tuple<double, double, double, double> thisMinVals = new Tuple<double, double, double, double>(0, 0, 0, 0);
                    double childCount = 0;
                    double childVocDur = 0;
                    double otherChildVocDur = 0;
                    double adultCount = 0;

                    switch (type)
                    {
                        case "CHD":
                            childVocDur = minCount;
                            break;
                        case "CHC":
                            childCount = minCount;
                            break;
                        case "CX":
                            otherChildVocDur = minCount;
                            break;
                        case "MAN":
                        case "FAN":
                            adultCount = minCount;
                            break;
                    }
                    thisMinVals = new Tuple<double, double, double, double>(childVocDur, childCount, otherChildVocDur, adultCount);

                    if (!minDate.ContainsKey(minTime))
                    {
                        minDate.Add(minTime, thisMinVals);
                    }
                    else
                    {
                        Tuple<double, double, double, double> exMinDate = minDate[minTime];
                        minDate[minTime] = new Tuple<double, double, double, double>(exMinDate.Item1 + childVocDur, exMinDate.Item2 + childCount, exMinDate.Item3 + otherChildVocDur, exMinDate.Item4 + adultCount);

                    }
                    minStartTime = minTime.AddMinutes(1);
                }



            }


        }

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
            times["14864"] = 19 - 1;
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

        public Dictionary<String, List<Onset>> onsets = new Dictionary<string, List<Onset>>();
        public void addToPersonInfoSum(ref Dictionary<String, PersonInfo> sum, String sumKey, double vc, double vd, double tc, double ac, double no, double oln)
        {
            addToPersonInfoSum(ref sum, sumKey, vc, vd, tc, ac, no, oln, 0);
        }
        public void addToPersonInfoSum(ref Dictionary<String, PersonInfo> sum, String sumKey, double vc, double vd, double tc, double ac, double no, double oln, double otherChild)
        {

            /*****SUM******/
            if (!sum.ContainsKey(sumKey))
                sum.Add(sumKey, new PersonInfo());
            sum[sumKey].vc = sum[sumKey].vc + vc;
            sum[sumKey].vd = sum[sumKey].vd + vd;
            sum[sumKey].tc = sum[sumKey].tc + tc;
            sum[sumKey].ac = sum[sumKey].ac + ac;
            sum[sumKey].no = sum[sumKey].no + no;
            sum[sumKey].oln = sum[sumKey].oln + oln;
            sum[sumKey].otherChild = sum[sumKey].otherChild + otherChild;
            /*****SUM******/
        }
        public static Dictionary<String, PersonInfo> sum = new Dictionary<string, PersonInfo>();
        public static Dictionary<DateTime, bool> daysDone = new Dictionary<DateTime, bool>();
        public Dictionary<String, List<PersonInfo>> readLenaItsFiles(int countDays)
        {
            Boolean dayDone = daysDone.ContainsKey(this.day);
            onsets = new Dictionary<string, List<Onset>>();
            Boolean doSmartNames = false;
            TextWriter swsn = null;
            if (doSmartNames)
                swsn = new StreamWriter(cf.syncFilePre + "_SMARTNAMES" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);

            Dictionary<String, List<PersonInfo>> rawLenaInfo = new Dictionary<String, List<PersonInfo>>();



            //Date	Subject	SubjectType	segmentid	voctype	recstart	startsec	endsec	starttime	endtime	duration	uttlen
            string[] folders = { cf.lenaFileDir + "/ITS/" };// Directory.GetDirectories(cf.lenaFileDir + "/ITS/");

            if (cf.classSettings.mappingBy == "CLASS")
                folders = Directory.GetDirectories(cf.lenaFileDir + "/ITS/");

            TextWriter sw = null;
            TextWriter swoffsets = null;
            TextWriter swm = null;
            TextWriter swlt = null;
            TextWriter swava = null;
            if ((cf.settings.doOnsets || cf.settings.doSocialOnsets))
            {
                swava = new StreamWriter(cf.syncFilePre + "_AVA" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);
                sw = new StreamWriter(cf.syncFilePre + "_ONSETS" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);
                swoffsets = new StreamWriter(cf.syncFilePre + "_OFFSETTIMES" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);
            }
            if (cf.settings.doMinVocs)
            {
                swm = new StreamWriter(cf.syncFilePre + "_MIN_VOCS" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);
            }
            if (cf.settings.lenaTimes)
            {
                swlt = new StreamWriter(cf.syncFilePre + "_LENA_TIMES" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);
            }

            //using (sw = new StreamWriter(cf.syncFilePre + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_" + cf.version + "_ONSETS.CSV", countDays > 0))
            {
                TextWriter swd = null;
                TextWriter swqa = new StreamWriter(cf.syncFilePre + "_qas.CSV", true);
                if (cf.settings.doDbs)
                    swd = new StreamWriter(cf.syncFilePre + "_DBS" + cf.settings.fileNameVersion + ".CSV", countDays > 0);
                if (countDays == 0)
                {
                    if (cf.settings.doDbs)
                        swd.WriteLine("Date,Subject,SubjectType,Conv_avg_db,Conv_avg_peak,Child_avg_db,Child_avg_peak");
                    if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                    {
                        sw.WriteLine("File,Date,Subject,LenaID,SubjectType,segmentid,voctype,recstart,startsec,endsec,starttime,endtime,duration,seg_duration,wordcount,avg_db,avg_peak,turn_taking ");
                        //<AVA rawScore="-0.432" standardScore="ICD" estimatedMLU="ICD" estimatedDevAge="ICD" vocalizationCnt="ORH" vocalizationLen="ORH" MLV="ORH" />
                        swava.WriteLine("Date,Subject,LenaID,SubjectType,rawScore,standardScore,estimatedMLU,estimatedDevAge,vocalizationCnt,vocalizationLen,MLV  ");
                        swoffsets.WriteLine("File,Date,Subject,LenaID,SubjectType,OriginalStart,OriginalStartTime,Start,StartTime");
                    }
                    if (cf.settings.doMinVocs)
                    {
                        swm.WriteLine("File,Date,Subject,LenaID,SubjectType,Time,Key Child Utt Duration, Key Child Utt Count, Other Child Utt Duration, Adult Word Count");
                    }
                    if (cf.settings.lenaTimes)
                    {
                        swlt.WriteLine("Date,ITS File,AudioFile,AlignedAudioFile,DLP,Subject,LENA_START");
                        //e20170306_105341_014866_aligned.wav
                    }
                }
                // TextWriter ssw = new StreamWriter(cf.syncFilePre + "_SUMMARY" + cf.settings.fileNameVersion + ".CSV", true);// countDays > 0);

                foreach (string folder in folders)
                {
                    String folderName = folder.Substring(folder.LastIndexOf("/") + 1);
                    DateTime folderDate;
                    if (cf.classSettings.mappingBy != "CLASS" ||
                        (DateTime.TryParse(folderName, out folderDate) && folderDate >= day && folderDate < day.AddDays(1)))
                    {
                        string[] files = Directory.GetFiles(folder);
                        files = Directory.GetFiles(folder);
                        foreach (string file in files)
                        {
                            String fileName = Path.GetFileName(file);
                            {

                                Console.WriteLine(file);
                                String lenaId = fileName;// file.Substring(file.IndexOf("\\") + 1);
                                lenaId = lenaId.Substring(cf.lenaVersion == "SP" ? 16 : 17, 6);
                                if (lenaId.Substring(0, 2) == "00")
                                    lenaId = lenaId.Substring(2);
                                else if (lenaId.Substring(0, 1) == "0")
                                    lenaId = lenaId.Substring(1);

                                if (lenaId == "26860")
                                    lenaId = lenaId;
                                //min child voc dur, child 
                                double at = getAdjustedSecs(lenaId);
                                XmlDocument doc = new XmlDocument();
                                doc.Load(file);

                                MappingRow mr = cf.getLenaMapping(lenaId, day);
                                String pibid = mr.BID;
                                String pitype = mr.type;

                                XmlNodeList ava = cf.lenaVersion == "SP" ? doc.ChildNodes[0].SelectNodes("ProcessingUnit/AVA") : doc.ChildNodes[2].SelectNodes("ProcessingUnit/AVA");
                                //MLV  ava[0].Attributes["MLV"].Value
                                if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets) && pibid != "")
                                {

                                    swava.WriteLine(this.day + "," +
                                        pibid + "," +
                                        lenaId + "," +
                                        pitype + "," +
                                        ava[0].Attributes["rawScore"].Value + "," +
                                        ava[0].Attributes["standardScore"].Value + "," +
                                        ava[0].Attributes["estimatedMLU"].Value + "," +
                                        ava[0].Attributes["estimatedDevAge"].Value + "," +
                                        ava[0].Attributes["vocalizationCnt"].Value + "," +
                                        ava[0].Attributes["vocalizationLen"].Value + "," +
                                        ava[0].Attributes["MLV"].Value);
                                    //<AVA rawScore="-0.432" standardScore="ICD" estimatedMLU="ICD" estimatedDevAge="ICD" vocalizationCnt="ORH" vocalizationLen="ORH" MLV="ORH" />
                                    //swava.WriteLine("Date,Subject,LenaID,SubjectType,rawScore,standardScore,estimatedMLU,estimatedDevAge,vocalizationCnt,
                                    //vocalizationLen,MLV  ");
                                }



                                XmlNodeList rec = cf.lenaVersion == "SP" ? doc.ChildNodes[0].SelectNodes("ProcessingUnit/Recording") : doc.ChildNodes[2].SelectNodes("ProcessingUnit/Recording");


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
                                int recnum = 0;
                                Boolean startedAnotherDay = false;
                                foreach (XmlNode recording in rec)
                                {
                                    recnum++;
                                    double recStartSecs = Convert.ToDouble(recording.Attributes["startTime"].Value.Substring(2, recording.Attributes["startTime"].Value.Length - 3));

                                    Dictionary<DateTime, Tuple<double, double, double, double>> minDate = new Dictionary<DateTime, Tuple<double, double, double, double>>();
                                    DateTime recStartTimeOriginal = DateTime.Parse(recording.Attributes["startClockTime"].Value);
                                    XmlNodeList nodes = recording.SelectNodes("Conversation|Pause");
                                    XmlNodeList nodesP = recording.SelectNodes("Conversation");
                                    double adjustedSecs = getAdjustedSecs(lenaId);
                                    DateTime recStartTime = Config.geFullTime(recStartTimeOriginal.AddSeconds(adjustedSecs));


                                    mr = cf.getLenaMapping(lenaId, recStartTime);
                                    pibid = mr.BID;
                                    pitype = mr.type;
                                    double subjectAvgDb = 1;
                                    double subjectMaxDb = 1;
                                    double subjectConvAvgDb = 1;
                                    double subjectConvMaxDb = 1;

                                    if (doSmartNames)
                                    {
                                        String dir = "PRIDE/PRIDE_LEAP/PRIDE_LEAP_AM/";
                                        String superDir = "";
                                        String superCopyDir = "";
                                        String fileType = "ITS";
                                        String renameDir = "mkdir -p /projects2/cg/dmlab/IBSS/PRIDE/PRIDE_LEAP/PRIDE_LEAP_AM/01-23-2019/LENA_Data/ITS_RENAMED";
                                        switch (cf.classroom)
                                        {
                                            case "PRIDE_LEAP_AM":

                                                superDir = "/projects2/cg/dmlab/IBSS/" + dir + Config.getDayDashStr(this.day) + "/LENA_Data/" + fileType + "/" + fileName;
                                                //superCopyDir = "/projects2/cg/dmlab/IBSS/" + dir + Config.getDayDashStr(this.day) + "/LENA_Data/" + fileType + "_RE/" + fileName;
                                                renameDir = "/projects2/cg/dmlab/IBSS/" + dir + Config.getDayDashStr(this.day) + "/LENA_Data/" + fileType + "_RENAMED";
                                                break;

                                        }
                                        String newSmartName = Config.getDayStr(this.day) + pibid.Replace("Lab", "L") + "_" + fileName.Substring(2);
                                        swsn.WriteLine(fileName + "," + newSmartName + "," +
                                            superDir + "," +
                                            "mkdir -p " + renameDir + ", " +
                                            "mkdir -p " + renameDir.Replace("PRIDE_LEAP_AM/", "").Replace("PRIDE_LEAP_PM/", "") + ", " +
                                            "cp " + superDir + " " + renameDir + "/" + newSmartName + ", " +
                                            "cp " + superDir + " " + renameDir.Replace("PRIDE_LEAP_AM/", "").Replace("PRIDE_LEAP_PM/", "") + "/" + newSmartName);
                                        //swsn.WriteLine(fileName.Replace(".its",".wav") + "," + Config.getDayStr(this.day) + "_" + pibid + "_" + fileName.Replace(".its", ".wav"));
                                    }

                                    Boolean isAm = cf.classroom.IndexOf("_AM") > 0;
                                    Boolean isPm = cf.classroom.IndexOf("_PM") > 0;
                                    if (recStartTime.Day != day.Day)
                                    {
                                        swqa.WriteLine("ANOTHER DAY ," + recStartTime.ToShortDateString() + "," + day.ToShortDateString() + ", LENA," +
                                            lenaId + " ,BID:, " + pibid + " ,REC: ," + recnum + ", START: , " + recStartSecs);
                                    }
                                    else
                                    {
                                        swqa.WriteLine("SAME DAY, " + recStartTime.ToShortDateString() + " , " + day.ToShortDateString() + " ,LENA," +
                                                lenaId + ", BID:, " + pibid + " ,REC: ," + recnum + ", START: , " + recStartSecs);

                                    }



                                    if (recStartTime.Day == day.Day)
                                    {
                                        if (cf.settings.lenaTimes)
                                        {
                                            //swm.WriteLine("Date,File,AudioFile,AlignedAudioFile,DLP,Subject,LENA_START");
                                            //e20170306_105341_014866_aligned.wav
                                            String audioFileName = fileName.Substring(0, fileName.IndexOf("."));
                                            String audioFileName_aligned = audioFileName + "_aligned.wav";
                                            audioFileName = audioFileName + ".wav";
                                            swlt.WriteLine(this.day + "," + fileName + "," + audioFileName + "," + audioFileName_aligned + "," + lenaId + "," + pibid + "," + recStartTime);


                                            /* sw.WriteLine(file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," +
                                                                        segmentNumber + ",CHN_CHF SegmentUtt," +
                                                                        Config.getTimeStr(recStartTime) + "," + startSecs + "," + endSecs + "," +
                                                                        Config.getTimeStr(start) + "," +
                                                                        Config.getTimeStr(end )+ "," +
                                                                        String.Format("{0:0.00}", pi.vd) + "," +
                                                                        String.Format("{0:0.00}", pi.bd) + ",," +
                                                                        String.Format("{0:0.00}", pi.avDb) + "," +
                                                                        String.Format("{0:0.00}", pi.maxDb) +
                                                                        "," + start.Hour + "," + start.Minute + "," + start.Second +
                                                                        "," + end.Hour + "," + end.Minute + "," + end.Second);*/
                                        }

                                        //  swoffsets.WriteLine(file + "," + this.day + "," + pibid + "," + lenaId + "," + mr.type + "," + recStartTimeOriginal + "," + Config.getTimeStr(recStartTimeOriginal) + ","+recStartTime+ ","+ Config.getTimeStr(recStartTime));//  

                                        foreach (XmlNode conv in nodes)
                                        {
                                            String sumKey = "";
                                            String num = conv.Attributes["num"].Value;
                                            XmlNodeList segments = conv.SelectNodes("Segment");
                                            double startSecs = Convert.ToDouble(conv.Attributes["startTime"].Value.Substring(2, conv.Attributes["startTime"].Value.Length - 3)) - recStartSecs;
                                            double endSecs = Convert.ToDouble(conv.Attributes["endTime"].Value.Substring(2, conv.Attributes["endTime"].Value.Length - 3)) - recStartSecs;
                                            DateTime start = Config.geFullTime(recStartTime.AddSeconds(startSecs));
                                            DateTime end = Config.geFullTime(recStartTime.AddSeconds(endSecs));
                                            PersonInfo pi = new PersonInfo();
                                            if ((((!isAm) && (!isPm)) ||
                                                 (isAm && start.Hour < 11) ||
                                                 (isPm && start.Hour >= 11)))

                                            {
                                                mr = cf.getLenaMapping(lenaId, start);
                                            if (conv.Name == "Conversation")
                                            {
                                                double tc = Convert.ToDouble(conv.Attributes["turnTaking"].Value);
                                                double db = Convert.ToDouble(conv.Attributes["average_dB"].Value);
                                                double mdb = Convert.ToDouble(conv.Attributes["peak_dB"].Value);
                                                subjectConvAvgDb = subjectConvAvgDb == 1 ? db : (subjectConvAvgDb + db) != 0 ? (subjectConvAvgDb + db) / 2 : 0;
                                                subjectConvMaxDb = subjectConvMaxDb == 1 ? mdb : (subjectConvMaxDb + mdb) != 0 ? (subjectConvMaxDb + mdb) / 2 : 0;

                                                if (tc > 0)
                                                {
                                                    /*****SUM******/
                                                    sumKey = start.ToShortDateString() + "," + (mr.BID.Trim() != "" ? mr.BID : lenaId) + "," + lenaId;
                                                    addToPersonInfoSum(ref sum, sumKey, 0, 0, tc, 0, 0, 0);
                                                    /*****SUM******/

                                                    pi.dt = start;
                                                    pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                    pi.bid = mr.BID;// ubiAndBId[1];
                                                    pi.lenaId = lenaId;
                                                    pi.bd = (end - start).Seconds + ((end - start).Milliseconds > 0 ? ((end - start).Milliseconds / 1000.00) : 0.00); //endSecs - startSecs;
                                                    pi.tc = tc;
                                                    pi.pType = mr.type;
                                                        pi.longBid = mr.longBID;
                                                        pi.gender = mr.sex;
                                                        pi.lang = mr.lang;
                                                        pi.diagnosis = mr.aid;
                                                    addMsToRawLena(ref rawLenaInfo, pi);

                                                    if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                    {
                                                        //sw.WriteLine("File,Date,Subject,LenaID,SubjectType,segmentid,voctype,
                                                        //recstart,startsec,endsec,starttime,endtime,
                                                        //duration,seg_duration,wordcount,avg_db,avg_peak,turn_taking ");
                                                        sw.WriteLine(file + "," +
                                                            this.day + "," +
                                                            pi.bid + "," +
                                                            pi.lenaId + "," +
                                                            mr.type + "," +
                                                            segmentNumber +
                                                            ",Conversation_turnTaking," +
                                                            Config.getTimeStr(recStartTime) + "," +
                                                            startSecs + "," +
                                                            endSecs + "," +
                                                            Config.getTimeStr(start) + "," +
                                                            Config.getTimeStr(end) + "," +
                                                            String.Format("{0:0.00}", pi.vd) + "," +
                                                            String.Format("{0:0.00}", pi.bd) + "," +
                                                            "," +
                                                            String.Format("{0:0.00}", pi.avDb) + "," +
                                                            String.Format("{0:0.00}", pi.maxDb) + "," +
                                                            tc + "," +
                                                            start.Hour + "," +
                                                            start.Minute + "," +
                                                            start.Second + "," +
                                                            end.Hour + "," +
                                                            end.Minute + "," +
                                                            end.Second);

                                                        if (!onsets.ContainsKey(pi.bid))
                                                            onsets.Add(pi.bid, new List<Onset>());
                                                        onsets[pi.bid].Add(new Onset(file,
                                                            this.day,
                                                            pi.bid,
                                                            pi.lenaId,
                                                            mr.type,
                                                            segmentNumber,
                                                           "Conversation_turnTaking",
                                                            recStartTime,
                                                            startSecs,
                                                            endSecs,
                                                            start,
                                                            end,
                                                            pi.vd,
                                                            pi.bd,
                                                            0,
                                                            pi.avDb,
                                                            pi.maxDb,
                                                            tc, mr.aid));
                                                    }
                                                }
                                            }

                                            foreach (XmlNode seg in segments)
                                            {
                                                //startClockTime
                                                segmentNumber++;
                                                startSecs = Convert.ToDouble(seg.Attributes["startTime"].Value.Substring(2, seg.Attributes["startTime"].Value.Length - 3)) - recStartSecs;
                                                endSecs = Convert.ToDouble(seg.Attributes["endTime"].Value.Substring(2, seg.Attributes["endTime"].Value.Length - 3)) - recStartSecs;
                                                start = Config.geFullTime(recStartTime.AddMilliseconds(startSecs * 1000));
                                                end = Config.geFullTime(recStartTime.AddMilliseconds(endSecs * 1000));
                                                pi = new PersonInfo();
                                                pi.dt = start;

                                                mr = cf.getLenaMapping(lenaId, start);
                                                pi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                pi.bid = mr.BID;// ubiAndBId[1];
                                                pi.lenaId = lenaId;
                                                    pi.longBid = mr.longBID;
                                                    pi.gender = mr.sex;
                                                    pi.lang = mr.lang;
                                                    pi.diagnosis = mr.aid;
                                                    String speaker = seg.Attributes["spkr"].Value;
                                                pi.bd = (end - start).Seconds + ((end - start).Milliseconds > 0 ? ((end - start).Milliseconds / 1000.00) : 0); //endSecs - startSecs;
                                                Boolean add = false;
                                                pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                switch (speaker)
                                                {
                                                    case "CHN":
                                                    case "CHF":

                                                        ///
                                                        // if (test == 0)
                                                        //   break;
                                                        childSegmentNumber++;
                                                        //if (mr.type == "Child")
                                                        {
                                                            pi.vd = Convert.ToDouble(seg.Attributes["childUttLen"].Value.Substring(1, seg.Attributes["childUttLen"].Value.Length - 2));
                                                            pi.vc = Convert.ToDouble(seg.Attributes["childUttCnt"].Value);
                                                          
                                                            pi.childSegments = childSegmentNumber;


                                                            /*****SUM******/
                                                            sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                            addToPersonInfoSum(ref sum, sumKey, pi.vc, pi.vd, 0, 0, 0, 0);
                                                            /*****SUM******/

                                                            if (pi.vc < 0 || pi.bd < 0)
                                                            {
                                                                bool stop = true;
                                                                start = Config.geFullTime(recStartTime.AddMilliseconds(startSecs * 1000));
                                                                end = Config.geFullTime(recStartTime.AddMilliseconds(endSecs * 1000));
                                                            }
                                                            if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                            {
                                                                sw.WriteLine(file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," +
                                                                    segmentNumber + ",CHN_CHF SegmentUtt," +
                                                                    Config.getTimeStr(recStartTime) + "," + startSecs + "," + endSecs + "," +
                                                                    Config.getTimeStr(start) + "," +
                                                                    Config.getTimeStr(end) + "," +
                                                                    String.Format("{0:0.00}", pi.vd) + "," +
                                                                    String.Format("{0:0.00}", pi.bd) + ",," +
                                                                    String.Format("{0:0.00}", pi.avDb) + "," +
                                                                    String.Format("{0:0.00}", pi.maxDb) + ",," +
                                                                    start.Hour + "," + start.Minute + "," + start.Second +
                                                                    "," + end.Hour + "," + end.Minute + "," + end.Second);

                                                                if (!onsets.ContainsKey(pi.bid))
                                                                    onsets.Add(pi.bid, new List<Onset>());
                                                                onsets[pi.bid].Add(new Onset(file,
                                                            this.day,
                                                            pi.bid,
                                                            pi.lenaId,
                                                            mr.type,
                                                            segmentNumber,
                                                           "CHN_CHF SegmentUtt",
                                                            recStartTime,
                                                            startSecs,
                                                            endSecs,
                                                            start,
                                                            end,
                                                            pi.vd,
                                                            pi.bd,
                                                            pi.vc,//changed for lynns defemse reviewer 11/22/19
                                                            pi.avDb,
                                                            pi.maxDb,
                                                            0, mr.aid));
                                                            }
                                                            if (cf.settings.doMinVocs)
                                                            {
                                                                setMinData(ref minDate, start, end, "CHC", pi.vc);
                                                            }
                                                            pi.vd = 0;

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
                                                                    double cstartSecs = Convert.ToDouble(seg.Attributes[att].Value.Substring(2, seg.Attributes[att].Value.Length - 3)) - recStartSecs;
                                                                    double cendSecs = Convert.ToDouble(seg.Attributes["endCry" + cryStep].Value.Substring(2, seg.Attributes["endCry" + cryStep].Value.Length - 3)) - recStartSecs;
                                                                    DateTime cstart = Config.geFullTime(recStartTime.AddMilliseconds(cstartSecs * 1000));
                                                                    DateTime cend = Config.geFullTime(recStartTime.AddMilliseconds(cendSecs * 1000));
                                                                    PersonInfo cpi = new PersonInfo();
                                                                    cpi.dt = cstart;
                                                                    mr = cf.getLenaMapping(lenaId, start);
                                                                        cpi.longBid = mr.longBID;
                                                                        cpi.gender = mr.sex;
                                                                        cpi.lang = mr.lang;
                                                                        cpi.diagnosis = mr.aid;
                                                                    cpi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                                    cpi.bid = mr.BID;// ubiAndBId[1];
                                                                    cpi.lenaId = lenaId;
                                                                    cpi.bd = (cend - cstart).Seconds + ((cend - cstart).Milliseconds > 0 ? (cend - cstart).Milliseconds / 1000.00 : 0); //cendSecs - cstartSecs;
                                                                    cpi.cry = cpi.bd;
                                                                    cpi.pType = mr.type;
                                                                    //Config.getTimeStr(end) + "," +
                                                                    //Math.Round(pi.vd, 2) + "," +
                                                                    newSwLine = (file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," + segmentNumber +
                                                                         ",CHN_CHF Cry," +
                                                                         Config.getTimeStr(recStartTime) + "," +
                                                                         cstartSecs + "," + cendSecs + "," +
                                                                         Config.getTimeStr(cstart) + "," +
                                                                         Config.getTimeStr(cend) + "," +
                                                                         String.Format("{0:0.00}", cpi.cry) + "," +
                                                                         String.Format("{0:0.00}", pi.bd));

                                                                    if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                                    {
                                                                        if (!onsets.ContainsKey(pi.bid))
                                                                            onsets.Add(pi.bid, new List<Onset>());
                                                                        onsets[pi.bid].Add(new Onset(file,
                                                                this.day,
                                                                pi.bid,
                                                                pi.lenaId,
                                                                mr.type,
                                                                segmentNumber,
                                                               "CHN_CHF Cry",
                                                                recStartTime,
                                                                cstartSecs,
                                                                cendSecs,
                                                                cstart,
                                                                cend,
                                                                pi.vd,
                                                                pi.bd,
                                                                0,
                                                                0,
                                                                0,
                                                                0, mr.aid));
                                                                    }

                                                                    if (mr.type == "Child")
                                                                        addMsToRawLena(ref rawLenaInfo, cpi);

                                                                }
                                                                else if (atts.Name.IndexOf("startUtt") == 0)
                                                                {
                                                                    String cryStep = atts.Name.Substring(8);
                                                                    String att = atts.Name;
                                                                    double cstartSecs = Convert.ToDouble(seg.Attributes[att].Value.Substring(2, seg.Attributes[att].Value.Length - 3)) - recStartSecs;
                                                                    double cendSecs = Convert.ToDouble(seg.Attributes["endUtt" + cryStep].Value.Substring(2, seg.Attributes["endUtt" + cryStep].Value.Length - 3)) - recStartSecs;
                                                                    DateTime cstart = Config.geFullTime(recStartTime.AddMilliseconds(cstartSecs * 1000));
                                                                    DateTime cend = Config.geFullTime(recStartTime.AddMilliseconds(cendSecs * 1000));
                                                                    PersonInfo cpi = new PersonInfo();
                                                                    cpi.dt = cstart;
                                                                    mr = cf.getLenaMapping(lenaId, start);
                                                                    cpi.ubiId = mr.UbiID;// ubiAndBId[0];
                                                                        cpi.longBid = mr.longBID;
                                                                        cpi.gender = mr.sex;
                                                                        cpi.lang = mr.lang;
                                                                        cpi.diagnosis = mr.aid;
                                                                    cpi.bid = mr.BID;// ubiAndBId[1];
                                                                    cpi.lenaId = lenaId;
                                                                    cpi.bd = (cend - cstart).Seconds + ((cend - cstart).Milliseconds > 0 ? (cend - cstart).Milliseconds / 1000.00 : 0); //cendSecs - cstartSecs;
                                                                    cpi.vd = cpi.bd;
                                                                    cpi.pType = mr.type;
                                                                    //pi.vc = Convert.ToDouble(seg.Attributes["childUttCnt"].Value);
                                                                    //cpi.cry = cpi.bd;
                                                                    //Config.getTimeStr(end) + "," +
                                                                    //Math.Round(pi.vd, 2) + "," +
                                                                    //sw.WriteLine("File,Date,Subject,LenaID,SubjectType,segmentid,voctype,
                                                                    //recstart,startsec,endsec,starttime,endtime,
                                                                    //duration,seg_duration,wordcount,avg_db,avg_peak,turn_taking ");

                                                                    newSwLine = (file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," + segmentNumber +
                                                                    ",CHN_CHF Utt," +
                                                                        Config.getTimeStr(recStartTime) + "," +
                                                                        cstartSecs + "," +
                                                                        cendSecs + "," +
                                                                        Config.getTimeStr(cstart) + "," +
                                                                        Config.getTimeStr(cend) + "," +
                                                                        String.Format("{0:0.00}", cpi.vd) + "," +
                                                                        String.Format("{0:0.00}", pi.bd) + ",,,,," +
                                                                        start.Hour + "," + start.Minute + "," + start.Second +
                                                                     "," + end.Hour + "," + end.Minute + "," + end.Second);


                                                                    if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                                    {
                                                                        if (!onsets.ContainsKey(pi.bid))
                                                                            onsets.Add(pi.bid, new List<Onset>());
                                                                        onsets[pi.bid].Add(new Onset(file,
                                                            this.day,
                                                            pi.bid,
                                                            pi.lenaId,
                                                            mr.type,
                                                            segmentNumber,
                                                           "CHN_CHF Utt",
                                                            recStartTime,
                                                            cstartSecs,
                                                            cendSecs,
                                                            cstart,
                                                            cend,
                                                            pi.vd,
                                                            pi.bd,
                                                            0,
                                                            0,
                                                            0,
                                                            0, mr.aid));
                                                                    }


                                                                    if (mr.type == "Child")
                                                                        addMsToRawLena(ref rawLenaInfo, cpi);


                                                                    if (cf.settings.doMinVocs)
                                                                    {
                                                                        setMinData(ref minDate, cstart, cend, "CHD", cpi.vd);
                                                                    }
                                                                }
                                                                if ((!dayDone) && newSwLine != "" && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                                    sw.WriteLine(newSwLine);
                                                            }
                                                            //////////////////////
                                                            add = true;
                                                        }
                                                        break;
                                                    case "FAN":
                                                        pi.ac = Convert.ToDouble(seg.Attributes["femaleAdultWordCnt"].Value);

                                                        /*****SUM******/
                                                        sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                        addToPersonInfoSum(ref sum, sumKey, 0, 0, 0, pi.ac, 0, 0);
                                                        /*****SUM******/
                                                        if(Math.Round(startSecs,0)== 8791)
                                                            {
                                                                int f = 1;
                                                            }
                                                        //if (mr.type == "Lab" || mr.type == "Teacher")
                                                        {
                                                            pi.ad = Convert.ToDouble(seg.Attributes["femaleAdultUttLen"].Value.Substring(1, seg.Attributes["femaleAdultUttLen"].Value.Length - 2));
                                                            //pi.vd = Convert.ToDouble(seg.Attributes["femaleAdultUttLen"].Value.Substring(1, seg.Attributes["femaleAdultUttLen"].Value.Length - 2));
                                                            //pi.vc = Convert.ToDouble(seg.Attributes["femaleAdultWordCnt"].Value);
                                                            pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                            pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                            pi.childSegments = childSegmentNumber;
                                                            if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                            {//Config.getTimeStr(end) + "," +
                                                             //Math.Round(pi.vd, 2) + "," +
                                                                sw.WriteLine(file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," + segmentNumber +
                                                                    ",FAN SegmentUtt," +
                                                                    Config.getTimeStr(recStartTime) + "," +
                                                                    startSecs + "," + endSecs + "," +
                                                                    Config.getTimeStr(start) + "," +
                                                                    Config.getTimeStr(end) + "," +
                                                                    String.Format("{0:0.00}", pi.ad) + "," +
                                                                    String.Format("{0:0.00}", pi.bd) + "," +
                                                                    String.Format("{0:0.00}", pi.ac) + "," +
                                                                    String.Format("{0:0.00}", pi.avDb) + "," +
                                                                    String.Format("{0:0.00}", pi.maxDb));

                                                                if (!onsets.ContainsKey(pi.bid))
                                                                    onsets.Add(pi.bid, new List<Onset>());
                                                                onsets[pi.bid].Add(new Onset(file,
                                                        this.day,
                                                        pi.bid,
                                                        pi.lenaId,
                                                        mr.type,
                                                        segmentNumber,
                                                       "FAN SegmentUtt",
                                                        recStartTime,
                                                        startSecs,
                                                        endSecs,
                                                        start,
                                                        end,
                                                        pi.ad,
                                                        pi.bd,
                                                        pi.ac,
                                                        pi.avDb,
                                                        pi.maxDb,
                                                        0, mr.aid));
                                                            }
                                                            if (cf.settings.doMinVocs)
                                                            {
                                                                if (start.Minute == 14 && pi.bid == "6B")
                                                                {
                                                                    Boolean stp = true;
                                                                }

                                                                setMinData(ref minDate, start, end, "FAN", pi.ac);
                                                            }
                                                        }
                                                        //if (mr.type == "Lab" || mr.type == "Teacher")
                                                        add = true;
                                                        break;
                                                    case "MAN":
                                                        pi.ac = Convert.ToDouble(seg.Attributes["maleAdultWordCnt"].Value);

                                                        /*****SUM******/
                                                        sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                        addToPersonInfoSum(ref sum, sumKey, 0, 0, 0, pi.ac, 0, 0);
                                                        /*****SUM******/


                                                        // if (mr.type == "Lab" || mr.type == "Teacher")
                                                        {
                                                            pi.ad = Convert.ToDouble(seg.Attributes["maleAdultUttLen"].Value.Substring(1, seg.Attributes["maleAdultUttLen"].Value.Length - 2));
                                                            pi.ac = Convert.ToDouble(seg.Attributes["maleAdultWordCnt"].Value);
                                                            pi.avDb = Convert.ToDouble(seg.Attributes["average_dB"].Value);
                                                            pi.maxDb = Convert.ToDouble(seg.Attributes["peak_dB"].Value);
                                                            pi.childSegments = childSegmentNumber;
                                                            if ((!dayDone) && (cf.settings.doOnsets || cf.settings.doSocialOnsets))
                                                            {//Config.getTimeStr(end) + "," +
                                                             //Math.Round(pi.vd, 2) + "," +
                                                                sw.WriteLine(file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," +
                                                                    mr.type + "," + segmentNumber +
                                                                    ",MAN SegmentUtt," +
                                                                    Config.getTimeStr(recStartTime) + "," +
                                                                    startSecs + "," + endSecs + "," +
                                                                    Config.getTimeStr(start) + "," +
                                                                    Config.getTimeStr(end) + "," +
                                                                    String.Format("{0:0.00}", pi.ad) + "," +
                                                                    String.Format("{0:0.00}", pi.bd) + "," +
                                                                    String.Format("{0:0.00}", pi.ac) + "," +
                                                                    String.Format("{0:0.00}", pi.avDb) + "," +
                                                                    String.Format("{0:0.00}", pi.maxDb));

                                                                if (!onsets.ContainsKey(pi.bid))
                                                                    onsets.Add(pi.bid, new List<Onset>());
                                                                onsets[pi.bid].Add(new Onset(file,
                                                            this.day,
                                                            pi.bid,
                                                            pi.lenaId,
                                                            mr.type,
                                                            segmentNumber,
                                                           "MAN SegmentUtt",
                                                            recStartTime,
                                                            startSecs,
                                                            endSecs,
                                                            start,
                                                            end,
                                                            pi.ad,
                                                            pi.bd,
                                                            pi.ac,
                                                            pi.avDb,
                                                            pi.maxDb,
                                                            0, mr.aid));
                                                            }
                                                            if (cf.settings.doMinVocs)
                                                            {
                                                                setMinData(ref minDate, start, end, "MAN", pi.ac);
                                                            }
                                                        }
                                                        //if (mr.type == "Lab" || mr.type == "Teacher")
                                                        add = true;
                                                        break;
                                                    case "OLN":
                                                        pi.bd = endSecs - startSecs;
                                                        pi.oln = pi.bd;
                                                        /*****SUM******/
                                                        sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                        addToPersonInfoSum(ref sum, sumKey, 0, 0, 0, 0, 0, pi.oln);
                                                        /*****SUM******/
                                                        add = true;
                                                        break;
                                                    case "NON":
                                                        pi.bd = endSecs - startSecs;
                                                        pi.no = pi.bd;
                                                        /*****SUM******/
                                                        sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                        addToPersonInfoSum(ref sum, sumKey, 0, 0, 0, 0, pi.no, 0);
                                                        /*****SUM******/
                                                        add = true;
                                                        break;
                                                    case "CXN":
                                                    case "CXF":
                                                        if (cf.settings.doMinVocs)
                                                        {
                                                            setMinData(ref minDate, start, end, "CX", pi.bd);
                                                        }
                                                        /*****SUM******/
                                                        sumKey = this.day.ToShortDateString() + "," + (pi.bid.Trim() != "" ? pi.bid : lenaId) + "," + lenaId;
                                                        addToPersonInfoSum(ref sum, sumKey, 0, 0, 0, 0, 0, 0, pi.bd);
                                                            if (cf.settings.doOnsets)
                                                            {
                                                                sw.WriteLine(file + "," + this.day + "," + pi.bid + "," + pi.lenaId + "," + mr.type + "," +
                                                                    segmentNumber + ",CXN_CXF SegmentUtt," +
                                                                    Config.getTimeStr(recStartTime) + "," + startSecs + "," + endSecs + "," +
                                                                    Config.getTimeStr(start) + "," +
                                                                    Config.getTimeStr(end) + "," +
                                                                    String.Format("{0:0.00}", pi.vd) + "," +
                                                                    String.Format("{0:0.00}", pi.bd) + ",," +
                                                                    String.Format("{0:0.00}", pi.avDb) + "," +
                                                                    String.Format("{0:0.00}", pi.maxDb) + ",," +
                                                                    start.Hour + "," + start.Minute + "," + start.Second +
                                                                    "," + end.Hour + "," + end.Minute + "," + end.Second);
                                                            }


                                                                /*****SUM******/
                                                                break;

                                                }

                                                if (add)
                                                {

                                                    addMsToRawLena(ref rawLenaInfo, pi);
                                                }

                                            }//////


                                        }
                                    }
                                    }

                                    if (cf.settings.doDbs)
                                        swd.WriteLine(this.day + "," + pibid + "," + pitype + "," + (subjectConvAvgDb != 1 ? subjectConvAvgDb : 0) + "," + (subjectConvMaxDb != 1 ? subjectConvMaxDb : 0) + "," + (subjectAvgDb != 1 ? subjectAvgDb : 0) + "," + (subjectMaxDb != 1 ? subjectMaxDb : 0));
                                    foreach (DateTime md in minDate.Keys)
                                    {
                                        //Dictionary<DateTime, Tuple<double, double, double, double>> minDate = new Dictionary<DateTime, Tuple<double, double, double, double>>();
                                        //Config.getTimeStr(end) + "," +
                                        //Math.Round(pi.vd, 2) + "," +
                                        Tuple<double, double, double, double> minVal = minDate[md];
                                        swm.WriteLine(file + "," + this.day + "," + pibid + "," + lenaId + "," + pitype + "," +
                                            Config.getTimeStr(md) +
                                            "," + String.Format("{0:0.00}", minVal.Item1) +
                                            "," + String.Format("{0:0.00}", minVal.Item2) +
                                            "," + String.Format("{0:0.00}", minVal.Item3) +
                                            "," + String.Format("{0:0.00}", minVal.Item4));

                                    }
                                }
                                // 

                            }

                        }
                    }
                }
                if ((cf.settings.doOnsets || cf.settings.doSocialOnsets))
                {
                    swoffsets.Close();
                    sw.Close();
                    swava.Close();
                }

                if (cf.settings.doDbs)
                    swd.Dispose();
                if (cf.settings.doMinVocs)
                    swm.Dispose();

                if (cf.settings.lenaTimes)
                    swlt.Dispose();

                if (doSmartNames)
                    swsn.Dispose();

                swqa.Close();
            }
            if (!dayDone)
                daysDone.Add(this.day, true);
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
        public void addMsToRawLena(ref Dictionary<String, List<PersonInfo>> rl, PersonInfo lpi)
        {
            lpi.dt = Config.getMsTime(lpi.dt);
            lpi.startTime = Config.getMsTime(lpi.startTime);
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
                        else if (false)
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
                    int ms = time.Millisecond > 0 ? time.Millisecond / 100 * 100 : time.Millisecond;// + 100;
                    time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, ms);
                    DateTime timeEnd = data.dt.AddSeconds(data.bd);
                    ms = timeEnd.Millisecond > 0 ? timeEnd.Millisecond / 100 * 100 : timeEnd.Millisecond;// + 100;
                    timeEnd = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, timeEnd.Hour, timeEnd.Minute, timeEnd.Second, ms);
                    if (!startSet)
                    {
                        startLenaTimes.Add(person, time);
                        startSet = true;
                        if (maxLenaTimes < time)
                            maxLenaTimes = time;
                    }

                    Double blockDur = data.bd;
                    Double vocDur = data.vd;
                    Double aDur = data.ad;
                    blockDur = (timeEnd - time).Seconds + (timeEnd - time).Milliseconds > 0 ? ((timeEnd - time).Milliseconds / 1000.00) : 0;
                    Double vocCount = data.vc;
                    Double turnCount = data.tc;
                    Double a = data.ac;
                    Double ad = data.ad;
                    Double n = data.no;
                    Double o = data.oln;
                    Double cry = data.cry;
                    Double avDb = data.avDb;
                    Double maxDb = data.maxDb;
                    double segs = data.childSegments;

                    if (personTotalCounts.ContainsKey(person))/////////////////
                    {
                        //Tuple<double, double, double, double, double> totalInfo = personTotalCounts[person];

                        personTotalCounts[person].vd = personTotalCounts[person].vd + vocDur;
                        personTotalCounts[person].ad = personTotalCounts[person].ad + aDur;
                        personTotalCounts[person].vc = personTotalCounts[person].vc + vocCount;
                        personTotalCounts[person].tc = personTotalCounts[person].tc + turnCount;
                        personTotalCounts[person].ac = personTotalCounts[person].ac + a;
                        personTotalCounts[person].no = personTotalCounts[person].no + n;
                        personTotalCounts[person].oln = personTotalCounts[person].oln + o;
                        personTotalCounts[person].cry = personTotalCounts[person].cry + cry;
                        personTotalCounts[person].avDb = personTotalCounts[person].avDb + avDb;
                        personTotalCounts[person].maxDb = personTotalCounts[person].maxDb + maxDb;
                        personTotalCounts[person].childSegments = personTotalCounts[person].childSegments + segs;

                        //personTotalCounts[person] = new Tuple<double, double, double, double, double>(totalInfo.Item1 + vocDur, totalInfo.Item2 + vocCount, totalInfo.Item3 + turnCount, totalInfo.Item4 + a, totalInfo.Item5 + n);
                    }
                    else
                    {
                        PersonInfo pi = new PersonInfo();
                        pi.vd = vocDur;
                        pi.ad = aDur;
                        pi.vc = vocCount;
                        pi.tc = turnCount;
                        pi.ac = a;
                        pi.no = n;
                        pi.oln = o;
                        pi.cry = cry;
                        pi.avDb = avDb;
                        pi.maxDb = maxDb;
                        pi.childSegments = segs;
                        personTotalCounts.Add(person, pi);
                        personTotalCountsWUbi.Add(person, new PersonInfo());
                        //personTotalCounts.Add(person, new Tuple<double, double, double, double, double>(vocDur, vocCount, turnCount, a, n));
                    }
                    if (blockDur > 0)
                    {
                        double turnCount10 = (turnCount / blockDur) / 10;
                        double vocCount10 = (vocCount / blockDur) / 10;
                        double vocDur10 = (vocDur / blockDur) / 10;
                        double aDur10 = (aDur / blockDur) / 10;
                        double adults10 = (a / blockDur) / 10;
                        double noise10 = (n / blockDur) / 10;
                        double oln10 = (o / blockDur) / 10;
                        double cry10 = (cry / blockDur) / 10;
                        double avDb10 = (avDb / blockDur) / 10;
                        double maxDb10 = (maxDb / blockDur) / 10;
                        double segs10 = (segs / blockDur) / 10;
                        do
                        {
                            if (activities.ContainsKey(time))
                            {
                                if (activities[time].ContainsKey(person))
                                {
                                    activities[time][person].hasULData = true;
                                }
                                if (cf.allLena && (!activities.ContainsKey(time)))
                                {
                                    activities.Add(time, new Dictionary<string, PersonInfo>());
                                }
                                if (cf.allLena && (!activities[time].ContainsKey(person)))
                                {
                                    activities[time].Add(person, new PersonInfo());
                                }

                                if (activities[time].ContainsKey(person))
                                {
                                    //activities[time][person] = new PersonInfo(activities[time][person].xPos, activities[time][person].yPos, activities[time][person].lx, activities[time][person].ly, activities[time][person].rx, activities[time][person].ry, activities[time][person].orientation, 
                                    //vocDur > 0, vocCount10, turnCount10, vocDur10, adults10, noise10);


                                    activities[time][person].wasTalking = vocDur > 0 || activities[time][person].wasTalking;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                    activities[time][person].isCrying = cry > 0 || activities[time][person].isCrying;//, vocCount10, turnCount10, vocDur10, adults10, noise10;
                                    activities[time][person].vc += vocCount10;
                                    activities[time][person].tc += turnCount10;
                                    activities[time][person].vd += vocDur10;
                                    activities[time][person].ad += aDur10;
                                    activities[time][person].ac += adults10;
                                    activities[time][person].oln += oln10;
                                    activities[time][person].no += noise10;
                                    activities[time][person].cry += cry10;

                                    activities[time][person].avDb += avDb10;
                                    activities[time][person].maxDb += maxDb10;
                                    activities[time][person].childSegments += segs10;
                                    /*double avDb10 = (avDb / blockDur) / 10;
                            double maxDb10 = (maxDb / blockDur) / 10;
                            double segs10 = (segs / blockDur) / 10;*/
                                    if (personTotalCountsWUbi.ContainsKey(person))/////////////////
                                    {
                                        personTotalCountsWUbi[person].vc += vocCount10;
                                        personTotalCountsWUbi[person].tc += turnCount10;
                                        personTotalCountsWUbi[person].vd += vocDur10;
                                        personTotalCountsWUbi[person].ad += aDur10;
                                        personTotalCountsWUbi[person].ac += adults10;
                                        personTotalCountsWUbi[person].oln += oln10;
                                        personTotalCountsWUbi[person].no += noise10;
                                        personTotalCountsWUbi[person].cry += cry10;
                                        personTotalCountsWUbi[person].avDb += avDb10;
                                        personTotalCountsWUbi[person].maxDb += maxDb10;
                                        personTotalCountsWUbi[person].childSegments += segs10;
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
        }
        public void setLenaData2(Dictionary<String, List<PersonInfo>> lenadata)
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
        public Tuple<double, double, double> getCenterAndOrientationFromLR(PersonInfo pi)
        {
            try
            {
                if (pi.x == 0 &&
                pi.y == 0 &&
                pi.lx != 0 &&
                pi.ly != 0 &&
                pi.rx != 0 &&
                pi.ry != 0)
                {
                    //getCenter

                    /*public static double getCenter(double x, double x2)
        {
            double l = Math.Abs(x2 - x) / 2;
            return x < x2 ? x + l : x2 + l;
        }*/
                    pi.y = getCenter(pi.ly, pi.ry);
                    pi.x = getCenter(pi.lx, pi.rx);
                    //activities[dt][person].ori_chaoming


                    pi.ori_chaoming = Math.Atan2(pi.ry - pi.ly, pi.rx - pi.lx) / Math.PI * 180 + 90;
                    if(pi.ori_chaoming > 360)
                    {
                        pi.ori_chaoming = pi.ori_chaoming;
                    }
                    pi.ori_chaoming = pi.ori_chaoming > 360 ? pi.ori_chaoming - 360 : pi.ori_chaoming;


                }
            }
            catch (Exception e)
            {

            }
            return new Tuple<double, double, double>(pi.x, pi.y, pi.ori_chaoming);
        }
        public Dictionary<string, PairInfo> pairInteractions = new Dictionary<string, PairInfo>();
        public void countInteractionsNew(DateTime trunk, Dictionary<String, List<PersonInfo>> lenaInfo)
        {

            List<String> appPairs = new List<string>();
            Dictionary<String, int> onsetPos = new Dictionary<string, int>();

            Dictionary<DateTime, Dictionary<String, Tuple<DateTime, DateTime>>> flags = new Dictionary<DateTime, Dictionary<string, Tuple<DateTime, DateTime>>>();

            bool trunkDetailFile = true;

            try
            {
                TextWriter sw = null;
                TextWriter swa = null;

                if (cf.settings.doAngleFiles)
                    sw = new StreamWriter(cf.root + cf.classroom + "/SYNC/interaction_angles_xy_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + szDay + "_" + cf.settings.fileNameVersion + ".CSV");
                if (cf.settings.doApproach)
                    swa = new StreamWriter(cf.root + cf.classroom + "/SYNC/approach_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + szDay + "_" + cf.settings.fileNameVersion + ".CSV");

                {
                    if (cf.settings.doAngleFiles)
                        sw.WriteLine("Person 1, Person2, Interaction Time, Interaction Millisecond, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2,Type1, Type2, Gender1, Gender2, Diagnosis1, Diagnosis2, WasTalking1, WasTalking2 ");

                    if (cf.settings.doApproach)
                        swa.WriteLine("Person 1, Person2, Interaction Time, Interaction Millisecond,d1,d2,approachMeters,x10,y10,x20,y20,x11,y11,x21,y21, WithinGR, WithinGRAnd45deg, Angle1, Angle2,Type1, Type2, Gender1, Gender2, Diagnosis1, Diagnosis2 ");

                    int opic = 0;


                    foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> opi in activities.OrderBy(key => key.Key))
                    {
                        opic++;
                        try
                        {
                            int pc = 0;
                            int ppc = 0;
                            int pic = 0;
                            DateTime dt = opi.Key;


                            if (trunkDetailFile && dt.CompareTo(trunk) <= 0 && ((!cf.justFreePlay) || (isThisFreePlay(dt))))/////
                            {

                                foreach (String person in activities[dt].Keys)
                                {
                                    pc++;
                                    // if((person=="10D" || person=="1D" )&& isWithLenaStart(dt, person))
                                    try
                                    {



                                        if ((cf.settings.subs.Count == 0 || cf.settings.subs.Contains(person)) &&
                                        ((cf.justUbi || (!cf.settings.startFromLena) || isWithLenaStart(dt, person))))
                                        {

                                            //If we have left and right info for this person then proceed
                                            if ((!double.IsNaN(activities[dt][person].x)) &&
                                           (!double.IsNaN(activities[dt][person].y)) &&
                                           (!double.IsNaN(activities[dt][person].lx)) &&
                                           (!double.IsNaN(activities[dt][person].ly)) &&
                                           (!double.IsNaN(activities[dt][person].rx)) &&
                                           (!double.IsNaN(activities[dt][person].ry)))
                                            {
                                                Tuple<double, double, double> xyo = getCenterAndOrientationFromLR(activities[dt][person]);
                                                activities[dt][person].x = xyo.Item1;
                                                activities[dt][person].y = xyo.Item2;
                                                activities[dt][person].ori_chaoming = xyo.Item3;

                                                if (activities[dt][person].x != 0 &&
                                                        activities[dt][person].y != 0 &&
                                                        (!double.IsNaN(activities[dt][person].x)) &&
                                                        (!double.IsNaN(activities[dt][person].y)))
                                                {



                                                    ppc = 0;
                                                    foreach (String p in activities[dt].Keys)
                                                    {
                                                        ppc++;


                                                        xyo = getCenterAndOrientationFromLR(activities[dt][p]);
                                                        activities[dt][p].x = xyo.Item1;
                                                        activities[dt][p].y = xyo.Item2;
                                                        activities[dt][p].ori_chaoming = xyo.Item3;

                                                        if ((!p.Equals(person)) &&
                                                            (cf.settings.subs.Count == 0 || cf.settings.subs.Contains(p)) &&
                                                                                isWithLenaStart(dt, p) &&
                                                                                activities[dt][p].x != 0 &&
                                                                                activities[dt][p].y != 0 &&
                                                                                activities[dt][p].rx != 0 &&
                                                                                activities[dt][p].ry != 0 &&
                                                                                activities[dt][p].lx != 0 &&
                                                                                activities[dt][p].ly != 0 &&
                                                                                (!double.IsNaN(activities[dt][p].x)) &&
                                                                                    (!double.IsNaN(activities[dt][p].y)) &&
                                                                                    (!double.IsNaN(activities[dt][p].lx)) &&
                                                                                    (!double.IsNaN(activities[dt][p].ly)) &&
                                                                                    (!double.IsNaN(activities[dt][p].rx)) &&
                                                                                    (!double.IsNaN(activities[dt][p].ry)))
                                                        {
                                                            Boolean inversePair = false;
                                                            String subject = p;
                                                            String partner = person;
                                                            String pair = p + "-" + person;
                                                            if ((!cf.pairs.Contains(pair)) && cf.pairs.Contains(person + "-" + p))
                                                            {
                                                                inversePair = true;
                                                                pair = person + "-" + p;
                                                                subject = person;
                                                                partner = p;
                                                            }



                                                            if (!isWithLenaStart(dt, p))
                                                            {

                                                                if (!flags.ContainsKey(day))
                                                                    flags.Add(day, new Dictionary<string, Tuple<DateTime, DateTime>>());

                                                                if ((!flags[day].ContainsKey(pair)) && startLenaTimes.ContainsKey(p))
                                                                {
                                                                    DateTime lenaStart = startLenaTimes[p];
                                                                    flags[day].Add(pair, new Tuple<DateTime, DateTime>(dt, lenaStart));
                                                                    Console.WriteLine(pair + " on " + day.ToShortDateString() + " LOG TimeStamp: " +
                          Config.getDateTimeStr(dt) +
                          " LENA START: " + Config.getDateTimeStr(lenaStart));
                                                                }





                                                            }
                                                            if (!pairInteractions.ContainsKey(pair))
                                                                pairInteractions.Add(pair, new PairInfo());

                                                            if (!inversePair)
                                                            {
                                                                if (!pairInteractions[pair].subjectSet)
                                                                {
                                                                    pairInteractions[pair].subjectSet = true;
                                                                }
                                                                else
                                                                {
                                                                    pairInteractions[pair].subject.individualTime += 0.1;////
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (!pairInteractions[pair].partnerSet)
                                                                {
                                                                    pairInteractions[pair].partnerSet = true;
                                                                }
                                                                else
                                                                {
                                                                    pairInteractions[pair].partner.individualTime += 0.1;
                                                                }
                                                            }

                                                            pairInteractions[pair].sharedTimeInSecs += .1;
                                                            double dist = calcSquaredDist(activities[dt][person], activities[dt][p]);
                                                            MappingRow person1 = cf.getMapping(person, dt);
                                                            MappingRow person2 = cf.getMapping(p, dt);


                                                            Boolean withinGofR = (dist <= (cf.gOfR * cf.gOfR)) && (dist >= (cf.gOfRMin * cf.gOfRMin));
                                                            Tuple<double, double> angles = withinOrientationData(activities[dt][p], activities[dt][person]);

                                                            /*****T1 HACK right tag stopped working on 2/12/19 at 10:28:38.644 left 00:11:CE:00:00:00:02:CE right 00:11:CE:00:00:00:02:F2*****/
                                                            Boolean hackThisT1 = cf.settings.hackT1? cf.getMapping(person, dt).leftTag.Trim() == "00:11:CE:00:00:00:02:CE" && dt >= new DateTime(2019, 02, 12, 10, 28, 38, 644) && dt <= new DateTime(2019, 06, 3) : false;
                                                            //cf.getMapping(person, dt).leftTag.Trim() == "00:11:CE:00:00:00:02:CE" && dt >= new DateTime(2019, 02, 12, 10, 28, 38, 644) && dt <= new DateTime(2019, 06, 3);
                                                                                   /*****T1 HACK right tag *****/

                                                            Boolean orientedCloseness = withinGofR && ((Math.Abs(angles.Item1) <= 45 && Math.Abs(angles.Item2) <= 45) || hackThisT1);

                                                            Boolean wasTalking = activities[dt][person].wasTalking;
                                                            if (cf.settings.doApproach)
                                                            {
                                                                double dist0 = 0;
                                                                double dist1 = 0;
                                                                DateTime dt0 = dt.AddMilliseconds(-100);
                                                                
                                                                if (activities.ContainsKey(dt0))
                                                                {
                                                                    if (activities[dt0].ContainsKey(p))
                                                                    {
                                                                        if((!Double.IsNaN(activities[dt0][p].x)) &&
                                                                            (!Double.IsNaN(activities[dt0][p].y)) &&
                                                                            (!Double.IsNaN(activities[dt0][person].x)) &&
                                                                            (!Double.IsNaN(activities[dt0][person].y)) &&
                                                                                activities[dt0][p].x != 0 &&
                                                                                activities[dt0][p].y != 0 &&
                                                                                activities[dt0][p].rx != 0 &&
                                                                                activities[dt0][p].ry != 0 &&
                                                                                activities[dt0][p].lx != 0 &&
                                                                                activities[dt0][p].ly != 0 &&
                                                                                activities[dt0][person].x != 0 &&
                                                                                activities[dt0][person].y != 0 &&
                                                                                activities[dt0][person].rx != 0 &&
                                                                                activities[dt0][person].ry != 0 &&
                                                                                activities[dt0][person].lx != 0 &&
                                                                                activities[dt0][person].ly != 0)
                                                                            {
                                                                                dist1 = Math.Sqrt(calcSquaredDist(activities[dt][person], activities[dt0][p]));
                                                                                dist0 = Math.Sqrt(calcSquaredDist(activities[dt0][person], activities[dt0][p]));
                                                                            double approachMeters = dist0 - dist1;

                                                                            if (Double.IsNaN(approachMeters))
                                                                            {
                                                                                bool stop = true;
                                                                                dist1 = Math.Sqrt(calcSquaredDist(activities[dt][person], activities[dt0][p]));
                                                                                dist0 = Math.Sqrt(calcSquaredDist(activities[dt0][person], activities[dt0][p]));
                                                                            }
                                                                            else
                                                                            {
                                                                                String appLine = person + "," +
                                                                                    p + "," +
                                                                                    dt.ToLongTimeString() + "," +
                                                                                    dt.Millisecond + "," +
                                                                                    dist0 + "," +
                                                                                    dist1 + "," +
                                                                                    approachMeters + "," +
                                                                                    activities[dt0][person].x + "," +
                                                                                    activities[dt0][person].y + "," +
                                                                                    activities[dt0][p].x + "," +
                                                                                    activities[dt0][p].y + "," +
                                                                                    activities[dt][person].x + "," +
                                                                                    activities[dt][person].y + "," +
                                                                                    activities[dt][p].x + "," +
                                                                                    activities[dt][p].y + "," +
                                                                                    (withinGofR ? "TRUE" : "FALSE") + "," +
                                                                                    (orientedCloseness ? "TRUE" : "FALSE") + "," +
                                                                                    (angles.Item1) + "," +
                                                                                    (angles.Item2) + "," +
                                                                                    person1.type + "," +
                                                                                    person2.type + "," +
                                                                                    person1.sex + "," +
                                                                                    person2.sex + "," +
                                                                                    person1.aid + "," +
                                                                                    person2.aid;

                                                                                if(appLine.Split(',').Length!=25)
                                                                                {


                                                                                    Console.WriteLine("APP LINE " + appLine);
                                                                                }
                                                                                swa.WriteLine( appLine);
                                                                            }
                                                                        }
                                                                         
                                                                    }

                                                                }
                                                                 
                                                                 
                                                            }
                                                            if (withinGofR)
                                                            {

                                                                if (cf.settings.doAngleFiles)
                                                                {



                                                                    sw.WriteLine(person + "," +
                                                                        p + "," +
                                                                        dt.ToLongTimeString() + "," +
                                                                        dt.Millisecond + "," +
                                                                        (withinGofR ? "0.1" : "0") + "," +
                                                                        (orientedCloseness ? "0.1" : "0") + "," +
                                                                        (angles.Item1) + "," +
                                                                        (angles.Item2) + "," +
                                                                        activities[dt][person].lx + "," +
                                                                        activities[dt][person].ly + "," +
                                                                        activities[dt][person].rx + "," +
                                                                        activities[dt][person].ry + "," +
                                                                        activities[dt][p].lx + "," +
                                                                        activities[dt][p].ly + "," +
                                                                        activities[dt][p].rx + "," +
                                                                        activities[dt][p].ry + "," +
                                                                    person1.type + "," +
                                                                    person2.type + "," +
                                                                    person1.sex + "," +
                                                                    person2.sex + "," +
                                                                    person1.aid + "," +
                                                                    person2.aid + "," +
                                                                    activities[dt][person].wasTalking + "," +
                                                                    activities[dt][p].wasTalking);
                                                                }

                                                                pairInteractions[pair].closeTimeInSecs += .1;

                                                                if (orientedCloseness)
                                                                {
                                                                    pairInteractions[pair].closeAndOrientedTimeInSecs += .1;

                                                                }
                                                            }
                                                            //if(wasTalking)
                                                            Double tc = activities[dt][person].tc;
                                                            Double vc = activities[dt][person].vc;
                                                            Double vd = activities[dt][person].vd;
                                                            Double ad = activities[dt][person].ad;
                                                            Double a = activities[dt][person].ac;
                                                            Double n = activities[dt][person].no;
                                                            Double o = activities[dt][person].oln;
                                                            Double c = activities[dt][person].cry;

                                                            if (wasTalking || vd > 0 || ad > 0 || tc > 0 || a > 0 || n > 0 || vc > 0 || o > 0 || c > 0)
                                                            {
                                                                if (dist <= (cf.gOfR * cf.gOfR) && dist >= (cf.gOfRMin * cf.gOfRMin))
                                                                {

                                                                    if (hackThisT1 || justProx || withinOrientation(activities[dt][p], activities[dt][person], 45))
                                                                    {
                                                                        if (activities[dt][person].cry > 0 && activities[dt][p].cry > 0)
                                                                        {
                                                                            if (person.Equals(pair.Split('-')[0]))
                                                                            {
                                                                                pairInteractions[pair].closeAndOrientedCryInSecs += (activities[dt][person].cry);
                                                                                pairInteractions[pair].subject.cryingTime += (activities[dt][person].cryingTime);
                                                                            }
                                                                            else
                                                                            {//p is in the first part of the pair
                                                                                pairInteractions[pair].closeAndOrientedCryInSecs += (activities[dt][p].cry);
                                                                                pairInteractions[pair].subject.cryingTime += (activities[dt][p].cryingTime);
                                                                            }
                                                                        }

                                                                        List<PersonInfo> pi = lenaInfo[person];
                                                                        pic = 0;
                                                                        foreach (PersonInfo i in pi)
                                                                        {
                                                                            pic++;

                                                                            if (opic == 20532 && pc == 4 && ppc == 6 && pic == 6613)
                                                                            {
                                                                                bool stop = true;//
                                                                            }
                                                                            DateTime dt2 = i.dt;
                                                                            DateTime dt3 = i.dt.AddSeconds(i.bd);
                                                                            int ms = dt2.Millisecond > 0 ? dt2.Millisecond / 100 * 100 : dt2.Millisecond;// + 100;
                                                                            dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, dt2.Minute, dt2.Second, ms);
                                                                            ms = dt3.Millisecond > 0 ? dt3.Millisecond / 100 * 100 : dt3.Millisecond;// + 100;
                                                                            dt3 = new DateTime(dt3.Year, dt3.Month, dt3.Day, dt3.Hour, dt3.Minute, dt3.Second, ms);
                                                                            i.bd = (dt3 - dt2).Seconds + ((dt3 - dt2).Milliseconds / 1000.00);
                                                                            if (dt >= dt2 && dt <= dt3)
                                                                            {
                                                                                tc = i.tc > 0 && i.bd > 0 ? (i.tc / i.bd) / 10 : 0;
                                                                                a = i.ac > 0 && i.bd > 0 ? (i.ac / i.bd) / 10 : 0;
                                                                                ad = i.ad > 0 && i.bd > 0 ? (i.ad / i.bd) / 10 : 0;
                                                                                n = i.no > 0 && i.bd > 0 ? (i.no / i.bd) / 10 : 0;
                                                                                vc = i.vc > 0 && i.bd > 0 ? (i.vc / i.bd) / 10 : 0;///
                                                                                o = i.oln > 0 && i.bd > 0 ? (i.oln / i.bd) / 10 : 0;
                                                                                c = i.cry > 0 && i.bd > 0 ? (i.cry / i.bd) / 10 : 0;
                                                                                double vd2 = i.vd > 0 && i.bd > 0 ? (i.vd / i.bd) / 10 : 0;
                                                                                if (person.Equals(pair.Split('-')[0]))
                                                                                {
                                                                                    pairInteractions[pair].subject.vd += vd2;
                                                                                    pairInteractions[pair].subject.vc += vc;
                                                                                    pairInteractions[pair].subject.tc += tc;
                                                                                    pairInteractions[pair].subject.ac += a;
                                                                                    pairInteractions[pair].subject.ad += ad;
                                                                                    pairInteractions[pair].subject.no += n;
                                                                                    pairInteractions[pair].subject.oln += o;
                                                                                    pairInteractions[pair].subject.cry += c;
                                                                                }
                                                                                else //p is in the first part of the pair
                                                                                {
                                                                                    pairInteractions[pair].partner.vd += vd2;
                                                                                    pairInteractions[pair].partner.vc += vc;
                                                                                    pairInteractions[pair].partner.tc += tc;
                                                                                    pairInteractions[pair].partner.ac += a;
                                                                                    pairInteractions[pair].partner.ac += ad;
                                                                                    pairInteractions[pair].partner.no += n;
                                                                                    pairInteractions[pair].partner.oln += o;
                                                                                    pairInteractions[pair].partner.cry += c;
                                                                                }///

                                                                            }
                                                                        }

                                                                        if (wasTalking)
                                                                        {
                                                                            //pairStats[pair] += 0.1;
                                                                            /////ONSETS
                                                                            if (cf.settings.doSocialOnsets && onsets.ContainsKey(subject))
                                                                            {
                                                                                int sPos = 0;
                                                                                if (!onsetPos.ContainsKey(subject))
                                                                                    onsetPos.Add(subject, 0);
                                                                                else
                                                                                    sPos = onsetPos[subject];

                                                                                for (; sPos < onsets[subject].Count; sPos++)
                                                                                {
                                                                                    if (dt.CompareTo(onsets[subject][sPos].startTime) < 0)
                                                                                        break;
                                                                                    if (dt.CompareTo(onsets[subject][sPos].startTime) >= 0 &&
                                                                                        dt.CompareTo(onsets[subject][sPos].endTime) <= 0)
                                                                                    {
                                                                                        onsets[subject][sPos].inSocialContact = true;
                                                                                        if (person1.type == "Child" && person2.type=="Child")
                                                                                        {
                                                                                            onsets[subject][sPos].inChildSocialContact = true;
                                                                                        }
                                                                                        if (person1.type == "Child" || person2.type == "Child")
                                                                                        {
                                                                                            onsets[subject][sPos].inOneChildSocialContact = true;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                onsetPos[subject] = sPos;



                                                                                int pPos = 0;
                                                                                if (!onsetPos.ContainsKey(partner))
                                                                                    onsetPos.Add(partner, 0);
                                                                                else
                                                                                    pPos = onsetPos[partner];

                                                                                for (; pPos < onsets[partner].Count; pPos++)
                                                                                {
                                                                                    if (dt.CompareTo(onsets[partner][pPos].startTime) < 0)
                                                                                        break;
                                                                                    if (dt.CompareTo(onsets[partner][pPos].startTime) >= 0 &&
                                                                                        dt.CompareTo(onsets[partner][pPos].endTime) <= 0)
                                                                                    {
                                                                                        onsets[partner][pPos].inSocialContact = true;
                                                                                        if (person1.type == "Child" && person2.type == "Child")
                                                                                        {
                                                                                            onsets[partner][pPos].inChildSocialContact = true;
                                                                                        }
                                                                                        if (person1.type == "Child" || person2.type == "Child")
                                                                                        {
                                                                                            onsets[partner][pPos].inOneChildSocialContact = true;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                onsetPos[partner] = pPos;


                                                                            }

                                                                            pairInteractions[pair].closeAndOrientedTalkInSecs += 0.1;
                                                                            if (person.Equals(pair.Split('-')[0]))
                                                                            {
                                                                                pairInteractions[pair].subject.interactionTime += 0.1;
                                                                            }
                                                                            else //p is in the first part of the pair
                                                                            {
                                                                                pairInteractions[pair].partner.interactionTime += 0.1;
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
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("INTERACTION ERROR 2: " + e.Message);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("INTERACTION ERROR 1: " + e.Message);
                        }
                    }
                }
                /*if (writeFile)
                {
                    using (sw = new StreamWriter(cf.version + "interaction_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".csv"))
                    {
                        sw.WriteLine("Person 1, Person2, Interaction Time, Total Time, Interaction Normalized");
                        foreach (String s in pairInteractions.Keys)
                        {
                            double interactionTime = Math.Round(pairInteractions[s].closeAndOrientedTimeInSecs, 1);
                            double totalTime = Math.Round(pairInteractions[s].sharedTimeInSecs, 1);
                            double interactionNormalized = interactionTime / totalTime;
                            sw.WriteLine(s.Split('-')[0] + "," + s.Split('-')[1] + "," + interactionTime + "," + totalTime + "," + interactionNormalized);
                        }
                    }
                }*/
                sw.Close();
                swa.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            foreach (DateTime dayDate in flags.Keys)
            {
                foreach (String daypairDate in flags[dayDate].Keys)
                {
                    /*Console.WriteLine(daypairDate + " TimeStamp: " +
                        Config.getDateTimeStr(flags[dayDate][daypairDate].Item1) +
                        " LENA START: " + Config.getDateTimeStr(flags[dayDate][daypairDate].Item2));*/
                }
            }
            if (cf.settings.doSocialOnsets)
            {
                writeSocialOnsets();
            }

        }
        public bool getChildWavSocial = true;
        public void writeSocialOnsets()
        {
            TextWriter sw = null;
            TextWriter sw2 = null;
            if (cf.settings.doSocialOnsets)
            {
                String szFileName = cf.syncFilePre + "_SOCIALONSETS_" + cf.settings.fileNameVersion + ".CSV";
                bool fileExists = File.Exists(szFileName);

                sw = new StreamWriter(szFileName, true);// countDays > 0);
                sw2 = new StreamWriter(szFileName.Replace(".CSV","_WAVSSOC.CSV"), true);// countDays > 0);
                if (!fileExists)
                    sw.WriteLine("File,Date,Subject,LenaID,SubjectType,segmentid,voctype,recstart,startsec,endsec,starttime,endtime,duration,seg_duration,wordcount,avg_db,avg_peak,turn_taking ");
                foreach (String s in onsets.Keys)
                {
                    foreach (Onset os in onsets[s])
                    {


                        sw.WriteLine(os.file + "," +
                         os.day + "," +
                         os.subject + "," +
                         os.lenaID + "," +
                         os.subjectType + "," +
                         os.segmentId + "," +
                         os.vocType + "," +
                         os.recStart + "," +
                         os.startSecs + "," +
                         os.endSecs + "," +
                         os.startTime + "," +
                         os.endTime + "," +
                         os.duration + "," +
                         os.segmentDuration + "," +
                         os.wordCount + "," +
                         os.avgDb + "," +
                         os.dbPeak + "," +
                         os.turnTaking + "," +
                        (os.inSocialContact ? "YES" : "NO") + "," + os.diagnosis);

                        if (getChildWavSocial && os.inChildSocialContact && os.subjectType == "Child")
                        {
                            if((os.inSocialContact ? "YES" : "NO")=="NO")
                            {
                               // Console.WriteLine("NOSOCCONTACT ");
                            }

                            if ( os.vocType == "CHN_CHF SegmentUtt"  )
                                {
                                        sw2.WriteLine(os.file + "," +
                                    os.day + "," +
                                    os.subject + "," +
                                    os.lenaID + "," +
                                    os.subjectType + "," +
                                    os.segmentId + "," +
                                    os.vocType + "," +
                                    os.recStart + "," +
                                    os.startSecs + "," +
                                    os.endSecs + "," +
                                    os.startTime + "," +
                                    os.endTime + "," +
                                    os.duration + "," +
                                    os.segmentDuration + "," +
                                    os.wordCount + "," +
                                    os.avgDb + "," +
                                    os.dbPeak + "," +
                                    os.turnTaking + "," +
                                   (os.inSocialContact ? "YES" : "NO") + "," + os.diagnosis);
                                    }
                            if  (os.vocType == "FAN SegmentUtt" || os.vocType == "MAN SegmentUtt")
                            {
                                sw2.WriteLine(os.file + "," +
                            os.day + "," +
                            os.subject + "," +
                            os.lenaID + "," +
                            os.subjectType + "," +
                            os.segmentId + "," +
                            os.vocType + "," +
                            os.recStart + "," +
                            os.startSecs + "," +
                            os.endSecs + "," +
                            os.startTime + "," +
                            os.endTime + "," +
                            os.duration + "," +
                            os.segmentDuration + "," +
                            os.wordCount + "," +
                            os.avgDb + "," +
                            os.dbPeak + "," +
                            os.turnTaking + "," +
                           (os.inSocialContact ? "YES" : "NO") + "," + os.diagnosis);
                            }

                        }
                        

                        
                    }
                }
                sw.Close();
                sw2.Close();
            }
        }
        public void countInteractionsNewOld(List<String> subs, DateTime trunk, Dictionary<String, List<PersonInfo>> lenaInfo)
        {
            //cd.countInteractionsNew(subs, configInfo.settings.doAngleFiles, configInfo.settings.doAngleFiles, true, trunkAt, rawLena); //count interactions but no need to write a file
            bool trunkDetailFile = true;
            try
            {
                TextWriter sw = null;
                if (cf.settings.doAngleFiles)
                    sw = new StreamWriter(cf.root + cf.classroom + "/SYNC/interaction_angles_xy_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + day.Month + "_" + day.Day + "_" + day.Year + "_" + cf.settings.fileNameVersion + ".csv");
                {
                    if (cf.settings.doAngleFiles)
                        sw.WriteLine("Person 1, Person2, Interaction Time, Interaction, 45Interaction, Angle1, Angle2, Leftx,Lefty,Rightx,Righty, Leftx2,Lefty2,Rightx2,Righty2");
                    //foreach (DateTime dt in activities.Keys)
                    //{
                    foreach (KeyValuePair<DateTime, Dictionary<String, PersonInfo>> opi in activities.OrderBy(key => key.Key))
                    {
                        /*if (max < 0)
                            break;

                        max--;
                        /////TESTING*/
                        DateTime dt = opi.Key;

                        if (trunkDetailFile && dt.CompareTo(trunk) <= 0 && ((!cf.justFreePlay) || (isThisFreePlay(dt))))/////
                        {

                            foreach (String person in activities[dt].Keys)
                            {
                                if (subs.Count == 0 || subs.Contains(person))
                                {
                                    if (cf.justUbi || (!cf.settings.startFromLena) || isWithLenaStart(dt, person))
                                    {
                                        /*if (!individualTime.ContainsKey(person))
                                        {
                                            individualTime.Add(person, 0.0);
                                        }
                                        else
                                        {
                                            individualTime[person] += 0.1;
                                        }*/

                                        //LQ: why 0 for ind time and pairtime.1
                                        foreach (String p in activities[dt].Keys)
                                        {

                                            if (!p.Equals(person) && (subs.Count == 0 || subs.Contains(p)))
                                            {
                                                Boolean inversePair = false;
                                                String subject = p;
                                                String partner = person;
                                                String pair = p + "-" + person;
                                                if ((!cf.pairs.Contains(pair)) && cf.pairs.Contains(person + "-" + p))
                                                {
                                                    inversePair = true;
                                                    pair = person + "-" + p;
                                                    subject = person;
                                                    partner = p;
                                                }

                                                if (!pairInteractions.ContainsKey(pair))
                                                    pairInteractions.Add(pair, new PairInfo());

                                                if (!inversePair)
                                                {
                                                    if (!pairInteractions[pair].subjectSet)
                                                    {
                                                        pairInteractions[pair].subjectSet = true;
                                                    }
                                                    else
                                                    {
                                                        pairInteractions[pair].subject.individualTime += 0.1;
                                                    }
                                                }
                                                else
                                                {
                                                    if (!pairInteractions[pair].partnerSet)
                                                    {
                                                        pairInteractions[pair].partnerSet = true;
                                                    }
                                                    else
                                                    {
                                                        pairInteractions[pair].partner.individualTime += 0.1;
                                                    }
                                                }

                                                pairInteractions[pair].sharedTimeInSecs += .1;
                                                double dist = calcSquaredDist(activities[dt][person], activities[dt][p]);
                                                if (dist <= (cf.gOfR * cf.gOfR) && dist >= (cf.gOfRMin * cf.gOfRMin))
                                                {
                                                    pairInteractions[pair].closeTimeInSecs += .1;
                                                    Tuple<double, double> angles = withinOrientationData(activities[dt][p], activities[dt][person]);
                                                    Boolean orientedCloseness = Math.Abs(angles.Item1) <= 45 && Math.Abs(angles.Item2) <= 45;
                                                    if (cf.settings.doAngleFiles)
                                                    {
                                                        sw.WriteLine(person + "," + p + "," + dt.ToLongTimeString() + ",0.1," + (orientedCloseness ? "0.1," : "0,") + (angles.Item1) + "," + (angles.Item2) + "," + activities[dt][person].lx + "," + activities[dt][person].ly + "," + activities[dt][person].rx + "," + activities[dt][person].ry + "," + activities[dt][p].lx + "," + activities[dt][p].ly + "," + activities[dt][p].rx + "," + activities[dt][p].ry);
                                                    }
                                                    if (orientedCloseness)
                                                    {
                                                        pairInteractions[pair].closeAndOrientedTimeInSecs += .1;

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
                                                if (!p.Equals(person) && (subs.Count == 0 || subs.Contains(p)))
                                                {
                                                    double dist = calcSquaredDist(activities[dt][p], activities[dt][person]);
                                                    if (dist <= (cf.gOfR * cf.gOfR) && dist <= (cf.gOfRMin * cf.gOfRMin))
                                                    {

                                                        if (justProx || withinOrientation(activities[dt][p], activities[dt][person], 45))
                                                        {
                                                            String pair = p + "-" + person;
                                                            Boolean inversePair = false;
                                                            if (!pairInteractions.ContainsKey(pair))
                                                            {
                                                                if (pairInteractions.ContainsKey(person + "-" + p))
                                                                {
                                                                    pair = person + "-" + p;
                                                                    inversePair = true;
                                                                }

                                                            }
                                                            if (activities[dt][person].cry > 0 && activities[dt][p].cry > 0)
                                                            {
                                                                if (person.Equals(pair.Split('-')[0]))
                                                                {
                                                                    pairInteractions[pair].closeAndOrientedCryInSecs += (activities[dt][person].cry);
                                                                    pairInteractions[pair].subject.cryingTime += (activities[dt][person].cryingTime);
                                                                    //pairCry[pair] += (activities[dt][person].cry);
                                                                    //pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3 + activities[dt][person].cry, pairStatsSeparated[pair].Item4);// + activities[dt][p].cry);
                                                                }
                                                                else
                                                                {//p is in the first part of the pair
                                                                    pairInteractions[pair].closeAndOrientedCryInSecs += (activities[dt][p].cry);
                                                                    pairInteractions[pair].subject.cryingTime += (activities[dt][p].cryingTime);
                                                                    //pairCry[pair] += (activities[dt][p].cry);
                                                                    //pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4 + activities[dt][p].cry);
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
                                                                        //PairInfo pairInfo = pairStatsSep[pair];
                                                                        if (person.Equals(pair.Split('-')[0]))
                                                                        {

                                                                            /*pairInfo.p1.vd += vd2;
                                                                            pairInfo.p1.vc += vc;
                                                                            pairInfo.p1.tc += tc;
                                                                            pairInfo.p1.ac += a;
                                                                            pairInfo.p1.no += n;
                                                                            pairInfo.p1.oln += o;
                                                                            pairInfo.p1.cry += c;*/
                                                                            pairInteractions[pair].subject.vd += vd2;
                                                                            pairInteractions[pair].subject.vc += vc;
                                                                            pairInteractions[pair].subject.tc += tc;
                                                                            pairInteractions[pair].subject.ac += a;
                                                                            pairInteractions[pair].subject.no += n;
                                                                            pairInteractions[pair].subject.oln += o;
                                                                            pairInteractions[pair].subject.cry += c;
                                                                        }
                                                                        else //p is in the first part of the pair
                                                                        {
                                                                            /*pairInfo.p2.vd += vd2;
                                                                            pairInfo.p2.vc += vc;
                                                                            pairInfo.p2.tc += tc;
                                                                            pairInfo.p2.ac += a;
                                                                            pairInfo.p2.no += n;
                                                                            pairInfo.p2.oln += o;
                                                                            pairInfo.p2.cry += c;*/

                                                                            pairInteractions[pair].partner.vd += vd2;
                                                                            pairInteractions[pair].partner.vc += vc;
                                                                            pairInteractions[pair].partner.tc += tc;
                                                                            pairInteractions[pair].partner.ac += a;
                                                                            pairInteractions[pair].partner.no += n;
                                                                            pairInteractions[pair].partner.oln += o;
                                                                            pairInteractions[pair].partner.cry += c;
                                                                        }///

                                                                    }
                                                                }



                                                            }
                                                            if (wasTalking)
                                                            {
                                                                //pairStats[pair] += 0.1;
                                                                pairInteractions[pair].closeAndOrientedTalkInSecs += 0.1;
                                                                if (person.Equals(pair.Split('-')[0]))
                                                                {
                                                                    pairInteractions[pair].subject.interactionTime += 0.1;
                                                                    //pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1 + 0.1, pairStatsSeparated[pair].Item2, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4);
                                                                }
                                                                else //p is in the first part of the pair
                                                                {
                                                                    pairInteractions[pair].partner.interactionTime += 0.1;
                                                                    //pairStatsSeparated[pair] = new Tuple<double, double, double, double>(pairStatsSeparated[pair].Item1, pairStatsSeparated[pair].Item2 + 0.1, pairStatsSeparated[pair].Item3, pairStatsSeparated[pair].Item4);
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
                        }
                    }
                }
                /*if (writeFile)
                {
                    using (sw = new StreamWriter(cf.version + "interaction_output_" + (trunkDetailFile ? "trunk_" : "") + (cf.justFreePlay ? "freeplay_" : "") + day.Month + "_" + day.Day + "_" + day.Year + ".csv"))
                    {
                        sw.WriteLine("Person 1, Person2, Interaction Time, Total Time, Interaction Normalized");
                        foreach (String s in pairInteractions.Keys)
                        {
                            double interactionTime = Math.Round(pairInteractions[s].closeAndOrientedTimeInSecs, 1);
                            double totalTime = Math.Round(pairInteractions[s].sharedTimeInSecs, 1);
                            double interactionNormalized = interactionTime / totalTime;
                            sw.WriteLine(s.Split('-')[0] + "," + s.Split('-')[1] + "," + interactionTime + "," + totalTime + "," + interactionNormalized);
                        }
                    }
                }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //public void countInteractions(Config cf, ClassroomDay day, bool writeFile, bool writeDetailFile, DateTime trunk, Dictionary<string, List<PersonInfo>> lenaInfo)

        public void addPersonInfo(PersonInfo personInfo, ref PersonInfo target, double div)
        {
            target.vd += (personInfo.vd / div);
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

