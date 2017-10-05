using PwhPropData.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IPwhManager
	{
		Task<int> AddPortfolioAsync(FundedPortfolio portfolio);
		Task<int> AddApmPortfolioAsync(FundedPortfolio portfolio);
		Task DeletePortfolios(IEnumerable<int> portfolioIds);
		Task AddHoldingsStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingsStatements);
		Task<PortfolioHeader> GetHeaderByPartfolioIdAsync(int portfolioId);
		Task<IEnumerable<PortfolioHeader>> GetApmPortfolioHeadersAsync();
		Task<IEnumerable<HoldingsStatement>> GetHoldingsStatementsAsync(int portfolioId, DateTime startDate);
		Task UpdateHoldingStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements);
	}
}
