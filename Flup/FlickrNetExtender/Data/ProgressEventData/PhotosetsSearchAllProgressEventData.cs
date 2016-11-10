using FlickrNet;
using System;

namespace FlickrNetExtender.Data.ProgressEventData
{
    public class PhotosetsSearchAllProgressEventData : ProgressEventDataBase
    {
        public readonly PhotosetCollection PhotosetCollection;

        public PhotosetsSearchAllProgressEventData(DateTime startTime, PhotosetCollection photosetCollection, bool searchFinished = false) 
            : base(startTime, searchFinished)
        {
            PhotosetCollection = photosetCollection;
        }
    }
}
