using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    class UL_CLASS_DAY_PROCESSOR_Program
    {
        public DateTime day;
        public Config configInfo;

        public UL_CLASS_DAY_PROCESSOR_Program(Config c, DateTime d)
        {
            configInfo = c;
            day = d;
        }


    }
}
