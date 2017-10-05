using PwhPropData.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IPwhStorage
	{
		Task<int> AddFoundPortfolioAsync(FundedPortfolio foundPortfolio);
		Task AddHoldingsStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements);
		Task<IEnumerable<PortfolioHeader>> GetPortfolioHeadersAsync(IEnumerable<int> portfolioIds);
		Task<IEnumerable<PortfolioHeader>> GetPortfolioHeadersByAttributeAsync(PortfolioAttribute attribute);
		Task<IEnumerable<Entities.PortfolioHeader>> GetPortfolioHeadersByQueryAsync();
		Task<IEnumerable<HoldingsStatement>> GetHoldingsStatementsAsync(int portfolioId, DateTime date);
		Task DeletePortfoliosAsync(IEnumerable<int> portfolioIds);
		Task UpdateHoldingStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements);
	}
}
