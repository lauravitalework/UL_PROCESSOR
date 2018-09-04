﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 
//COMMIT 2
namespace UL_PROCESSOR
{
    
     
    class UL_PROCESSOR_Program
    {
        public int chunkSize = 4;
        public void processClassroom(String version, String dir, Boolean doTenFiles, Boolean doSumDayFiles, Boolean doSumAllFiles, Boolean doAnglesFiles, String classroom, String lenaVersion, String szDates, Boolean fromIts, Boolean doGR, Boolean doVel)
        {
            ClassroomDay.first = true;
            List<DateTime> dateChunks = new List<DateTime>();
            List<DateTime> lastDateChunks = new List<DateTime>();
            List<String> fileNames=new List<string>();
            int count = 0;
            int total = 0;
            foreach (String szDay in szDates.Split(','))
            {
                count++;
                total++;
                if (count>chunkSize)
                {
                    if(lastDateChunks.Count > 0)
                        dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);
                    Console.WriteLine("PROCESSING LB"+total);
                    UL_CLASS_PROCESSOR_Program ul0 = new UL_CLASS_PROCESSOR_Program(new Config(dir,classroom),
                    dateChunks, version + total+"_", doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles,fromIts, doGR, doVel);
                    ul0.configInfo.lenaVersion = lenaVersion;
                    Console.WriteLine("PROCESSING LB1 1");
                    ul0.process(false);
                    lastDateChunks = dateChunks;
                    dateChunks = new List<DateTime>();
                    count = 1;
                    fileNames=fileNames.Concat(ul0.fileNames).ToList();
                }
                dateChunks.Add(Convert.ToDateTime(szDay));

                 
            }
            
            if (lastDateChunks.Count > 0)
                dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);


            UL_CLASS_PROCESSOR_Program ull = new UL_CLASS_PROCESSOR_Program(new Config(dir,classroom),
            dateChunks, version + total+"_", doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles,fromIts, doGR, doVel);
            ull.configInfo.lenaVersion = lenaVersion;
            ull.process(true);
            fileNames = fileNames.Concat(ull.fileNames).ToList(); 
            if(doSumAllFiles)
            MergeCsvs(fileNames, fileNames[fileNames.Count - 1].Replace(".", "ALL."));


        }
        static void MergeCsvs(List<String> file_names, String destinationfilename)
        {
            StreamReader rdr = new StreamReader(file_names[0]);
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
        static void Main(string[] args)
        {

            //PythonRunner pr = new PythonRunner();
            //pr.runScript();
            //ItsReader r = new ItsReader();
            // r.read();
            FileComparer fc = new FileComparer();
            //fc.compareFiles(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
            //    "C://LVL//LADYBUGS1//SYNC//ADEX_07242018_267290963_2_PAIRACTIVITY_ALL_LADYBUGS1_TOTALSALL.CSV", "C://LVL//LADYBUGS1//SYNC//ITS_07242018_900772189_2_PAIRACTIVITY_ALL_LADYBUGS1_TOTALSALL.CSV", false,80,false);

            DateTime n = DateTime.Now;
            String fileNameVersion = (n.Month > 10 ? n.Month.ToString() : "0" + n.Month.ToString()) +
                                     (n.Day > 10 ? n.Day.ToString() : "0" + n.Day.ToString()) +
                                     n.Year + "_" + new Random().Next()+"_";

            if(args.Length>1)
            {
                String[] settings = args[0].Split(' ');
                String dir = settings[0];
                Boolean doTenFiles = true;
                Boolean doSumDayFiles = true;
                Boolean doSumAllFiles = true;
                Boolean doAngleFiles = true; //to implement
                Boolean getFromIts = false;
                Boolean doGR = false;
                Boolean doVel = false;
                for (int a=1;a<settings.Length;a++)
                {
                    String[] setting = settings[a].Split(':');
                    if (setting.Length > 1)
                    {
                        switch (setting[0].Trim())
                        {
                            case "TEN":
                                doTenFiles = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "SUMDAY":
                                doSumDayFiles = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "SUMALL":
                                doSumAllFiles = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "ANGLES":
                                doAngleFiles = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "GR":
                                doGR = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "VEL":
                                doVel = setting[1].Trim().ToUpper() == "YES";
                                break;
                            case "ITS":
                                getFromIts = setting[1].Trim().ToUpper() == "YES";
                                if (getFromIts)
                                    fileNameVersion = "ITS_" + fileNameVersion;
                                else
                                    fileNameVersion = "ADEX_" + fileNameVersion;
                                break;

                        }
                    }
                }
                int argCount = 0;

                foreach (String arg in args)
                {
                    argCount++;
                    if (argCount > 1)
                    {
                         
                        String[] vars = arg.Split(' ');
                        UL_PROCESSOR_Program pc = new UL_PROCESSOR_Program();
                        pc = new UL_PROCESSOR_Program();
                        pc.processClassroom(fileNameVersion, dir, doTenFiles,doSumDayFiles,doSumAllFiles,doAngleFiles, vars[0].Trim(), vars[1].Trim(), vars[2].Trim(), getFromIts, doGR, doVel);
                         
                    }
                }

            }
            Console.ReadLine();
        }

    }
}
