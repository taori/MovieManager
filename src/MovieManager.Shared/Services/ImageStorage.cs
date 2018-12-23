using System.IO;
using System.Threading.Tasks;
using MovieManager.Shared.Interfaces;

namespace MovieManager.Shared.Services
{
	public class ImageToken
	{
		public string Raw { get; }

		public ImageToken(string raw)
		{
			Raw = raw;
		}

		public string PathSanitized => Raw?.Replace('/', Path.DirectorySeparatorChar);
	}

	public class ImageStorage : IImageStorage
	{
		/// <inheritdoc />
		public async Task SaveAsync(byte[] data, ImageToken imageToken)
		{
			var path = CreateImagePath(imageToken);
			if (File.Exists(path))
			{
				using (var writer = new FileStream(path, FileMode.Truncate))
				{
					await writer.WriteAsync(data, 0, data.Length);
				}
			}
			else
			{
				var fileInfo = new FileInfo(path);
				if(!fileInfo.Directory.Exists)
					fileInfo.Directory.Create();

				using (var writer = new FileStream(path, FileMode.CreateNew))
				{
					await writer.WriteAsync(data, 0, data.Length);
				}
			}
		}

		/// <inheritdoc />
		public string CreateImagePath(ImageToken imageToken)
		{
			return FilePathHelper.GetPath("Images", imageToken.PathSanitized);
		}
	}
}