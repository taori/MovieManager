// 

using System.Threading.Tasks;
using MovieManager.Shared.Services;

namespace MovieManager.Shared.Interfaces
{
	public interface IImageStorage
	{
		Task SaveAsync(byte[] data, ImageToken imageToken);
		string CreateImagePath(ImageToken imageToken);
	}
}