using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FFTWSharp;
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
        static bool readWav(string filename, out float[] L, out float[] R)
        {
            L = R = null;
            //float [] left = new float[1];

            //float [] right;
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);

                    // chunk 0
                    int chunkID = reader.ReadInt32();
                    int fileSize = reader.ReadInt32();
                    int riffType = reader.ReadInt32();


                    // chunk 1
                    int fmtID = reader.ReadInt32();
                    int fmtSize = reader.ReadInt32(); // bytes for this chunk
                    int fmtCode = reader.ReadInt16();
                    int channels = reader.ReadInt16();
                    int sampleRate = reader.ReadInt32();
                    int byteRate = reader.ReadInt32();
                    int fmtBlockAlign = reader.ReadInt16();
                    int bitDepth = reader.ReadInt16();

                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        int fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    // chunk 2
                    int dataID = reader.ReadInt32();
                    int bytes = reader.ReadInt32();

                    // DATA!
                    byte[] byteArray = reader.ReadBytes(bytes);

                    int bytesForSamp = bitDepth / 8;
                    int samps = bytes / bytesForSamp;


                    float[] asFloat = null;
                    switch (bitDepth)
                    {
                        case 64:
                            double[]
                            asDouble = new double[samps];
                            Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
                            asFloat = Array.ConvertAll(asDouble, e => (float)e);
                            break;
                        case 32:
                            asFloat = new float[samps];
                            Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
                            break;
                        case 16:
                            Int16[]
                            asInt16 = new Int16[samps];
                            Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                            asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                            break;
                        default:
                            return false;
                    }

                    switch (channels)
                    {
                        case 1:
                            L = asFloat;
                            R = null;
                            return true;
                        case 2:
                            L = new float[samps];
                            R = new float[samps];
                            for (int i = 0, s = 0; i < samps; i++)
                            {
                                L[i] = asFloat[s++];
                                R[i] = asFloat[s++];
                            }
                            return true;
                        default:
                            return false;
                    }
                }
            }
            catch
            {
                Console.WriteLine("...Failed to load note: " + filename);
                return false;
                //left = new float[ 1 ]{ 0f };
            }
             
        }

        static void Main(string[] args)
        {
            /*
            float[] LMaster;
            float[] R;
            readWav("C://LENA_New_Audio//20180627_144122_026860.wav", out LMaster, out R);
            float[] L2;
            readWav("C://LENA_New_Audio//20180627_144359_026864.wav", out L2, out R);


            float[] z = new float[LMaster.Length];
            

            if (LMaster.Length>L2.Length)
            {
                float[] newValues = new float[LMaster.Length-L2.Length];
                //newValues[0] = 0x00;                                // set the prepended value
                //Array.Copy(newValues, 0, L2, 0, newValues.Length);
                 newValues.CopyTo(z, 0);
                L2.CopyTo(z, newValues.Length);
            }
            else
            {
                z = L2;
            }
            */
            


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
