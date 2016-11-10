using FlickrNet;

namespace FlickrNetExtender.Data.Results
{
    public class AddPhotoToPhotosetResult : ParallelOperationResultBase
    {
        public readonly Photoset Photoset;
        public readonly string PhotoId;

        public AddPhotoToPhotosetResult(Photoset photoset, string photoId)
        {
            Photoset = photoset;
            PhotoId = photoId;
        }
    }

}
