using System;
using System.Threading.Tasks;
using MovieManager.Shared.Interfaces;
using NLog;

namespace MovieManager.Shared.Services
{
	public class WebLoader : IWebLoader
	{
		private static readonly ILogger Log = LogManager.GetLogger(nameof(WebLoader));

		/// <inheritdoc />
		public async Task<string> GetContentAsync(string url)
		{
			try
			{
				using (var client = HttpClientFactory.Create())
				{
					return await client.GetStringAsync(url);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
				return string.Empty;
			}
		}
	}
}