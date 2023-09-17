using FlashCards.Server.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FlashCards.Server.Data
{
	public class ServiceDbContext : DbContext
	{
		readonly AppSettings _appSettings;

		public ServiceDbContext(DbContextOptions<ServiceDbContext> options, IOptions<AppSettings> appSettings) : base(options)
		{
			_appSettings = appSettings.Value;
		}

		public DbSet<User> Users { get; set; }
		public DbSet<MetaData> MetaData { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

		}
	}
}
