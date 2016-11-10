using FlickrNet;
using FlickrNetExtender.Data;
using FlickrNetExtender.Data.Parallel;
using FlickrNetExtender.Data.Results;
using FlickrNetExtender.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vt.extensions;
using vt.log;

namespace FlickrNetExtender
{
    public class Flup
    {
        /// <summary>
        /// Every Flickr API application needs to obtain an API 'key'
        /// https://www.flickr.com/services/api/auth.howto.web.html
        /// </summary>
        private const string FlupApiKey = "f0ad8c2163ac5d0af97f94c9c1f7b299";

        /// <summary>
        /// A 'shared secret' for the api key is then issued by flickr. This secret is used in the signing process
        /// https://www.flickr.com/services/api/auth.spec.html
        /// </summary>
        private const string FlupSharedSecret = "21262bc2e93fc737";


        private readonly VtLog _logger;
        private readonly IniHelper _ini;


        /// <summary>
        /// konstruktor
        /// </summary>
        public Flup(VtLog logger)
        {
            _logger = logger;
            _ini = new IniHelper();
        }

        
        /// <summary>
        /// zakladni beh flupu
        /// </summary>
        public void Run()
        {            
            var localFiles = GetLocalFiles();                                       // seznam lokalnich souboru 
            if (localFiles.Any())
            {
                var flickr = CreateAuthenticatedFlickrInstance();
                var flupFlickrPhotos = GetFlupFlickrPhotos(flickr).Result;          // seznam vsech Photo z flickru - paralelni metoda pak ceka na dokonceni vsech tasku 
                var photosWithoutSet = CheckPhotosWithoutPhotoset(flickr, flupFlickrPhotos); // zjisteni Photo instanci na flickru, ktere nepatri do zadneho Photosetu

                List<PathWithFlickrFiles> newLocalFiles;                            // porovnanim lokalniho adresare a obsahu flickru ziskam seznam novinek na lokalu
                List<PathWithFlickrFiles> filesWithoutSet;
                FindNewLocalFiles(localFiles, flupFlickrPhotos, photosWithoutSet, out newLocalFiles, out filesWithoutSet); 
                UpdateUnassignedPhotos(flickr, filesWithoutSet, photosWithoutSet);  // nastavit photoset u nezarazenych photos
                SynchronizePhotos(flickr, newLocalFiles);                           // novinky nahrat na flickr
            }
        }


        /// <summary>
        /// Dodatecne zarazeni Photo instanci (jiz existujicich na Flickru) do Photosetu
        /// </summary>
        private void UpdateUnassignedPhotos(FlickrExtender flickr, List<PathWithFlickrFiles> filesWithoutSet, List<Photo> photosWithoutSet)
        {
            if (filesWithoutSet.Any())
            {
                var flickrPhotoSets = flickr.ParallelPhotosetsGetList(ParallelPhotosetsGetListProgressEvent).Result;

                var photosWithoutSetCrc
                        = photosWithoutSet
                            // TODO predelat na extension metodu Photo classu?
                            .SelectMany(photo => photo.Tags.Where(tag => !string.IsNullOrEmpty(tag) && tag.StartsWith(File4Flickr.FLUPCRC_TAG, StringComparison.InvariantCultureIgnoreCase))
                            .Select(crcTag =>
                            {
                                var crcFromTag = crcTag.Remove(0, File4Flickr.FLUPCRC_TAG.Length);
                                long crc;
                                long.TryParse(crcFromTag, out crc);
                                return new { photo, crc };
                            }))
                            .ToList();

                foreach (var pathWithFiles in filesWithoutSet)
                {
                    // zpetne dohledam pres CRC instanci Photo na serveru
                    var crcInPath = pathWithFiles.Photos.Select(p => p.Crc).ToList();
                    // seznam unassigned PhotoId pro tento adresar (photoset)
                    var photoIdsForPath = photosWithoutSetCrc.Where(p => crcInPath.Contains(p.crc)).Select(p => p.photo.PhotoId).ToList(); // TODO nasledne by se tu mel nejak resit orderby? nejspis by mel pribyt dalsi FLUP tag s poradim? zatim na to kaslu, pro potreby zalohovani to nema smysl

                    AddFilesToPhotoset(flickr, flickrPhotoSets, pathWithFiles.PhotosetName, photoIdsForPath);
                }
            }
        }


