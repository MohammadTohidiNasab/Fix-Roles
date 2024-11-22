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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the relationship between CustomUser and Advertisement
            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.CustomUser)
                .WithMany(u => u.Advertisements)
                .HasForeignKey(a => a.CustomUserId); // Must be of type string now
        }

    }
}
