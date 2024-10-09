using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using Microsoft.EntityFrameworkCore;

namespace LandsatReflectance.Api.Services;


public class TargetsDbContext : DbContext
{
    public DbSet<Target> Targets { get; set; }
    public DbSet<UserTarget> UserTargets { get; set; }
    
    public TargetsDbContext(DbContextOptions<TargetsDbContext> options) : base(options)
    { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new UserTypeConfiguration().Configure(modelBuilder.Entity<User>());
        new TargetTypeConfiguration().Configure(modelBuilder.Entity<Target>());
        new UserTargetTypeConfiguration().Configure(modelBuilder.Entity<UserTarget>());
    }
}

public sealed class TargetsService
{
    private ILogger<TargetsService> m_logger;
    private TargetsDbContext m_targetsDbContext;
    
    public TargetsService(ILogger<TargetsService> logger, TargetsDbContext targetsDbContext)
    {
        m_logger = logger;
        m_targetsDbContext = targetsDbContext;
    }

    public async Task AddTargets(Guid userGuid, List<Target> targets)
    {
        await using var transaction = await m_targetsDbContext.Database.BeginTransactionAsync();

        try
        {
            await m_targetsDbContext.Targets.AddRangeAsync(targets);
            await m_targetsDbContext.UserTargets.AddRangeAsync(targets.Select(target => new UserTarget(userGuid, target.Guid)));
            
            await m_targetsDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
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
    
    public async Task<List<Target>> GetTargetsByUserEmail(string email)
    {
        string rawSqlTemplate = 
            $"""
            SELECT t.targetguid, scenepath, scenerow, latitude, longitude, mincloudcover, maxcloudcover, notificationoffset
            FROM Users AS u
            INNER JOIN UsersTargets AS ut ON u.UserGuid = ut.UserGuid
            INNER JOIN Targets AS t ON ut.TargetGuid = t.TargetGuid
            WHERE u.Email = '{email}'
            """;

        return await m_targetsDbContext.Targets
            .FromSqlRaw(rawSqlTemplate)
            .AsNoTracking()
            .ToListAsync();
    }
}