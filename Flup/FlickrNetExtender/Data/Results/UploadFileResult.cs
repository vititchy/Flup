using FlickrNet;
using FlickrNetExtender.Data.Results;
using System;
using vt.extensions;

namespace FlickrNetExtender.Data.Results
{
    public class UploadFileResult : ParallelOperationResultBase
    {
        public readonly File4Flickr File4Flickr;
        public readonly string PhotoId;
        public readonly FlickrApiException FlickrApiException;
        public readonly int RetryAttemp;

        public string UploadSpeed { get { return (File4Flickr != null) ? File4Flickr.Length.ToBitesPerSecSpeed(Duration) : "0"; } }
        

        public UploadFileResult(File4Flickr file4Flickr, DateTime startTime, int retryAttemp) : base(startTime)
        {
            File4Flickr = file4Flickr;
            RetryAttemp = retryAttemp;
        }

        public UploadFileResult(File4Flickr file4Flickr, DateTime startTime, int retryAttemp, string photoId) : this(file4Flickr, startTime, retryAttemp)
        {
            PhotoId = photoId;
        }

        public UploadFileResult(File4Flickr file4Flickr, DateTime startTime, int retryAttemp, FlickrApiException flickrApiException) : this(file4Flickr, startTime, retryAttemp)
        {
            PhotoId = null;
            FlickrApiException = flickrApiException;
        }
      
    }
}
