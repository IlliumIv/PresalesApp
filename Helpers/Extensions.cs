using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Helpers
{
    public static class NullableDateTimeExtension
    {
        public static DateTime? ToLocal(this DateTime? dateTime)
        {
            if (dateTime == null) return null;
            var ts = (DateTime)dateTime;
            return ts.TimeOfDay == TimeSpan.Zero ? ts : ts.ToLocalTime();
        }
    }
}
