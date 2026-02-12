using Microsoft.EntityFrameworkCore;
using URLShortererApi.Entities;

namespace URLShortererApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ShortLink> ShortLinks => Set<ShortLink>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var link = modelBuilder.Entity<ShortLink>();

            link.ToTable("short_links");

            link.HasKey(x => x.Id);

            link.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(32);

            link.HasIndex(x => x.Code)
                .IsUnique();

            link.Property(x => x.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);

            link.Property(x => x.CreatedAt)
                .IsRequired();

            link.Property(x => x.Clicks)
                .HasDefaultValue(0);

            base.OnModelCreating(modelBuilder);
        }

    }
}
