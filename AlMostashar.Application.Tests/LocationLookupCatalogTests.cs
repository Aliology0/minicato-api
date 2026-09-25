using AlMostashar.Application.Features.Lookups.Queries.GetCities;
using AlMostashar.Application.Features.Lookups.Queries.GetGovernorates;
using AlMostashar.Application.Tests.TestSupport;
using AlMostashar.Application.Features.Lookups.DTOs;

namespace AlMostashar.Application.Tests;

public class LocationLookupCatalogTests
{
    [Fact]
    public async Task GetGovernorates_ReturnsFullArabicGovernorateCatalog()
    {
        var handler = new GetGovernoratesQueryHandler(TestLocationCatalog.Instance);
        var result = await handler.Handle(new GetGovernoratesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var governorates = Assert.IsAssignableFrom<IReadOnlyList<GovernorateLookupDto>>(result.Value);
        Assert.Equal(27, governorates.Count);
        Assert.Contains(governorates, g => g.Id == 1 && g.Name == "القاهرة");
        Assert.Contains(governorates, g => g.Id == 27 && g.Name == "سوهاج");
    }

    [Fact]
    public async Task GetCities_ReturnsFullImportedCityCatalogForGovernorate()
    {
        var handler = new GetCitiesQueryHandler(TestLocationCatalog.Instance);
        var cairoResult = await handler.Handle(new GetCitiesQuery(1), CancellationToken.None);
        var gizaResult = await handler.Handle(new GetCitiesQuery(2), CancellationToken.None);

        Assert.True(cairoResult.IsSuccess);
        Assert.True(gizaResult.IsSuccess);
        var cairoCities = Assert.IsAssignableFrom<IReadOnlyList<CityLookupDto>>(cairoResult.Value);
        var gizaCities = Assert.IsAssignableFrom<IReadOnlyList<CityLookupDto>>(gizaResult.Value);
        var totalCities = TestLocationCatalog.Instance.GetGovernorates()
            .SelectMany(g => TestLocationCatalog.Instance.GetCities(g.Id))
            .Count();
        Assert.Equal(396, totalCities);
        Assert.NotEmpty(cairoCities);
        Assert.NotEmpty(gizaCities);
        Assert.Contains(cairoCities, c => c.Id == 39 && c.Name == "مدينة نصر");
        Assert.Contains(gizaCities, c => c.Id == 73 && c.Name == "الدقى");
    }
}
