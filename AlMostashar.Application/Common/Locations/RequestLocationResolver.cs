using AlMostashar.Application.Common.Constants;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Common.Locations;

public static class RequestLocationResolver
{
    public static Result<RequestLocationSnapshot> ResolveRequired(
        ILocationCatalog catalog,
        int governorateId,
        int cityId)
    {
        if (!catalog.TryGetGovernorate(governorateId, out var governorate))
        {
            return Result<RequestLocationSnapshot>.Failure(
                new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
        }

        if (!catalog.TryGetCity(cityId, out var city))
        {
            return Result<RequestLocationSnapshot>.Failure(
                new Error("Validation.CityLookupNotFound", Messages.Validation.CityLookupNotFound));
        }

        if (city is null || city.GovernorateId != governorateId)
        {
            return Result<RequestLocationSnapshot>.Failure(
                new Error("Validation.CityGovernorateMismatch", Messages.Validation.CityGovernorateMismatch));
        }

        if (governorate is null)
        {
            return Result<RequestLocationSnapshot>.Failure(
                new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
        }

        return Result<RequestLocationSnapshot>.Success(new RequestLocationSnapshot(
            governorate.Id,
            governorate.Name,
            city.Id,
            city.Name));
    }

    public static Result<LocationFilterSnapshot> ResolveOptional(
        ILocationCatalog catalog,
        int? governorateId,
        int? cityId)
    {
        string? governorateName = null;
        string? governorateEnglishName = null;
        string? cityName = null;
        string? cityEnglishName = null;

        if (governorateId.HasValue)
        {
            if (!catalog.TryGetGovernorate(governorateId.Value, out var governorate))
            {
                return Result<LocationFilterSnapshot>.Failure(
                    new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
            }

            if (governorate is null)
            {
                return Result<LocationFilterSnapshot>.Failure(
                    new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
            }

            governorateName = governorate.Name;
            governorateEnglishName = governorate.EnglishName;
        }

        if (cityId.HasValue)
        {
            if (!catalog.TryGetCity(cityId.Value, out var city))
            {
                return Result<LocationFilterSnapshot>.Failure(
                    new Error("Validation.CityLookupNotFound", Messages.Validation.CityLookupNotFound));
            }

            if (city is null)
            {
                return Result<LocationFilterSnapshot>.Failure(
                    new Error("Validation.CityLookupNotFound", Messages.Validation.CityLookupNotFound));
            }

            if (governorateId.HasValue && city.GovernorateId != governorateId.Value)
            {
                return Result<LocationFilterSnapshot>.Failure(
                    new Error("Validation.CityGovernorateMismatch", Messages.Validation.CityGovernorateMismatch));
            }

            cityName = city.Name;
            cityEnglishName = city.EnglishName;
            governorateId ??= city.GovernorateId;

            if (governorateName is null)
            {
                if (!catalog.TryGetGovernorate(city.GovernorateId, out var governorate))
                {
                    return Result<LocationFilterSnapshot>.Failure(
                        new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
                }

                if (governorate is null)
                {
                    return Result<LocationFilterSnapshot>.Failure(
                        new Error("Validation.GovernorateLookupNotFound", Messages.Validation.GovernorateLookupNotFound));
                }

                governorateName = governorate.Name;
                governorateEnglishName = governorate.EnglishName;
            }
        }

        return Result<LocationFilterSnapshot>.Success(
            new LocationFilterSnapshot(
                governorateId,
                governorateName,
                governorateEnglishName,
                cityId,
                cityName,
                cityEnglishName));
    }
}
