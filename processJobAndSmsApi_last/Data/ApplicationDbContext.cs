using Microsoft.EntityFrameworkCore;
using processJobAndSmsApi.Models;


namespace processJobAndSmsApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for tables
        public DbSet<Users> Users { get; set; }  
        public DbSet<UserJob> UserJob { get; set; }  
        public DbSet<UsersContact> UsersContact { get; set; }  
        public DbSet<NumberManagement> NumberManagement { get; set; }  
        public DbSet<UsersRouteType> UsersRouteType { get; set; }  
        public DbSet<UsersSMS> UsersSMS { get; set; }
        public DbSet<UsersSmsSmartUrl> UsersSmsSmartUrls { get; set; }
        public DbSet<SmartLinkHit> SmartLinkHits { get; set; }
        public DbSet<UsersCategory> UsersCategory { get; set; }



        // Model configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<UserJob>().ToTable("users_job"); 
            modelBuilder.Entity<UsersSMS>().ToTable("users_sms");
            modelBuilder.Entity<UsersSmsSmartUrl>().ToTable("users_sms_smart_urls");
            modelBuilder.Entity<SmartLinkHit>().ToTable("users_sms_smart_urls_visits");
        }
    }
}