        /// <summary>
        /// Porovnanim (pres CRC) souboru z lokalniho disku a souboru z Flickeru dohleda nove soubory na lokalu 
        /// 
        /// + kvulu rychlosti rovnou dohledam i seznam souboru, ktere patri k Photo instancim, ktere jiz na Flickru jsou, ale nejsou zarazeny do Photoseto 
        /// (coz obvykle nastane prerusenim prenosu mezi uploadem fotky a zarazeni do setu)
        /// </summary>
        /// <param name="sourcePaths">seznam zdrojovych adresaru ze soubory</param>
        /// <param name="flupFlickrPhotos">seznam FLUP souboru z Flickeru</param>
        /// <returns></returns>
        private void FindNewLocalFiles(Dictionary<DirectoryInfo, List<FileInfo>> sourcePaths, 
                                       List<Photo> flupFlickrPhotos,
                                       List<Photo> photosWithoutSet,
                                       out List<PathWithFlickrFiles> newLocalFiles,
                                       out List<PathWithFlickrFiles> filesWithoutSet)
        {
            _logger.Write("Looking for new local files...");

            var flupFlickrPhotosCrcSet = GetNumericCRC(flupFlickrPhotos);
            var photosWithoutSetCrcSet = GetNumericCRC(photosWithoutSet);

            var startTime = DateTime.Now;
            var filesTotal = sourcePaths.Sum(p => p.Value.Count);
            var filesProcessed = 0;
            var bytesTotal = sourcePaths.Sum(p => p.Value.Sum(f => f.Length));
            var bytesProcessed = 0L;

            newLocalFiles = new List<PathWithFlickrFiles>();
            filesWithoutSet = new List<PathWithFlickrFiles>();

            foreach (var sourcePath in sourcePaths)
            {
                // ze souboru v directory zalozim set File4Flickr se spoctenym CRC - paralelne to je o dost rychlejsi
                var f4fSet = sourcePath.Value.AsParallel()
                                             .Select(fileInfo => new File4Flickr(fileInfo, _ini.SourcePath, _ini.NumberOfBytesForCRC).PrepareCrc())
                                             .ToList();

                var newInPath = new List<File4Flickr>();
                var withoutSet = new List<File4Flickr>();
                foreach (var f4f in f4fSet)
                {
                    // uz existuje na flickru? (poznam pres tag FLUPCRCxxxxx u fotky)
                    var crcExistsOnFlickr = flupFlickrPhotosCrcSet.Contains(f4f.Crc);
                    if (crcExistsOnFlickr)
                    {
                        var withoutPhotoset = photosWithoutSetCrcSet.Contains(f4f.Crc);
                        if (withoutPhotoset)
                        {
                            withoutSet.Add(f4f); // soubor/photo jiz na flickeru je, neni vsak v zadnem photosetu
                        }
                    }
                    else
                    {
                        newInPath.Add(f4f); // jeste na flickru neexistuje
                    }
                }
                // nejake nove soubory v adresari?
                if (newInPath.Any())
                {
                    var directoryWithPhotos = new PathWithFlickrFiles(sourcePath.Key, newInPath);
                    newLocalFiles.Add(directoryWithPhotos);
                }
                // nejake soubory/photo bez photosetu?
                if (withoutSet.Any())
                {
                    var unassignedPhotos = new PathWithFlickrFiles(sourcePath.Key, withoutSet);
                    filesWithoutSet.Add(unassignedPhotos);
                }

                bytesProcessed += f4fSet.Sum(p => p.Length);
                filesProcessed += f4fSet.Count;
                GetNewLocalFilesPrintProgress(startTime, filesTotal, filesProcessed, bytesTotal, bytesProcessed);
            }

            GetNewLocalFilesPrintFinalMessage(newLocalFiles);
        }

        
        /// <summary>
        /// pro zrychlene dohledani existence CRC si existujici CRC vykousnu ze stringu do pole longu
        /// </summary>
        /// <returns>seznam CRC prevedenych na cislo</returns>
        private static List<long> GetNumericCRC(List<Photo> photos)
        {
            var result = photos
                            .SelectMany(p => p.Tags.Where(tag => !string.IsNullOrEmpty(tag) && tag.StartsWith(File4Flickr.FLUPCRC_TAG, StringComparison.InvariantCultureIgnoreCase))
                            .Select(crcTag =>
                                {
                                    var crcFromTag = crcTag.Remove(0, File4Flickr.FLUPCRC_TAG.Length);
                                    long crc;
                                    long.TryParse(crcFromTag, out crc);
                                    return crc;
                                }))
                            .ToList();
            return result;
        }


