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
			portfolio.Attributes = new List<PortfolioAttribute>()
			{
				new PortfolioAttribute() { ApplicationCode = Settings.PwhApplicationCode, Name = Settings.PwhApmAttribute, Value = Settings.PwhApmAttribute }
			};
			return await AddPortfolioAsync(portfolio);
		}

		public async Task DeletePortfolios(IEnumerable<int> portfolioIds)
		{
			await _pwhStorage.DeletePortfoliosAsync(portfolioIds);
		}

		public async Task AddHoldingsStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingsStatements)
		{
			if (holdingsStatements != null)
			{
				await _pwhStorage.AddHoldingsStatementsAsync(portfolioId, holdingsStatements);
			}
		}

		public async Task<IEnumerable<PortfolioHeader>> GetHeadersByPartfolioIdAsync(int portfolioId)
		{
			return await _pwhStorage.GetPortfolioHeadersAsync(new int[] { portfolioId });
		}

		public Task<IEnumerable<PortfolioHeader>> GetHeadersByAttributeAsync(PortfolioAttribute att)
		{
			return _pwhStorage.GetPortfolioHeadersByAttributeAsync(att);
		}

		public Task<IEnumerable<PortfolioHeader>> GetHeadersByAttributeNameAsync(string attributeName)
		{
			PortfolioAttribute att = new PortfolioAttribute() { ApplicationCode = Settings.PwhApplicationCode, Name = attributeName };
			return _pwhStorage.GetPortfolioHeadersByAttributeAsync(att);
		}

		public Task<IEnumerable<Entities.PortfolioHeader>> GetHeadersByQuery()
		{
			return _pwhStorage.GetPortfolioHeadersByQueryAsync();
		}

		public async Task<IEnumerable<HoldingsStatement>> GetHoldingsStatementsAsync(int portfolioId, DateTime date)
		{
			return await _pwhStorage.GetHoldingsStatementsAsync(portfolioId, date);
		}

		public async Task UpdateHoldingStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements)
		{
			await _pwhStorage.UpdateHoldingStatementsAsync(portfolioId, holdingStatements);
		}
	}
}
