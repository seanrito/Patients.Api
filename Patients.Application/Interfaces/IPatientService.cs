using Patients.Application.Dtos;

namespace Patients.Application.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResultDto<PatientDto>> GetAllAsync(PatientQueryParamsDto query);
        Task<PatientDto?> GetByIdAsync(int id);
        Task<PatientDto> CreateAsync(CreatePatientDto dto);
        Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto);
        Task<bool> DeleteAsync(int id);
        Task<byte[]> ExportToCsvAsync(PatientQueryParamsDto query);
    }
}
