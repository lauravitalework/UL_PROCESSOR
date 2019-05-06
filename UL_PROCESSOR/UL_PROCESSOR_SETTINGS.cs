using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    class UL_PROCESSOR_SETTINGS
    {
        public Boolean doUbiChildFiles = false;
        public Boolean doTenFiles = true;
        public Boolean doSumDayFiles = true;
        public Boolean doSumAllFiles = true;
        public Boolean doAngleFiles = true; //to implement
        public Boolean getFromIts = false;
        public Boolean doGR = true;
        public Boolean doVel = true;
        public Boolean doOnsets = true;
        public Boolean doSocialOnsets = true;
        public Boolean doMinVocs = true;
        public Boolean doDbs = true;
        public String dir;
        public Boolean doAll10OfSecs = false;//false;//true;
        public Boolean startFromLena = true;//false;//true;
        //public Boolean noLena = true;//false;//true;//false;//true;
        //public double minDistance = 1.5 * 1.5; //the squared value of g(r) cutoff in meters
        public String fileNameVersion = DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year + "_" + new Random().Next();
        public List<String> subs = new List<string>();
        public Boolean qaHoles = false;
        public Boolean lenaTimes = false;
        public void from(String[] args)
        {
            String[] settings = args[0].Split(' ');
            dir = settings[0];

            UL_PROCESSOR_SETTINGS settingParams = new UL_PROCESSOR_SETTINGS();
            for (int a = 1; a < settings.Length; a++)
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
                        case "ONSETS":
                            doOnsets = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "DBS":
                            doDbs = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "ITS":
                            getFromIts = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "HOLES":
                            qaHoles = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "LENATIMES":
                            lenaTimes = setting[1].Trim().ToUpper() == "YES";
                            break;

                    }
                }
            }

        }

    }
    class UL_PROCESSOR_CLASS_SETTINGS
    {
        public String classroom;
        public String lenaVersion;
        public String szDates;
        public String mappingBy = "CLASS";//"CLASS";"DAY"
        public double gOfR = 1.5;
        public void from(String[] vars)
        {
            classroom = vars[0].Trim();
            lenaVersion = vars[1].Trim();
             
            if(vars.Length>3)
            {
                gOfR = double.TryParse(vars[3].Trim(), out gOfR) ? gOfR : 1.5;
                szDates = vars[4].Trim();
                mappingBy = vars[2].Trim();
            }
            else
                szDates = vars[2].Trim();

        }
    }
}
