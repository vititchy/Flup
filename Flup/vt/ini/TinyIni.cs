using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vt.ini
{
    /// <summary>
    /// zaklad pro praci s INI souborem
    /// </summary>
    public abstract class TinyIni
    {
        #region Properties

        private readonly List<TinyIniLine> IniLineSet = new List<TinyIniLine>();

        public bool IsChanged { get; protected set; }

        #endregion
        

        #region Constructors

        public TinyIni()
        {
            IsChanged = false;
        }


        public TinyIni(string fileName) : this()
        {
             Load(fileName);
        }

        #endregion


        #region Methods

        public void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fStream, Encoding.UTF8))
                {
                    string prevSection = string.Empty;
                    string sourceLine;
                    while ((sourceLine = streamReader.ReadLine()) != null)
                    {
                        var iniLine = ParseLine(sourceLine, prevSection);
                        IniLineSet.Add(iniLine);
                        prevSection = iniLine.Section;
                    }
                }
            }
        }


        /// <summary>
        /// ulozeni do souboru
        /// - poradi IniLine by se melo zachovat, prazdny radky a komenty musi zustat
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            string prevSection = null;
            var sb = new StringBuilder();

            foreach (var iniLine in IniLineSet)
            {
                switch (iniLine.LineType)
                {
                    case TinyIniLineType.Section:
                        sb.AppendLine(string.Format("[{0}]", iniLine.Section));
                        break;

                    case TinyIniLineType.KeyValue:
                        if (prevSection != iniLine.Section)
                        {
                            prevSection = iniLine.Section;
                            sb.AppendLine(string.Format("[{0}]", iniLine.Section));
                        }
                        sb.AppendLine(string.Format("{0}={1}", iniLine.Key, iniLine.Value));
                        break;

                    case TinyIniLineType.Other:
                        sb.AppendLine(string.Format("# {0}", iniLine.Value));
                        break;

                    default:
                        break;
                }
            }

            using (var streamWriter = new StreamWriter(fileName))
            {
                streamWriter.WriteLine(sb);
            }
        }



        public void AddValue(string section, string key, string value)
        {
            NormalizeKey(ref section);
            NormalizeKey(ref key);
            value = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();

            var newLine = new TinyIniLine()
            {
                LineType = TinyIniLineType.KeyValue,
                Section = section,
                Key = key,
                Value = value
            };
            IniLineSet.Add(newLine);
            IsChanged = true;
        }



        public bool ExistsKey(string section, string key)
        {
            var line = FindIniLineForKey(section, key);
            return (line != null);
        }



        public string GetString(string section, string key, string defaultValue = null)
        {
            var line = FindIniLineForKey(section, key);
            return (line != null) ? line.Value : defaultValue;
        }


        /// <summary>
        /// tady je otazka co vse povazovat za true ;-)
        /// </summary>
        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            var value = GetString(section, key);
            NormalizeKey(ref value);
            return ((value == "TRUE") || (value == "1"));
        }

        public int GetInt(string section, string key, int defaultValue = 0)
        {
            var value = GetString(section, key);
            NormalizeKey(ref value);
            var result = Convert.ToInt32(value);
            return result; 
        }


        #endregion
        

        #region Other Members
        
        private TinyIniLine FindIniLineForKey(string section, string key)
        {
            NormalizeKey(ref section);
            NormalizeKey(ref key);

            var line = IniLineSet.FirstOrDefault(p => CompareKeys(p.Section, section) && CompareKeys(p.Key, key));
            return line;
        }

        
        /// <summary>
        /// trim and uppercase string key
        /// </summary>
        private void NormalizeKey(ref string key)
        {
            key = (!string.IsNullOrEmpty(key)) ? key.Trim().ToUpper() : string.Empty;
        }


        private bool CompareKeys(string s1, string s2)
        {
            NormalizeKey(ref s1);
            NormalizeKey(ref s2);
            var equal = string.Compare(s1, s2, ignoreCase: true);
            return (equal == 0);
        }


        /// <summary>
        /// zakladni parsovaci fce jednoho radku ini souboru
        /// </summary>
        /// <param name="line">zdrojovy radek</param>
        /// <param name="prevSection">posledni dohledana sekce ini</param>
        /// <returns>TinyIniLine</returns>
        private TinyIniLine ParseLine(string line, string prevSection) 
        {
            var iniLine = new TinyIniLine()
            {
                Section = prevSection,
                LineType = TinyIniLineType.Other,
                SourceLine = line
            };

            if (!string.IsNullOrEmpty(line))
            {
                line = line.Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    var firstChar = line.First();
                    if ((firstChar == '[') && (line.Last() == ']'))
                    {
                        // [] sekce
                        iniLine.LineType = TinyIniLineType.Section;
                        iniLine.Section = line.Substring(1, line.Count() - 2);
                    }
                    else if (firstChar != ';')
                    {
                        // key=value
                        iniLine.LineType = TinyIniLineType.KeyValue;
                        iniLine.Section = prevSection;
                        var eqIndex = line.IndexOf('=');
                        if (eqIndex < 0)
                        {
                            iniLine.Key = line;
                        }
                        else
                        {
                            iniLine.Key = line.Substring(0, eqIndex);
                            iniLine.Value = line.Substring(eqIndex + 1);
                        }
                    }
                    else
                    {
                        iniLine.Section = prevSection;
                    }
                }
            }
            return iniLine;
        }

        #endregion
    }
}
