using System;
using System.IO;

namespace vt.ini
{
    /// <summary>
    /// ini v app folderu uzivatele
    /// 
    /// v realite to bude napr: C:\Users\tichy\AppData\Roaming\ appDataSubFolder \ iniFileName .ini
    /// </summary>
    public class TinyIniAppFolder : TinyIniWithFileName
    {
        /// <summary>
        /// konstruktor
        /// </summary>
        /// <param name="appDataSubFolder">adresar pod AppData folderem</param>
        /// <param name="iniFileName">jmeno ini souboru</param>
        public TinyIniAppFolder(string appDataSubFolder, string iniFileName)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appDataFolder, appDataSubFolder);
            Directory.CreateDirectory(folder);

            FileName = Path.Combine(folder, iniFileName);
            Load(FileName);
        }

    }
}
