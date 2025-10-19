using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGetPatientsForExportProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createProcedure = @"
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPatientsForExport]
    @Name NVARCHAR(50) = NULL,
    @DocumentNumber NVARCHAR(20) = NULL,
    @CreatedFrom DATETIME2 = NULL,
    @CreatedTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.PatientId,
        p.DocumentType,
        p.DocumentNumber,
        p.FirstName,
        p.LastName,
        p.BirthDate,
        p.PhoneNumber,
        p.Email,
        p.CreatedAt,
        p.RowVersion
    FROM 
        Patients p
    WHERE 
        (@Name IS NULL OR (p.FirstName LIKE '%' + @Name + '%' OR p.LastName LIKE '%' + @Name + '%'))
        AND (@DocumentNumber IS NULL OR p.DocumentNumber = @DocumentNumber)
        AND (@CreatedFrom IS NULL OR p.CreatedAt >= @CreatedFrom)
        AND (@CreatedTo IS NULL OR p.CreatedAt <= @CreatedTo)
    ORDER BY 
        p.CreatedAt DESC;
END";

            migrationBuilder.Sql(createProcedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetPatientsForExport]");
        }
    }
}
