FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServicoPalavra.sln ./
COPY src/ServicoPalavra.Api/ServicoPalavra.Api.csproj src/ServicoPalavra.Api/
COPY src/ServicoPalavra.Application/ServicoPalavra.Application.csproj src/ServicoPalavra.Application/
COPY src/ServicoPalavra.Domain/ServicoPalavra.Domain.csproj src/ServicoPalavra.Domain/
COPY src/ServicoPalavra.Infrastructure/ServicoPalavra.Infrastructure.csproj src/ServicoPalavra.Infrastructure/
COPY tests/ServicoPalavra.UnitTests/ServicoPalavra.UnitTests.csproj tests/ServicoPalavra.UnitTests/
COPY tests/ServicoPalavra.IntegrationTests/ServicoPalavra.IntegrationTests.csproj tests/ServicoPalavra.IntegrationTests/
RUN dotnet restore ServicoPalavra.sln

COPY . .
RUN dotnet publish src/ServicoPalavra.Api/ServicoPalavra.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ServicoPalavra.Api.dll"]
