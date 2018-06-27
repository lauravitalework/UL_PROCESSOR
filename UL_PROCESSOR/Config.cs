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
        public String root = "C://LVL/";//"D://"
        public String classroom = "APPLETREE"; //"LADYBUGS2";// "PANDAS";//"LADYBUGS1";//"APPLETREE";//"LADYBUGS2";//"DEBBIE";
        public String freePlayTimesFile ="/FREEPLAYTIMES.CSV";
        public String mappingFile = "/MAPPING";
        public String adjustFile = "/ADJUST";
        public String ubisenseFile = "MiamiChild.";
        public String ubisenseTagsFile = "MiamiLocation.";
        public String ubisenseFileDir = "/Ubisense_Data/FROMSUPER/";// LADYBUG/";
        public String lenaFileDir = "/LENA_Data/";// LADYBUG/";
        public String lenaFile = "/LENA_Data/ACTIVITYBLOCKS/LENAACTIVITYBLOCKALL.csv";
        public String syncFilePre = "/SYNC/MERGED";

        public Boolean justFreePlay = false;//true;
        public DateTime from = new DateTime(2017, 3, 31);
        public DateTime to = new DateTime(2017, 4, 1);

        public int mappingBIdCol = 3;
        public int mappingUbiIdCol = 4;
        public int mappingLenaIdCol = 9;
        public int mappingStartCol = 11;
        public int mappingExpiredCol = 12;
        public int mappingAbsentCol = 13;
        public int mappingAidCol = 14;
        public int mappingSexCol = 15;
        public int mappingTypeCol = 17;


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
        public Dictionary<String, Boolean> tagTest = new Dictionary<string, bool>();//test delete

        public Config(String r, String c)
        {
            classroom = c;
            root = r;
            freePlayTimesFile = root + classroom + freePlayTimesFile;
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
            mappingFile = root + classroom +"_" + classroom + ".CSV";
            adjustFile = root + classroom + adjustFile + "_" + classroom + ".csv";
            ubisenseFileDir = root + classroom + ubisenseFileDir;
            lenaFileDir = root + classroom + lenaFileDir;
            lenaFile = root + classroom + lenaFile;
            syncFilePre = root + classroom + syncFilePre;
        }
          
        public void readMappings()
        {
            using (StreamReader sr = new StreamReader(mappingFile))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    try
                    {
                        String[] line = sr.ReadLine().Split(',');
                        if (line[7].Trim() == "00:11:CE:00:00:00:02:BE")
                        {

                            bool stop = true;
                        }
                        if (line.Length > 8 && (line[mappingUbiIdCol].Trim() != "" && line[mappingLenaIdCol].Trim() != ""))
                        {
                            MappingRow mr = new MappingRow();
                            //4 6
                            mr.LenaId = line[mappingLenaIdCol].Trim();
                            mr.UbiID = line[mappingUbiIdCol].Trim();
                            mr.BID = line[mappingBIdCol].Trim();
                            if (!bids.Contains(mr.BID))
                                bids.Add(mr.BID);
                            mr.Expiration = Convert.ToDateTime(line[mappingExpiredCol]);
                            mr.Start = Convert.ToDateTime(line[mappingStartCol]);
                            mr.absences = getAbsentDays(line[mappingAbsentCol]);
                            mr.aid = line[mappingAidCol].Trim();
                            mr.sex = line[mappingSexCol].Trim();
                            mr.leftTag = line[5].Trim();
                            mr.rightTag = line[7].Trim();
                            //tagTest.Add(mr.leftTag, false);
                            //tagTest.Add(mr.rightTag, false);
                            mr.sex = line[mappingSexCol].Trim();
                            mr.aid = line[mappingAidCol].Trim();
                            mr.type = line[mappingTypeCol].Trim();

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
                    catch (Exception e)
                    { }

                }
            }

            for (int b = 0; b<bids.Count; b++)
            {
                String bid1 = bids[b];
                for (int b2 = b+1; b2 < bids.Count; b2++)
                {
                    String bid2 = bids[b2];
                    pairs.Add(bid1 + "-" + bid2);
                }
            }
            using (StreamReader sr = new StreamReader(freePlayTimesFile))
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
                    { }

                }
            }
            getAdjustedTimes();
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
                                if(!adjustedTimes.ContainsKey(rowDay))
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
        public MappingRow getMapping(String bid, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if(mapRows.ContainsKey(bid))
            {
                foreach(MappingRow r in mapRows[bid])
                {
                    if(d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                    {
                        mr = r;
                        break;
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
                        mr = r;
                        break;
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
                        mr = r;
                        break;
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
                        mr = r;
                        break;
                    }

                }
            }
            return mr;
        }
        public MappingRow getLenaMapping(String lenaId, DateTime d)
        {
            MappingRow mr = new MappingRow();
            if (mapRowsLena.ContainsKey(lenaId))
            {
                foreach (MappingRow r in mapRowsLena[lenaId])
                {
                    if (d.CompareTo(r.Expiration) < 0 && d.CompareTo(r.Start) >= 0)
                    {
                        mr = r;
                        break;
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
    }
}
