using AutoMapper;
using Patients.Application.Dtos;
using Patients.Domain.Entities;

namespace Patients.Application.Mappings
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<Patient, PatientDto>().ReverseMap();
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>();
        }
    }
}
