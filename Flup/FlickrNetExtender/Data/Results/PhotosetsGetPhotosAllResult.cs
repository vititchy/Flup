using System;
using System.Collections.Generic;

namespace FlickrNetExtender.Data.Results
{
    public class PhotosetsGetPhotosAllResult : ParallelOperationResultBase
    {
        public readonly string PhotosetId;
        public readonly List<string> PhotoIdSet;


        public PhotosetsGetPhotosAllResult(string photosetId)
        {
            PhotosetId = photosetId;
            PhotoIdSet = new List<string>();
        }
    }
}
