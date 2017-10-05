using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IAdcManager
	{ 
		Task<IEnumerable<KeyValuePair<string, double>>> GetRecommendations(int portfolioId);
	}
}
