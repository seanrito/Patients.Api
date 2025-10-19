# Script de ayuda para ejecutar el proyecto con Docker
# Uso: .\run-docker.ps1 [comando]

param(
    [Parameter(Position=0)]
    [ValidateSet("up", "down", "build", "logs", "sql-only", "restart", "clean", "status", "check")]
    [string]$Command = "up"
)

Write-Host "?? Patients API - Docker Helper" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Función para verificar si Docker está corriendo
function Test-DockerRunning {
    try {
        $null = docker ps 2>&1
        return $true
    }
    catch {
        return $false
    }
}

# Verificar Docker antes de ejecutar comandos (excepto para 'check')
if ($Command -ne "check") {
    if (-not (Test-DockerRunning)) {
        Write-Host "? ERROR: Docker Desktop no está corriendo" -ForegroundColor Red
        Write-Host ""
        Write-Host "?? Pasos para solucionar:" -ForegroundColor Yellow
        Write-Host "  1. Abre Docker Desktop desde el menú de inicio" -ForegroundColor White
        Write-Host "  2. Espera a que aparezca 'Docker Desktop is running'" -ForegroundColor White
        Write-Host "  3. Verifica con: " -NoNewline -ForegroundColor White
        Write-Host ".\run-docker.ps1 check" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "?? O ejecuta: " -NoNewline -ForegroundColor Yellow
        Write-Host "Start-Process 'C:\Program Files\Docker\Docker\Docker Desktop.exe'" -ForegroundColor Cyan
        Write-Host ""
        exit 1
    }
}

switch ($Command) {
    "up" {
        Write-Host "?? Iniciando todos los contenedores..." -ForegroundColor Green
        docker-compose up
    }
    "build" {
        Write-Host "?? Construyendo e iniciando contenedores..." -ForegroundColor Green
        docker-compose up --build
    }
    "down" {
        Write-Host "?? Deteniendo todos los contenedores..." -ForegroundColor Yellow
        docker-compose down
    }
    "clean" {
        Write-Host "?? Deteniendo contenedores y eliminando volúmenes..." -ForegroundColor Red
        Write-Host "??  ADVERTENCIA: Esto eliminará la base de datos!" -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? (s/n)"
        if ($confirm -eq "s") {
            docker-compose down -v
            Write-Host "? Limpieza completa realizada" -ForegroundColor Green
        } else {
            Write-Host "? Operación cancelada" -ForegroundColor Yellow
        }
    }
    "sql-only" {
        Write-Host "?? Iniciando solo SQL Server..." -ForegroundColor Green
        Write-Host "Puedes ejecutar la API desde Visual Studio" -ForegroundColor Gray
        docker-compose up sqlserver
    }
    "logs" {
        Write-Host "?? Mostrando logs de la API..." -ForegroundColor Green
        docker-compose logs -f patients-api
    }
    "restart" {
        Write-Host "?? Reiniciando la API..." -ForegroundColor Green
        docker-compose restart patients-api
        Write-Host "? API reiniciada" -ForegroundColor Green
    }
    "status" {
        Write-Host "?? Estado de los contenedores:" -ForegroundColor Green
        docker-compose ps
        Write-Host ""
        Write-Host "?? Endpoints disponibles:" -ForegroundColor Cyan
        Write-Host "  API: http://localhost:5000" -ForegroundColor White
        Write-Host "  Swagger: http://localhost:5000/swagger" -ForegroundColor White
        Write-Host "  SQL Server: localhost:1433" -ForegroundColor White
        Write-Host "  Usuario SQL: sa" -ForegroundColor White
        Write-Host "  Password SQL: YourStrong@Password123" -ForegroundColor White
    }
    "check" {
        Write-Host "?? Verificando Docker..." -ForegroundColor Yellow
        Write-Host ""
        
        # Verificar si Docker está instalado
        $dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
        if (-not $dockerInstalled) {
            Write-Host "? Docker no está instalado" -ForegroundColor Red
            Write-Host "?? Descarga Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Cyan
            exit 1
        }
        
        Write-Host "? Docker está instalado" -ForegroundColor Green
        docker --version
        Write-Host ""
        
        # Verificar si Docker está corriendo
        if (Test-DockerRunning) {
            Write-Host "? Docker Desktop está corriendo" -ForegroundColor Green
            Write-Host ""
            Write-Host "?? Contenedores activos:" -ForegroundColor Cyan
            docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
            Write-Host ""
            Write-Host "?? ¡Todo listo para usar Docker!" -ForegroundColor Green
        }
        else {
            Write-Host "? Docker Desktop NO está corriendo" -ForegroundColor Red
            Write-Host ""
            Write-Host "?? Para iniciarlo:" -ForegroundColor Yellow
            Write-Host "  • Busca 'Docker Desktop' en el menú de inicio" -ForegroundColor White
            Write-Host "  • O ejecuta este comando:" -ForegroundColor White
            Write-Host "    Start-Process 'C:\Program Files\Docker\Docker\Docker Desktop.exe'" -ForegroundColor Cyan
            Write-Host ""
            
            $startDocker = Read-Host "¿Quieres que intente iniciarlo automáticamente? (s/n)"
            if ($startDocker -eq "s") {
                Write-Host "?? Iniciando Docker Desktop..." -ForegroundColor Yellow
                try {
                    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
                    Write-Host "? Esperando a que Docker inicie (esto puede tomar 30-60 segundos)..." -ForegroundColor Yellow
                    
                    $timeout = 0
                    while ($timeout -lt 60) {
                        Start-Sleep -Seconds 2
                        if (Test-DockerRunning) {
                            Write-Host ""
                            Write-Host "? ¡Docker Desktop está corriendo!" -ForegroundColor Green
                            return
                        }
                        $timeout += 2
                        Write-Host "." -NoNewline -ForegroundColor Gray
                    }
                    
                    Write-Host ""
                    Write-Host "??  Docker está tardando más de lo esperado" -ForegroundColor Yellow
                    Write-Host "   Espera un poco más y verifica manualmente" -ForegroundColor White
                }
                catch {
                    Write-Host ""
                    Write-Host "? No se pudo iniciar Docker Desktop automáticamente" -ForegroundColor Red
                    Write-Host "   Inícialo manualmente desde el menú de inicio" -ForegroundColor White
                }
            }
        }
    }
    default {
        Write-Host "? Comando no reconocido" -ForegroundColor Red
        Write-Host ""
        Write-Host "Comandos disponibles:" -ForegroundColor Yellow
        Write-Host "  check     - Verifica si Docker está corriendo" -ForegroundColor White
        Write-Host "  up        - Inicia todos los contenedores" -ForegroundColor White
        Write-Host "  build     - Construye e inicia los contenedores" -ForegroundColor White
        Write-Host "  down      - Detiene todos los contenedores" -ForegroundColor White
        Write-Host "  clean     - Detiene y elimina volúmenes (?? borra BD)" -ForegroundColor White
        Write-Host "  sql-only  - Inicia solo SQL Server" -ForegroundColor White
        Write-Host "  logs      - Muestra logs de la API" -ForegroundColor White
        Write-Host "  restart   - Reinicia la API" -ForegroundColor White
        Write-Host "  status    - Muestra el estado de los contenedores" -ForegroundColor White
    }
}
