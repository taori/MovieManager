using System;
using System.Net.Http;
using System.Threading.Tasks;
using MovieManager.Shared.Interfaces;
using NLog;

namespace MovieManager.Shared.Services
{
	public class ImageLoader : IImageLoader
	{
		private static readonly ILogger Log = LogManager.GetLogger(nameof(ImageLoader));

		public HttpClient Client { get; }

		public ImageLoader(HttpClient client)
		{
			Client = client;
		}

		/// <inheritdoc />
		public async Task<byte[]> LoadAsync(Uri fullImageUrl)
		{
			try
			{

				Log.Debug($"Downloading {fullImageUrl}.");
				var bytes = await Client.GetByteArrayAsync(fullImageUrl);
				return bytes;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return Array.Empty<byte>();
			}
		}
	}
}