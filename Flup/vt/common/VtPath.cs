using System;
using System.IO;

namespace vt.common
{
    public static class VtPath
    {
        public static string GetRelativePath(string filespec, string rootFolder)
        {
            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!rootFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootFolder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(rootFolder);
            var result = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
            return result;
        }
    }
}
