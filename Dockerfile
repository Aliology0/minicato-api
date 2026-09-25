FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["AlMostashar.Api/Minicato.Api.csproj", "AlMostashar.Api/"]
COPY ["AlMostashar.Application/Minicato.Application.csproj", "AlMostashar.Application/"]
COPY ["AlMostashar.Domain/Minicato.Domain.csproj", "AlMostashar.Domain/"]
COPY ["AlMostashar.Infrastructure/Minicato.Infrastructure.csproj", "AlMostashar.Infrastructure/"]

RUN dotnet restore "AlMostashar.Api/Minicato.Api.csproj"

COPY . .

WORKDIR "/src/AlMostashar.Api"

RUN dotnet publish "Minicato.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Minicato.Api.dll"]