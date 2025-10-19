namespace Patients.Application.DTOs
{
    public class PatientQueryParamsDto
    {
        public string? Name { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
