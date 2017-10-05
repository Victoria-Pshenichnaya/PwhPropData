using System;
using System.Collections.Generic;

namespace PwhPropData.Core.Entities
{
	public class PortfolioHeader
	{
		public PortfolioAccessibility? Accessibility { get; set; }
		public string Code { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public int? NumberOfConstituents { get; set; }
		public string OrganizationCode { get; set; }
		public string OwnerId { get; set; }
		public string Realm { get; set; }
		public PortfolioType Type { get; set; }
		public DateTime? CreatedDateTime { get; set; }
		public DateTime? LastModifiedDateTime { get; set; }
		public string Description { get; set; }
		public string DefaultCurrencyKey { get; set; }
		public int? DefaultBenchmarkId { get; set; }
		public string DefaultClassificationKey { get; set; }
		public PortfolioHeader DefaultBenchmarkPortfolioHeader { get; set; }
		public string Family { get; set; }
		public DateTime? FirstHoldingsStatementDate { get; set; }
		public DateTime? LastHoldingsStatementDate { get; set; }
		public PortfolioVisibility? Visibility { get; set; }
		public List<PortfolioAccess> PortfolioAccessControl { get; set; }
	}
}
