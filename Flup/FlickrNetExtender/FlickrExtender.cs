using FlickrNet;
using FlickrNetExtender.Data;
using FlickrNetExtender.Data.Exceptions;
using FlickrNetExtender.Data.Parallel;
using FlickrNetExtender.Data.Results;
using FlickrNetExtender.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using vt.extensions;
using vt.log;

namespace FlickrNetExtender
{
    /// <summary>
    /// rozsireni FlickrNet.Flick tridy
    /// 
    /// ! pro paralelni akce je potreba FlicrkNetu podhodit upraveny WebClient ! viz WebClientEx, ktery obejde defaultni nastaveni limitu soubeznych spojeni 
    /// 
    /// ! tez je potreba vypnout interni cache FlickNETu ! pri paralelnim behu akorat vytizi procesor + disk a zpomaly komunikaci :-/
    /// 
    /// 
    /// 
    /// 
    /// zakladni Flickr pojmy:
    /// 
    ///   Flickr has several types of containers for images, each with a different purpose.
    ///  
    ///   Photostream: 
    ///        All images on Flickr exist only in photostreams. Anywhere else is just a pictorial link back to the 
    ///        photostream.Your photostream is only for images you have created.
    ///        Your Photostream: https://www.flickr.com/photos/me
    ///        FAQ about Photostreams: https://www.flickr.com/help/photos/#29
    ///  
    ///   Sets: 
    ///        Sets are also only for your images. Sets can help you organize your photostream into categories. (In reality, 
    ///        sets only contain pictorial links back to your photostream; however, it will appear the images are "in" the sets.) 
    ///        You can organize your sets here: https://www.flickr.com/photos/organize/?start_tab=sets
    ///        Your Sets: https://www.flickr.com/photos/me/sets
    ///        There is no FAQ on Sets (that I could find)
    ///  
    ///   Collections: 
    ///        Collections are only for pro account holders.They can be used to organize sets and/or other collections.A collection 
    ///        can contain sets* or* collections, but not both in the same collection.
    ///        Your Collections: https://www.flickr.com/photos/me/collections/
    ///        FAQs about Collections: https://www.flickr.com/help/faq/search/?q=collections
    ///  
    ///   Galleries: 
    ///        Galleries are for other people's images and cannot contain any of your own images. They are currently limited
    ///        to 18 images per gallery, but you can create as many galleries as you want. Other users can choose whether their images 
    ///        can be added to galleries.
    ///        Your Galleries: https://www.flickr.com/photos/me/galleries/
    ///        FAQs about Galleries: https://www.flickr.com/help/faq/search/?q=galleries
    ///  
    ///   Groups: 
    ///        Groups can contain your own images and other people's images. Group administrators can set limits on how many images members
    ///        can add, can set up a moderation queue for the group, and can issue invitations for images to be added to the group (invites 
    ///        will bypass limits and queues).
    ///        Groups to Which You Belong: https://www.flickr.com/groups/
    ///        FAQs about Groups: https://www.flickr.com/help/faq/search/?q=groups
    ///  
    ///   Albums: 
    ///        Flickr doesn't have a container called "albums," though one part of the FAQ (https://www.flickr.com/help/photos/#59) uses the 
    ///        word "album" to refer to "sets."
    /// 
    /// </summary>
    public class FlickrExtender : Flickr 
    {
        #region Properties

        private const int SearchOptions_PerPage_MAX = 500; // Number of XXX to return per page. If this argument is omitted, it defaults to 100. The maximum allowed value is 500.

        private const int DefaultParallelConnections = 6;  // Number of http parallel connections

        private readonly VtLog Logger;

        #endregion 


        #region Events

        /// <summary>
        /// progress event z metody VTPhotosSearchByTag
        /// </summary>
        public event EventHandler<FlickrProgressEventData> SearchProgressEvent;

        #endregion


        #region Methods

        public FlickrExtender VtCheckAuth()
        {
            var checkToken = AuthOAuthCheckToken();
            // zkusim null dotazem zda jsem autentifikovan
            if ((checkToken != null) && (checkToken.User != null))
            {
                Logger.Write(string.Format("Successfully authenticated as ({0}, {1}, {2}) with permissions '{3}'.",
                                checkToken.User.FullName,   // v t
                                checkToken.User.UserId,     // 12345678@N08
                                checkToken.User.UserName,   // tncjklm
                                checkToken.Permissions));
                return this;
            }
            return null;
        }
        

