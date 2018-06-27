using System;
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
        public void processClassroom(String version, String dir, Boolean doTenFiles, Boolean doSumDayFiles, Boolean doSumAllFiles, Boolean doAnglesFiles, String classroom, String szDates)
        {
            List<DateTime> dateChunks = new List<DateTime>();
            List<DateTime> lastDateChunks = new List<DateTime>();
            
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
                    dateChunks, version + "_" + total, doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles);
                    Console.WriteLine("PROCESSING LB1 1");
                    ul0.process(false);
                    lastDateChunks = dateChunks;
                    dateChunks = new List<DateTime>();
                    count = 1;
                     
                }
                dateChunks.Add(Convert.ToDateTime(szDay));

                 
            }
            
            if (lastDateChunks.Count > 0)
                dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);


            UL_CLASS_PROCESSOR_Program ull = new UL_CLASS_PROCESSOR_Program(new Config(dir,classroom),
            dateChunks, version + "_" + total, doTenFiles, doSumDayFiles, doSumAllFiles, doAnglesFiles);
            ull.process(true);
              
        }
        
        static void Main(string[] args)
        {
            

            ItsReader r = new ItsReader();
           // r.read();


            DateTime n = DateTime.Now;
            String fileNameVersion = "ADJUSTEDLENATIME_" + (n.Month > 10 ? n.Month.ToString() : "0" + n.Month.ToString()) +
                                     (n.Day > 10 ? n.Day.ToString() : "0" + n.Day.ToString()) +
                                     n.Year + "_" + new Random().Next();

            if(args.Length>1)
            {
                String[] settings = args[0].Split(' ');
                String dir = settings[0];
                Boolean doTenFiles = true;
                Boolean doSumDayFiles = true;
                Boolean doSumAllFiles = true;
                Boolean doAngleFiles = true; //to implement

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
                        pc.processClassroom(fileNameVersion, dir, doTenFiles,doSumDayFiles,doSumAllFiles,doAngleFiles, vars[0].Trim(), vars[1].Trim());
                         
                    }
                }

            }
            Console.ReadLine();
        }

    }
}
