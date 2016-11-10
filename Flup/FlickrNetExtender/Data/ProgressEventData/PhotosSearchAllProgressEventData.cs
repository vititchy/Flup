using FlickrNet;
using System;

namespace FlickrNetExtender.Data.ProgressEventData
{
    public class PhotosSearchAllProgressEventData : ProgressEventDataBase
    {
        public readonly PhotoCollection PhotoCollection;

        public PhotosSearchAllProgressEventData(DateTime startTime, PhotoCollection photoCollection, bool searchFinished = false) : base(startTime, searchFinished)
        {
            PhotoCollection = photoCollection;
        }
    }
}
