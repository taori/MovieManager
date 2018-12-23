using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovieManager.Data
{
	public class DesignContext : IDesignTimeDbContextFactory<MovieContext>
	{
		/// <inheritdoc />
		public MovieContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<MovieContext>();
			builder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=movieContextdesign;Trusted_Connection=True;MultipleActiveResultSets=true");
			return new MovieContext(builder.Options);
		}
	}
}