        private void GetNewLocalFilesPrintFinalMessage(List<PathWithFlickrFiles> newLocalDirs)
        {
            if (newLocalDirs.Any())
            {
                var newPhotosCount = newLocalDirs.SelectMany(p => p.Photos).Count();
                var newPhotosFileSize = newLocalDirs.SelectMany(p => p.Photos).Sum(p => p.Length);
                _logger.Write(string.Format("New files were found: {0} folders, {1} files, {2}", newLocalDirs.Count, newPhotosCount, newPhotosFileSize.ToFileSize()));
            }
            else
            {
                _logger.Write("No new files.");
            }
        }


        private void GetNewLocalFilesPrintProgress(DateTime startTime, float filesTotal, float filesProcessed, long bytesTotal, long bytesProcessed)
        {
            var finishedPercent = ((filesProcessed / filesTotal) * 100);
            var duration = DateTime.Now - startTime;
            var eta = bytesProcessed.ToETA(bytesTotal, duration);

            Console.Write(string.Format("\r{0}% ({1} of {2}, {3} of {4})  ETA:{5:mm\\:ss}      \r", 
                                    (int)finishedPercent,
                                    filesProcessed, 
                                    filesTotal,
                                    bytesProcessed.ToFileSize(), 
                                    bytesTotal.ToFileSize(),
                                    eta));
        }


        /// <summary>
        /// vytvoreni instance VtFlickr
        /// </summary>
        private FlickrExtender CreateAuthenticatedFlickrInstance()
        {
            _logger.Write("Trying to connect Flickr...");

            var flickr = new FlickrExtender(FlupApiKey, FlupSharedSecret, _logger, _ini.UseDefaultProxy).VtCheckAuth();
            return flickr;
        }


        /// <summary>
        /// dohledani souboru z Flickru, kter maji tag FLUP
        /// - zde by bylo idealni pouzit vyhledavaci VtPhotosSearchByTag, ktera pouziva fce flickeru PhotosSearch, bohuzel vyhledavani chvili funguje
        /// a chvili zas ne :-/ takze vezmu vse a rucne vyhodim neflupove soubory :-/
        /// </summary>
        private async Task<List<Photo>> GetFlupFlickrPhotos(FlickrExtender flickr)
        {
            _logger.Write("Looking for Flickr files...");

            var allFlickrFilesFromUser = await flickr.ParallelGetPhotos(PhotosSearchReportProgress);
            return allFlickrFilesFromUser;

            // obcas funguje, obcas ne :-/ - vubec prace s Flickr tagama je dost podivna, nektere si upravi, nektere neupravi a search API je nedohleda :-/
            //
            // var flickrPhotoCollection = flickr.VtPhotosSearchByTag(File4Flickr.FLUP_TAG);
            // _logger.Write(string.Format("Found {0} photos with {1} tag.", flickrPhotoCollection.Count, File4Flickr.FLUP_TAG));
            // return flickrPhotoCollection;
        }


