using System;
using System.Threading.Tasks;

namespace MovieManager.Shared.Interfaces
{
	public interface IImageLoader
	{
		Task<byte[]> LoadAsync(Uri fullImageUrl);
	}
}