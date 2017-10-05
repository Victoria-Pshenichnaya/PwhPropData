using PwhPropData.Core.Common;
using PwhPropData.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PwhPropData.Core.Storages
{
	public interface IAdcStorage
	{ 
		Task<string> GetData(int portfolioId);
	}
	public class AdcStorage : IAdcStorage
	{
		private readonly ILogger _logger = null;

		public AdcStorage(ILogger logger)
		{
			Guard.NotNull(logger, nameof(logger));
			_logger = logger;
		}

		public async Task<string> GetData(int portfolioId)
		{
			string requestBody = Settings.AdcRequestBody.Replace(Settings.AdcRequestBodyPortfolioIdStr, portfolioId.ToString());
			IList<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("reutersuuid", Settings.UUID) };
			string contentType = "application/x-www-form-urlencoded; charset=UTF-8";
			string method = "POST";
			return await GetData(Settings.AdcUrl, contentType, method, headers, requestBody);
		}

		private async Task<string> GetData(string requestUri, string contentType, string method, IEnumerable<KeyValuePair<string, string>> headers, string requestBody)
		{
			string responseText = null;

			// Create a request object
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(requestUri);

			// Mundane details...
			req.ContentType = contentType;
			req.Method = method;
			foreach (var header in headers)
			{
				req.Headers.Add(header.Key, header.Value);
			}

			// Preparing request body

			// Encoding the request...
			UTF8Encoding encoding = new UTF8Encoding();
			using (Stream requestStream = req.GetRequestStream())
				requestStream.Write(encoding.GetBytes(requestBody), 0, encoding.GetByteCount(requestBody));

			// ... sending it and receiving the response
			try
			{
				var resp = await req.GetResponseAsync();
				using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
				{
					responseText = sr.ReadToEnd();
				}
			}
			catch (WebException e)
			{
				_logger.LogError(e.Message);
			}
			return responseText;
		}
	}
}
