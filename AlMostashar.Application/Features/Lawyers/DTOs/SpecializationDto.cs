namespace AlMostashar.Application.Features.Lawyers.DTOs;

using System.Text.Json.Serialization;

public record SpecializationDto(
    [property: JsonPropertyName("id")] int Id, 
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("arabicName")] string? ArabicName = null);