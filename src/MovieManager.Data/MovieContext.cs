using Microsoft.EntityFrameworkCore;
using MovieManager.Shared.Models;

namespace MovieManager.Data
{
	public class MovieContext : DbContext
	{
		/// <inheritdoc />
		public MovieContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<LinkEntry> Entries { get; set; }

		public DbSet<LinkGroup> Groups { get; set; }
	}
}