using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace vt.extensions
{
    public static class DirectoryExtension
    {
        /// <summary>
        /// dohleda v adresari soubory majici jednu z extensions
        /// </summary>
        /// <param name="path">zdrojova cesta</param>
        /// <param name="extensionList">seznam pripon souboru ... napr "jpg;jpeg;gif"</param>
        /// <param name="extensionListSeparator">oddelovac v extension listu</param>
        /// <param name="searchOption">SearchOption</param>
        /// <returns></returns>
        public static List<string> GetFilesByExt(string path, string extensionList, char extensionListSeparator = ';', SearchOption searchOption = SearchOption.AllDirectories)
        {
            var extensionSet = extensionList.Split(new char[] { extensionListSeparator })
                                            .Select(p => "." + p.ToLower())
                                            .ToList();

            var files = Directory.EnumerateFiles(path, "*.*", searchOption).ToList();
            var result = files.Where(fileName => extensionSet.Contains(Path.GetExtension(fileName).ToLower()))
                                  .ToList();
            return result;
        }
    }
}
