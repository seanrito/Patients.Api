@echo off
echo ========================================
echo   Patients API - Docker Quick Start
echo ========================================
echo.

:menu
echo Selecciona una opcion:
echo.
echo 1. Iniciar aplicacion (API + SQL Server)
echo 2. Ver logs de la API
echo 3. Ver logs de SQL Server
echo 4. Ver estado de los contenedores
echo 5. Detener aplicacion
echo 6. Detener y limpiar todo (incluyendo datos)
echo 7. Reconstruir imagenes
echo 8. Abrir Swagger en navegador
echo 9. Salir
echo.

set /p option="Ingresa tu opcion (1-9): "

if "%option%"=="1" goto start
if "%option%"=="2" goto logs-api
if "%option%"=="3" goto logs-sql
if "%option%"=="4" goto status
if "%option%"=="5" goto stop
if "%option%"=="6" goto clean
if "%option%"=="7" goto rebuild
if "%option%"=="8" goto swagger
if "%option%"=="9" goto exit

echo Opcion invalida
goto menu

:start
echo.
echo Iniciando aplicacion...
docker-compose up -d
echo.
echo ? Aplicacion iniciada!
echo ? API: http://localhost:5000
echo ? Swagger: http://localhost:5000/swagger
echo ? SQL Server: localhost:1433
echo.
pause
goto menu

:logs-api
echo.
echo Mostrando logs de la API (Ctrl+C para salir)...
docker-compose logs -f patients-api
goto menu

:logs-sql
echo.
echo Mostrando logs de SQL Server (Ctrl+C para salir)...
docker-compose logs -f sqlserver
goto menu

:status
echo.
echo Estado de los contenedores:
docker-compose ps
echo.
pause
goto menu

:stop
echo.
echo Deteniendo aplicacion...
docker-compose down
echo.
echo ? Aplicacion detenida
echo.
pause
goto menu

:clean
echo.
echo ADVERTENCIA: Esto eliminara todos los datos!
set /p confirm="¿Estas seguro? (S/N): "
if /i "%confirm%"=="S" (
    docker-compose down -v
    echo ? Limpieza completa realizada
) else (
    echo Operacion cancelada
)
echo.
pause
goto menu

:rebuild
echo.
echo Reconstruyendo imagenes...
docker-compose build --no-cache
docker-compose up -d
echo.
echo ? Imagenes reconstruidas e iniciadas
echo.
pause
goto menu

:swagger
echo.
echo Abriendo Swagger UI...
start http://localhost:5000/swagger
pause
goto menu

:exit
echo.
echo Saliendo...
exit
