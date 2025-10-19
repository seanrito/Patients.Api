# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Patients.Api/Patients.Api.csproj", "Patients.Api/"]
COPY ["Patients.Application/Patients.Application.csproj", "Patients.Application/"]
COPY ["Patients.Domain/Patients.Domain.csproj", "Patients.Domain/"]
COPY ["Patients.Infrastructure/Patients.Infrastructure.csproj", "Patients.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Patients.Api/Patients.Api.csproj"

# Copiar el resto del código
COPY . .

# Build del proyecto
WORKDIR "/src/Patients.Api"
RUN dotnet build "Patients.Api.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "Patients.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Punto de entrada
ENTRYPOINT ["dotnet", "Patients.Api.dll"]
