using CommunicationLifecycle.Core.Entities;

namespace CommunicationLifecycle.Infrastructure.Repositories;

public interface ICommunicationTypeRepository
{
    Task<CommunicationType?> GetByCodeAsync(string typeCode);
    Task<CommunicationType?> GetByCodeWithStatusesAsync(string typeCode);
    Task<IEnumerable<CommunicationType>> GetAllAsync();
    Task<IEnumerable<CommunicationType>> GetActiveAsync();
    Task<CommunicationType> AddAsync(CommunicationType communicationType);
    Task<CommunicationType> UpdateAsync(CommunicationType communicationType);
    Task DeleteAsync(string typeCode);
    Task<bool> ExistsAsync(string typeCode);
    Task<IEnumerable<string>> GetValidStatusesForTypeAsync(string typeCode);
} 