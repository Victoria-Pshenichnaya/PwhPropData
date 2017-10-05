using System.Collections.Generic;

namespace PwhPropData.Core.Entities
{
	public class FundedPortfolio
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public IList<PortfolioAttribute> Attributes { get; set; }
	}
}
