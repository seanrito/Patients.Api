namespace Patients.Domain.Entities;

public class AuditLog
{
    public long AuditLogId { get; set; }
    public string Entity { get; set; } = null!;
    public int EntityId { get; set; }
    public string Action { get; set; } = null!;
    public string Username { get; set; } = "system";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Changes { get; set; }
}