        public PhotoCollection VtPhotosSearchByTag(string tag)
        {
            var photoCollection = new PhotoCollection();
            //## Photo Extras
            //                One of the hardest things to understand initially is that not all properties are returned by Flickr, you have to explicity request them.  
            //For example the following code would be used to return the Tags and the LargeUrl for a selection of photos:
            //~~~
            //var options = new PhotoSearchOptions
            //{
            //    Tags = "colorful",
            //    PerPage = 20,
            //    Page = 1,
            //    Extras = PhotoSearchExtras.LargeUrl | PhotoSearchExtras.Tags
            //};

            var startTime = DateTime.Now;
            PhotoCollection searchedPhotosForPage = null;
            var page = 1;
            do
            {
                RaiseSearchProgressEvent("PhotosSearch", photoCollection.Count, startTime);

                var options = new PhotoSearchOptions
                {
                    PrivacyFilter = PrivacyFilter.None,
                    UserId = "me",
                    MediaType = MediaType.All,
                    PerPage = SearchOptions_PerPage_MAX,    // Number of photos to return per page. If this argument is omitted, it defaults to 100. The maximum allowed value is 500.
                    Page = page,                            // The page of results to return.If this argument is omitted, it defaults to 1.
                    Extras = PhotoSearchExtras.All,         // co chci, aby mi flickr api vratilo!! 
                    Tags = tag,
                    TagMode = TagMode.AnyTag
                };
                searchedPhotosForPage = PhotosSearch(options);

                foreach (var photo in searchedPhotosForPage)
                {
                    photoCollection.Add(photo);
                }
                page++;

            } while (searchedPhotosForPage.Count >= SearchOptions_PerPage_MAX);

            return photoCollection;
        }

                        
        public PhotoCollection VtPhotosSearchAll()
        {
            var photoCollection = new PhotoCollection();
            var startTime = DateTime.Now;
            PhotoCollection searchedPhotosForPage = null;
            var page = 1;
            do
            {
                RaiseSearchProgressEvent("PhotosSearch", photoCollection.Count, startTime);

                var options = new PhotoSearchOptions
                {
                    PrivacyFilter = PrivacyFilter.None,
                    UserId = "me",
                    MediaType = MediaType.All,
                    PerPage = SearchOptions_PerPage_MAX,    // Number of photos to return per page. If this argument is omitted, it defaults to 100. The maximum allowed value is 500.
                    Page = page,                            // The page of results to return.If this argument is omitted, it defaults to 1.
                    Extras = PhotoSearchExtras.All,         // co chci, aby mi flickr api vratilo!! 
                    TagMode = TagMode.AnyTag
                };
                searchedPhotosForPage = PhotosSearch(options);

                foreach (var photo in searchedPhotosForPage)
                {
                    photoCollection.Add(photo);
                }
                page++;

            } while (searchedPhotosForPage.Count >= SearchOptions_PerPage_MAX);

            return photoCollection;
        }


        public PhotoCollection VtPhotosGetNotInSet()
        {
            var photoCollection = new PhotoCollection();

            var startTime = DateTime.Now;
            PhotoCollection searchedPhotosForPage = null;
            var page = 1;
            do
            {
                RaiseSearchProgressEvent("PhotosGetNotInSet", photoCollection.Count, startTime);

                var options = new PartialSearchOptions
                {
                    PrivacyFilter = PrivacyFilter.None,
                    //UserId = "me",
                    //MediaType = MediaType.All,
                    PerPage = SearchOptions_PerPage_MAX,    // Number of photos to return per page. If this argument is omitted, it defaults to 100. The maximum allowed value is 500.
                    Page = page,                            // The page of results to return.If this argument is omitted, it defaults to 1.
                    Extras = PhotoSearchExtras.All,         // co chci, aby mi flickr api vratilo!! 
                    //Tags = tag,
                    //TagMode = TagMode.AnyTag
                };
                searchedPhotosForPage = PhotosGetNotInSet(options);

                foreach (var photo in searchedPhotosForPage)
                {
                    photoCollection.Add(photo);
                }
                page++;

            } while (searchedPhotosForPage.Count >= SearchOptions_PerPage_MAX);

            return photoCollection;
        }

