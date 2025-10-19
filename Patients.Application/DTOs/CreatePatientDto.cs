namespace Patients.Application.Dtos
{
    public class CreatePatientDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
