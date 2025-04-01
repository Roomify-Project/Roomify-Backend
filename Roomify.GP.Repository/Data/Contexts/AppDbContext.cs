using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Repository.Data.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ✅ كيانات المشروع الأصلية
       // public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Description> Descriptions { get; set; }
        public DbSet<PortfolioPost> PortfolioPosts { get; set; }

        // ✅ جدول OTP الموحد لتأكيد الإيميل واستعادة الباسورد
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EmailConfirmationToken>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        }
    }
}
