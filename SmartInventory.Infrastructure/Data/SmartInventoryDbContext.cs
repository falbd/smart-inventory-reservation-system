using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Infrastructure.Entities;

namespace SmartInventory.Infrastructure.Data

{
    public class SmartInventoryDbContext : DbContext
    {
        public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options)
        : base(options)
        {

        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Sku)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.Sku)
                    .IsUnique();

                entity.Property(x => x.Price)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<StockItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Product)
                    .WithOne()
                    .HasForeignKey<StockItem>(x => x.ProductId);

                entity.HasIndex(x => x.ProductId)
                    .IsUnique();
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId);

                entity.Property(x => x.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
