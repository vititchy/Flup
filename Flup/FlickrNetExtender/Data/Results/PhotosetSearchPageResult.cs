using FlickrNet;

namespace FlickrNetExtender.Data.Results
{
    public  class PhotosetSearchPageResult : ParallelOperationResultBase
    {
        public readonly int Page;
        public readonly PhotosetCollection PhotosetCollection;

        public PhotosetSearchPageResult(int page, PhotosetCollection photosetCollection) : base()
        {
            Page = page;
            PhotosetCollection = photosetCollection;
        }
    }

}
