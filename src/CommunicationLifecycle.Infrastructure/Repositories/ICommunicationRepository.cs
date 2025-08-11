using CommunicationLifecycle.Core.Entities;

namespace CommunicationLifecycle.Infrastructure.Repositories;

public interface ICommunicationRepository
{
    Task<Communication?> GetByIdAsync(int id);
    Task<Communication?> GetByIdWithHistoryAsync(int id);
    Task<IEnumerable<Communication>> GetAllAsync();
    Task<IEnumerable<Communication>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<Communication>> GetByTypeAsync(string typeCode);
    Task<IEnumerable<Communication>> GetByStatusAsync(string status);
    Task<IEnumerable<Communication>> GetByTypeAndStatusAsync(string typeCode, string status);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByTypeAsync(string typeCode);
    Task<Communication> AddAsync(Communication communication);
    Task<Communication> UpdateAsync(Communication communication);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
} 