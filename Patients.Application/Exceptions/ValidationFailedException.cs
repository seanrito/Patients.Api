using FluentValidation.Results;

namespace Patients.Application.Exceptions
{
    public class ValidationFailedException : Exception
    {
        public ValidationFailedException(ValidationResult validationResult)
            : base("Se han producido uno o más errores de validación.")
        {
            Errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}