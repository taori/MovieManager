using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieManager.Shared.Interfaces
{
	public interface IContentStorage
	{
		IEnumerable<string> GetFilePaths();
		string GenerateFilePath(DateTime date);
		Task SaveAsync(string path, string content);
	}
}