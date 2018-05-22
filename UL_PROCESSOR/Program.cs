using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//COMMIT 1 
namespace UL_PROCESSOR
{
    
     
    class UL_PROCESSOR_Program
    {
        public int chunkSize = 4;
        public void processClassroom(String version, String dir, String classroom, String szDates)
        {
            List<DateTime> dateChunks = new List<DateTime>();
            List<DateTime> lastDateChunks = new List<DateTime>();
            String szDates1 = "3/3/2017,3/10/2017,3/17/2017,3/31/2017," +
                "4/7/2017,4/21/2017,4/28/2017," +
                "5/12/2017,5/19/2017,5/26/2017";
            int count = 0;
            int total = 0;
            foreach (String szDay in szDates.Split(','))
            {
                count++;
                total++;
                if (count>chunkSize)
                {
                    //dates.Add(dateChunks);
                    if(lastDateChunks.Count > 0)
                        dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count-1]);
                    Console.WriteLine("PROCESSING LB"+total);
                    UL_CLASS_PROCESSOR_Program ul0 = new UL_CLASS_PROCESSOR_Program(new Config(dir,classroom),
                    dateChunks, version + "_" + total);
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


            Console.WriteLine("PROCESSING LB" + total);
            UL_CLASS_PROCESSOR_Program ull = new UL_CLASS_PROCESSOR_Program(new Config(dir,classroom),
            dateChunks, version + "_" + total);
            ull.process(true);
              
        }
        public void processLadyBugs1(String version)
        {
            List<DateTime> dateChunks = new List<DateTime>();
            List<DateTime> lastDateChunks = new List<DateTime>();
            String szDates = "3/3/2017,3/10/2017,3/17/2017,3/31/2017," +
                "4/7/2017,4/21/2017,4/28/2017," +
                "5/12/2017,5/19/2017,5/26/2017";
            int count = 0;
            int total = 0;
            foreach (String szDay in szDates.Split(','))
            {
                count++;
                total++;
                if (count > chunkSize)
                {
                    //dates.Add(dateChunks);
                    if (lastDateChunks.Count > 0)
                        dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count - 1]);
                    Console.WriteLine("PROCESSING LB" + total);
                    UL_CLASS_PROCESSOR_Program ul0 = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS1"),
                    dateChunks, version + "_" + total);
                    Console.WriteLine("PROCESSING LB1 1");
                    ul0.process(false);
                    lastDateChunks = dateChunks;
                    dateChunks = new List<DateTime>();
                    count = 1;

                }
                dateChunks.Add(Convert.ToDateTime(szDay));


            }
            if (lastDateChunks.Count > 0)
                dateChunks.Insert(0, lastDateChunks[lastDateChunks.Count - 1]);


            Console.WriteLine("PROCESSING LB" + total);
            UL_CLASS_PROCESSOR_Program ull = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS1"),
            dateChunks, version + "_" + total);
            ull.process(true);

        }
        public void processLadyBugs2(String version)
        {

            Console.WriteLine("PROCESSING LB2  o");
            UL_CLASS_PROCESSOR_Program ul0 = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS2"),
            new List<DateTime>() {
                   new DateTime(2017, 10, 24)  
                    ,new DateTime(2017, 11, 3)
            }, 
                   version + "_0");
            Console.WriteLine("PROCESSING LB2 1");

            ul0.process(false);

             Console.WriteLine("PROCESSING LB2");
             UL_CLASS_PROCESSOR_Program ul = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS2"),
             new List<DateTime>() {
                   new DateTime(2017, 10, 24) ,
                   new DateTime(2017, 11, 3),
                   new DateTime(2017, 11, 17),
                   new DateTime(2017, 12, 14) }, version + "_1");
             Console.WriteLine("PROCESSING LB2 1");

             ul.process(false);

             UL_CLASS_PROCESSOR_Program ul2 = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS2"),
             new List<DateTime>() {
                   new DateTime(2017, 12, 14),
                   new DateTime(2018, 1, 11),
                   new DateTime(2018, 2, 2),
                   new DateTime(2018, 2, 16)}, version + "_2");
             Console.WriteLine("PROCESSING LB2 2");

             ul2.process(false);

            UL_CLASS_PROCESSOR_Program ul3 = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "LADYBUGS2"),
            new List<DateTime>() {
                 new DateTime(2018, 2, 16),
                 new DateTime(2018, 3, 13),
                 new DateTime(2018, 3, 20)}, version + "_3");
            Console.WriteLine("PROCESSING LB2 2");
            ul3.process(true);
        }
        public void processPandas(String version)
        {
            Console.WriteLine("PROCESSING PANDAS");
            UL_CLASS_PROCESSOR_Program ul = ul = new UL_CLASS_PROCESSOR_Program(new Config("C://LVL//", "PANDAS"),
               new List<DateTime>() {
                 new DateTime(2018, 2, 1) ,
                 new DateTime(2018, 2, 8),
                 new DateTime(2018, 3, 1)
               }, version);
            Console.WriteLine("PROCESSING PANDAS 1");
            ul.process(true); 
        }

        static void Main(string[] args)
        {
            DateTime n = DateTime.Now;
            String fileNameVersion = "V" + (n.Month > 10 ? n.Month.ToString() : "0" + n.Month.ToString()) +
                                     (n.Day > 10 ? n.Day.ToString() : "0" + n.Day.ToString()) +
                                     n.Year + "_" + new Random().Next();

            if(args.Length>1)
            {
                String dir = args[0];
                int argCount = 0;
                foreach (String arg in args)
                {
                    argCount++;
                    if (argCount > 1)
                    {
                        String[] vars = arg.Split(' ');
                        UL_PROCESSOR_Program pc = new UL_PROCESSOR_Program();
                        pc = new UL_PROCESSOR_Program();
                        pc.processClassroom(fileNameVersion,
                            dir, vars[0].Trim(), vars[1].Trim());
                            
                      /*  switch (vars[0])
                        {
                            case "LADYBUGS1":
                                pc = new UL_PROCESSOR_Program();
                                pc.processClassroom(fileNameVersion,
                                    "C://LVL//", vars[0].Trim(),
                                    "3/3/2017,3/10/2017,3/17/2017,3/31/2017," +
                                    "4/7/2017,4/21/2017,4/28/2017," +
                                    "5/12/2017,5/19/2017,5/26/2017");
                                break;
                            case "LADYBUGS2":
                                pc = new UL_PROCESSOR_Program();
                                pc.processClassroom(fileNameVersion,
                                    "C://LVL//", vars[0].Trim(),
                                    "10/24/2017,11/3/2017,11/17/2017,12/14/2017," +
                                    "1/11/2018,2/2/2018,2/16/2018");
                                break;
                            case "PANDAS":
                                pc = new UL_PROCESSOR_Program();
                                pc.processClassroom(fileNameVersion,
                                    "C://LVL//", vars[0].Trim(),
                                    "2/1/2018,2/8/2018,3/1/2018");
                                break;
                            case "APPLETREE":
                                pc = new UL_PROCESSOR_Program();
                                pc.processClassroom(fileNameVersion,
                                    "C://LVL//", vars[0].Trim(),
                                    "3/3/2017,3/10/2017,3/17/2017,3/31/2017," +
                                    "4/7/2017,4/21/2017,4/28/2017," +
                                    "5/12/2017,5/19/2017,5/26/2017");
                                break;

                        }*/
                    }
                }

            }
            
        }
    }
}
