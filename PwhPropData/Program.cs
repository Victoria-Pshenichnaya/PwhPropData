using PwhPropData.Core.Converters;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Managers;
using PwhPropData.Core.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
//using Pwh = PwhPropData.Core.PwhService;

namespace PwhPropData.Core
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Logger logger = new Logger();

				PwhManager pwhManager = new PwhManager(logger, new PwhStorage(logger, new PwhConverter(new UserIdentityProvider() { Uuid = Settings.UUID })));

				PwhPropDataManager pwhPropDataManager = new PwhPropDataManager(logger, pwhManager, new AdcManager(logger, new AdcStorage(logger)));

				IEnumerable<PortfolioHeader> headers = pwhManager.GetHeadersByQuery().Result;

				if (headers != null)
				{
					Console.WriteLine($"{headers.Count()} header(s) was(were) recieved.");
					foreach (PortfolioHeader header in headers)
					{
						Console.WriteLine(header.Name);
					}
				}
				else
				{
					Console.WriteLine($"There is no any headers.");
				}


				//Console.WriteLine("Input Portfolio Name: ");
				//string name = Console.ReadLine();

				//FundedPortfolio portfolio = new FundedPortfolio()
				//{
				//	Name = name,
				//	Code = $"{name.ToUpper()}CODE",
				//};
				//int portfolioId = pwhManager.AddApmPortfolioAsync(portfolio).Result;

				//IList<HoldingsStatement> holdingsStatements = new List<HoldingsStatement>();
				//holdingsStatements.Add(new HoldingsStatement()
				//{
				//	HoldingsStatementDate = DateTime.Today.AddDays(-1),
				//	LastModifiedDateTime = null,
				//	Holdings = new List<HoldingData>()
				//	{
				//		new HoldingData() { CompanyId = "HON.N", Recommendation = 2 },
				//		new HoldingData() { CompanyId = "FB.O", Recommendation = 1 },
				//		new HoldingData() { CompanyId = "CL.Z", Recommendation = 3 },
				//		new HoldingData() { CompanyId = "CL.N", Recommendation = 5 }
				//	}
				//});

				//pwhManager.AddHoldingsStatementsAsync(portfolioId, holdingsStatements);


				pwhPropDataManager.UpdateRecommendationsForApmPortfolios();

				//Console.WriteLine("Input Portfolio Id: ");
				//int.TryParse(Console.ReadLine(), out int pId);
				//pwhPropDataManager.UpdateRecommendationsForApmPortfolio(pId);

				//pwhManager.SaveAndGetById(portfolio, holdingsStatements);
				//pwhManager.GetHeadersByQuery();

				//pwhManager.SaveAndGetByAttribute(portfolio, holdingsStatements);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: {0}", ex.Message);
			}
			Console.ReadLine();
		}
	}
}
