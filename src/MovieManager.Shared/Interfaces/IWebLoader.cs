using System.Threading.Tasks;

namespace MovieManager.Shared.Interfaces
{
	public interface IWebLoader
	{
		Task<string> GetContentAsync(string url);
	}
}