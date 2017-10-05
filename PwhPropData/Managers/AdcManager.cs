using Newtonsoft.Json;
using PwhPropData.Core.Interfaces;
using PwhPropData.Core.Storages;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace PwhPropData.Core.Managers
{
	public class AdcData
	{
		public string cols { get; set; }
		public string status { get; set; }
		public object[][] rows { get; set; }
	}

	public class AdcManager : IAdcManager
	{
		private readonly ILogger _logger = null;
		private readonly IAdcStorage _adcStorage = null;
		public AdcManager(ILogger logger, IAdcStorage adcStorage)
		{
			_logger = logger;
			_adcStorage = adcStorage;
		}

		public async Task<IEnumerable<KeyValuePair<string, double>>> GetRecommendations(int portfolioId)
		{
			string json = await _adcStorage.GetData(portfolioId);
			return DeserializeJson(json);
		}

		private IEnumerable<KeyValuePair<string, double>> DeserializeJson(string json)
		{
			AdcData jsonData = JsonConvert.DeserializeObject<AdcData>(json);

			for (int i = 1; i < jsonData.rows.Length; i++)
			{
				string key = jsonData.rows[i][0].ToString();
				string valueStr = jsonData.rows[i][1].ToString();
				if (!double.TryParse(valueStr, out double value)) break;

				yield return new KeyValuePair<string, double>(key, value);
			}
		}
	}
}
