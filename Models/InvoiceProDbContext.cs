using System;
using Microsoft.EntityFrameworkCore;

namespace BillFlow.API.Models
{
    public partial class InvoiceProDbContext : DbContext
    {
        public InvoiceProDbContext()
        {
        }

        public InvoiceProDbContext(DbContextOptions<InvoiceProDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<User> Users { get; set; }

        // Removed hardcoded connection string to follow best practices
        // Configuration now comes from Program.cs via dependency injection

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("Individual");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CompanyName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Permissions).HasMaxLength(500);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Employees)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CompanyName).HasMaxLength(100).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
