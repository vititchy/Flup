using System.IO;
using vt.common;

namespace FlickrNetExtender.Data
{
    /// <summary>
    /// vazba mezi fyzickym souborem na disku a Photo instanci na Flikru
    /// </summary>
    public class File4Flickr
    {
        public readonly static string FLUP_TAG = "FLUP";     // bacha od flickru se to vraci vzdy malyma pismenama, ackoli na webu jsou velka ;-)
        public readonly static string FLUPCRC_TAG = "FLUPCRC";

        public readonly string FullName;
        public readonly string RelativePathWithFileName;
        public readonly string RelativePath;
        public readonly long Length;

        public readonly int NumberOfBytesForCRC;

        private long? _crc;
        public long Crc
        {
            get
            {
                if (!_crc.HasValue)
                {
                    PrepareCrc();
                }
                return _crc.Value;
            }
        }


        public string FlupTags
        {
            get
            {
                var flickrTags = string.Format("{0} {1}{2}", FLUP_TAG, FLUPCRC_TAG, Crc  /*, FlupRelativePathTag, FLupPhotoSetTag */ );
                return flickrTags;
            }
        }



        public File4Flickr(FileInfo fileInfo, string sourceRootPath, int numberOfBytesForCRC)
        {
            NumberOfBytesForCRC = numberOfBytesForCRC;
            if (fileInfo != null)
            {
                FullName = fileInfo.FullName;
                RelativePathWithFileName = VtPath.GetRelativePath(FullName, sourceRootPath);
                RelativePath = Path.GetDirectoryName(RelativePathWithFileName);
                Length = fileInfo.Length;
            }
        }


        public File4Flickr PrepareCrc()
        {
            _crc = !string.IsNullOrEmpty(FullName) ? Adler32.FileChecksum(FullName, NumberOfBytesForCRC) : 0;
            return this;
        }

    }
}
