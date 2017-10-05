using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IAdcManager
	{
		IEnumerable<KeyValuePair<string, double>> GetRecommendations(int portfolioId);
	}
}
