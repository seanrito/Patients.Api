namespace Patients.Application.Exceptions
{
    public class DuplicatePatientException : Exception
    {
        public DuplicatePatientException(string message)
            : base(message)
        {
        }
    }
}
