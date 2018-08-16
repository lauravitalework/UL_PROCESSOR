using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace UL_PROCESSOR
{
    class FileComparer
    {
        public Boolean compareFiles(int[] ids, String f1, String f2, Boolean stop, int stopCol, Boolean excludeNA)
        {
            Boolean flagPass = true;
            Dictionary<String, String> d1 ;
            Dictionary<String, String> d2 ;
            if(stop)
            {
                d1 = getFile(ids, f1, stopCol);
                d2=getFile(ids, f2, stopCol);
            }
            else
            {
                d1 = getFile(ids, f1);
                d2 = getFile(ids, f2);
            }
            Dictionary<String, String> difs = new Dictionary<string, string>();


            foreach (String key in d1.Keys)
            {
                String l1 = d1[key];
                String l2 = "";
                if(d2.ContainsKey(key))
                {
                    l2 = d2[key];
                    if(!compareLines(l1,l2, excludeNA))
                    {
                        difs.Add(key, l1 + "|"+l2);
                        Console.WriteLine("LINES DIFFER: " + key + " (" + l1+"|"+l2 + ")");
                    }
                    else
                    {
                        //Console.WriteLine("LINES MATCH: " + key + " (" + l1 + "|" + l2 + ")");
                    }
                }
                else
                {
                    difs.Add(key, l1+"|");
                    Console.WriteLine("LINE NOT FOUND IN F2: " + key + " (" + l1 + ")");
                }
            }


            return flagPass;
 
        }

        public Boolean compareLines(String l1, String l2, Boolean excludeNA)
        {

            Boolean match = false;

            String[] v1 = l1.Split(',');
            String[] v2 = l1.Split(',');
            int c = 0;
            foreach (String val in v1)
            {
                try
                {
                    double n1 = 0;
                    string val2 = v2[c];
                    if (val.Trim() == "NA" || val2.Trim() == "NA")
                    {
                        match = true;
                    }
                    else
                    if (double.TryParse(val, out n1))
                    {
                        double n2 = 0;
                        if (double.TryParse(val2, out n2))
                        {
                            match = Math.Round(n1, 0) == Math.Round(n2, 0);
                        }
                    }
                }
                catch (Exception e)
                {

                }
                c++;
            }
            return match;
        }
        public Dictionary<String, String> getFile(int[] ids, String f1, int stopCol)
        {
            Dictionary<String, String> res = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(f1))
            {
                sr.ReadLine();
                int maxIdx = ids[ids.Length - 1];
                while (!sr.EndOfStream)
                {
                    try
                    {
                        String szline = sr.ReadLine().Trim();
                        String szlineRes = sr.ReadLine().Trim();
                        String[] line = szline.Split(',');
                        if (line.Length >= maxIdx)
                        {
                            String key = "";
                            for (int i = 0; i < ids.Length; i++)
                            {
                                key += line[i].Trim() + "|";
                            }
                            for (int i = 0; i < line.Length && i < stopCol; i++)
                            {
                                szlineRes += line[i].Trim() + ",";
                            }
                            res.Add(key, szlineRes);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }


                return res;
            }
        }
        public Boolean compareLines(String l1,String l2)
        {

            Boolean match = false;

            String[] v1 = l1.Split(',');
            String[] v2 = l1.Split(',');
            int c = 0;
            foreach(String val in v1)
            {
                try
                {
                    double n1 =0;
                    if(double.TryParse(val, out n1))
                    {
                        double n2 = 0;
                        if (double.TryParse(v2[c], out n2))
                        {
                            match = Math.Round(n1, 0) == Math.Round(n2, 0);
                        }
                    }
                }
                catch(Exception e)
                {

                }
                c++;
            }
            return match;
        }
        public Dictionary<String,String> getFile(int[] ids, String f1)
        {
            Dictionary<String, String> res = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(f1))
            {
                sr.ReadLine();
                int maxIdx = ids[ids.Length - 1];
                while (!sr.EndOfStream)
                {
                    try
                    {
                        String szline = sr.ReadLine().Trim();
                        String[] line = szline.Split(',');
                        if (line.Length >= maxIdx)
                        {
                            String key = "";
                            for(int i=0;i<ids.Length;i++)
                            {
                                key += line[i].Trim() + "|";
                            }
                            res.Add(key, szline);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }


                return res;
            }
        }
    }
}
