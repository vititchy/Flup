using System;

namespace FlickrNetExtender.Data
{
    public class FlickrProgressEventData
    {
        public readonly string FlickrAction;
        public readonly DateTime StartTime;
        public readonly int ProgressCount;

        public FlickrProgressEventData(string flickrAction, int progressCount, DateTime startTime)
        {
            FlickrAction = flickrAction;
            ProgressCount = progressCount;
            StartTime = startTime;
        }
    }
}
