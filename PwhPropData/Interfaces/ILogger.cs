using System;

namespace PwhPropData.Core.Interfaces
{
	public interface ILogger
	{
		void LogMessage(string message);
		void LogError(string error);
		void LogError(Exception ex, string error);
	}
}
