using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.Identity;
using System.Reflection.Emit;

namespace Roomify.GP.Repository.Data.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ✅ كيانات المشروع الأصلية
        //public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Prompt> Prompts { get; set; }
        public DbSet<AIResult> AIResults { get; set; }
        public DbSet<AIResultHistory> AIResultHistories { get; set; }
        public DbSet<SavedDesign> SavedDesigns { get; set; }
        public DbSet<PortfolioPost> PortfolioPosts { get; set; }
        public DbSet<Message> Messages { get; set; }
        // ✅ جدول OTP الموحد لتأكيد الإيميل واستعادة الباسورد
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region FollowSetup
            builder.Entity<UserFollow>()
                .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

            builder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserFollow>()
                .HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);
            #endregion

            #region CommentSetup
            builder.Entity<Comment>()
                .HasOne(c => c.ApplicationUser)
                .WithMany()  // no navigation on ApplicationUser side
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.PortfolioPost)
                .WithMany(pp => pp.Comments)
                .HasForeignKey(c => c.PortfolioPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(c => c.SavedDesign)
                .WithMany(sd => sd.Comments)
                .HasForeignKey(c => c.SavedDesignId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region LikeSetup
            builder.Entity<Like>()
                .HasOne(l => l.ApplicationUser)
                .WithMany()  // no navigation on ApplicationUser side
                .HasForeignKey(l => l.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.PortfolioPost)
                .WithMany(pp => pp.Likes)
                .HasForeignKey(l => l.PortfolioPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(l => l.SavedDesign)
                .WithMany(sd => sd.Likes)
                .HasForeignKey(l => l.SavedDesignId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Emailconfirmation
            builder.Entity<EmailConfirmationToken>()
                .HasOne(e => e.PendingRegistration)
                .WithMany()
                .HasForeignKey(e => e.PendingRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

        }
    }
}
