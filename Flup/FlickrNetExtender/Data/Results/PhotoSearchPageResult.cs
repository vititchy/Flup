using FlickrNet;

namespace FlickrNetExtender.Data.Results
{
    public class PhotoSearchPageResult : ParallelOperationResultBase
    {
        public readonly PhotoSearchOptions PhotoSearchOptions;
        public readonly PhotoCollection PhotoCollection;

        public PhotoSearchPageResult(PhotoSearchOptions photoSearchOptions, PhotoCollection photoCollection) : base()
        {
            PhotoSearchOptions = photoSearchOptions;
            PhotoCollection = photoCollection;
        }
    }
}
