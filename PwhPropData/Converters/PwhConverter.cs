using PwhPropData.Core.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using Pwh = PwhPropData.Core.PwhProxy.PwhService;//PwhPropData.Core.PwhService;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Interfaces;

namespace PwhPropData.Core.Converters
{
	public class PwhConverter : IPwhConverter
	{
		private IUserIdentityProvider _userIdentityProvider = null;

		public PwhConverter(IUserIdentityProvider userIdentityProvider)
		{
			_userIdentityProvider = userIdentityProvider;
		}

		public Pwh.HoldingsStatement ConvertHoldingsStatement(HoldingsStatement holdingsStatement)
		{
			if (holdingsStatement == null)
			{
				return null;
			}

			Pwh.Holdings pwhHoldings = null;
			if (holdingsStatement.Holdings != null)
			{
				pwhHoldings = new Pwh.Holdings();
				pwhHoldings.AddRange(holdingsStatement.Holdings.Select(holding =>
				{
					return new Pwh.Holding()
					{
						Units = holding.Recommendation,
						SecurityItem = new Pwh.SecurityItem()
						{
							SymbolValues = new Pwh.SymbolValues()
								{
								new Pwh.SymbolValue()
								{
									SymbolType = "RIC",
									SymbolValueMember = holding.CompanyId
								}
								}
						}
					};
				}));
			}

			return new Pwh.HoldingsStatement()
			{
				HoldingsStatementHeader = new Pwh.HoldingsStatementHeader()
				{
					HoldingsStatementDate = DateTime.SpecifyKind(holdingsStatement.HoldingsStatementDate, DateTimeKind.Unspecified).Date,
					LastModifiedDateTime = holdingsStatement.LastModifiedDateTime ?? DateTime.MinValue
				},
				Holdings = pwhHoldings
			};
		}

		public Pwh.HoldingsStatements ConvertHoldingsStatements(IEnumerable<HoldingsStatement> holdingStatements)
		{
			Pwh.HoldingsStatements statements = new Pwh.HoldingsStatements();
			statements.AddRange(holdingStatements.Select(ConvertHoldingsStatement));
			return statements;
		}

		public Pwh.FundedPortfolio ConvertFundedPortfolio(FundedPortfolio fundedPortfolio)
		{
			Pwh.FundedPortfolio pwhPortfolio = new Pwh.FundedPortfolio();
			pwhPortfolio.PortfolioHeader = new Pwh.PortfolioHeader()
			{
				Accessibility = Pwh.PortfolioAccessibility.ReadWrite,
				Code = fundedPortfolio.Code,
				Visibility = Pwh.PortfolioVisibility.Private,
				Name = fundedPortfolio.Name,
				NumberOfConstituents = 10,
				OrganizationCode = "",
				OwnerId = "",
				Realm = "",
				CreatedDateTime = DateTime.Now,
				LastModifiedDateTime = null,
				Description = "VP Desc",
				PortfolioHeaderExtendedProperties = new Pwh.PortfolioHeaderExtendedProperties()
				{
					DeafultClassification = null,
					DefaultBenchmarkId = null,
					DefaultISOCurrencyCode = null,
					PortfolioAccessControl = null
				}

			};

			pwhPortfolio.PortfolioHeader.Type = null; // PWH doesn't allow to specify portfolio type in this request
			pwhPortfolio.PortfolioHeader.PortfolioId = 0; // PWH doesn't allow to specify portfolio id in this request

			Pwh.PortfolioAttributes attributes = new Pwh.PortfolioAttributes();
			attributes.AddRange(fundedPortfolio.Attributes.Select(att => new Pwh.PortfolioAttribute() { ApplicationCode = att.ApplicationCode, AttributeName = att.Name, AttributeValue = att.Value }));

			pwhPortfolio.PortfolioAttributes = attributes;
			pwhPortfolio.HoldingsStatementHeaders = null;
			return pwhPortfolio;
		}

