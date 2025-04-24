using Microsoft.EntityFrameworkCore;
using processJobAndSmsApi.Models;
using processJobAndSmsApi.Models.Dtos;
using processJobAndSmsApi.Services;

namespace processJobAndSmsApi.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        // DbSet properties for tables
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<UserJob> UserJob { get; set; } = null!;
        public DbSet<UsersContact> UsersContact { get; set; } = null!;
        public DbSet<NumberManagement> NumberManagement { get; set; } = null!;
        public DbSet<UsersRouteType> UsersRouteType { get; set; } = null!;
        public DbSet<UsersSMS> UsersSMS { get; set; } = null!;
        public DbSet<UsersSmsSmartUrl> UsersSmsSmartUrls { get; set; } = null!;
        public DbSet<SmartLinkHit> SmartLinkHits { get; set; } = null!;
        public DbSet<UsersPanelSettings> UsersPanelSettings { get; set; } = null!;
        public DbSet<UsersCategory> UsersCategory { get; set; } = null!;
        public DbSet<RouteType> RouteType { get; set; } = null!;


        // Model configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserJob>().ToTable("users_job");
            modelBuilder.Entity<UsersSMS>().ToTable("users_sms");
            modelBuilder.Entity<UsersSmsSmartUrl>().ToTable("users_sms_smart_urls");
            modelBuilder.Entity<SmartLinkHit>().ToTable("users_sms_smart_urls_visits");
            modelBuilder.Entity<UsersPanelSettings>().ToTable("users_panel_settings");
            modelBuilder.Entity<UsersCategory>().ToTable("users_category");
            modelBuilder.Entity<RouteType>().ToTable("route_type");

        }


        public async Task<int> GetRegisteredUsersCountAsync(string userId, string period)
        {
            var now = DateTime.UtcNow;
            var query = Users.AsQueryable();

            switch (period.ToLower())
            {
                case "today":
                    query = query.Where(u => DateTime.Parse(u.RegistrationDate!).Date == now.Date);
                    break;
                case "weekly":
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    query = query.Where(u => u.RegistrationDate != null && DateTime.Parse(u.RegistrationDate!) >= startOfWeek);
                    break;
                case "monthly":
                    query = query.Where(u => u.RegistrationDate != null &&
                                           DateTime.Parse(u.RegistrationDate!).Month == now.Month &&
                                           DateTime.Parse(u.RegistrationDate!).Year == now.Year);
                    break;
            }

            return await query.CountAsync();
        }

        public async Task<List<Users>> GetRecentUsersAsync(string userId, int count)
        {
            return await Users
                .Where(u => u.ParentId == userId)
                .OrderByDescending(u => u.RegistrationDate)
                .Take(count)
                .ToListAsync();
        }

    }




}