namespace AlMostashar.Application.Common.Models;

public record PaymentResultDto(string PaymentUrl, string ClientSecret, string IntentionId);
