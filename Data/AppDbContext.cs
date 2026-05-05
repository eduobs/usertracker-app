using Microsoft.EntityFrameworkCore;
using UserTracker.Models;

namespace UserTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserAccess> UserAccesses => Set<UserAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccess>(e =>
        {
            e.HasIndex(x => x.FingerprintHash);
            e.HasIndex(x => x.IpAddress);
            e.HasIndex(x => x.AccessedAt);
            e.HasIndex(x => x.SessionId);
        });
    }
}
