using EAI.Core.Interfaces;
using EAI.Core.Models;
using EAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EAI.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly EAIDbContext _context;
    private readonly ILogger<AuditRepository> _logger;

    public AuditRepository(EAIDbContext context, ILogger<AuditRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added audit log for request {RequestId}", auditLog.RequestId);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate, AuditFilters filters)
    {
        var query = _context.AuditLogs
            .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(filters.Department))
        {
            // Note: This would need to be joined with request data in a real implementation
        }

        if (!string.IsNullOrEmpty(filters.RequestType))
        {
            // Note: This would need to be joined with request data in a real implementation
        }

        if (filters.MinConfidenceScore.HasValue)
        {
            query = query.Where(log => log.ConfidenceScore >= filters.MinConfidenceScore.Value);
        }

        if (filters.MaxConfidenceScore.HasValue)
        {
            query = query.Where(log => log.ConfidenceScore <= filters.MaxConfidenceScore.Value);
        }

        if (!string.IsNullOrEmpty(filters.Decision))
        {
            query = query.Where(log => log.Decision == filters.Decision);
        }

        return await query.OrderByDescending(log => log.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetPolicyUsageAsync(string policyId, DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(log => log.Timestamp >= startDate && 
                         log.Timestamp <= endDate &&
                         log.PolicyReferences.Contains(policyId))
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }
}
