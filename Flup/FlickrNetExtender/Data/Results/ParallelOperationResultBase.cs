using System;
using vt.extensions;

namespace FlickrNetExtender.Data.Results
{
    /// <summary>
    /// zakladni trida s vysledkem paralelni operace
    /// </summary>
    public abstract class ParallelOperationResultBase
    {

        public readonly DateTime StartTime;
        public DateTime? FinishTime { get; private set; }

        public TimeSpan Duration { get { return StartTime.ToDurationFrom(FinishTime ?? DateTime.Now); } }

        public bool IsCompleted { get; private set; }
        public bool IsSuccessfullyCompleted { get; private set; }

        public Exception Exception { get; private set; }

        public ParallelOperationResultBase(DateTime? startTime = null)
        {
            StartTime = startTime ?? DateTime.Now;
            IsSuccessfullyCompleted = false;
            IsCompleted = false;
            Exception = null;
            FinishTime = null;
        }


        public void Finalize(Exception exception = null)
        {
            Exception = exception;
            IsSuccessfullyCompleted = (exception == null);
            FinishTime = DateTime.Now;
            IsCompleted = true;
        }
    }
}