        private void PhotosSearchReportProgress(ParallelOperationData<PhotoSearchPageResult> data)
        {
            if (data.IsCompleted)
            {
                // finani hlaseni, dopoctu i pocet photo instanci na flickru, ktere maji FLUP tag
                var withFlupTag = data.Results.SelectMany(p => p.PhotoCollection).Count(p => p.Tags.Any(t => string.Compare(t, File4Flickr.FLUP_TAG, ignoreCase: true) == 0));
                var message = string.Format("Found: {0} flickr files ({1} with FLUP tag).", data.Results?.SelectMany(p => p.PhotoCollection).Count(), withFlupTag);
                _logger.Write(message);
            }
            else
            {
                // prubezny progress
                var message = string.Format("Found: {0} flickr files. (T:{1})", data.Results?.SelectMany(p => p.PhotoCollection).Count(), data.Tasks?.Count);
                WriteProgressMsg(data.IsCompleted, message);
            }
        }


        /// <summary>
        /// provede nahrani novych souboru z lokalu na Flickr
        /// </summary>
        private void SynchronizePhotos(FlickrExtender flickr, List<PathWithFlickrFiles> newDirs)
        {
            if (newDirs.Any())
            {
                var flickrPhotoSets = flickr.ParallelPhotosetsGetList(ParallelPhotosetsGetListProgressEvent).Result;

                var totalCount = newDirs.SelectMany(p => p.Photos).Count();
                var totalLength = newDirs.SelectMany(p => p.Photos).Sum(p => p.Length);
                var uploadedLength = 0L;
                var uploadedCount = 0;
                var totalStart = DateTime.Now;

                // pripadne prazdne adresare vyhodim - nemeli by tu vsak uz byt ;-)
                newDirs = newDirs.Where(p => p.Photos.Any()).ToList();

                foreach (var dir in newDirs)
                {
                    _logger.Write(string.Format("Processing folder:'{0}'-'{1}' files:{2}", dir.DirectoryInfo.FullName, dir.PhotosetName, dir.Photos.Count));

                    // pridat soubory
                    var uploadFileResults = flickr.ParallelUploadFiles(dir.Photos, p => ParallelUploadFilesReportAction(p)).Result;

                    // ty pak zaradit do photosetu
                    AddFilesToPhotoset(flickr, flickrPhotoSets, dir.PhotosetName, uploadFileResults);

                    // jen hlaseni o prubehu
                    ReportTotalProgress(totalCount, totalLength, ref uploadedLength, ref uploadedCount, totalStart, uploadFileResults);
                }
            }
        }


        private void ParallelPhotosetsGetListProgressEvent(ParallelOperationData<PhotosetSearchPageResult> data)
        {
            var message = string.Format("Found: {0} photosets.", data.Results.SelectMany(p => p.PhotosetCollection).Count());
            WriteProgressMsg(data.IsCompleted, message);
        }


        private void WriteProgressMsg(bool isCompleted, string message)
        {
            Console.Write("\r");
            if (isCompleted)
            {
                _logger.Write(message); // finalni zapis do logu
            }
            else
            {
                Console.Write(message);  // pseudo progress ;-) ... updatuju porad stejnou radku
            }
        }


        private void ParallelUploadFilesReportAction(ParallelOperationDataWithInput<UploadFileResult, File4Flickr> data)
        {
            if (data.JustFinishedResult != null)
            {
                _logger.Write(string.Format("Upload file {0} of {1}, Filename:'{5}' PhotoId:{2} (R:{3} T:{4})",
                                        data.Results.Count,
                                        data.InputData.Count,
                                        data.JustFinishedResult.PhotoId,
                                        data.JustFinishedResult.RetryAttemp, 
                                        data.Tasks.Count,
                                        data.JustFinishedResult.File4Flickr.RelativePathWithFileName));
            }

            if (data.IsCompleted)
            {
                var uploadFileSizeSum = data.Results.Where(p => p.File4Flickr != null).Sum(p => p.File4Flickr.Length);
                _logger.Write(string.Format("Upload finished: {0} files {1} at {2}", 
                                        data.Results.Count, 
                                        uploadFileSizeSum.ToFileSize(), 
                                        uploadFileSizeSum.ToBitesPerSecSpeed(data.Duration)));
            }
        }


