USE [PatientsDb]
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetPatientsForExport]
    @Name NVARCHAR(80) = NULL,
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
        p.CreatedAt
    FROM 
        Patients p
    WHERE 
        (@Name IS NULL OR (p.FirstName LIKE '%' + @Name + '%' OR p.LastName LIKE '%' + @Name + '%'))
        AND (@DocumentNumber IS NULL OR p.DocumentNumber = @DocumentNumber)
        AND (@CreatedFrom IS NULL OR p.CreatedAt >= @CreatedFrom)
        AND (@CreatedTo IS NULL OR p.CreatedAt <= @CreatedTo)
    ORDER BY 
        p.CreatedAt DESC;
END