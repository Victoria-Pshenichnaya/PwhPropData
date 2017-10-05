using PwhPropData.Core.Common;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PwhPropData.Core.Managers
{
	public class PwhManager : IPwhManager
	{
		private IPwhStorage _pwhStorage = null;
		private ILogger _logger = null;

		public PwhManager(ILogger logger, IPwhStorage pwhStorage)
		{
			_pwhStorage = pwhStorage;
			_logger = logger;
		}

		public async Task<int> AddPortfolioAsync(FundedPortfolio portfolio)
		{
			return await _pwhStorage.AddFoundPortfolioAsync(portfolio);
		}
			
		public async Task<int> AddApmPortfolioAsync(FundedPortfolio portfolio)
		{
			Guard.NotNull(portfolio, nameof(portfolio));

			portfolio.Attribute = new PortfolioAttribute() { ApplicationCode = Settings.PwhApplicationCode, Name = Settings.PwhApmAttribute };
			return await AddPortfolioAsync(portfolio);
		}

		public async Task DeletePortfolios(IEnumerable<int> portfolioIds)
		{
			Guard.NotNull(portfolioIds, nameof(portfolioIds));

			await _pwhStorage.DeletePortfoliosAsync(portfolioIds);
		}

		public async Task AddHoldingsStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingsStatements)
		{
			Guard.NotNull(holdingsStatements, nameof(holdingsStatements));

			if (holdingsStatements != null)
			{
				await _pwhStorage.AddHoldingsStatementsAsync(portfolioId, holdingsStatements);
			}
		}

		public async Task<PortfolioHeader> GetHeaderByPartfolioIdAsync(int portfolioId)
		{
			IEnumerable<PortfolioHeader> portfolios = await _pwhStorage.GetPortfolioHeadersAsync(new int[] { portfolioId });

			if ((portfolios != null) && (portfolios.Count() > 0))
			{
				return portfolios.First();
			}
			return null;
		}

		public async Task<IEnumerable<PortfolioHeader>> GetApmPortfolioHeadersAsync()
		{
			PortfolioAttribute att = new PortfolioAttribute() { ApplicationCode = Settings.PwhApplicationCode, Name = Settings.PwhApmAttribute };
			return await _pwhStorage.GetPortfolioHeadersByAttributeAsync(att);
		}

		public async Task<IEnumerable<HoldingsStatement>> GetHoldingsStatementsAsync(int portfolioId, DateTime startDate)
		{
			return await _pwhStorage.GetHoldingsStatementsAsync(portfolioId, startDate, DateTime.Today);
		}

		public async Task UpdateHoldingStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements)
		{
			await _pwhStorage.UpdateHoldingStatementsAsync(portfolioId, holdingStatements);
		}
	}
}
