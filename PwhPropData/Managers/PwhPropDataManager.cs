using PwhPropData.Core.Common;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PwhPropData.Core.Managers
{
	public class PwhPropDataManager : IPwhPropDataManager
	{
		private readonly ILogger _logger = null;
		private readonly IPwhManager _pwhManager = null;
		private readonly IAdcManager _adcManager = null;
		

		public PwhPropDataManager(ILogger logger, IPwhManager pwhManager, IAdcManager adcManager)
		{
			Guard.NotNull(logger, nameof(logger));
			Guard.NotNull(pwhManager, nameof(pwhManager));
			Guard.NotNull(adcManager, nameof(adcManager));

			_logger = logger;
			_pwhManager = pwhManager;
			_adcManager = adcManager;
		}

		public async void UpdateRecommendationsForApmPortfolios()
		{
			IEnumerable<PortfolioHeader> apmPortfolios = _pwhManager.GetApmPortfolioHeadersAsync().Result;
			_logger.LogMessage($"Count of APM portfolios: {apmPortfolios.Count()}, Ids: {string.Join(",", apmPortfolios.Select(p => p.Id))}");

			foreach (PortfolioHeader portfolio in apmPortfolios)
			{
				await UpdateRecommendationsForApmPortfolio(portfolio.Id);
			}
		}

		public async Task UpdateRecommendationsForApmPortfolio(int portfolioId)
		{
			IEnumerable<KeyValuePair<string, double>> recommenadations = await _adcManager.GetRecommendations(portfolioId);

			if ((recommenadations != null) && (recommenadations.Count() > 0))
			{
				_logger.LogMessage($"Count of recommenadations: {recommenadations.Count()}");
				foreach (var rec in recommenadations)
				{
					_logger.LogMessage($"{rec.Key} - {rec.Value}");
				}

				DateTime holdingsStatementDate = DateTime.Today;

				IEnumerable<HoldingsStatement> holdingsStatements = _pwhManager.GetHoldingsStatementsAsync(portfolioId, holdingsStatementDate).Result;

				if ((holdingsStatements != null) && (holdingsStatements.Count() > 0))
				{
					await _pwhManager.UpdateHoldingStatementsAsync(portfolioId, holdingsStatements);
				}
				else
				{
					await _pwhManager.AddHoldingsStatementsAsync(portfolioId, new List<HoldingsStatement>()
					{
						new HoldingsStatement()
						{
							HoldingsStatementDate = DateTime.Now,
							Holdings = recommenadations.Select(r => new HoldingData() { CompanyId = r.Key, Recommendation = r.Value })
						}
					});
				}
			}
		}
	}
}
