using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UL_PROCESSOR
{
    class Onset
    {
        public String file = "";
        public String subject = "";
        public DateTime day;
        public String lenaID = "";
        public String subjectType = "";
        public int segmentId = 0;
        public String vocType = "";
        public DateTime recStart;
        public double startSecs;
        public double endSecs;
        public DateTime startTime;
        public DateTime endTime;
        public double duration;
        public double segmentDuration;
        public double wordCount;
        public double avgDb;
        public double dbPeak;
        public double turnTaking;
        public bool inSocialContact = false;
        public bool inChildSocialContact = false;
        public bool inOneChildSocialContact = false;
        public String diagnosis = "Typical";


 

        public Onset(String filea,
        DateTime daya,
        String subjecta,
        String lenaIDa,
        String subjectTypea,
        int segmentIda,
        String vocTypea,
        DateTime recStarta,
        double startSecsa,
        double endSecsa,
        DateTime startTimea,
        DateTime endTimea,
        double durationa,
        double segmentDurationa,
        double wordCounta,
        double avgDba,
        double dbPeaka,
        double turnTakinga,
        String diagnosisa)
        {
            file = filea;
            subject = subjecta;
            day= daya;
            lenaID = lenaIDa;
            subjectType = subjectTypea;
            segmentId = segmentIda;
            vocType = vocTypea;
            recStart= recStarta;
            startSecs= startSecsa;
            endSecs= endSecsa;
            startTime= startTimea;
            endTime= endTimea;
            duration= durationa;
            segmentDuration= segmentDurationa;
            wordCount= wordCounta;
            avgDb= avgDba;
            dbPeak= dbPeaka;
            turnTaking= turnTakinga;
            diagnosis = diagnosisa;
    }
        
    }
}
