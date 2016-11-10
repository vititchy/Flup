using System;
using System.Globalization;
using System.Linq;

namespace vt.extensions
{
    public static class LongExtensions
    {
        static readonly string[] SizeSuffixes = { "b", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// convert long into formatted number with the correct size metric
        /// alternative: http://www.danylkoweb.com/Blog/10-extremely-useful-net-extension-methods-8J
        /// </summary>
        public static string ToFileSize(this long value)
        {
            if (value < 0)
            {
                return "-" + ToFileSize(-value);
            }
            if (value == 0)
            {
                return "0" + SizeSuffixes.First();
            }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1}{1}", adjustedSize, SizeSuffixes[mag]).Replace(',', '.');
        }


        /// <summary>
        /// convert long into string format like 1.2Mb/s
        /// </summary>
        public static string ToBitesPerSecSpeed(this long value, TimeSpan duration)
        {
            if (duration.TotalSeconds > 0)
            {
                var bitesSize = value * 8;
                var bitesPerSec = bitesSize / duration.TotalSeconds;
                var result = ((long)bitesPerSec).ToFileSize() + "/s";
                return result;
            }
            return "?";
        }


        /// <summary>
        /// convert long into string format like 1.2Mb/s
        /// </summary>
        public static string ToBitesPerSecSpeed(this long value, DateTime startTime, DateTime finishTime)
        {
            return value.ToBitesPerSecSpeed(finishTime - startTime);
        }



        public static string ToPercentFromTotal(this long value, long total, string formatString = "0.##")
        {
            var percent = (total > 0) ? (((float)value / total) * 100) : 0;
            var result = percent.ToString(formatString, CultureInfo.InvariantCulture) + "%";
            return result;
        }



        /// <summary>
        /// convert long into ETA string like "1.22:33"
        /// </summary>
        public static string ToETA(this long partValue, long total, TimeSpan partDuration)
        {
            if (total > 0)
            {
                var finishedPercent = (((float)partValue / total) * 100);
                if (finishedPercent > 0)
                {
                    var etaSeconds = (partDuration.TotalSeconds / finishedPercent) * Math.Max((100 - finishedPercent), 0);
                    // ochrana proti preteceni TimeSpanu
                    etaSeconds = Math.Min(etaSeconds, TimeSpan.MaxValue.TotalSeconds - 1);
                    var eta = TimeSpan.FromSeconds(etaSeconds);

                    string format;
                    if (eta.TotalDays > 1)
                    {
                        format = "{0:d\\.hh\\:mm}d";
                    }
                    else if (eta.TotalHours > 1)
                    {
                        format = "{0:hh\\:mm}h";
                    }
                    else
                    {
                        format = "{0:mm\\:ss}m";
                    }
                    return string.Format(format, eta);
                }
            }
            return "?";
        }


        public static int ToInt(this long value)
        {
            if(value > int.MaxValue)
            {
                return int.MaxValue;
            }
            if(value < int.MinValue)
            {
                return int.MinValue;
            }
            return Convert.ToInt32(value);
        }

    }
}
