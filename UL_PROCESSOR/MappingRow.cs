using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    public class MappingRow
    {
        public DateTime Start = new DateTime();
        public DateTime Expiration = new DateTime();
        public String BID = "";
        public String longBID = "";
        public String shortBID = "";
        public String lang="";
        public String UbiID = "";
        public String LenaId = "";
        public String leftTag = "";
        public String rightTag = "";
        public String aid = "";
        public String sex = "";
        public String type = "";
        public String dob = "";
        public List<DateTime> absences = new List<DateTime>();

        public DateTime day = new DateTime();


        public Boolean isAbsent(DateTime day)
        {
            Boolean isAbsent = false;
            foreach (DateTime aday in absences)
            {
                if (DateTime.Compare(day, aday) == 0)
                {
                    isAbsent = true;
                    break;
                }
            }
            return isAbsent;
        }
    }
}
