namespace Patients.Application.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException() 
            : base("El registro ha sido modificado por otro usuario. Por favor, actualice e intente nuevamente.") { }
    }
}