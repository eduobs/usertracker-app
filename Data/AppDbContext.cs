using Microsoft.EntityFrameworkCore;
using UserTracker.Models;

namespace UserTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserAccess> UserAccesses => Set<UserAccess>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccess>(e =>
        {
            e.HasIndex(x => x.FingerprintHash);
            e.HasIndex(x => x.IpAddress);
            e.HasIndex(x => x.AccessedAt);
            e.HasIndex(x => x.SessionId);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });
    }
}