        internal FlickrExtender SetOnUploadProgress(Action<UploadProgressEventArgs> onUploadProgressAction)
        {
            OnUploadProgress += (s, e) => onUploadProgressAction(e);
            return this;
        }

        internal FlickrExtender SetSearchProgressEvent(Action<FlickrProgressEventData> searchProgressEvent)
        {
            SearchProgressEvent += (s, e) => searchProgressEvent(e);
            return this;
        }

        /// <summary>
        /// Return the list of galleries created by a user. Sorted from newest to oldest.
        /// </summary>
        public GalleryCollection VtGetListOfGalleries()
        {
            var galleryCollection = new GalleryCollection();

            var startTime = DateTime.Now;
            GalleryCollection searchedForPage = null;
            var page = 1;
            do
            {
                RaiseSearchProgressEvent("GalleriesGetList", galleryCollection.Count, startTime);

                searchedForPage = GalleriesGetList(page, SearchOptions_PerPage_MAX);
                foreach (var gallery in searchedForPage)
                {
                    galleryCollection.Add(gallery);
                }
                page++;

            } while (searchedForPage.Count >= SearchOptions_PerPage_MAX);

            return galleryCollection;
        }


        /// <summary>
        /// dohledani seznamu PhotoSetu
        /// </summary>
        public PhotosetCollection VtPhotosetsGetList()
        {
            var photosetCollection = new PhotosetCollection();

            var startTime = DateTime.Now;
            PhotosetCollection searchedForPage = null;
            var page = 1;
            do
            {
                RaiseSearchProgressEvent("PhotosetsGetList", photosetCollection.Count, startTime);

                searchedForPage = PhotosetsGetList(page, SearchOptions_PerPage_MAX);
                foreach (var photoset in searchedForPage)
                {
                    photosetCollection.Add(photoset);
                }
                page++;

            } while (searchedForPage.Count >= SearchOptions_PerPage_MAX);

            return photosetCollection;
        }


        public bool VtExistsPhotoInPhotoset(string photoSetId, string photoId)
        {
            var photosetPhotoCollection = PhotosetsGetPhotos(photoSetId, PhotoSearchExtras.None /* nechci nic krome PhotoId */);
            var existsInPhotoset = photosetPhotoCollection.Any(p => p.PhotoId == photoId);
            return existsInPhotoset;
        }


