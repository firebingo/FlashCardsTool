using FlashCards.Server.Data.Models.Apkg;
using Microsoft.EntityFrameworkCore;

namespace FlashCards.Server.Data
{
	public class ApkgExportContext : DbContext
	{
		public ApkgExportContext(DbContextOptions<ApkgExportContext> options) : base(options)
		{

		}

		public DbSet<Cards> Cards { get; set; }
		public DbSet<Collection> Collection { get; set; }
		public DbSet<Graves> Graves { get; set; }
		public DbSet<Notes> Notes { get; set; }
		public DbSet<ReviewLog> ReviewLog { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

		}
	}
}
