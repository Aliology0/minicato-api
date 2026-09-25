namespace AlMostashar.Application.Common.Locations;

public interface ILocationCatalog
{
    IReadOnlyList<LocationGovernorate> GetGovernorates();
    IReadOnlyList<LocationCity> GetCities(int governorateId);
    bool TryGetGovernorate(int governorateId, out LocationGovernorate? governorate);
    bool TryGetCity(int cityId, out LocationCity? city);
    bool TryResolve(int governorateId, int cityId, out RequestLocationSnapshot? snapshot);
}

public sealed record LocationGovernorate(
    int Id,
    string Name,
    string EnglishName);

public sealed record LocationCity(
    int Id,
    int GovernorateId,
    string Name,
    string EnglishName);
