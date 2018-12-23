using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MovieManager.Shared.Interfaces;
using NLog;

namespace MovieManager.Shared.Services
{
	public class ContentCacheLoader
	{
		public IContentStorage Storage { get; }
		public IWebLoader WebLoader { get; }
		private static readonly ILogger Log = LogManager.GetLogger(nameof(ContentCacheLoader));

		public ContentCacheLoader(IContentStorage storage, IWebLoader webLoader)
		{
			Storage = storage;
			WebLoader = webLoader;
		}

		public async Task LoadContentOfDaysAsync(int recentDays)
		{
			Log.Info($"Downloading last {recentDays} days.");
			var r = new Random();

			foreach (var offset in Enumerable.Range(0, recentDays))
			{
				var referenceDate = DateTime.Now.Subtract(TimeSpan.FromDays(offset));
				if (!await TryDownloadContentsForDayAsync(referenceDate))
				{
					Log.Info($"Already downloaded {referenceDate:dd.MM.yyyy}.");
					continue;
				}

				var millisecondsDelay = r.Next(5000, 10000);
				Log.Info($"Waiting {millisecondsDelay}ms.");
				await Task.Delay(millisecondsDelay);
			}

			Log.Info($"Downloading done.");
		}
			
		private async Task<bool> TryDownloadContentsForDayAsync(DateTime referenceDate)
		{
			var filePath = Storage.GenerateFilePath(referenceDate);
			var currentPath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.c.html");
			var finalPath = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.f.html");

			if (DateTime.Now.Date == referenceDate.Date)
			{
				if (File.Exists(currentPath))
					File.Delete(currentPath);

				Log.Info($"Downloading {referenceDate:dd.MM.yyyy}");
				var content = await RequestDataForDateAsync(referenceDate);
				await Storage.SaveAsync(currentPath, content);
				Log.Info($"Saved.");
				return true;
			}
			else
			{
				if (File.Exists(currentPath))
					File.Delete(currentPath);
				if (File.Exists(finalPath))
					return false;

				Log.Info($"Downloading {referenceDate:dd.MM.yyyy}");
				var content = await RequestDataForDateAsync(referenceDate);
				await Storage.SaveAsync(finalPath, content);
				Log.Info($"Saved.");
				return true;
			}
		}

		private async Task<string> RequestDataForDateAsync(DateTime now)
		{
			var datePart = $"{now:yyyy}-{now:MM}-{now:dd}";
			// https://www.kinos.to/News-2018-12-13.html
			var url = $"https://www.kinos.to/News-{datePart}.html";
			var content = await WebLoader.GetContentAsync(url);
			return content;
		}
	}
}