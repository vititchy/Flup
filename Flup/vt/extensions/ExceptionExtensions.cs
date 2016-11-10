using System;
using System.Collections.Generic;

namespace vt.extensions
{
    public static class ExceptionExtensions
    {
        public static List<string> InnerExceptionsSet(this Exception ex)
        {
            var exceptionsSet = new List<string>();
            InnerEx(ref exceptionsSet, ex);
            return exceptionsSet; 
        }

        private static void InnerEx(ref List<string> exceptionsSet, Exception ex)
        {
            if(ex != null)
            {
                exceptionsSet.Add(ex.Message);
                InnerEx(ref exceptionsSet, ex.InnerException);
            }
        }
    }
}
