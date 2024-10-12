using LandsatReflectance.Api.Models;
using LandsatReflectance.Backend.Utils.EFConfigs;
using LandsatReflectance.Common.Models;
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

    public async Task TryAddTargetsAsync(Guid userGuid, List<Target> targets)
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
            m_logger.LogError($"\"AddTargets\" caught an exception with message: \"{exception.Message}\"");
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
    
    public async Task<List<Target>> TryGetTargetsByUserEmailAsync(string email)
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

    public async Task<Target?> TryDeleteTargetByGuidAsync(Guid targetGuid)
    {
        await using var transaction = await m_targetsDbContext.Database.BeginTransactionAsync();

        try
        {
            var targetToDelete = await m_targetsDbContext.Targets.FirstOrDefaultAsync(target => target.Guid == targetGuid);

            if (targetToDelete is null)
            {
                return null;
            }
            
            var userTargetToDelete = await m_targetsDbContext.UserTargets.FirstOrDefaultAsync(ut => ut.TargetGuid == targetGuid);
            if (userTargetToDelete is not null)
            {
                _ = m_targetsDbContext.UserTargets.Remove(userTargetToDelete);
            }

            _ = m_targetsDbContext.Targets.Remove(targetToDelete);
            
            await m_targetsDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return targetToDelete;
        }
        catch (Exception exception)
        {
            m_logger.LogError($"\"DeleteTargetByGuid\" caught an exception with message: \"{exception.Message}\"");
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Rollback failed, with message: \"{rollbackException.Message}\"");
            }
        }

        return null;
    }

    public async Task<Target?> TryReplaceTargetAsync(Target targetWithDifferentValues)
    {
        await using var transaction = await m_targetsDbContext.Database.BeginTransactionAsync();

        try
        {
            var targetToReplace = await m_targetsDbContext.Targets
                .Where(target => target.Guid == targetWithDifferentValues.Guid)
                .FirstOrDefaultAsync();

            if (targetToReplace is null)
            {
                return null;
            }

            targetToReplace.Path = targetWithDifferentValues.Path;
            targetToReplace.Row = targetWithDifferentValues.Row;
            targetToReplace.Latitude = targetWithDifferentValues.Latitude;
            targetToReplace.Longitude = targetWithDifferentValues.Longitude;
            targetToReplace.MinCloudCover = targetWithDifferentValues.MinCloudCover;
            targetToReplace.MaxCloudCover = targetWithDifferentValues.MaxCloudCover;
            targetToReplace.NotificationOffset = targetWithDifferentValues.NotificationOffset;

            await m_targetsDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return targetToReplace;
        }
        catch (Exception exception)
        {
            m_logger.LogError($"\"ReplaceTarget\" caught an exception with message: \"{exception.Message}\"");
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackException)
            {
                m_logger.LogCritical($"Rollback failed, with message: \"{rollbackException.Message}\"");
            }
        }

        return null;
    }
}