using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Domain;

namespace ProductCatalogApi.Data
{
    public class CatalogContext : DbContext
    {
        public DbSet<CatalogType> CatalogTypes { get; set; }
        public DbSet<CatalogBrand> CatalogBrands { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }

        public CatalogContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CatalogBrand>(cfg =>
            {
                cfg.ToTable("CatalogBrand");

                cfg.Property(c => c.Id)
                    .UseHiLo("CatalogBrandHiLo")
                    .IsRequired();

                cfg.Property(c => c.Brand)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            modelBuilder.Entity<CatalogType>(cfg =>
            {
                cfg.ToTable("CatalogType");

                cfg.Property(c => c.Id)
                    .UseHiLo("CatalogTypeHiLo")
                    .IsRequired();

                cfg.Property(c => c.Type)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            modelBuilder.Entity<CatalogItem>(cfg =>
            {
                cfg.ToTable("CatalogItem");

                cfg.Property(item => item.Id)
                    .IsRequired()
                    .UseHiLo("CatalogItemHiLo");

                cfg.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                cfg.Property(c => c.Price)
                    .IsRequired(true);

                cfg.HasOne(c => c.CatalogBrand)
                    .WithMany()
                    .HasForeignKey(c => c.CatalogBrandId);

                cfg.HasOne(c => c.CatalogType)
                   .WithMany()
                   .HasForeignKey(c => c.CatalogTypeId);
            });
        }
    }
}
