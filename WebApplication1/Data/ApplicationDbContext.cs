using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.ResetToken).HasColumnName("reset_token");
                entity.Property(e => e.ResetTokenExpiry).HasColumnName("reset_token_expiry");
                entity.Property(e => e.VerificationCode).HasColumnName("verification_code");
                entity.Property(e => e.VerificationCodeExpiry).HasColumnName("verification_code_expiry");
            });
        }
    }
}
