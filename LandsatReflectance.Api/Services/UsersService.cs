using LandsatReflectance.Api.Exceptions;
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

public sealed class UsersService
{
    private ILogger<UsersService> m_logger;
    private UsersDbContext m_usersDbContext;
    
    public UsersService(ILogger<UsersService> logger, UsersDbContext usersDbContext)
    {
        m_logger = logger;
        m_usersDbContext = usersDbContext;
    }

    public async Task TryAddUserAsync(User newUser)
    {
        if (await TryGetUserAsync(newUser.Email) is not null)
        {
            throw new UserRegistrationException($"A user with email \"{newUser.Email}\" already exists.", newUser);
        }
        
        await using var transaction = await m_usersDbContext.Database.BeginTransactionAsync();

        try
        {
            _ = await m_usersDbContext.Users.AddAsync(newUser);
            await m_usersDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
            m_logger.LogError($"\"TryAddUser\" caught an exception with message: \"{exception.Message}\"");
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Rollback failed, with message: \"{rollbackException.Message}\"");
            }
        }
    }
    
    public async Task<User?> TryGetUserAsync(string email)
    {
        var fetchedUsers = await m_usersDbContext.Users
            .AsNoTracking()
            .Where(user => user.Email.ToLower() == email.ToLower())
            .ToListAsync();

        if (fetchedUsers.Count > 1)
        {
            m_logger.LogCritical($"More than one users are registered with the email \"{email}\"");
            return null;
        }

        return fetchedUsers.FirstOrDefault();
    }
}