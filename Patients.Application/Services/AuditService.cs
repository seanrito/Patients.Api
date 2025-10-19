using Patients.Application.Interfaces;
using Patients.Domain.Entities;
using Patients.Infrastructure.Persistence;
using System;
using System.Text.Json;

namespace Patients.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string entity, int entityId, string action, string username, object? changes = null)
        {
            var audit = new AuditLog
            {
                Entity = entity,
                EntityId = entityId,
                Action = action,
                Username = username,
                CreatedAt = DateTime.UtcNow,
                Changes = changes != null ? JsonSerializer.Serialize(changes) : null
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
