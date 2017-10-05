using System.Collections.Generic;
using PwhPropData.Core.Entities;
using Pwh = PwhPropData.Core.PwhProxy.PwhService;//PwhPropData.Core.PwhService;

namespace PwhPropData.Core.Interfaces
{
	public interface IPwhConverter
	{
		Pwh.HoldingsStatement ConvertHoldingsStatement(HoldingsStatement holdingsStatement);
		Pwh.FundedPortfolio ConvertFundedPortfolio(FundedPortfolio fundedPortfolio);

		PortfolioHeader ConvertHeaders(Pwh.PortfolioHeader pwhHeader);
		Pwh.HoldingsStatements ConvertHoldingsStatements(IEnumerable<HoldingsStatement> holdingStatements);
	}
}