        private void AddPhotosToPhotosetReportAction(ParallelOperationDataWithInput<AddPhotoToPhotosetResult, string> data)
        {
            if (data.JustFinishedResult != null)
            {
                if (data.JustFinishedResult.IsSuccessfullyCompleted)
                {
                    _logger.Write(string.Format("File {0} added to photoset {1} (D:{2} T:{3})",
                        data.JustFinishedResult.PhotoId, data.JustFinishedResult.Photoset?.PhotosetId, data.JustFinishedResult.Duration.ToDuration(),
                        data.Tasks?.Count()));
                }
                else
                {
                    _logger.Write(string.Format("AddPhotoToPhotoset error: {0} (PhotoId:{1} Photoset:{2})",
                        data.JustFinishedResult.Exception?.Message, data.JustFinishedResult.PhotoId, data.JustFinishedResult.Photoset), VtLogState.Error);
                }
            }
            if (data.IsCompleted)
            {
                _logger.Write(string.Format("Added {0} files to photoset {1}. (D:{2})",
                    data.Results.Count, data.Results.FirstOrDefault()?.Photoset?.PhotosetId,
                    data.Duration.ToDuration()));
            }
        }

        
        private void ReportTotalProgress(int totalCount, long totalLength, ref long uploadedLength, ref int uploadedCount, DateTime totalStart, List<UploadFileResult> uploadFileResults)
        {
            uploadedCount += uploadFileResults.Count;
            uploadedLength += uploadFileResults.Sum(p => p.File4Flickr.Length);
            var totalDuration = DateTime.Now - totalStart;

            var finishedPercent = uploadedLength.ToPercentFromTotal(totalLength);
            _logger.Write(string.Format("Total progress {0} of {1} {2} of {3} {4} {5} ETA:{6})",
                                             uploadedCount,
                                             totalCount,
                                             uploadedLength.ToFileSize(),
                                             totalLength.ToFileSize(),
                                             finishedPercent,
                                             uploadedLength.ToBitesPerSecSpeed(totalDuration),
                                             uploadedLength.ToETA(totalLength, totalDuration)));
        }


        /// <summary>
        /// zarazeni set of Photo do Photosetu
        /// </summary>
        private void AddFilesToPhotoset(FlickrExtender flickr, List<Photoset> photosetCollection, string photoSetTitle, List<UploadFileResult> uploadedPhotos)
        {
            if (uploadedPhotos.Any())
            {
                // vychozi trideni dle filename
                var sortedFiles = uploadedPhotos.OrderBy(p => p.File4Flickr.FullName).Select(p => p.PhotoId).ToList();
                AddFilesToPhotoset(flickr, photosetCollection, photoSetTitle, sortedFiles);
            }
        }


        /// <summary>
        /// zaradi seznam Photo do Photosetu
        /// </summary>
        /// <param name="photosetCollection">seznam Photoset instanci z Flickru</param>
        /// <param name="photoSetTitle">title photosetu kam budu photo instance sazet - nemusi existovat</param>
        /// <param name="sortedPhotoIdSet">poradi Photos</param>
        private void AddFilesToPhotoset(FlickrExtender flickr, List<Photoset> photosetCollection, string photoSetTitle, List<string> sortedPhotoIdSet)
        {
            // existuje uz PhotoSet na flickru?
            var photoset = photosetCollection.FirstOrDefault(p => string.Equals(p.Title, photoSetTitle, StringComparison.OrdinalIgnoreCase));
            if (photoset == null)
            {
                // prvni photo - nutne pro zalozeni photosetu
                var firstPhotoId = sortedPhotoIdSet.FirstOrDefault();
                sortedPhotoIdSet.Remove(firstPhotoId);
                // zalozeni noveho photosetu + refreshnu seznam photosetu 
                photoset = flickr.PhotosetsCreate(photoSetTitle, firstPhotoId);
                _logger.Write(string.Format("Created new photoset Id:{0} Primary photoId:{1}", photoset.PhotosetId, firstPhotoId));

                // refresh seznam photosetu - nebo teoreticky by melo stacit pridat novy photose do kolekce, ale mozna je lepsi si stahnout aktualni data? zatim necham ADD
                photosetCollection.Add(photoset);
            }

            // paralelni pridani fotek - veskere reporty pres report event - tj i finalni report s ETA atd
            flickr.ParallelAddPhotosToPhotoset(photoset, sortedPhotoIdSet, AddPhotosToPhotosetReportAction, 15).Wait();

            // nasledne nastavit poradi fotek v photosetu - ze je primary photo uz removed zrejme nevadi, poradi si uchova
            flickr.PhotosetsReorderPhotos(photoset.PhotosetId, sortedPhotoIdSet.ToArray());

            _logger.Write(string.Format("Reorder photoset:{0} files:({1})", photoset.PhotosetId, string.Join(",", sortedPhotoIdSet)));
        }


