using System;
using System.IO;
using System.Linq;

namespace MovieManager.Shared
{
	public static class FilePathHelper
	{
		public static string GetPath(params string[] paths)
		{
			var front = new string[]
			{
				Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), "MovieManager"
			};
			var back = paths.SelectMany(d => d.Split(Path.DirectorySeparatorChar));
			var combine = Path.Combine(front.Concat(back).ToArray());
			return combine;
		}
	}
}