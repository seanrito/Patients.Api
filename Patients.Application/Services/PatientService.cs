using AutoMapper;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Patients.Application.Dtos;
using Patients.Application.DTOs;
using Patients.Application.Exceptions;
using Patients.Application.Interfaces;
using Patients.Domain.Entities;
using Patients.Infrastructure.Persistence;
using System.Text;

namespace Patients.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CreatePatientDto> _createValidator;
        private readonly IValidator<UpdatePatientDto> _updateValidator;
        private readonly IAuditService _auditService;

        public PatientService(
            ApplicationDbContext context, 
            IMapper mapper,
            IValidator<CreatePatientDto> createValidator,
            IValidator<UpdatePatientDto> updateValidator,
            IAuditService auditService)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _auditService = auditService;
        }

        public async Task<PagedResultDto<PatientDto>> GetAllAsync(PatientQueryParamsDto query)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize > 100 ? 100 : query.PageSize;

            var patientsQuery = _context.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                patientsQuery = patientsQuery.Where(p =>
                    p.FirstName.Contains(query.Name) ||
                    p.LastName.Contains(query.Name));
            }

            if (!string.IsNullOrWhiteSpace(query.DocumentNumber))
            {
                patientsQuery = patientsQuery.Where(p => p.DocumentNumber == query.DocumentNumber);
            }

            if (query.CreatedFrom.HasValue)
            {
                patientsQuery = patientsQuery.Where(p => p.CreatedAt >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                patientsQuery = patientsQuery.Where(p => p.CreatedAt <= query.CreatedTo.Value);
            }

            bool descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            patientsQuery = query.SortBy?.ToLower() switch
            {
                "firstname" => descending ? patientsQuery.OrderByDescending(p => p.FirstName) : patientsQuery.OrderBy(p => p.FirstName),
                "lastname" => descending ? patientsQuery.OrderByDescending(p => p.LastName) : patientsQuery.OrderBy(p => p.LastName),
                "createdat" => descending ? patientsQuery.OrderByDescending(p => p.CreatedAt) : patientsQuery.OrderBy(p => p.CreatedAt),
                _ => patientsQuery.OrderBy(p => p.PatientId)
            };

            var totalCount = await patientsQuery.CountAsync();

            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<PatientDto>>(patients);

            return new PagedResultDto<PatientDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            return patient == null ? null : _mapper.Map<PatientDto>(patient);
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult);
            }

            var exists = await _context.Patients
                .AsNoTracking()
                .AnyAsync(p => p.DocumentType == dto.DocumentType && p.DocumentNumber == dto.DocumentNumber);

            if (exists)
            {
                throw new DuplicatePatientException("Ya existe un paciente con el mismo tipo y número de documento.");
            }

            var patient = _mapper.Map<Patient>(dto);
            _context.Patients.Add(patient);

            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync("Patient", patient.PatientId, "Create", "system");

            return _mapper.Map<PatientDto>(patient);
        }

        public async Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailedException(validationResult);
            }

            var existing = await _context.Patients.FindAsync(id);
            if (existing == null)
                return null;

            var exists = await _context.Patients
                .AsNoTracking()
                .Where(p => p.PatientId != id)
                .AnyAsync(p => p.DocumentType == dto.DocumentType && p.DocumentNumber == dto.DocumentNumber);

            if (exists)
            {
                throw new DuplicatePatientException("Ya existe un paciente con el mismo tipo de documento y número de documento.");
            }

            // Verificar RowVersion
            if (dto.RowVersion != null && !existing.RowVersion.SequenceEqual(dto.RowVersion))
            {
                throw new ConcurrencyException();
            }

            _mapper.Map(dto, existing);
            
            try
            {
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Patient", existing.PatientId, "Update", "system", dto);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }

            return _mapper.Map<PatientDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

           await _auditService.LogAsync("Patient", id, "Delete", "system");

            return true;
        }

        public async Task<byte[]> ExportToCsvAsync(PatientQueryParamsDto query)
        {
            if (!query.CreatedFrom.HasValue)
                throw new ArgumentException("El parámetro 'createdFrom' es obligatorio para exportar pacientes.");

            // Usar el procedimiento almacenado directamente
            var patients = await _context.Patients
                .FromSqlRaw("EXEC sp_GetPatientsForExport @Name, @DocumentNumber, @CreatedFrom, @CreatedTo",
                    new SqlParameter("@Name", (object?)query.Name ?? DBNull.Value),
                    new SqlParameter("@DocumentNumber", (object?)query.DocumentNumber ?? DBNull.Value),
                    new SqlParameter("@CreatedFrom", (object?)query.CreatedFrom ?? DBNull.Value),
                    new SqlParameter("@CreatedTo", (object?)query.CreatedTo ?? DBNull.Value))
                .AsNoTracking()
                .ToListAsync();

            if (!patients.Any())
                throw new KeyNotFoundException("No se encontraron pacientes para exportar con los filtros proporcionados.");

            var sb = new StringBuilder();
            sb.AppendLine("DocumentType,DocumentNumber,FirstName,LastName,Email,BirthDate,CreatedAt");

            foreach (var p in patients)
            {
                // Escapar campos que podrían contener comas
                var line = string.Join(",",
                    EscapeCsvField(p.DocumentType),
                    EscapeCsvField(p.DocumentNumber),
                    EscapeCsvField(p.FirstName),
                    EscapeCsvField(p.LastName),
                    EscapeCsvField(p.Email ?? ""),
                    p.BirthDate.ToString("yyyy-MM-dd"),
                    p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                
                sb.AppendLine(line);
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
        }

    }

