using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PwhPropData.Core.Interfaces;
using PwhPropData.Core.Storages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwhPropData.Core.Managers
{
	public class adcData
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

		public IEnumerable<KeyValuePair<string, double>> GetRecommendations(int portfolioId)
		{
			string json = _adcStorage.GetData(portfolioId);
			adcData jsonData = JsonConvert.DeserializeObject<adcData>(json);
			//dynamic data = JObject.Parse(json);

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
