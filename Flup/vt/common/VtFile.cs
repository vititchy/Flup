using System;
using System.IO;
using vt.extensions;

namespace vt.common
{
    public static class VtFile
    {
        /// <summary>
        /// nacteni casti souboru do byte[]
        /// </summary>
        public static byte[] ReadBytes(string filename, int length)
        {
            if (File.Exists(filename))
            {
                var fileLen = new FileInfo(filename).Length;
                var resultLen = Math.Min(fileLen, length).ToInt();
                var result = new byte[resultLen];
                using (var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    reader.Seek(resultLen, SeekOrigin.Begin);
                    reader.Read(result, 0, resultLen);
                    reader.Close();
                }
                return result;
            }
            return null;
        }

    }
}
