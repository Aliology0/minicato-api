namespace AlMostashar.Application.Common.Locations;

public sealed record LocationFilterSnapshot(
    int? GovernorateId,
    string? GovernorateName,
    string? GovernorateEnglishName,
    int? CityId,
    string? CityName,
    string? CityEnglishName);
