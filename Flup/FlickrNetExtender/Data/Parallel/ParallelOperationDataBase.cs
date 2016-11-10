using FlickrNetExtender.Data.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vt.extensions;

namespace FlickrNetExtender.Data.Parallel
{
    /// <summary>
    /// zakladni base trida s daty pro paralelni akci
    /// </summary>
    /// <typeparam name="TResult">typ vysledku</typeparam>
    public abstract class ParallelOperationDataBase<TResult> 
        where TResult : ParallelOperationResultBase
    {
        public readonly DateTime StartTime;

        public DateTime? FinishTime { get; private set; }

        /// <summary>
        /// async operation duration 
        /// </summary>
        public TimeSpan Duration { get { return StartTime.ToDurationFrom(FinishTime ?? DateTime.Now); } }

        public readonly TaskScheduler TaskScheduler;

        public readonly List<Task<TResult>> Tasks;


        public readonly List<TResult> Results;

        public TResult JustFinishedResult { get; protected set; }

        /// <summary>
        /// async operation is completed
        /// </summary>
        public bool IsCompleted { get; private set; }


        public void SetCompletedState()
        {
            IsCompleted = true;
            FinishTime = DateTime.Now;
        }


        public void AddResult(TResult finishedResult)
        {
            Results.Add(finishedResult);
        }
        

        public ParallelOperationDataBase(TaskScheduler taskScheduler, List<Task<TResult>> taskList)
        {
            StartTime = DateTime.Now;
            IsCompleted = false;
            Results = new List<TResult>();
            TaskScheduler = taskScheduler;
            Tasks = taskList;
        }
    }
}
