using System;

namespace FlickrNetExtender.Data.ProgressEventData
{
    /// <summary>
    /// base pro data do report eventuv paralelnich akcich
    /// </summary>
    public abstract class ProgressEventDataBase
    {
        public readonly DateTime StartTime;
        public readonly bool IsFinished;

        public ProgressEventDataBase(DateTime startTime, bool isFinished)
        {
            StartTime = startTime;
            IsFinished = isFinished;
        }
    }
}
