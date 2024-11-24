using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Divar.Models
{
    public class DivarDbContext : IdentityDbContext<CustomUser>
    {
        public DivarDbContext(DbContextOptions<DivarDbContext> options)
            : base(options)
        {
        }

        // DbSet for Advertisement
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Role> Roles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the relationship between CustomUser and Advertisement
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.CustomUser)
                .WithMany(u => u.Advertisements)
                .HasForeignKey(a => a.CustomUserId); // Must be of type string now



            base.OnModelCreating(modelBuilder);

            // Configures the relationship between Role and AccessLevel
            modelBuilder.Entity<Role>()
                .Property(r => r.Permissions)
                .HasConversion(
                    v => string.Join(',', v),     // Convert the list of AccessLevel to a string
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => (AccessLevel)Enum.Parse(typeof(AccessLevel), e)).ToList()); // Convert back to List<AccessLevel>
        }
    }
}

