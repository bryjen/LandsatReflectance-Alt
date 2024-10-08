using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.EntityFrameworkCore;

namespace LandsatReflectance.Api.Services;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new UserTypeConfiguration().Configure(modelBuilder.Entity<User>());
    }
}

public class UsersService
{
}