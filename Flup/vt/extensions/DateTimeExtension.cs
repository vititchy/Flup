using System;

namespace vt.extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// duration to DateTime.Now
        /// </summary>
        /// <returns>TimeSpan duration</returns>
        public static TimeSpan ToDurationFrom(this DateTime value, DateTime? finish)
        {
            try
            {
                return (finish.HasValue && (value <= finish.Value)) ? (finish.Value - value) : new TimeSpan();
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                throw;
            }
            
        }

        public static TimeSpan ToDurationFromNow(this DateTime value)
        {
            return value.ToDurationFrom(DateTime.Now);
        }
    }
}