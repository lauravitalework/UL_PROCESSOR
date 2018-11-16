using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    class PersonDistAndVel
    {
        public List<DistAndVel> distAndVels = new List<DistAndVel>();
        public Tuple<double,double, double> calculateDistTimeVel()
        {
            Tuple<double, double> distAndVel = new Tuple<double, double>(0, 0);
            double d = 0;
            double t = 0;
            foreach (DistAndVel dv in distAndVels)
            {
                double secs = (dv.end - dv.start).TotalSeconds;
                if(dv.distInMeters > 0 && secs>0 )
                {
                    d += dv.distInMeters;
                    t += secs;
                }
                 
                
            }
            return new Tuple<double, double, double>(d, t, d>0&&t>0?d/t:0);


        }
    }
    class DistAndVel
    {
        public DateTime start;
        public DateTime end;
        public double distInMeters = 0;
    }
}
