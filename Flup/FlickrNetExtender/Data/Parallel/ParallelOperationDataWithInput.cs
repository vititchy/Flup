using FlickrNetExtender.Data.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlickrNetExtender.Data.Parallel
{
    /// <summary>
    /// trida s daty pro paralelni akci se vstupem i vystupem
    /// </summary>
    /// <typeparam name="TResult">typ vystupu</typeparam>
    /// <typeparam name="TInput">typ vstupu</typeparam>
    public class ParallelOperationDataWithInput<TResult, TInput> : ParallelOperationDataBase<TResult> 
        where TResult : ParallelOperationResultBase
        where TInput : class
    {
        public readonly IReadOnlyCollection<TInput> InputData;

        private readonly Action<ParallelOperationDataWithInput<TResult, TInput>> reportProgressAction;

        public int FinishedPercent
        {
            get
            {
                if ((Results != null) && (InputData != null))
                {
                    var finishedPercent = ((Results.Count / InputData.Count) * 100);
                    return finishedPercent;
                }
                return 0;
            }
        }


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
        public ParallelOperationDataWithInput(TaskScheduler taskScheduler, 
                                              Action<ParallelOperationDataWithInput<TResult, TInput>> reportProgressAction, 
                                              List<Task<TResult>> taskList,
                                              IReadOnlyCollection<TInput> inputData) : base(taskScheduler, taskList)
        {
            this.reportProgressAction = reportProgressAction;
            InputData = inputData;
        }
    }
}
