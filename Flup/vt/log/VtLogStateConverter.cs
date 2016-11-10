using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vt.log
{
	class VtLogStateConverter
	{
		public static ConsoleColor ToColor(VtLogState state)
		{
			switch (state)
			{
				case VtLogState.Info:
					return ConsoleColor.Yellow;
				case VtLogState.Debug:
					return ConsoleColor.Magenta;
				case VtLogState.Error:
					return ConsoleColor.Red;
				case VtLogState.Ok:
					return ConsoleColor.Green;
			}
			return Console.ForegroundColor; // jinak vracim vychozi barvu 
		}


		public static string ToString(VtLogState state)
		{
			switch (state)
			{
				case VtLogState.None:
					return null;
				case VtLogState.Info:
					return "Info";
				case VtLogState.Debug:
					return "Ddb";
				case VtLogState.Error:
					return "Err";
				case VtLogState.Ok:
					return "Ok";
				default:
					return "?";
			}
		}
	}
}
