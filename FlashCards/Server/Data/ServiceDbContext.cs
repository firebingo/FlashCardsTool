using FlashCards.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashCards.Server.Data
{
	public class ServiceDbContext : DbContext
	{
		public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options)
		{

		}

		public DbSet<MetaData> MetaData { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<CardSet> CardSet { get; set; }
		public DbSet<Card> Card { get; set; }
		public DbSet<CardSetCollection> CardCollection { get; set; }
		public DbSet<CardSetCollectionSets> CardCollectionSets { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<MetaData>()
				.ToTable("metaData");
			modelBuilder.Entity<User>()
				.ToTable("users")
				.HasMany(x => x.CardSets)
				.WithOne(x => x.User)
				.HasForeignKey(x => x.UserId);
			modelBuilder.Entity<User>()
				.HasMany(x => x.Collections)
				.WithOne(x => x.User)
				.HasForeignKey(x => x.UserId);
			modelBuilder.Entity<CardSet>()
				.ToTable("cardSet")
				.HasMany(x => x.Cards)
				.WithOne(x => x.CardSet)
				.HasForeignKey(x => x.SetId);
			modelBuilder.Entity<Card>()
				.ToTable("card");
			modelBuilder.Entity<CardSetCollection>()
				.ToTable("cardSetCollection")
				.HasMany(x => x.CollectionSets)
				.WithOne(x => x.Collection)
				.HasForeignKey(x => x.CollectionId);
			modelBuilder.Entity<CardSetCollectionSets>()
				.ToTable("cardSetCollectionSets")
				.HasKey(x => new { x.CollectionId, x.SetId });
			modelBuilder.Entity<CardSetCollectionSets>()
				.HasOne(x => x.CardSet)
				.WithMany(x => x.CollectionSets)
				.HasForeignKey(x => x.SetId);
			modelBuilder.Entity<CardSetCollectionSets>()
				.HasOne(x => x.Collection)
				.WithMany(x => x.CollectionSets)
				.HasForeignKey(x => x.CollectionId);
		}
	}
}
