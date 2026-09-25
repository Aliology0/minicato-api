using AlMostashar.Application.Common.Locations;
using AlMostashar.Infrastructure.Services;

namespace AlMostashar.Application.Tests.TestSupport;

public static class TestLocationCatalog
{
    private static readonly Lazy<ILocationCatalog> _catalog = new(CreateCatalog);

    public static ILocationCatalog Instance => _catalog.Value;

    public static LocationGovernorate GetGovernorateByEnglishName(string englishName)
        => Instance.GetGovernorates()
            .Single(g => string.Equals(g.EnglishName, englishName, StringComparison.OrdinalIgnoreCase));

    public static LocationCity GetCityByEnglishName(int governorateId, string englishName)
        => Instance.GetCities(governorateId)
            .Single(c => string.Equals(c.EnglishName, englishName, StringComparison.OrdinalIgnoreCase));

    private static ILocationCatalog CreateCatalog()
    {
        var repoRoot = ResolveRepoRoot();
        var citiesPath = Path.Combine(repoRoot, "AlMostashar.Api", "LocationCatalog", "cities.json");
        var statesPath = Path.Combine(repoRoot, "AlMostashar.Api", "LocationCatalog", "states.json");

        return new JsonLocationCatalog(citiesPath, statesPath);
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var apiPath = Path.Combine(current.FullName, "AlMostashar.Api");
            if (Directory.Exists(apiPath))
                return current.FullName;

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not resolve repository root for test location catalog.");
    }
}
