using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    public class PersonInfo
    {
        public DateTime startTime = new DateTime();
        public Boolean startTimeSet = false;

        public String ubiId = "";
        public String lenaId = "";
        public String bid = "";
        public DateTime dt = new DateTime();

        public Boolean wasTalking = false;
        public double x = 0;
        public double y = 0;
        public double z = 0;
        public double ori = 0;
        public double ori2 = 0;
        public double vd = 0;
        public double vc = 0;
        public double tc = 0;
        public double bd = 0;

        public double no = 0;
        public double ac = 0;
        public double oln = 0;

        public double lx = 0;
        public double ly = 0;
        public double rx = 0;
        public double ry = 0;

        public Boolean isFreePlay = false;
    }
}
