using System;
using System.IO;

namespace vt.log
{
    public class VtLogExeFolder : VtLog
    {
 
        public VtLogExeFolder(int maxLineLen = 0, bool showTime = false) : base(GetFullLogFileName(), maxLineLen, showTime)
        {
        }
        

        /// <summary>
        /// zkonstruje vlastni filename logu dle aktualniho exe
        /// </summary>
        private static string GetFullLogFileName()
        {
            string appName = GetExeLogFileName();
            var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appName + DEFAULT_EXT);
            return result;
        }

    }
}
