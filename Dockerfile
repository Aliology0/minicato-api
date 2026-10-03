FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AlMostashar.Api/AlMostashar.Api.csproj", "AlMostashar.Api/"]
COPY ["AlMostashar.Application/AlMostashar.Application.csproj", "AlMostashar.Application/"]
COPY ["AlMostashar.Domain/AlMostashar.Domain.csproj", "AlMostashar.Domain/"]
COPY ["AlMostashar.Infrastructure/AlMostashar.Infrastructure.csproj", "AlMostashar.Infrastructure/"]

RUN dotnet restore "AlMostashar.Api/AlMostashar.Api.csproj"

COPY . .

WORKDIR "/src/AlMostashar.Api"
RUN dotnet publish "AlMostashar.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet AlMostashar.Api.dll"]
