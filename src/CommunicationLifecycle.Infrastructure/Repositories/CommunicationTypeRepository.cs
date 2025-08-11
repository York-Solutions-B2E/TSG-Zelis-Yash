using Microsoft.EntityFrameworkCore;
using CommunicationLifecycle.Core.Entities;
using CommunicationLifecycle.Infrastructure.Data;

namespace CommunicationLifecycle.Infrastructure.Repositories;

public class CommunicationTypeRepository : ICommunicationTypeRepository
{
    private readonly CommunicationDbContext _context;

    public CommunicationTypeRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationType?> GetByCodeAsync(string typeCode)
    {
        return await _context.CommunicationTypes
            .FirstOrDefaultAsync(ct => ct.TypeCode == typeCode);
    }

    public async Task<CommunicationType?> GetByCodeWithStatusesAsync(string typeCode)
    {
        return await _context.CommunicationTypes
            .Include(ct => ct.TypeStatuses.OrderBy(ts => ts.DisplayOrder))
            .FirstOrDefaultAsync(ct => ct.TypeCode == typeCode);
    }

    public async Task<IEnumerable<CommunicationType>> GetAllAsync()
    {
        return await _context.CommunicationTypes
            .OrderBy(ct => ct.DisplayName)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommunicationType>> GetActiveAsync()
    {
        return await _context.CommunicationTypes
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.DisplayName)
            .ToListAsync();
    }

    public async Task<CommunicationType> AddAsync(CommunicationType communicationType)
    {
        _context.CommunicationTypes.Add(communicationType);
        await _context.SaveChangesAsync();
        return communicationType;
    }

    public async Task<CommunicationType> UpdateAsync(CommunicationType communicationType)
    {
        _context.Entry(communicationType).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return communicationType;
    }

    public async Task DeleteAsync(string typeCode)
    {
        var communicationType = await GetByCodeAsync(typeCode);
        if (communicationType != null)
        {
            _context.CommunicationTypes.Remove(communicationType);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string typeCode)
    {
        return await _context.CommunicationTypes.AnyAsync(ct => ct.TypeCode == typeCode);
    }

    public async Task<IEnumerable<string>> GetValidStatusesForTypeAsync(string typeCode)
    {
        return await _context.CommunicationTypeStatuses
            .Where(cts => cts.TypeCode == typeCode)
            .OrderBy(cts => cts.DisplayOrder)
            .Select(cts => cts.StatusCode)
            .ToListAsync();
    }
} 