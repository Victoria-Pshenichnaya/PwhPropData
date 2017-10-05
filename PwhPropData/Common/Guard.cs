using System;

namespace PwhPropData.Core.Common
{
	public static class Guard
	{
		/// <summary>
		/// Verify if parameter not null.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		public static void NotNull(object parameter, string parameterName)
		{
			if (parameter == null)
				throw new ArgumentNullException(parameterName);
		}

		/// <summary>
		/// Verify if parameter not null.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message.</param>
		public static void NotNull(object parameter, string parameterName, string message)
		{
			if (parameter == null)
				throw new ArgumentNullException(parameterName, message);
		}

		/// <summary>
		/// Verify if parameter not null or empty.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		public static void NotNullOrEmpty(string parameter, string parameterName)
		{
			if (string.IsNullOrEmpty(parameter))
				throw new ArgumentNullException(parameterName);
		}

		/// <summary>
		/// Verify if parameter not null or empty.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message.</param>
		public static void NotNullOrEmpty(string parameter, string parameterName, string message)
		{
			if (string.IsNullOrEmpty(parameter))
				throw new ArgumentNullException(parameterName, message);
		}

		/// <summary>
		/// Verify if parameter not null or empty or whitespace.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		public static void NotNullOrWhitespace(string parameter, string parameterName)
		{
			if (parameter.IsNullOrWhitespace())
				throw new ArgumentNullException(parameterName);
		}

		/// <summary>
		/// Verify if parameter not null or empty or whitespace.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message.</param>
		public static void NotNullOrWhitespace(string parameter, string parameterName, string message)
		{
			if (parameter.IsNullOrWhitespace())
				throw new ArgumentNullException(parameterName, message);
		}
	}
}
