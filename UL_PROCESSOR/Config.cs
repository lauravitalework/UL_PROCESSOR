using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace UL_PROCESSOR
{
    class Config
    {
        public UL_PROCESSOR_SETTINGS settings;
        public UL_PROCESSOR_CLASS_SETTINGS classSettings;

        public Boolean allLena = false;
        public Boolean justUbi = false;//false;//false;//true;
        public Boolean makeRL = false;//true;//false;

        public String szVersion = "V18"; 
        public String root = "C://LVL/";//"D://"
        public String classroom = "APPLETREE"; //"LADYBUGS2";// "PANDAS";//"LADYBUGS1";//"APPLETREE";//"LADYBUGS2";//"DEBBIE";
        public String freePlayTimesFile ="/FREEPLAYTIMES.CSV";
        public String extractTimesFile = "//EXTRACTTIMES.CSV";
        public String mappingFile = "/MAPPING";
        public String baseMappingFile = "/MAPPING";
        public String adjustFile = "/ADJUST";
        public String ubisenseFile = "MiamiChild.";
        public String ubisenseTagsFile = "MiamiLocation";
        public String ubisenseFileDir = "/Ubisense_Data/";// LADYBUG/";
        public String lenaFileDir = "/LENA_Data/";// LADYBUG/";
       // public String lenaITSDir = "/LENA_Data/";// LADYBUG/";
        
        public String lenaFile = "/LENA_Data/ACTIVITYBLOCKS/LENAACTIVITYBLOCKALL.csv";
        public String syncFilePre = "/SYNC/MERGED";
        public String lenaVersion = "PRO";
        public Boolean justFreePlay = false;//false;//true;
        public String lenaDateType = "utc";//"est"
        public DateTime from = new DateTime(2017, 3, 31);
        public DateTime to = new DateTime(2017, 4, 1);

        public int mappingBIdCol = 3;
        public int mappingUbiIdCol = 4;
        public int mappingLeftTagCol = 5;
        public int mappingRightTagCol =7;
        public int mappingLenaIdCol = 9;
        public int mappingStartCol = 11;
        public int mappingExpiredCol = 12;
        public int mappingAbsentCol = 13;
        public int mappingAidCol = 14;
        public int mappingSexCol = 15;
        public int mappingDobCol = 16;
        public int mappingTypeCol = 17;
        public int mappingLongBIdCol = 18;
        public int mappingShortBIdCol = 18;
        public int mappingAbsentPresentCol = 19;
        public int mappingLangCol = 20;

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
        //public int ubiFilePosCol = 3;
        public int ubiFileXPosCol = 3;
        public int ubiFileYPosCol = 4;
        public int ubiFileZPosCol = 5;
        public int ubiFileOriPosCol = 6;

        //public MappingRows mapRows = new MappingRows();
        public Dictionary<String, List<MappingRow>> mapRows = new Dictionary<string, List<MappingRow>>();
        public Dictionary<String, List<MappingRow>> mapRowsUbiL = new Dictionary<string, List<MappingRow>>();
        public Dictionary<String, List<MappingRow>> mapRowsUbiR = new Dictionary<string, List<MappingRow>>();
        public Dictionary<String, List<MappingRow>> mapRowsUbi = new Dictionary<string, List<MappingRow>>();
        public Dictionary<String, List<MappingRow>> mapRowsLena = new Dictionary<string, List<MappingRow>>();

        public List<String> bids = new List<string>();
        public List<String> pairs = new List<string>();

        public Dictionary<String, List<String>> freePlayTimes = new Dictionary<String, List<String>>();
        public Dictionary<String, List<String>> extractTimes = new Dictionary<String, List<String>>();
        public Dictionary<String, Boolean> tagTest = new Dictionary<string, bool>();//test delete
        public double gOfR = 1.5;
        public double gOfRMin = 0;
        //public Boolean mappingSet = false;
        public Config(UL_PROCESSOR_SETTINGS s, UL_PROCESSOR_CLASS_SETTINGS cs)
        {
            settings = s;
            classSettings = cs;
            gOfR = cs.gOfR;
            gOfRMin = cs.gOfRMin;
            classroom = cs.classroom;
            root = s.dir;
            freePlayTimesFile = root + classroom + freePlayTimesFile;
            extractTimesFile = root + classroom + extractTimesFile;
            mappingFile = root + classroom + mappingFile + "_" + classroom + ".CSV";
            adjustFile = root + classroom + adjustFile + "_" + classroom + ".csv";
            ubisenseFileDir = root + classroom + ubisenseFileDir;
            lenaFileDir = root + classroom + lenaFileDir;
            lenaFile = root + classroom + lenaFile;
            syncFilePre = root + classroom + syncFilePre;
            lenaVersion = cs.lenaVersion;
        }



        /*public Config(String r, String c)
        {
            classroom = c;
            root = r;
            freePlayTimesFile = root + classroom + freePlayTimesFile;
            extractTimesFile = root + classroom + extractTimesFile;
            mappingFile = root + classroom + mappingFile+"_"+classroom+".CSV";
            adjustFile = root + classroom + adjustFile + "_" + classroom + ".csv";
            ubisenseFileDir = root + classroom + ubisenseFileDir;
            lenaFileDir = root + classroom + lenaFileDir;
            lenaFile = root + classroom + lenaFile;
            syncFilePre = root + classroom + syncFilePre;
        }
        public Config()
        {
            freePlayTimesFile = root + classroom + freePlayTimesFile;
            extractTimesFile = root + classroom + extractTimesFile;
            mappingFile = root + classroom +"_" + classroom + ".CSV";
            adjustFile = root + classroom + adjustFile + "_" + classroom + ".csv";
            ubisenseFileDir = root + classroom + ubisenseFileDir;
            lenaFileDir = root + classroom + lenaFileDir;
            lenaFile = root + classroom + lenaFile;
            syncFilePre = root + classroom + syncFilePre;
        }*/
        public void clearMappings()
        {
            mapRows = new Dictionary<string, List<MappingRow>>();
            mapRowsUbiL = new Dictionary<string, List<MappingRow>>();
            mapRowsUbiR = new Dictionary<string, List<MappingRow>>();
            mapRowsUbi = new Dictionary<string, List<MappingRow>>();
            mapRowsLena = new Dictionary<string, List<MappingRow>>();

            bids = new List<string>();
            pairs = new List<string>();

            freePlayTimes = new Dictionary<String, List<String>>();
            extractTimes = new Dictionary<String, List<String>>();
        }
        public void readDayMappings(DateTime dt)
        {
            String dayFolder = (dt.Month < 10 ? "0" + dt.Month : dt.Month.ToString()) + "-" + (dt.Day < 10 ? "0" + dt.Day : dt.Day.ToString()) + "-" + dt.Year.ToString();
            baseMappingFile = root + classroom + "//MAPPING" + "_" + classroom + "_BASE.CSV";
            readMappingFile(root + classroom + "//" + dayFolder+"//MAPPINGS//MAPPING_"+classroom+".CSV", dt, dt.AddDays(1));
            //mappingFile = root + classroom + mappingFile + "_" + classroom + ".CSV";
            ubisenseFileDir = root + classroom + "/" + dayFolder + "/Ubisense_Data/";
            lenaFileDir = root + classroom + "/" + dayFolder + "/LENA_Data/";
            //mappingFile = root + classroom + "/" + dayFolder + "/MAPPING" + "_" + classroom + ".CSV";
           
        }
      

        public void readClassroomMappings()
        {
            baseMappingFile = root + classroom + "//MAPPING" + "_" + classroom + "_BASE.CSV";
            readMappingFile(mappingFile,DateTime.Now, DateTime.Now);
        }
        //Convert.ToDateTime(line[mappingExpiredCol]);
        //mr.Start = Convert.ToDateTime(line[mappingStartCol])
        //mappingFile = root + classroom + mappingFile + "_" + classroom + ".CSV";
        Boolean mappingStarted = false;

        public Dictionary<String, int> diagnosis = new Dictionary<string, int>();
        public Dictionary<String, int> languages = new Dictionary<string, int>();

        public void readMappingFile(String thisMappingFile, DateTime dt, DateTime dt2)
        {
            /******
             * CASE 1) CLASS FILE. START EXP ABS
             * CASE 2) DAY FILE. ----- --- ABS
             * CASE 3) DAY FILE. ----- --- ---
             * ****/
            //
            diagnosis = new Dictionary<string, int>();
            languages = new Dictionary<string, int>();

            try
            {
                Dictionary<String, MappingRow> baseMappings = new Dictionary<string, MappingRow>();
                    if (File.Exists(baseMappingFile))
                        using (StreamReader sr = new StreamReader(baseMappingFile))
                        {
                            String[] headers = sr.ReadLine().Split(',');
                        //diagnosis.Add(headers[mappingAidCol].Trim(), mappingAidCol);
                        //languages.Add(headers[mappingLangCol].Trim(), mappingLangCol);
                        Boolean rosterHeaderNotLnFn = !(headers[0].Contains("LN") || headers[0].Contains("FN"));
                            if (headers.Length >= (rosterHeaderNotLnFn?21:22))
                            {
                                for(int h=0;h<headers.Length && headers[h].Trim()!="";h++)
                                {
                                    if(headers[h].Trim().ToUpper().Contains("DIAGNOSIS") || headers[h].Trim().ToUpper().Contains("AID"))
                                    {
                                        diagnosis.Add(headers[h].Trim(), (rosterHeaderNotLnFn ? h+1 : h));
                                    }
                                    if (headers[h].Trim().ToUpper().Contains("LANGUAGE"))
                                    {
                                        languages.Add(headers[h].Trim(), (rosterHeaderNotLnFn ? h + 1 : h));
                                    }
                            }
                            }
                            while (!sr.EndOfStream)
                            {
                                try
                                {
                                    String[] line = sr.ReadLine().Split(',');

                                    if (line.Length > 8  )
                                    {
                                        MappingRow mr = new MappingRow();
                                        mr.BID = line[mappingBIdCol].Trim();
                                        mr.shortBID = line[mappingShortBIdCol].Trim() != "" ? line[mappingShortBIdCol].Trim() : mr.BID;
                                        //mr.lang = headers[mappingLangCol - 1].ToLower().Contains("language") ? line[mappingLangCol] : "";
                                        mr.sex = line[mappingSexCol].Trim();
                                        //mr.aid = line[mappingAidCol].Trim();
                                        mr.type = line[mappingTypeCol].Trim();
                                        mr.dob = line[mappingDobCol].Trim();

                                    if (diagnosis.Count >= 1)
                                    {
                                        int d = 0;
                                        foreach (String key in diagnosis.Keys)
                                        {
                                            if (d > 0)
                                                mr.aid += "|" + line[diagnosis[key]].Trim().Replace("|","-");
                                            else
                                                mr.aid += line[diagnosis[key]].Trim().Replace("|", "-");
                                            d++;
                                        }
                                    }
                                    if (languages.Count >= 1)
                                    {
                                       int  d = 0;
                                            foreach (String key in languages.Keys)
                                            {
                                                if (d > 0)
                                                    mr.lang += "|" + line[languages[key]].Trim().Replace("|", "-");
                                            else
                                                mr.lang += line[languages[key]].Trim().Replace("|", "-");
                                            d++;
                                            }
                                        }
                                        if (!baseMappings.ContainsKey(mr.BID))
                                        {
                                            baseMappings.Add(mr.BID, mr);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }

                            }

                        }




                    using (StreamReader sr = new StreamReader(thisMappingFile))
                {
                    String[] headers = sr.ReadLine().Split(',');
                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            String[] line = sr.ReadLine().Split(',');
                             
                            if (line.Length > 8 && (line[mappingUbiIdCol].Trim() != "" && line[mappingLenaIdCol].Trim() != ""))
                            {
                                //if (line.Length <= 18 ||
                                //    (line.Length > 18 && line[mappingUbiIdCol].Trim().ToUpper() != "ABSENT"))
                                //{
                                Boolean absent = classSettings.mappingBy=="DAY" && line.Length > 18 && line[mappingAbsentPresentCol].Trim().ToUpper() == "ABSENT";
                                //if(!absent)
                                { 
                                    MappingRow mr = new MappingRow();
                                    mr.LenaId = line[mappingLenaIdCol].Trim();
                                    
                                    mr.UbiID = line[mappingUbiIdCol].Trim();
                                    mr.BID = line[mappingBIdCol].Trim();
                                    mr.longBID = mappingLongBIdCol<line.Length && line[mappingLongBIdCol].Trim()!=""? line[mappingLongBIdCol].Trim():mr.BID;
                                    mr.lang = headers.Length>= mappingLangCol? headers[mappingLangCol - 1].ToLower().Contains("language") ? line[mappingLangCol]: "":"";
                                    mr.Expiration = this.classSettings.mappingBy == "CLASS" ? Convert.ToDateTime(line[mappingExpiredCol]) : absent?new DateTime(1900, 1,1):dt2;
                                    mr.Start = this.classSettings.mappingBy == "CLASS" ? Convert.ToDateTime(line[mappingStartCol]) : absent ? new DateTime(1900,1, 2) : dt;


                                    //logic for 11/24 pilot data
                                    if ((this.classSettings.mappingBy != "CLASS") &&
                                     dt >= new DateTime(2020, 12, 24) &&
                                     (line[mappingStartCol].Trim() != "" && line[mappingExpiredCol].Trim() != ""))
                                    {
                                        DateTime mrStart = Convert.ToDateTime(line[mappingStartCol].Trim());
                                        mr.Start = new DateTime(dt.Year, dt.Month, dt.Day, mrStart.Hour, mrStart.Minute, mrStart.Second);
                                        DateTime mrEnd = Convert.ToDateTime(line[mappingExpiredCol].Trim());
                                        mr.Expiration = new DateTime(dt.Year, dt.Month, dt.Day, mrEnd.Hour, mrEnd.Minute, mrEnd.Second);
                                    }
                                    else if ( (this.classSettings.mappingBy != "CLASS" ) &&
                                        dt >= new  DateTime(2020,11,24) &&
                                        (line[mappingStartCol].Trim()!=""&& line[mappingExpiredCol].Trim() != ""))
                                        {
                                            DateTime mrStart = Convert.ToDateTime(line[mappingStartCol].Trim());
                                            mr.Start = new DateTime(     2020, 11, 24, mrStart.Hour, mrStart.Minute, mrStart.Second);
                                            DateTime mrEnd = Convert.ToDateTime(line[mappingExpiredCol].Trim());
                                            mr.Expiration = new DateTime(2020, 11, 24, mrEnd.Hour, mrEnd.Minute, mrEnd.Second);
                                        } 

                                    mr.absences = getAbsentDays(line[mappingAbsentCol]);
                                    if (absent)
                                        mr.absences.Add(dt);
                                    mr.aid = line[mappingAidCol].Trim();
                                    mr.sex = line[mappingSexCol].Trim();
                                    mr.leftTag = line[mappingLeftTagCol].Trim();
                                    mr.rightTag = line[mappingRightTagCol].Trim();
                                    //tagTest.Add(mr.leftTag, false);
                                    //tagTest.Add(mr.rightTag, false);
                                    mr.sex = line[mappingSexCol].Trim();
                                    mr.aid = line[mappingAidCol].Trim();
                                    mr.type = line[mappingTypeCol].Trim();


                                    if (baseMappings.ContainsKey(mr.BID) || baseMappings.ContainsKey(mr.longBID))
                                    {
                                        String keyId = baseMappings.ContainsKey(mr.BID) ? mr.BID : mr.longBID;
                                        mr.BID = keyId;
                                        mr.shortBID = baseMappings[keyId].shortBID;
                                        mr.lang = baseMappings[keyId].lang;
                                        mr.aid = baseMappings[keyId].aid;
                                        mr.sex = baseMappings[keyId].sex;
                                        mr.aid = baseMappings[keyId].aid;
                                        mr.type = baseMappings[keyId].type;
                                        mr.dob = baseMappings[keyId].dob;
                                    }
                                    if (!bids.Contains(mr.BID))
                                        bids.Add(mr.BID);
                                    if (!mapRowsUbi.ContainsKey(mr.UbiID))
                                    {
                                        mapRowsUbi.Add(mr.UbiID, new List<MappingRow>());
                                    }
                                    if (!mapRowsLena.ContainsKey(mr.LenaId))
                                    {
                                        mapRowsLena.Add(mr.LenaId, new List<MappingRow>());
                                    }
                                    if (!mapRowsUbiL.ContainsKey(mr.leftTag))
                                    {
                                        mapRowsUbiL.Add(mr.leftTag, new List<MappingRow>());
                                    }
                                    if (!mapRowsUbiR.ContainsKey(mr.rightTag))
                                    {
                                        mapRowsUbiR.Add(mr.rightTag, new List<MappingRow>());
                                    }
                                    if (!mapRows.ContainsKey(mr.BID))
                                    {
                                        mapRows.Add(mr.BID, new List<MappingRow>());
                                    }
                                    mapRowsUbi[mr.UbiID].Add(mr);
                                    mapRowsUbiL[mr.leftTag].Add(mr);
                                    mapRowsUbiR[mr.rightTag].Add(mr);
                                    mapRowsLena[mr.LenaId].Add(mr);
                                    mapRows[mr.BID].Add(mr);

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }

                }

                bids.Sort();
                for (int b = 0; b < bids.Count; b++)
                {
                    String bid1 = bids[b];
                    for (int b2 = b + 1; b2 < bids.Count; b2++)
                    {
                        String bid2 = bids[b2];
                        if ((!pairs.Contains(bid1 + "-" + bid2)) && (!pairs.Contains(bid2 + "-" + bid1)))
                            pairs.Add(bid1 + "-" + bid2);
                    }
                }
                if (!mappingStarted)
                {
                    


                    if (File.Exists(freePlayTimesFile))
                        using (StreamReader sr = new StreamReader(freePlayTimesFile))
                        {
                            sr.ReadLine();
                            while (!sr.EndOfStream)
                            {
                                try
                                {
                                    String[] line = sr.ReadLine().Split(',');
                                    if (line.Length > 2 && line[0].Trim()!="")
                                    {
                                        String date = Convert.ToDateTime(line[0]).ToShortDateString().Trim();
                                        foreach (String timeFrame in line[1].Split('|'))
                                        {
                                            String[] times = timeFrame.Split('-');
                                            if (times.Length == 2)
                                            {
                                                List<String> timeFrames = new List<string>();

                                                if (!freePlayTimes.ContainsKey(date))
                                                {
                                                    timeFrames.Add(timeFrame);
                                                    freePlayTimes.Add(date, timeFrames);
                                                }
                                                else
                                                {
                                                    freePlayTimes[date].Add(timeFrame);
                                                }
                                            }

                                        }

                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("freePlayTimesFile "+e.Message);
                                }

                            }
                        }
                    if (File.Exists(extractTimesFile))
                        using (StreamReader sr = new StreamReader(extractTimesFile))
                        {
                            sr.ReadLine();
                            while (!sr.EndOfStream)
                            {
                                try
                                {
                                    String[] line = sr.ReadLine().Split(',');
                                    if (line.Length > 2)
                                    {
                                        String date = Convert.ToDateTime(line[0]).ToShortDateString().Trim();
                                        foreach (String timeFrame in line[1].Split('|'))
                                        {
                                            String[] times = timeFrame.Split('-');
                                            if (times.Length == 2)
                                            {
                                                List<String> timeFrames = new List<string>();

                                                if (!extractTimes.ContainsKey(date))
                                                {
                                                    timeFrames.Add(timeFrame);
                                                    extractTimes.Add(date, timeFrames);
                                                }
                                                else
                                                {
                                                    extractTimes[date].Add(timeFrame);
                                                }
                                            }

                                        }

                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("extractTimesFile "+e.Message);
                                }

                            }
                        }
                    getAdjustedTimes();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            mappingStarted = true;
        }
        public void setMapRowBase(ref MappingRow mr, String[] line, String[] headers)
        {
           mr.BID = line[mappingBIdCol].Trim();
            mr.shortBID = line[mappingShortBIdCol].Trim() != "" ? line[mappingShortBIdCol].Trim() : mr.BID;
            mr.lang = headers[mappingLangCol - 1].ToLower().Contains("language") ? line[mappingLangCol] : "";
            mr.aid = line[mappingAidCol].Trim();
            mr.sex = line[mappingSexCol].Trim();
            mr.leftTag = line[mappingLeftTagCol].Trim();
            mr.rightTag = line[mappingRightTagCol].Trim();
            mr.sex = line[mappingSexCol].Trim();
            mr.aid = line[mappingAidCol].Trim();
            mr.type = line[mappingTypeCol].Trim();

        }
        public Dictionary<DateTime, Dictionary<String, double>> adjustedTimes = new Dictionary<DateTime, Dictionary<string, double>>();
        public void getAdjustedTimes()
        {
            if (File.Exists(adjustFile))
            {
                using (StreamReader sr = new StreamReader(adjustFile))
                {
                    sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            String[] line = sr.ReadLine().Split(',');
                            if (line.Length > 5)
                            {
                                String lenaId = line[0].Substring(17, 6);
                                if (lenaId.Substring(0, 2) == "00")
                                lenaId = lenaId.Substring(2);
                                else if (lenaId.Substring(0, 1) == "0")
                                lenaId = lenaId.Substring(1);

                                DateTime rowDay = Convert.ToDateTime(line[1]);
                                double adjSecs = Convert.ToDouble(line[4]);
                                if (line.Length >=7 && line[6].Trim()!="")
                                {
                                    double ubiSecs = 0;
                                    if(Double.TryParse(line[6], out ubiSecs))
                                    adjSecs += ubiSecs;
                                }
                                if (!adjustedTimes.ContainsKey(rowDay))
                                {
                                    adjustedTimes.Add(rowDay, new Dictionary<string, double>());
                                }
                                adjustedTimes[rowDay].Add(lenaId, adjSecs);
                            }
                        }
                        catch(Exception  e)
                        {

                        }
                    }
                }
            }
            
        }
        public MappingRow getBaseMapping(String bid, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if(mapRows.ContainsKey(bid))
            {
                mr = mapRows[bid][0];
            }
            return mr;
        }
        public MappingRow getMapping(String bid, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if (mapRows.ContainsKey(bid))
            {
                foreach (MappingRow r in mapRows[bid])
                {
                    if (classSettings.mappingBy != "CLASS")
                    {
                       if (d.Year==r.Start.Year &&
                            d.Month == r.Start.Month &&
                            d.Day == r.Start.Day  )
                        {
                            mr = r;
                            break; 
                        }
                    }
                    else
                    {
                        if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                        {
                            if (!r.absences.Contains(new DateTime(d.Year, d.Month, d.Day)))
                            {
                                mr = r;
                                break;
                            }
                        }
                    }
                    

                     

                }
            }
            return mr;
        }
        public MappingRow getUbiMapping(String ubiId, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if (mapRowsUbi.ContainsKey(ubiId))
            {
                foreach (MappingRow r in mapRowsUbi[ubiId])
                {
                    if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                    {
                        if ((classSettings.mappingBy != "CLASS" || (!r.absences.Contains(new DateTime(d.Year, d.Month, d.Day)))))
                        {
                            mr = r;
                            break;
                        }
                    }

                }
            }
            return mr;
        }
        
        public MappingRow getUbiMappingL(String ubiId, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if (mapRowsUbiL.ContainsKey(ubiId))
            {
                foreach (MappingRow r in mapRowsUbiL[ubiId])
                {
                    if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                    {
                        if ((classSettings.mappingBy != "CLASS" || (!r.absences.Contains(new DateTime(d.Year, d.Month, d.Day)))))
                        {
                            mr = r;
                            break;
                        }
                    }

                }
            }
            return mr;
        }
        public MappingRow getUbiMappingR(String ubiId, DateTime d)
        {
            MappingRow mr = new MappingRow(); 
            if (mapRowsUbiR.ContainsKey(ubiId))
            {
                foreach (MappingRow r in mapRowsUbiR[ubiId])
                {
                    if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                    {
                        if ((classSettings.mappingBy != "CLASS" || (!r.absences.Contains(new DateTime(d.Year, d.Month, d.Day)))))
                        {
                            mr = r;
                            break;
                        }
                    }

                }
            }
            return mr;
        }
        public MappingRow getLenaMapping(String lenaId, DateTime d)
        {
            MappingRow mr = new MappingRow(); 
            if (lenaId == "11565")
            {
                Boolean stop = true;
            }
            if (mapRowsLena.ContainsKey(lenaId))
            {
                foreach (MappingRow r in mapRowsLena[lenaId])
                {
                    if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0 )
                    {
                        if((classSettings.mappingBy != "CLASS" || ( !r.absences.Contains(new DateTime(d.Year, d.Month, d.Day)) )))
                        {
                            mr = r;
                            break;
                        } 
                        
                    }

                }
            }
            return mr;
        }
        public List<DateTime> getAbsentDays(String absentDays)
        {
            List<DateTime> absentDaysArr = new List<DateTime>();
            String[] days = days = absentDays.Split('|');
            foreach (String day in days)
            {
                if (day.Trim() != "")
                    absentDaysArr.Add(Convert.ToDateTime(day));
            }

            return absentDaysArr;

        }
        public static DateTime getMsTime(DateTime first)
        {
            //int ms = t.Millisecond > 0 ? t.Millisecond / 100 * 100 : t.Millisecond;// + 100;
            //return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, ms);

            //targets will begin at closest 100 ms multiple of start
            int ms = first.Millisecond / 100 * 100 + 100;
            if (first.Millisecond % 100 == 0)
            {
                ms -= 100;
            }
            DateTime target = new DateTime();//will be next .1 sec
            if (ms == 1000)
            {
                if (first.Second <59)
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, first.Second + 1, 0);
                }
                else if (first.Minute < 59)
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute + 1, 0, 0);
                }
                else
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour + 1, 0, 0, 0);
                }
            }
            else
            {
                target = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, first.Second, ms);
            }
            /*if (ms == 1000)
            {
                if (first.Minute == 59)
                {
                    target = new DateTime(first.Year, first.Month, first.Day, first.Hour+1, 0, 0, 0);
                }
                else if (first.Second == 59)
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
            }*/
            return target;
        }
        public static DateTime geFullTime(DateTime first)
        {
            int ms = first.Millisecond;
            return new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, first.Second, ms);
           
        }
        public static String getDateTimeStr(DateTime t)
        {
            return t.Month + "/" + t.Date + "/" + t.Year + "/" + " " + getTimeStr( t);
        }
        public static String getDayStr(DateTime t)
        {
            return  (t.Month < 10 ? "0" + t.Month : t.Month.ToString()) +
                   t.Day+//(t.Day < 10 ? "0" + t.Day : t.Day.ToString()) +
                    t.Year.ToString().Substring(2);
        }
        public static String getDayDashStr(DateTime t)
        {
            return (t.Month < 10 ? "0" + t.Month : t.Month.ToString()) +"-"+
                   (t.Day < 10 ? "0" + t.Day : t.Day.ToString()) + "-" +
                    t.Year.ToString();
        }
        public static String getTimeStr(DateTime t)
        {
            return t.Hour + ":" + (t.Minute<10?"0"+ t.Minute: t.Minute.ToString()) + ":" +
                (t.Second < 10 ? "0" + t.Second : t.Second.ToString())  + "." +
                (t.Millisecond < 10 ? "00" + t.Millisecond : t.Millisecond < 100 ? "0" + t.Millisecond: t.Millisecond.ToString()) ;
        }

    }
}
