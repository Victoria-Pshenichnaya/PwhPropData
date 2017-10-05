using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IPwhPropDataManager
	{
		void UpdateRecommendationsForApmPortfolios();
		Task UpdateRecommendationsForApmPortfolio(int portfolioId);
	}
}
