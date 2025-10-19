#!/bin/bash

# Colores para mejor visualización
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================"
echo "  Patients API - Docker Quick Start"
echo "========================================"
echo ""

show_menu() {
    echo "Selecciona una opción:"
    echo ""
    echo "1. Iniciar aplicación (API + SQL Server)"
    echo "2. Ver logs de la API"
    echo "3. Ver logs de SQL Server"
    echo "4. Ver estado de los contenedores"
    echo "5. Detener aplicación"
    echo "6. Detener y limpiar todo (incluyendo datos)"
    echo "7. Reconstruir imágenes"
    echo "8. Abrir Swagger en navegador"
    echo "9. Salir"
    echo ""
}

start_app() {
    echo ""
    echo -e "${BLUE}Iniciando aplicación...${NC}"
    docker-compose up -d
    echo ""
    echo -e "${GREEN}? Aplicación iniciada!${NC}"
    echo -e "${GREEN}? API: http://localhost:5000${NC}"
    echo -e "${GREEN}? Swagger: http://localhost:5000/swagger${NC}"
    echo -e "${GREEN}? SQL Server: localhost:1433${NC}"
    echo ""
    read -p "Presiona Enter para continuar..."
}

logs_api() {
    echo ""
    echo -e "${BLUE}Mostrando logs de la API (Ctrl+C para salir)...${NC}"
    docker-compose logs -f patients-api
}

logs_sql() {
    echo ""
    echo -e "${BLUE}Mostrando logs de SQL Server (Ctrl+C para salir)...${NC}"
    docker-compose logs -f sqlserver
}

status() {
    echo ""
    echo -e "${BLUE}Estado de los contenedores:${NC}"
    docker-compose ps
    echo ""
    read -p "Presiona Enter para continuar..."
}

stop_app() {
    echo ""
    echo -e "${YELLOW}Deteniendo aplicación...${NC}"
    docker-compose down
    echo ""
    echo -e "${GREEN}? Aplicación detenida${NC}"
    echo ""
    read -p "Presiona Enter para continuar..."
}

clean_all() {
    echo ""
    echo -e "${RED}ADVERTENCIA: Esto eliminará todos los datos!${NC}"
    read -p "¿Estás seguro? (s/N): " confirm
    if [[ $confirm == [sS] ]]; then
        docker-compose down -v
        echo -e "${GREEN}? Limpieza completa realizada${NC}"
    else
        echo "Operación cancelada"
    fi
    echo ""
    read -p "Presiona Enter para continuar..."
}

rebuild() {
    echo ""
    echo -e "${BLUE}Reconstruyendo imágenes...${NC}"
    docker-compose build --no-cache
    docker-compose up -d
    echo ""
    echo -e "${GREEN}? Imágenes reconstruidas e iniciadas${NC}"
    echo ""
    read -p "Presiona Enter para continuar..."
}

open_swagger() {
    echo ""
    echo -e "${BLUE}Abriendo Swagger UI...${NC}"
    
    # Detectar sistema operativo
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        open http://localhost:5000/swagger
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        # Linux
        xdg-open http://localhost:5000/swagger 2>/dev/null || echo "Abre manualmente: http://localhost:5000/swagger"
    fi
    
    read -p "Presiona Enter para continuar..."
}

# Menú principal
while true; do
    clear
    echo "========================================"
    echo "  Patients API - Docker Quick Start"
    echo "========================================"
    echo ""
    show_menu
    read -p "Ingresa tu opción (1-9): " option
    
    case $option in
        1) start_app ;;
        2) logs_api ;;
        3) logs_sql ;;
        4) status ;;
        5) stop_app ;;
        6) clean_all ;;
        7) rebuild ;;
        8) open_swagger ;;
        9) echo ""; echo "Saliendo..."; exit 0 ;;
        *) echo ""; echo -e "${RED}Opción inválida${NC}"; sleep 2 ;;
    esac
done
