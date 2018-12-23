using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MovieManager.Shared.Interfaces;
using NLog;

namespace MovieManager.Shared.Services
{
	public class ContentStorage : IContentStorage
	{
		private static readonly NLog.ILogger Log = LogManager.GetLogger(nameof(ContentStorage));

		protected internal static string ContentRoot = FilePathHelper.GetPath("Content");

		/// <inheritdoc />
		public IEnumerable<string> GetFilePaths()
		{
			Log.Debug($"Loading files of {ContentRoot}.");
			var pathInfo = new DirectoryInfo(ContentRoot);
			if (!pathInfo.Exists)
			{
				pathInfo.Create();
			}
			return Directory.EnumerateFiles(ContentRoot);
		}

		/// <inheritdoc />
		public string GenerateFilePath(DateTime date)
		{
			var datePart = $"{date:yyyy}-{date:MM}-{date:dd}";
			return Path.Combine(ContentRoot, $"{datePart}.html");
		}

		/// <inheritdoc />
		public async Task SaveAsync(string path, string content)
		{
			var targetPath = Path.Combine(ContentRoot, path);
			var fileInfo = new FileInfo(targetPath);
			if (!fileInfo.Directory.Exists)
			{
				System.IO.Directory.CreateDirectory(fileInfo.DirectoryName);
			}

			using (var writer = new StreamWriter(targetPath))
			{
				await writer.WriteAsync(content);
			}
		}
	}
}