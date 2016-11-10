using System;
using vt.log;

namespace Flup
{
    class Program
    {        
        private static VtLog Logger = new VtLogAppFolder(showTime: true);

        static void Main(string[] args)
        {
            try
            {
                new FlickrNetExtender.Flup(Logger).Run();
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            Logger.Write("Finish.");


            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }
    }

}