        /// <summary>
        /// read a list of path + files in source ini path
        /// </summary>
        private Dictionary<DirectoryInfo, List<FileInfo>> GetLocalFiles()
        {
            _logger.Write(string.Format("Scanning {0}...", _ini.SourcePath));

            var filesInPath = DirectoryExtension.GetFilesByExt(_ini.SourcePath, _ini.SearchPattern, ',');
            var sourceFiles = filesInPath.OrderBy(p => p)
                                         .GroupBy(p => Path.GetDirectoryName(p))
                                         .ToDictionary(p => new DirectoryInfo(p.Key), p => p.Select(f => new FileInfo(f)).ToList());

            var lengthSum = filesInPath.Sum(p => new FileInfo(p).Length);
            _logger.Write(string.Format("{0} folders, {1} files, {2}", sourceFiles.Count, filesInPath.Count, lengthSum.ToFileSize()));

            return sourceFiles;
        }


        /// <summary>
        /// Resi problem s Photo soubory, ktere byly uspesne preneseny na Flickr, ale jiz nedoslo k zarazeni do Photosetu (preruseni prenosu atd), 
        /// tj ted visi na serveru nezarazene. Bohuzel Flickr API nenabizi zadnou fci na dohledani techto souboru, je tedy jedina moznost vzit vsechny 
        /// Flup fotky + Photosety a po stav porovnat :-/
        /// </summary>
        /// <param name="flupFlickrPhotos">seznam vsech Photo instanci na flickru</param>
        /// <returns>seznam Photo instanci na flickeru, ktere nejsou zarazeny v Photosetu</returns>
        private List<Photo> CheckPhotosWithoutPhotoset(FlickrExtender flickr, List<Photo> flupFlickrPhotos)
        {
            //1. seznam photosetu
            var photosets = flickr.ParallelPhotosetsGetList(ParallelPhotosetsGetListProgressEvent).Result;

            //2. seznam Photo od vsech Photoset - API nic takoveho neumoznuje, takze je nutne projit Photosety rucne
            var photosetsGetPhotosAllResultSet = flickr.ParallelGetPhotosFromPhotosets(photosets, ParallelGetPhotosFromPhotosetsProgressEvent, 10).Result;

            //3. vybrat z flupFlickrPhotos fotky bez photosetu
            var allPhotoIdWithPhotosetSet = photosetsGetPhotosAllResultSet.SelectMany(x => x.PhotoIdSet).ToList();
            var photosWithoutPhotoset = flupFlickrPhotos.Where(p => !allPhotoIdWithPhotosetSet.Contains(p.PhotoId)).ToList();
            return photosWithoutPhotoset;
        }


        private void ParallelGetPhotosFromPhotosetsProgressEvent(ParallelOperationDataWithInput<PhotosetsGetPhotosAllResult, Photoset> data)
        {
            var eta = ((long)(data.Results.Count)).ToETA(data.InputData.Count, data.Duration);
            var message = string.Format("R:{0} I:{2} T:{1} E:{3}", data.Results.Count, data.Tasks.Count, data.InputData.Count, eta);
            if (data.JustFinishedResult != null)
            {
                message += string.Format(" FIN D:{0}", data.JustFinishedResult.Duration);  // TODO pouze debug hlaseni - nezbrazovat?
            }
            WriteProgressMsg(data.IsCompleted, message);
        }

    }
}
