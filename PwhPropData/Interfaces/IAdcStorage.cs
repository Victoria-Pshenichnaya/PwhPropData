using System.Threading.Tasks;

namespace PwhPropData.Core.Interfaces
{
	public interface IAdcStorage
	{
		Task<string> GetData(int portfolioId);
	}
}
