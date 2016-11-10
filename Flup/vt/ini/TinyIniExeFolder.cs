using System.IO;
using System.Reflection;

namespace vt.ini
{
    /// <summary>
    /// ini file primo v ceste u exe aplikace se jmenem xxx.exe.ini - nemusi fungovat kvuli pravum!
    /// </summary>
    public class TinyIniExeFolder : TinyIniWithFileName
    {
        readonly string ExeFineName = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// konstruktor
        /// </summary>
        public TinyIniExeFolder() 
        {
            FileName = new FileInfo(ExeFineName + ".ini").FullName;
            Load(FileName);
        }
    }
}
