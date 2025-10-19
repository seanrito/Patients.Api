using Patients.Domain.Entities;

namespace Patients.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string entity, int entityId, string action, string username, object? changes = null);
    }
}
