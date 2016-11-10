using System;
using System.IO;
using System.Linq;
using System.Net;
using vt.extensions;

namespace vt.log
{
    public abstract class VtLog
	{
        protected static readonly string[] EXE_EXTENSIONS = {
            ".vshost.exe",
            ".exe"
        };
        protected static readonly string DEFAULT_LOG_FILENAME = "VT_LOG";
        protected static readonly string DEFAULT_EXT = ".log";

		private readonly string _logFileName;
		private readonly int _maxLineLen;
		private readonly bool _showTime;

		protected VtLog(string logFileName, int maxLineLen = 0, bool showTime = false)
		{
			_logFileName = logFileName;
			_maxLineLen = maxLineLen;
			_showTime = showTime;

            WriteStartInfo();
		}

  
        private void WriteStartInfo()
        {
            var startInfo = string.Format("START - {0}-{1}, FW:{2} OS:{3}",
                                          System.Reflection.Assembly.GetExecutingAssembly().FullName,
                                          System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                                          Environment.Version,
                                          Environment.OSVersion);
            Write(new string('=', startInfo.Length));
            Write(startInfo);
        }


        public void Write(string text, VtLogState state = VtLogState.None)
		{
			string toFileText;
			var timeStr = toFileText = GetTimeStr();
			Console.Write(timeStr);

			if (state != VtLogState.None)
			{
				var stateStr = VtLogStateConverter.ToString(state);
				if (!string.IsNullOrEmpty(stateStr))
				{
					stateStr = string.Format("[{0}] ", stateStr);
				}
				var prevForeColor = Console.ForegroundColor;
				Console.ForegroundColor = VtLogStateConverter.ToColor(state);
				Console.Write(stateStr);
				Console.ForegroundColor = prevForeColor;
				toFileText += stateStr;
			}

			text = string.Format("{0}", text);
			toFileText += text;
			Console.WriteLine(text);

			LogToFile(toFileText);
		}


		public void Write(Exception ex, bool debugInfo = true, VtLogState state = VtLogState.Error)
		{
            Write("Exception: ".PadRight(60, '='), state);
            if (ex != null)
            {
                Write(string.Format("Type: {0}", ex.GetType().FullName), state);
                Write(string.Format("Message: {0}", ex.Message), state);
                var webEx = ex as WebException;
                if (webEx != null)
                {
                    Write(string.Format("WebException.Status: {0}", webEx.Status), state);
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        Console.WriteLine("WebException.Response.Status Code: {0}", ((HttpWebResponse)webEx.Response).StatusCode);
                        Console.WriteLine("WebException.Response.Status Description: {0}", ((HttpWebResponse)webEx.Response).StatusDescription);
                    }
                }

                var exceptionsSet = ex.InnerExceptionsSet();
                if (exceptionsSet.Count > 1)
                {
                    Write("Exception list: ", state);
                    exceptionsSet.ForEach(p => Write(p, state));
                }

                if (debugInfo)
                {
                    var newLineSeparator = new string[] { Environment.NewLine };
                    Write("Source: ", state);
                    ex.Source.Split(newLineSeparator, StringSplitOptions.None).ToList().ForEach(p => Write(p, state));

                    Write("StackTrace: ", state);
                    //ar exceptionsSet = ex.InnerExceptionsSet();
                    ex.StackTrace.Split(newLineSeparator, StringSplitOptions.None).ToList().ForEach(p => Write(p, state));
                }
            }
            else
            {
                Write("[exception is null ?]");
            }
            Write(string.Empty.PadRight(60, '='), state);
        }
        

        private void LogToFile(string toFileText)
		{
			if (!string.IsNullOrEmpty(_logFileName))
			{
				File.AppendAllText(_logFileName, (toFileText != null) ? toFileText + Environment.NewLine : Environment.NewLine);
			}
		}

		
		private string GetTimeStr()
		{
			if (_showTime)
			{
				return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}.{6:000} ", 
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
			}
			else
			{
				return string.Empty;
			}
		}

        protected static string GetExeLogFileName()
        {
            var appName = AppDomain.CurrentDomain.FriendlyName;
            // exe filename bez koncove casti
            var exeFilename = EXE_EXTENSIONS.Where(p => appName.EndsWith(p, StringComparison.InvariantCultureIgnoreCase))
                                            .Select(p => appName.RemoveEndText(p))
                                            .FirstOrDefault();

            return exeFilename ?? DEFAULT_LOG_FILENAME;
        }

    }
}