		public PortfolioHeader ConvertHeaders(Pwh.PortfolioHeader pwhHeader)
		{
			var header = new PortfolioHeader()
			{
				Accessibility = ConvertPortfolioAccessibility(pwhHeader.Accessibility),
				Visibility = ConvertPortfolioVisibility(pwhHeader.Visibility, pwhHeader.OwnerId),
				Code = pwhHeader.Code,
				Id = pwhHeader.PortfolioId,
				Name = pwhHeader.Name,
				NumberOfConstituents = pwhHeader.NumberOfConstituents,
				OrganizationCode = pwhHeader.OrganizationCode,
				OwnerId = pwhHeader.OwnerId != null ? pwhHeader.OwnerId.ToUpper() : null,
				Realm = pwhHeader.Realm,
				Type = ConvertPortfolioType(pwhHeader.Type),
				CreatedDateTime = pwhHeader.CreatedDateTime,
				LastModifiedDateTime = pwhHeader.LastModifiedDateTime,
				Description = pwhHeader.Description,
			};

			if (pwhHeader.PortfolioHeaderExtendedProperties != null)
			{
				var properties = pwhHeader.PortfolioHeaderExtendedProperties;

				header.DefaultClassificationKey = ConvertClassificationKey(properties.DeafultClassification);

				header.DefaultBenchmarkId = properties.DefaultBenchmarkPortfolioHeader != null
												? properties.DefaultBenchmarkId
												: properties.DefaultBenchmarkId ?? 0;

				//header.DefaultBenchmarkPortfolioHeader = properties.DefaultBenchmarkPortfolioHeader != null
				//											 ? ConvertPortfolioHeader(
				//												 properties.DefaultBenchmarkPortfolioHeader)
				//											 : null;
				header.DefaultCurrencyKey = properties.DefaultISOCurrencyCode;
				header.Family = properties.Family;
				header.FirstHoldingsStatementDate = properties.FirstHoldingsStatementDate;
				header.LastHoldingsStatementDate = properties.LastHoldingsStatementDate;
				header.PortfolioAccessControl = ConvertPortfolioAccessControl(properties.PortfolioAccessControl);
			}

			if (string.IsNullOrEmpty(header.Family))
			{
				if (header.Realm == _userIdentityProvider.Uuid)
					header.Family = "Personal";
				else
					header.Family = header.Realm;
			}

			return header;
		}

		private PortfolioAccess ConvertPortfolioAccess(Pwh.PortfolioAccess portfolioAccess)
		{
			if (portfolioAccess == null)
				return null;

			return new PortfolioAccess()
			{
				EntityId = portfolioAccess.EntityId,
				EntityType = portfolioAccess.EntityType,
				AccessLevel = ConvertPortfolioAccessibility(portfolioAccess.AccessLevel)
			};
		}

		private List<PortfolioAccess> ConvertPortfolioAccessControl(IEnumerable<Pwh.PortfolioAccess> portfolioAccessControl)
		{
			if (portfolioAccessControl == null)
				return new List<PortfolioAccess>();

			var access = new List<PortfolioAccess>();
			access.AddRange(portfolioAccessControl.Select(ConvertPortfolioAccess));
			return access;
		}

		private PortfolioAccessibility? ConvertPortfolioAccessibility(Pwh.PortfolioAccessibility? portfolioAccessibility)
		{
			if (portfolioAccessibility == null)
				return null;

			switch (portfolioAccessibility)
			{
				case Pwh.PortfolioAccessibility.Read:
					return PortfolioAccessibility.Read;
				case Pwh.PortfolioAccessibility.ReadWrite:
					return PortfolioAccessibility.ReadWrite;
				default:
					throw new ArgumentOutOfRangeException("portfolioAccessibility");
			}
		}

		private PortfolioVisibility? ConvertPortfolioVisibility(Pwh.PortfolioVisibility? portfolioVisibility, string ownerId)
		{
			if (string.Equals(ownerId, "SAMPLEPORTFOLIOS", StringComparison.OrdinalIgnoreCase))
				return PortfolioVisibility.Shared; // pwh returns "Private" instead of "Shared" for sample portfolios

			if (portfolioVisibility == null)
				return null;

			switch (portfolioVisibility)
			{
				case Pwh.PortfolioVisibility.Company:
					return PortfolioVisibility.Company;
				case Pwh.PortfolioVisibility.Private:
					return PortfolioVisibility.Private;
				case Pwh.PortfolioVisibility.Shared:
					return PortfolioVisibility.Shared;
				case Pwh.PortfolioVisibility.WorkGroup:
					return PortfolioVisibility.WorkGroup;
				default:
					throw new ArgumentOutOfRangeException("portfolioVisibility");
			}
		}

		private PortfolioType ConvertPortfolioType(string portfolioType)
		{
			Guard.NotNullOrEmpty(portfolioType, "portfolioType");

			switch (portfolioType)
			{
				case "FundedPortfolio":
					return PortfolioType.Funded;
				default:
					throw new ArgumentOutOfRangeException("portfolioType");
			}
		}
		private string ConvertClassificationKey(Pwh.ClassificationItem classificationKey)
		{
			if (classificationKey == null)
				return null;

			return classificationKey.Code;
		}
	}
}
