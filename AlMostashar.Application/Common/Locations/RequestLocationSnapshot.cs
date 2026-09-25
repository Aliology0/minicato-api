namespace AlMostashar.Application.Common.Locations;

public sealed record RequestLocationSnapshot(
    int GovernorateId,
    string Governorate,
    int CityId,
    string City);
