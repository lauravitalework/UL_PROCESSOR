using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    class UL_PROCESSOR_SETTINGS
    {
        public Boolean doTenFiles = true;
        public Boolean doSumDayFiles = true;
        public Boolean doSumAllFiles = true;
        public Boolean doAngleFiles = true; //to implement
        public Boolean getFromIts = false;
        public Boolean doGR = false;
        public Boolean doVel = false;
        public Boolean doOnsets = false;
        public Boolean doDbs = false;
        public String dir;

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
                            doVel = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "DB":
                            doVel = setting[1].Trim().ToUpper() == "YES";
                            break;
                        case "ITS":
                            getFromIts = setting[1].Trim().ToUpper() == "YES";
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
        public void from(String[] vars)
        {
            classroom = vars[0].Trim();
            lenaVersion = vars[1].Trim();
            szDates = vars[2].Trim();
        }
    }
}
