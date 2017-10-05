using System;

namespace PwhPropData.Core.Common
{
	/// <summary>
	/// Extensions for <see cref="String"/>.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Determines whether specified string is null or empty or whitespace.
		/// </summary>
		/// <param name="value">The value to verify.</param>
		/// <returns>
		///   <c>true</c> if is null or empty or whitespace; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNullOrWhitespace(this string value)
		{
			return string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim());
		}

		/// <summary>
		/// Determines whether <param name="source"/> contains <param name="value"/> with specific <see cref="StringComparison"/>
		/// </summary>
		/// <param name="source">The source string</param>
		/// <param name="value">The value to check</param>
		/// <param name="comparisonType">The type of comparison</param>
		/// <returns>True if source contains value, false otherwise</returns>
		public static bool Contains(this string source, string value, StringComparison comparisonType)
		{
			return source.IndexOf(value, comparisonType) >= 0;
		}

	}
}
