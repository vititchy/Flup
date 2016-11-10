using FlickrNetExtender.Data.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlickrNetExtender.Data.Parallel
{
    /// <summary>
    /// trida s daty pro paralelni akci, ktera ma pouze vystupem
    /// </summary>
    /// <typeparam name="TResult">typ vystupu</typeparam>
    public class ParallelOperationData<TResult> : ParallelOperationDataBase<TResult> where TResult : ParallelOperationResultBase
    {
        private readonly  Action<ParallelOperationData<TResult>> reportProgressAction;


        public void ReportProgress(TResult finished = null)
        {
            if (reportProgressAction != null)
            {
                JustFinishedResult = finished;
                reportProgressAction.Invoke(this);
            }
        }

        
        /// <summary>
        /// konstruktor
        /// </summary>
        public ParallelOperationData(TaskScheduler taskScheduler, Action<ParallelOperationData<TResult>> reportProgressAction, List<Task<TResult>> taskList) 
            : base(taskScheduler, taskList)
        {
            this.reportProgressAction = reportProgressAction;
        }
    }

}
