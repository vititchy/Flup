using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlickrNetExtender.Data
{
    /// <summary>
    /// adresar se seznamem souboru pro flickr
    /// </summary>
    public class PathWithFlickrFiles
    {
        public readonly DirectoryInfo DirectoryInfo;
        public readonly List<File4Flickr> Photos;
        
        /// <summary>
        /// jmeno photosetu - vypocteno dle relative path k souborum
        /// </summary>
        public readonly string PhotosetName;

        public PathWithFlickrFiles(DirectoryInfo directoryInfo, List<File4Flickr> photos)
        {
            DirectoryInfo = directoryInfo;
            Photos = photos;
            PhotosetName = Photos.Any() ? Photos.First().RelativePath : null;
        }
    }
}
