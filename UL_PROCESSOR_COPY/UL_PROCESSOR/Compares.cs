using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    public class MappingUbiComparer : IComparer<MappingRow>
    {
        public int Compare(MappingRow x, MappingRow y)
        {
            int res = x.UbiID.CompareTo(y.UbiID);
            return res == 0 ? y.day.CompareTo(x.Expiration) <= 0 && y.day.CompareTo(x.Start) >= 0 ? 0 : -1 : res;
        }
    }
    public class MappingLenaComparer : IComparer<MappingRow>
    {
        public int Compare(MappingRow x, MappingRow y)
        {
            int res = x.LenaId.CompareTo(y.LenaId);
            return res == 0 ? y.day.CompareTo(x.Expiration) <= 0 && y.day.CompareTo(x.Start) >= 0 ? 0 : -1 : res;
        }
    }

    public class DateTimeComparer : IComparer<PersonInfo>
    {
        public int Compare(PersonInfo x, PersonInfo y)
        {
            int sameToSec = x.dt.CompareTo(y.dt);
            if (sameToSec == 0)
            {
                if (x.dt.Millisecond == y.dt.Millisecond) return 0;
                return x.dt.Millisecond < y.dt.Millisecond ? -1 : 1;
            }
            return sameToSec;
        }
    }
}
