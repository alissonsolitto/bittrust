using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Model
{
    public class FeesRecommendedModel
    {
        public int fastestFee { get; set; }
        public int halfHourFee { get; set; }
        public int hourFee { get; set; }
    }

    public class Fee
    {
        public int minFee { get; set; }
        public int maxFee { get; set; }
        public int dayCount { get; set; }
        public int memCount { get; set; }
        public int minDelay { get; set; }
        public int maxDelay { get; set; }
        public int minMinutes { get; set; }
        public int maxMinutes { get; set; }
    }
    
    public class FeesListModel
    {
        public List<Fee> fees { get; set; }
    }
}
