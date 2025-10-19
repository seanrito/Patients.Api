using Microsoft.EntityFrameworkCore;
using Patients.Domain.Entities;


namespace Patients.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId);
            entity.Property(e => e.DocumentType).HasMaxLength(10);
            entity.Property(e => e.DocumentNumber).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();
        });

        // Configuración de AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });
    }
}
