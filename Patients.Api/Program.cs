using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Patients.Api.Middlewares;
using Patients.Application.Interfaces;
using Patients.Application.Mappings;
using Patients.Application.Services;
using Patients.Application.Validators;
using Patients.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Patients API",
        Version = "v1",
        Description = "API para la gestión de pacientes (Prueba Técnica Backend)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Sebastián Rodríguez",
            Email = "seanrito@gmail.com"
        }
    });
});
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(180); // 3 minutos para operaciones pesadas
            })
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .EnableDetailedErrors(builder.Environment.IsDevelopment());
        // EnableSensitiveDataLogging removido para evitar warnings
});

// Dependency Injection
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Mappers
builder.Services.AddAutoMapper(typeof(PatientProfile));


var app = builder.Build();

// Crear base de datos y aplicar migraciones ANTES de iniciar la app
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Reintentar hasta 10 veces con delay incremental
    int maxRetries = 10;
    int retryCount = 0;
    
    while (retryCount < maxRetries)
    {
        try
        {
            logger.LogInformation("Intento {Retry}/{MaxRetries} - Verificando conexión a base de datos...", retryCount + 1, maxRetries);
            
            // Aumentar el timeout del comando para migraciones
            db.Database.SetCommandTimeout(180); // 3 minutos
            
            // IMPORTANTE: Cambiar la cadena de conexión temporalmente a 'master' para crear la BD
            var connectionString = db.Database.GetConnectionString();
            if (connectionString != null && !connectionString.Contains("Database=master"))
            {
                // Intentar conectar primero
                try
                {
                    var canConnect = await db.Database.CanConnectAsync();
                    logger.LogInformation("Base de datos accesible");
                }
                catch (Exception)
                {
                    // Si falla, probablemente la BD no existe, así que la creamos
                    logger.LogInformation("Base de datos no existe, creándola...");
                    await db.Database.EnsureCreatedAsync();
                    logger.LogInformation("Base de datos creada");
                }
            }
            
            // Verificar si hay migraciones pendientes
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            logger.LogInformation("Migraciones pendientes: {Count}", pendingMigrations.Count());
            
            if (pendingMigrations.Any())
            {
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation("  - {Migration}", migration);
                }
                
                logger.LogInformation("Aplicando migraciones...");
                await db.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas exitosamente.");
            }
            else
            {
                logger.LogInformation("Base de datos ya está actualizada.");
            }
            
            // Éxito, salir del bucle
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            
            if (retryCount >= maxRetries)
            {
                logger.LogError(ex, "Error al inicializar la base de datos después de {MaxRetries} intentos", maxRetries);
                logger.LogWarning("La aplicación continuará sin aplicar migraciones.");
                logger.LogWarning("Aplícalas manualmente con: dotnet ef database update");
                break;
            }
            
            var waitSeconds = retryCount * 5; // 5s, 10s, 15s, etc.
            logger.LogWarning(ex, "Intento {Retry} falló. Esperando {WaitSeconds}s antes de reintentar...", retryCount, waitSeconds);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
        }
    }
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Patients API v1");
});


app.UseHttpsRedirection();

// Middleware global de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

