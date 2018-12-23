using Microsoft.EntityFrameworkCore;
using MovieManager.Data;

namespace MovieManager.Desktop.Services
{
	public static class DbContextFactory
	{
		public static MovieContext Create()
		{
			var builder = new DbContextOptionsBuilder<MovieContext>();
			builder.EnableSensitiveDataLogging();
			builder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=movieContextLocal;Trusted_Connection=True;MultipleActiveResultSets=true");
			var context = new MovieContext(builder.Options);
			return context;
		}
	}
}