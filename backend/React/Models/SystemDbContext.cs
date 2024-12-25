using Microsoft.EntityFrameworkCore;

namespace React.Models
{
    public class SystemDbContext : DbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Shop> Shop { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<CommentReview> CommentReview { get; set; }
        public DbSet<LikeCommentReview> LikeCommentReview { get; set; }
        public DbSet<RatingReview> RatingReview { get; set; }
        public DbSet<CommentPost> CommentPost { get; set; }
        public DbSet<RatingPost> RatingPost { get; set; }
        public DbSet<ImagePost> ImagePost { get; set; }
        public DbSet<ImageReview> ImageReview { get; set; }
        public DbSet<LikeCommentPost> LikeCommentPost { get; set; }
  



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Review)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Shop)
                .WithMany(s => s.Review)  
                .HasForeignKey(r => r.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shop>()
                .HasOne(s => s.User)
                .WithOne(u => u.Shop)
                .HasForeignKey<Shop>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shop>()
                .HasOne(s => s.Post)
                .WithOne(p => p.Shop)
                .HasForeignKey<Post>(p => p.ShopId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Shop)
                .WithOne(s => s.Post)
                .HasForeignKey<Post>(p => p.ShopId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-9SD0L97; Initial Catalog=lbs; User ID=sa; password=123; TrustServerCertificate= True");
        }
    }
}
