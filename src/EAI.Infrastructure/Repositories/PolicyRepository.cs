using EAI.Core.Interfaces;
using EAI.Core.Models;
using EAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EAI.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly EAIDbContext _context;
    private readonly ILogger<PolicyRepository> _logger;

    public PolicyRepository(EAIDbContext context, ILogger<PolicyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PolicyDocument>> GetAllAsync()
    {
        return await _context.Policies
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<PolicyDocument?> GetByIdAsync(string id)
    {
        return await _context.Policies
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<IEnumerable<PolicyDocument>> GetRelevantPoliciesAsync(string requestType, Dictionary<string, object> context)
    {
        return await _context.Policies
            .Where(p => p.IsActive && 
                       p.PolicyType == requestType &&
                       p.EffectiveDate <= DateTime.UtcNow &&
                       p.ExpiryDate >= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task AddAsync(PolicyDocument policy)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added policy {PolicyId}", policy.Id);
    }

    public async Task UpdateAsync(PolicyDocument policy)
    {
        _context.Policies.Update(policy);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated policy {PolicyId}", policy.Id);
    }

    public async Task DeleteAsync(string id)
    {
        var policy = await _context.Policies.FindAsync(id);
        if (policy != null)
        {
            policy.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Soft deleted policy {PolicyId}", id);
        }
    }
}
