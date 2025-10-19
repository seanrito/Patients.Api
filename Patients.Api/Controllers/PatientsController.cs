using Microsoft.AspNetCore.Mvc;
using Patients.Application.Dtos;
using Patients.Application.DTOs;
using Patients.Application.Interfaces;

namespace Patients.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Obtiene una lista paginada de pacientes.
        /// </summary>
        /// <remarks>
        /// Permite aplicar filtros opcionales y ordenamiento dinámico.
        /// 
        /// **Parámetros admitidos:**
        /// - `name`: filtra por coincidencia parcial en nombres o apellidos.  
        /// - `documentNumber`: busca coincidencia exacta.  
        /// - `createdFrom`, `createdTo`: rango de fechas de creación.  
        /// - `sortBy`: campo para ordenar (FirstName, LastName, CreatedAt).  
        /// - `sortDir`: dirección de ordenamiento (`asc` o `desc`).  
        /// - `page` y `pageSize`: controlan la paginación (máx 100 por página).  
        ///
        /// **Ejemplo:**
        /// ```
        /// GET /api/patients?name=seb&sortBy=CreatedAt&sortDir=desc&page=1&pageSize=5
        /// ```
        /// </remarks>
        /// <param name="query">Parámetros de búsqueda, orden y paginación.</param>
        /// <response code="200">Devuelve una lista paginada de pacientes.</response>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<PatientDto>>> GetAll([FromQuery] PatientQueryParamsDto query)
        {
            var result = await _patientService.GetAllAsync(query);
            return Ok(result);
        }



        /// <summary>
        /// Obtiene un paciente por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            return patient == null ? NotFound() : Ok(patient);
        }

        /// <summary>
        /// Crea un nuevo paciente.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            var created = await _patientService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PatientId }, created);
        }

        /// <summary>
        /// Actualiza la información de un paciente existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            var updated = await _patientService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        /// <summary>
        /// Elimina un paciente por su ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _patientService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }


        /// <summary>
        /// Exporta pacientes filtrados en formato CSV.
        /// </summary>
        /// <remarks>
        /// Devuelve un archivo CSV de los pacientes creados después de `createdFrom`,
        /// aplicando los mismos filtros que el listado.
        /// 
        /// Ejemplo:
        /// GET /api/patients/export?createdFrom=2025-01-01
        /// </remarks>
        /// <param name="query">Filtros y parámetros de exportación.</param>
        /// <returns>Archivo CSV con los pacientes filtrados.</returns>
        [HttpGet("export")]
        [Produces("text/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] PatientQueryParamsDto query)
        {
            var csvBytes = await _patientService.ExportToCsvAsync(query);
            var fileName = $"patients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

    }
}