using System.Text.Json;
using AlMostashar.Application.Common.Locations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AlMostashar.Infrastructure.Services;

public sealed class JsonLocationCatalog : ILocationCatalog
{
    private readonly IReadOnlyList<LocationGovernorate> _governorates;
    private readonly IReadOnlyList<LocationCity> _cities;
    private readonly IReadOnlyDictionary<int, LocationGovernorate> _governoratesById;
    private readonly IReadOnlyDictionary<int, LocationCity> _citiesById;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<LocationCity>> _citiesByGovernorateId;

    public JsonLocationCatalog(IConfiguration configuration, IHostEnvironment hostEnvironment)
        : this(GetConfiguredSourcePaths(configuration, hostEnvironment))
    {
    }

    public JsonLocationCatalog(params string[] sourcePaths)
    {
        if (sourcePaths.Length == 0)
            throw new InvalidOperationException("Location catalog source files are not configured.");

        var governorates = new Dictionary<int, LocationGovernorate>();
        var cities = new Dictionary<int, LocationCity>();

        foreach (var sourcePath in sourcePaths)
        {
            var normalizedPath = Path.GetFullPath(sourcePath);
            if (!File.Exists(normalizedPath))
                throw new FileNotFoundException($"Location catalog source file not found: {normalizedPath}");

            using var document = JsonDocument.Parse(File.ReadAllText(normalizedPath));
            foreach (var row in ExtractDataRows(document.RootElement))
            {
                if (TryMapGovernorate(row, out var governorate))
                {
                    if (governorates.TryGetValue(governorate.Id, out var existingGovernorate))
                    {
                        if (existingGovernorate != governorate)
                            throw new InvalidOperationException($"Conflicting governorate definition for id {governorate.Id}.");
                    }
                    else
                    {
                        governorates[governorate.Id] = governorate;
                    }

                    continue;
                }

                if (TryMapCity(row, out var city))
                {
                    if (cities.TryGetValue(city.Id, out var existingCity))
                    {
                        if (existingCity != city)
                            throw new InvalidOperationException($"Conflicting city definition for id {city.Id}.");
                    }
                    else
                    {
                        cities[city.Id] = city;
                    }
                }
            }
        }

        if (governorates.Count == 0 || cities.Count == 0)
            throw new InvalidOperationException("Location catalog files do not contain governorate/city datasets.");

        var orphanCity = cities.Values.FirstOrDefault(c => !governorates.ContainsKey(c.GovernorateId));
        if (orphanCity is not null)
            throw new InvalidOperationException($"City {orphanCity.Id} references unknown governorate {orphanCity.GovernorateId}.");

        _governorates = governorates.Values.OrderBy(g => g.Id).ToList();
        _cities = cities.Values.OrderBy(c => c.Id).ToList();
        _governoratesById = governorates;
        _citiesById = cities;
        _citiesByGovernorateId = _cities
            .GroupBy(c => c.GovernorateId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<LocationCity>)g.OrderBy(c => c.Name).ToList());
    }

    public IReadOnlyList<LocationGovernorate> GetGovernorates() => _governorates;

    public IReadOnlyList<LocationCity> GetCities(int governorateId)
        => _citiesByGovernorateId.TryGetValue(governorateId, out var cities)
            ? cities
            : [];

    public bool TryGetGovernorate(int governorateId, out LocationGovernorate? governorate)
        => _governoratesById.TryGetValue(governorateId, out governorate);

    public bool TryGetCity(int cityId, out LocationCity? city)
        => _citiesById.TryGetValue(cityId, out city);

    public bool TryResolve(int governorateId, int cityId, out RequestLocationSnapshot? snapshot)
    {
        snapshot = null;

        if (!TryGetGovernorate(governorateId, out var governorate) || !TryGetCity(cityId, out var city))
            return false;

        if (city!.GovernorateId != governorateId)
            return false;

        snapshot = new RequestLocationSnapshot(governorate!.Id, governorate.Name, city.Id, city.Name);
        return true;
    }

    private static string[] GetConfiguredSourcePaths(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        var configuredPaths = configuration.GetSection("LocationCatalog:SourceFiles")
            .Get<string[]>();

        var sourceFiles = configuredPaths is { Length: > 0 }
            ? configuredPaths
            : ["LocationCatalog/cities.json", "LocationCatalog/states.json"];

        return sourceFiles
            .Select(path => Path.IsPathRooted(path) ? path : Path.Combine(hostEnvironment.ContentRootPath, path))
            .ToArray();
    }

    private static IEnumerable<JsonElement> ExtractDataRows(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var element in root.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
                continue;

            if (element.TryGetProperty("type", out var typeElement) &&
                typeElement.ValueKind == JsonValueKind.String &&
                typeElement.GetString() == "table" &&
                element.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in dataElement.EnumerateArray())
                {
                    if (row.ValueKind == JsonValueKind.Object)
                        yield return row;
                }

                continue;
            }

            yield return element;
        }
    }

    private static bool TryMapGovernorate(JsonElement row, out LocationGovernorate governorate)
    {
        governorate = null!;
        if (!TryReadInt(row, "id", out var id))
            return false;

        if (!TryReadString(row, "governorate_name_ar", out var arabicName))
            return false;

        var englishName = TryReadString(row, "governorate_name_en", out var englishValue)
            ? englishValue
            : string.Empty;

        governorate = new LocationGovernorate(id, arabicName, englishName);
        return true;
    }

    private static bool TryMapCity(JsonElement row, out LocationCity city)
    {
        city = null!;
        if (!TryReadInt(row, "id", out var id))
            return false;

        if (!TryReadInt(row, "governorate_id", out var governorateId))
            return false;

        if (!TryReadString(row, "city_name_ar", out var arabicName))
            return false;

        var englishName = TryReadString(row, "city_name_en", out var englishValue)
            ? englishValue
            : string.Empty;

        city = new LocationCity(id, governorateId, arabicName, englishName);
        return true;
    }

    private static bool TryReadInt(JsonElement row, string propertyName, out int value)
    {
        value = 0;
        if (!row.TryGetProperty(propertyName, out var property))
            return false;

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.TryGetInt32(out value),
            JsonValueKind.String => int.TryParse(property.GetString(), out value),
            _ => false
        };
    }

    private static bool TryReadString(JsonElement row, string propertyName, out string value)
    {
        value = string.Empty;
        if (!row.TryGetProperty(propertyName, out var property))
            return false;

        if (property.ValueKind != JsonValueKind.String)
            return false;

        value = property.GetString()?.Trim() ?? string.Empty;
        return value.Length > 0;
    }
}
