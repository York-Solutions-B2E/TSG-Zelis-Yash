using Microsoft.EntityFrameworkCore;
using CommunicationLifecycle.Core.Entities;
using CommunicationLifecycle.Infrastructure.Data;

namespace CommunicationLifecycle.Infrastructure.Repositories;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly CommunicationDbContext _context;

    public CommunicationRepository(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<Communication?> GetByIdAsync(int id)
    {
        return await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Communication?> GetByIdWithHistoryAsync(int id)
    {
        return await _context.Communications
            .Include(c => c.StatusHistory.OrderByDescending(h => h.OccurredUtc))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Communication>> GetAllAsync()
    {
        return await _context.Communications
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Communication>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.Communications
            .OrderByDescending(c => c.LastUpdatedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Communication>> GetByTypeAsync(string typeCode)
    {
        return await _context.Communications
            .Where(c => c.TypeCode == typeCode)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Communication>> GetByStatusAsync(string status)
    {
        return await _context.Communications
            .Where(c => c.CurrentStatus == status)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Communication>> GetByTypeAndStatusAsync(string typeCode, string status)
    {
        return await _context.Communications
            .Where(c => c.TypeCode == typeCode && c.CurrentStatus == status)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Communications.CountAsync();
    }

    public async Task<int> GetCountByTypeAsync(string typeCode)
    {
        return await _context.Communications
            .CountAsync(c => c.TypeCode == typeCode);
    }

    public async Task<Communication> AddAsync(Communication communication)
    {
        communication.CreatedUtc = DateTime.UtcNow;
        communication.LastUpdatedUtc = DateTime.UtcNow;

        // Add initial status history entry
        communication.StatusHistory.Add(new CommunicationStatusHistory
        {
            StatusCode = communication.CurrentStatus,
            OccurredUtc = communication.CreatedUtc,
            Notes = "Initial status"
        });

        _context.Communications.Add(communication);
        await _context.SaveChangesAsync();
        return communication;
    }

    public async Task<Communication> UpdateAsync(Communication communication)
    {
        communication.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Entry(communication).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return communication;
    }

    public async Task DeleteAsync(int id)
    {
        var communication = await GetByIdAsync(id);
        if (communication != null)
        {
            _context.Communications.Remove(communication);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Communications.AnyAsync(c => c.Id == id);
    }
} 