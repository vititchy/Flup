using System;

namespace vt.extensions
{
    public static class TimeSpanExtension
    {
        /// <summary>
        /// convert long into ETA string like "1.22:33"
        /// </summary>
        public static string ToDuration(this TimeSpan value)
        {
            var format = "{0:mm\\:ss}";
            if (value.TotalDays > 1)
            {
                format = "{0:d\\.hh\\:mm}";
            }
            else if (value.TotalHours > 1)
            {
                format = "{0:hh\\:mm}";
            }
            else
            {
                format = "{0:mm\\:ss}";
            }
            var result = string.Format(format, value);
            return result;
        }
    }
}
