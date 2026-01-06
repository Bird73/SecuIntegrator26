# Use ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use SDK for Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SecuIntegrator26.API/SecuIntegrator26.API.csproj", "src/SecuIntegrator26.API/"]
COPY ["src/SecuIntegrator26.Core/SecuIntegrator26.Core.csproj", "src/SecuIntegrator26.Core/"]
COPY ["src/SecuIntegrator26.Infrastructure/SecuIntegrator26.Infrastructure.csproj", "src/SecuIntegrator26.Infrastructure/"]
COPY ["src/SecuIntegrator26.Services/SecuIntegrator26.Services.csproj", "src/SecuIntegrator26.Services/"]
COPY ["src/SecuIntegrator26.Shared/SecuIntegrator26.Shared.csproj", "src/SecuIntegrator26.Shared/"]
RUN dotnet restore "src/SecuIntegrator26.API/SecuIntegrator26.API.csproj"
COPY . .
WORKDIR "/src/src/SecuIntegrator26.API"
RUN dotnet build "SecuIntegrator26.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecuIntegrator26.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecuIntegrator26.API.dll"]
