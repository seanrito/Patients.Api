using FluentValidation;
using Patients.Application.Dtos;

namespace Patients.Application.Validators
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientDto>
    {
        public CreatePatientValidator()
        {
            RuleFor(x => x.DocumentType)
                .NotEmpty().WithMessage("El tipo de documento es obligatorio.")
                .MaximumLength(10);

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("El número de documento es obligatorio.")
                .MaximumLength(20);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(80);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(80);

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("La fecha de nacimiento no puede ser futura.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("El correo electrónico no es válido.");
        }
    }
}