        /// <summary>
        /// zakladni upload Photo na flickr s podporou retry pokusu (flickr je zrejme pretizeny a obcas haze service unavailable atd)
        /// </summary>
        public UploadFileResult UploadFile(File4Flickr file4Flickr, int maxRetryAttemps = 10)
        {
            var startTime = DateTime.Now;
            string photoId = null;
            var retryAttemp = 0;

            while (string.IsNullOrEmpty(photoId))
            {
                try
                {
                    photoId = UploadPicture(file4Flickr);

                    // nechapu co to ma znamenat, ale jednou se ted stalo, ze upload probehl v poradku, avsak photoId bylo prazdne?? a na flickru fotky skutecne nejsou
                    // vyvolam tedy vyjimku, aby probehlo uploadovani znovu
                    if (string.IsNullOrEmpty(photoId))
                    {
                        throw new FlickrExtenderException("PhotoId is empty!");
                    }
                }
                catch (Exception ex) when (ex is WebException || ex is IOException || ex is FlickrExtenderException)
                {
                    // v pripade nekterych http protocol erroru zkusim retry - mozna by bylo vhodne to zkouset pri kazde chybe?
                    if (retryAttemp++ < maxRetryAttemps)
                    {
                        if (ex is WebException)
                        {
                            Logger.Write(string.Format("WebException message:{0} status:{1}", ex.Message, (ex as WebException).Status), VtLogState.Error);
                        }
                        else
                        {
                            Logger.Write(string.Format("{0} error:{1}", ex.GetType().Name, ex.Message), VtLogState.Error);
                        }
                        Logger.Write(string.Format("Retry #:{0}", retryAttemp), VtLogState.Info);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (FlickrApiException flickrApiException)
                {
                    // doslo k nejake flickr API chybe (treba 5: Filetype was not recognised), takze nezkousim retry a skoncim upload s chybou
                    var result = new UploadFileResult(file4Flickr, startTime, retryAttemp, flickrApiException);
                    return result;
                }
            }
            return new UploadFileResult(file4Flickr, startTime, retryAttemp, photoId);
        }


        private string UploadPicture(File4Flickr file4Flickr)
        {
            using (var filestream = new FileStream(file4Flickr.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //Arguments https://www.flickr.com/services/api/upload.api.html
                //photo
                //    The file to upload.
                //title(optional)
                //    The title of the photo.
                //description(optional)
                //    A description of the photo.May contain some limited HTML.
                //tags(optional)
                //    A space-seperated list of tags to apply to the photo.
                //is_public, is_friend, is_family(optional)
                //    Set to 0 for no, 1 for yes.Specifies who can view the photo.
                //safety_level(optional)
                //    Set to 1 for Safe, 2 for Moderate, or 3 for Restricted.
                //content_type(optional)
                //    Set to 1 for Photo, 2 for Screenshot, or 3 for Other.
                //hidden(optional)
                //    Set to 1 to keep the photo in global search results, 2 to hide from public searches.
                var photoId = UploadPicture(filestream,
                                                   Path.GetFileName(file4Flickr.FullName),
                                                   null,
                                                   null,
                                                   file4Flickr.FlupTags,
                                                   false,
                                                   false,
                                                   false,
                                                   ContentType.None,
                                                   SafetyLevel.Restricted,
                                                   HiddenFromSearch.Hidden);
                filestream.Close();
                return photoId;
            }
        }

        #endregion



        #region Methods-Async


        private async Task<PhotosetSearchPageResult> PhotosetsGetListForPage(int page)
        {
            var searchedPhotoSetsForPage = await Task.Run(() =>
                    {
                        var photoCollection = PhotosetsGetList(page, SearchOptions_PerPage_MAX);
                        return photoCollection;
                    });

            var result = new PhotosetSearchPageResult(page, searchedPhotoSetsForPage);
            return result;
        }
        

        /// <summary>
        /// paralelni dohledani seznamu PhotoSetu
        /// </summary>
        public async Task<List<Photoset>> ParallelPhotosetsGetList(Action<ParallelOperationData<PhotosetSearchPageResult>> progressEvent = null, 
                                                                   int taskCount = DefaultParallelConnections)
        {
            var pageCount = taskCount;

            // pripadna podporapredcasneho ukonceni tasku, viz: https://johnbadams.wordpress.com/2012/03/10/understanding-cancellationtokensource-with-tasks/

            // seznam tasku s http dotazy - zatim rozbehnu nekolik uvodnich dotazu a postupne po jejich jednotlivem splneni pridavam dalsi
            var taskList = Enumerable.Range(1, pageCount)
                                     .Select(p => PhotosetsGetListForPage(p))
                                     .ToList();

            var data = new ParallelOperationData<PhotosetSearchPageResult>(GetTaskSheduler(), progressEvent, taskList);
            data.ReportProgress();

            while (taskList.Any())
            {
                // pockam na prvni dokonceny task
                Task<PhotosetSearchPageResult> firstFinishedTask = await Task.WhenAny(taskList);
                taskList.Remove(firstFinishedTask);
                PhotosetSearchPageResult finishedTask = await firstFinishedTask; // vysledek hotoveho tasku
            
                // seznam jiz stazenych photosetu
                var downloaded = data.Results.SelectMany(p => p.PhotosetCollection).ToList();
                // detekce posledni stranky - downloaduju stale dalsi a dalsi stranky az do doby kdy zacnou prichazet duplicitni zaznamy v PhotoCollection,
                // to urcite neni optimalni, ale flickr bohuzel nijak posledni stranku neidentifikuje :-/ takze ji musim takto pseudo dohledat
                var anyNewPhotosets = finishedTask.PhotosetCollection.Any(p => !downloaded.Any(r => r.PhotosetId == p.PhotosetId));
                
                // posledni stranka nebyla dohledana, takze rozbehnu task pro stahovani dalsi
                if (anyNewPhotosets)
                {
                    data.AddResult(finishedTask);
                    data.ReportProgress(finishedTask);
                    pageCount++;
                    taskList.Add(PhotosetsGetListForPage(pageCount));
                }
            }
            data.SetCompletedState();
            data.ReportProgress();

            var final = data.Results.SelectMany(p => p.PhotosetCollection).ToList();
            return final;
        }

  
        /// <summary>
        /// async akce pro dohledani Photo instanci pro jednu page
        /// </summary>
        private async Task<PhotoSearchPageResult> PhotosSearchPage(int page /*, CancellationToken ct - pripadna podpora predcasneho ukonceni tasku */)
        {
            var options = new PhotoSearchOptions
            {
                PrivacyFilter = PrivacyFilter.None,
                UserId = "me",
                MediaType = MediaType.All,
                PerPage = SearchOptions_PerPage_MAX,    // Number of photos to return per page. If this argument is omitted, it defaults to 100. The maximum allowed value is 500.
                Page = page,                            // The page of results to return.If this argument is omitted, it defaults to 1.
                Extras = PhotoSearchExtras.Tags,        // TODO !! PhotoSearchExtras.All,         // co chci, aby mi flickr api vratilo!! 
                TagMode = TagMode.AnyTag
            };

            var searchedPhotosForPage = await Task.Run(() =>
            {
                var photoCollection = PhotosSearch(options);
                return photoCollection;
            });

            var result = new PhotoSearchPageResult(options, searchedPhotosForPage);
            return result;
        }
        

        /// <summary>
        /// dohledani vsech Photo instacni na flickeru pro zalogovaneho usera
        /// </summary>
        public async Task<List<Photo>> ParallelGetPhotos(Action<ParallelOperationData<PhotoSearchPageResult>> progressEvent = null, 
                                                         int taskCount = DefaultParallelConnections)
        {
            var pageCount = taskCount;

            // seznam tasku s http dotazy - zatim rozbehnu nekolik uvodnich dotazu a postupne po jejich jednotlivem splneni pridavam dalsi
            var taskList = Enumerable.Range(1, pageCount)
                                     .Select(page => PhotosSearchPage(page))
                                     .ToList();

            // data s vysledky procesu, tasky a vstupnimi daty - pro drzeni mezi vysledku i reportovani
            var data = new ParallelOperationData<PhotoSearchPageResult>(GetTaskSheduler(), progressEvent, taskList);
            data.ReportProgress();

            while (taskList.Any())
            {
                Task<PhotoSearchPageResult> firstFinishedTask = await Task.WhenAny(taskList);
                taskList.Remove(firstFinishedTask);
                var finishedTask = await firstFinishedTask;
                
                // seznam jiz stazenych photosetu
                var downloadedId = data.Results.SelectMany(p => p.PhotoCollection).Select(p => p.PhotoId).ToList();
                // detekce posledni stranky - downloaduju stale dalsi a dalsi stranky az do doby kdy zacnou prichazet duplicitni zaznamy v PhotoCollection,
                // to urcite neni optimalni, ale flickr bohuzel nijak posledni stranku neidentifikuje :-/ takze ji musim takto pseudo dohledat
                //var newPhotos 
                var neexistujici = finishedTask.PhotoCollection.Select(p => p).Where(p => !downloadedId.Contains(p.PhotoId)).ToList();

                // posledni stranka nebyla dohledana, takze rozbehnu task pro stahovani dalsi
                if (neexistujici.Any())
                {
                    data.AddResult(finishedTask);
                    data.ReportProgress(finishedTask);
                    pageCount++;
                    taskList.Add(PhotosSearchPage(pageCount));
                }
            }
            data.SetCompletedState();
            data.ReportProgress();

            var final = data.Results.SelectMany(p => p.PhotoCollection).ToList();
            return final;
        }

      
        /// <summary>
        /// ziska TaskScheduler pro pripadne reportovani progressu do UI vlakna - pro cmdline aplikaci nejspis nema smysl
        /// </summary>
        private static TaskScheduler GetTaskSheduler()
        {
            TaskScheduler taskScheduler;
            if (SynchronizationContext.Current != null)
            {
                taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            else
            {
                // If there is no SyncContext for this thread (e.g. we are in a unit test
                // or console scenario instead of running in an app), then just use the
                // default scheduler because there is no UI thread to sync with.
                taskScheduler = TaskScheduler.Current;
            }
            return taskScheduler;
        }
                

        private async Task<UploadFileResult> UploadFileAsync(File4Flickr file4Flickr)
        {
            var result = await Task.Run(() =>
                 {
                     var uploadFileResult = UploadFile(file4Flickr);
                     return uploadFileResult;
                 });
            return result;
        }


        /// <summary>
        /// zakladni sablona spusteni paralelnich akce    
        /// 
        /// zakladni idea spusteni vice async akci paralelne viz: 
        /// "Start Multiple Async Tasks and Process Them As They Complete (C#)": https://msdn.microsoft.com/en-us/library/mt674889.aspx
        /// </summary>
        /// <typeparam name="TInput">typ input dat</typeparam>
        /// <typeparam name="TResult">typ vystupu</typeparam>
        /// <param name="inputData">vstupni data</param>
        /// <param name="singleAction">jednotliva akce jez bude spoustena paralelne</param>
        /// <param name="progressEvent">event pro reportovani</param>
        /// <param name="taskCount">kolik bude paralelnich behu</param>
        /// <returns>list TResult vysledku</returns>
        private async Task<List<TResult>> ParallelEngine<TInput, TResult>(List<TInput> inputData,
                                                                          Func<int, TInput, Task<TResult>> singleAction,
                                                                          Action<ParallelOperationDataWithInput<TResult, TInput>> progressEvent = null,
                                                                          int taskCount = DefaultParallelConnections)
            where TInput : class 
            where TResult : ParallelOperationResultBase
        {
            // kopie puvodnich vstupnich dat - z fronty postupne ubiram, 
            var workingQueue = inputData.ToQueue();

            // kolik paralelnich tasku budu pouzivat
            taskCount = Math.Min(workingQueue.Count, taskCount);

            // seznam tasku s http dotazy - zatim rozbehnu nekolik uvodnich dotazu a postupne po jejich jednotlivem splneni pridavam dalsi
            var taskList = Enumerable.Range(1, taskCount)
                                     .Select(i => singleAction(i, workingQueue.Dequeue())) 
                                     .ToList();
            var taskId = taskList.Count;
            // data s vysledky procesu, tasky a vstupnimi daty - pro drzeni mezivyseldku i reportovani
            var data = new ParallelOperationDataWithInput<TResult, TInput>(GetTaskSheduler(), progressEvent, taskList, inputData);
            data.ReportProgress();

            while (taskList.Any())
            {
                var firstFinishedTask = await Task.WhenAny(taskList);   // pockam na prvni dokonceny task
                taskList.Remove(firstFinishedTask);                     // dokonceny vyhodit ze seznamu
                var finishedResult = await firstFinishedTask;           // ziskat result od dokonceneho tasku

                // pokud je jeste co, zalozim dalsi task
                if (workingQueue.Any())
                {
                    taskList.Add(singleAction(taskId++ ,workingQueue.Dequeue()));   
                }
                data.AddResult(finishedResult);
                data.ReportProgress(finishedResult);
            }
            // finalni report
            data.SetCompletedState();
            data.ReportProgress();

            return data.Results;
        }


        /// <summary>
        /// paralelni pridani photos do set
        /// </summary>
        public async Task<List<AddPhotoToPhotosetResult>>ParallelAddPhotosToPhotoset(
                                                                    Photoset photoset,
                                                                    List<string> photoIds,
                                                                    Action<ParallelOperationDataWithInput<AddPhotoToPhotosetResult, string>> progressEvent = null,
                                                                    int taskCount = DefaultParallelConnections)
        {
            var result = await ParallelEngine(photoIds,  
                                              (taskId, photoId) => AddPhotoToPhotosetAsync(photoset, photoId),
                                              progressEvent, 
                                              taskCount);
            return result;
        }

        
        /// <summary>
        /// paralelni upload souboru
        /// </summary>
        public async Task<List<UploadFileResult>> ParallelUploadFiles(List<File4Flickr> files4Flickr,
                                                                      Action<ParallelOperationDataWithInput<UploadFileResult, File4Flickr>> progressEvent = null,
                                                                      int taskCount = DefaultParallelConnections)
        {
            var result = await ParallelEngine(files4Flickr,
                                              (taskId, file4flickr) => UploadFileAsync(file4flickr),
                                              progressEvent,
                                              taskCount);
            return result;
        }

        
        /// <summary>
        /// pro konkretni photoset sezene vsechny zarazene Photo 
        /// </summary>
        private async Task<PhotosetsGetPhotosAllResult> PhotosetsGetPhotosAllPages(string photosetId)
        {
            var result = new PhotosetsGetPhotosAllResult(photosetId);

            try
            {
                var photos = await Task.Run(() =>
                {
                    var page = 1;
                    int availablePages;
                    var photoIds = new List<string>();
                    do
                    {
                        var photosForPage = PhotosetsGetPhotos(photosetId, PhotoSearchExtras.None, page, SearchOptions_PerPage_MAX);
                        photosForPage.ToList().ForEach(p => photoIds.Add(p.PhotoId));

                        availablePages = photosForPage.Pages; // The number of pages available from Flickr.
                        page++;
                    } while (availablePages >= page);
                    return photoIds;
                });
                result.PhotoIdSet.AddRange((IEnumerable<string>)photos);
                result.Finalize();
            }
            catch (Exception ex)
            {
                result.Finalize(ex);
            }
            return result;
        }


        /// <summary>
        /// paralelni dohledani Photos od setu
        /// </summary>
        public async Task<List<PhotosetsGetPhotosAllResult>> ParallelGetPhotosFromPhotosets(
                                                                    List<Photoset> photosets,
                                                                    Action<ParallelOperationDataWithInput<PhotosetsGetPhotosAllResult, Photoset>> progressEvent = null,
                                                                    int taskCount = DefaultParallelConnections)
        {
            var result = await ParallelEngine(photosets,
                                              (taskId, photoset) => PhotosetsGetPhotosAllPages(photoset.PhotosetId),
                                              progressEvent,
                                              taskCount);
            return result;
        }

        
        /// <summary>
        /// pridani jednoho obrazku do photosetu
        /// </summary>
        private async Task<AddPhotoToPhotosetResult> AddPhotoToPhotosetAsync(Photoset photoset, string photoId)
        {
            var taskResult = await Task.Run(() =>
            {
                var result = new AddPhotoToPhotosetResult(photoset, photoId);
                try
                {
                    PhotosetsAddPhoto(photoset.PhotosetId, photoId);
                    result.Finalize();
                }
                catch (Exception ex)
                {
                    result.Finalize(ex);
                }
                return result;
            });

            return taskResult;
        }

        #endregion


        #region Constructors

        public FlickrExtender(string apiKey, string sharedSecret, VtLog logger, bool useDefaultProxy = true) : base(apiKey, sharedSecret)
        {
            InstanceCacheDisabled = true;

            // mam pouzit vychozi proxy z nastaveni windows? 
            if (!useDefaultProxy)
            {
                WebRequest.DefaultWebProxy = null;
            }

            Logger = logger;

            ProcessOAuth();
        }

        #endregion



        #region Other_Members
        
        /// <summary>
        /// provedeni OAuth atentifikace - budnajdu klice ulozene v ini nebo musi Flickr nove vygenerovat
        /// </summary>
        private void ProcessOAuth()
        {
            var secretIni = new FlickrExtenderSecretIni();

            // nejdrive zkusim dohrat ulozene hodnoty z ini souboru 
            // jinak pres OAuth ziskat pristupovy token
            if (secretIni.SecretExists)
            {
                OAuthAccessToken = secretIni.Token;
                OAuthAccessTokenSecret = secretIni.TokenSecret;
            }
            else
            {
                // v ini nic neni - Flickr musi vygnerovat nove
                var requestToken = OAuthGetRequestToken("oob");
                var url = OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);

                Console.WriteLine("Follow this URL to authorise yourself on Flickr: {0}", url);
                Console.Write("Paste in the token it gives you: ");
                var authToken = Console.ReadLine();
                try
                {
                    var accessToken = OAuthGetAccessToken(requestToken, authToken);
                    secretIni.Token = accessToken.Token;
                    secretIni.TokenSecret = accessToken.TokenSecret;
                    // TODO dotaz na Save?
                    secretIni.Save();
                }
                catch (Exception ex)
                {
                    Logger.Write(string.Format("Failed to get access token. Error message: {0}", ex.Message), VtLogState.Error);
                }
            }
        }
        

        private void RaiseSearchProgressEvent(string flickrAction, int progressCount, DateTime startTime)
        {
            if (SearchProgressEvent != null)
            {
                var eventData = new FlickrProgressEventData(flickrAction, progressCount, startTime);
                SearchProgressEvent(this, eventData);
            }
        }

        #endregion
    }

    





    



















}
