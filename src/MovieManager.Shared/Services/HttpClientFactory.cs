using System.Net.Http;

namespace MovieManager.Shared.Services
{
	public static class HttpClientFactory
	{
		public static HttpClient Create()
		{
			var client = new HttpClient();
			var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
			client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
			return client;
		}
	}
}