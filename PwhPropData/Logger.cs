using PwhPropData.Core.Interfaces;
using System;

namespace PwhPropData.Core
{
	public class Logger : ILogger
	{
		public string Messages { get; set; }

		public void LogMessage(string message)
		{
			Console.WriteLine(message);
		}

		public void LogError(string error)
		{
			Messages += $"{Environment.NewLine}-->{error}";
			Console.WriteLine($"--> {error}");
		}
		public void LogError(Exception ex, string error)
		{
			LogError(error);

			if ((ex != null) && (ex.InnerException != null))
			{
				LogError(ex.InnerException, ex.InnerException.Message);
			}

			AggregateException agEx = ex as AggregateException;
			if (agEx != null)
			{
				foreach (var e in agEx.InnerExceptions)
				{
					if (e.InnerException != null)
					{
						LogError(e.InnerException, e.InnerException.Message);
					}
				}
			}
		}
	}
}
