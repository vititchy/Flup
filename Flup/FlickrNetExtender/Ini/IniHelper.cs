using System;
using System.IO;
using vt.ini;

namespace FlickrNetExtender.Ini
{
    /// <summary>
    /// ini s nastavenim zakladnim nastavenim, defaultne dohledavan u exe (viz TinyIniExeFolder)
    /// (secret tokeny jsou v jinem ini v APP folderu uzivatele!)
    /// </summary>
    public class IniHelper
    {
        private const string INI_SECTION_FLUP = "Flup";
        private const string INI_SEARCHPATTERN_DEFAULT = "*.jp*";
        private const int INI_NumberOfBytesForCRC_DEFAULT = int.MaxValue;

        /// <summary>
        /// main ini file
        /// </summary>
        private readonly TinyIniExeFolder IniMain;

        public bool UseDefaultProxy
        {
            get { return IniMain.GetBool(INI_SECTION_FLUP, "UseDefaultProxy"); }
        }

        public string SourcePath
        {
            get
            {
                var sourcePath = IniMain.GetString(INI_SECTION_FLUP, "SourcePath");
                if (!string.IsNullOrEmpty(sourcePath))
                {
                    if (Directory.Exists(sourcePath))
                    {
                        return sourcePath;
                    }
                    throw new Exception(string.Format("SourcePath not found: '{0}'.", sourcePath));
                }
                throw new Exception("Ini - SourcePath not defined.");
            }
        }
        
        private string _searchPattern;
        public string SearchPattern
        {
            get { return _searchPattern ?? (_searchPattern = IniMain.GetString(INI_SECTION_FLUP, "SearchPattern", INI_SEARCHPATTERN_DEFAULT)); }
        }
        
        private int? _numberOfBytesForCRC = null;
        public int NumberOfBytesForCRC
        {
            get
            {
                if (!_numberOfBytesForCRC.HasValue)
                {
                    _numberOfBytesForCRC = IniMain.GetInt(INI_SECTION_FLUP, "NumberOfBytesForCRC", INI_NumberOfBytesForCRC_DEFAULT);
                }
                return _numberOfBytesForCRC.Value;
            }
        }


        #region Constructors

        public IniHelper()
        {
            // main ini file
            IniMain = new TinyIniExeFolder();
        }

        #endregion
    }
}
