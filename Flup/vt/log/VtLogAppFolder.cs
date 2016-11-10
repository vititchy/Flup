using System;
using System.IO;

namespace vt.log
{
    /// <summary>
    /// VtLog zapisujici do APP folderu
    /// </summary>
    public class VtLogAppFolder : VtLog
    {
        public VtLogAppFolder(string appDataSubFolder, string iniFileName, int maxLineLen = 0, bool showTime = false)
            : base(GetFullLogFileName(appDataSubFolder, iniFileName), maxLineLen, showTime)
        {
        }

        public VtLogAppFolder(string iniFileName, int maxLineLen = 0, bool showTime = false)
            : base(GetFullLogFileName(GetExeLogFileName(), iniFileName), maxLineLen, showTime)
        {
        }
        public VtLogAppFolder(int maxLineLen = 0, bool showTime = false)
            : base(GetFullLogFileName(GetExeLogFileName(), GetExeLogFileName() + DEFAULT_EXT), maxLineLen, showTime)
        {
        }


        /// <summary>
        /// zkonstruje log filename v APP foldru usera
        /// </summary>
        private static string GetFullLogFileName(string appDataSubFolder, string iniFileName)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appDataFolder, appDataSubFolder);
            Directory.CreateDirectory(folder);

            var fileName = Path.Combine(folder, iniFileName);
            return fileName;
        }
    }
}
