using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using AlMostashar.Application.Common.Exceptions;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalAi.DTOs;
using AlMostashar.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlMostashar.Infrastructure.Services;

public sealed class LegalAiClient : ILegalAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly LegalAiOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LegalAiClient> _logger;

    public LegalAiClient(
        HttpClient httpClient,
        IOptions<LegalAiOptions> options,
        IMemoryCache cache,
        ILogger<LegalAiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LegalAiChatResponse> AskAsync(string query, CancellationToken cancellationToken)
    {
        var totalWatch = Stopwatch.StartNew();
        var cacheKey = $"legal-ai:chat:{NormalizeQuery(query)}";
        if (_options.CacheEnabled && _cache.TryGetValue(cacheKey, out LegalAiChatResponse? cached) && cached is not null)
        {
            cached.CacheHit = true;
            cached.ElapsedMilliseconds = totalWatch.ElapsedMilliseconds;
            _logger.LogInformation(
                "Legal AI chat cache hit. Host={Host} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                cached.ElapsedMilliseconds);
            return cached;
        }

        _logger.LogInformation(
            "Legal AI chat cache miss. Host={Host} TimeoutSeconds={TimeoutSeconds} ApiKeyConfigured={ApiKeyConfigured}",
            _httpClient.BaseAddress?.Host,
            _options.TimeoutSeconds,
            !string.IsNullOrWhiteSpace(_options.ApiKey));

        var request = new HttpRequestMessage(HttpMethod.Post, "chat")
        {
            Content = JsonContent.Create(new LegalAiChatRequest { Query = query }, options: JsonOptions)
        };

        AddInternalTokenHeader(request);

        try
        {
            var hfWatch = Stopwatch.StartNew();
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            hfWatch.Stop();
            _logger.LogInformation(
                "Legal AI HF chat call completed. Host={Host} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                (int)response.StatusCode,
                hfWatch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Legal AI service returned non-success status code {StatusCode}.",
                    (int)response.StatusCode);

                throw new LegalAiServiceUnavailableException("Legal AI service returned a non-success response.");
            }

            var aiResponse = await response.Content.ReadFromJsonAsync<LegalAiChatResponse>(JsonOptions, cancellationToken);
            if (aiResponse is null)
            {
                throw new LegalAiServiceUnavailableException("Legal AI service returned an empty response.");
            }

            totalWatch.Stop();
            aiResponse.CacheHit = false;
            aiResponse.ElapsedMilliseconds = totalWatch.ElapsedMilliseconds;
            if (_options.CacheEnabled && _options.CacheDurationMinutes > 0)
            {
                // Safe while Legal AI answers are not user-specific. Scope/disable if personalization is added.
                _cache.Set(
                    cacheKey,
                    aiResponse,
                    TimeSpan.FromMinutes(_options.CacheDurationMinutes));
            }

            return aiResponse;
        }
        catch (LegalAiServiceUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or NotSupportedException)
        {
            _logger.LogWarning(
                "Legal AI service request failed. Host={Host} ExceptionType={ExceptionType} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                ex.GetType().Name,
                totalWatch.ElapsedMilliseconds);
            throw new LegalAiServiceUnavailableException("Legal AI service request failed.", ex);
        }
    }

    public async Task WarmupAsync(CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        var request = new HttpRequestMessage(HttpMethod.Post, "warmup");
        AddInternalTokenHeader(request);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            watch.Stop();
            _logger.LogInformation(
                "Legal AI warmup call completed. Host={Host} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                (int)response.StatusCode,
                watch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Legal AI warmup returned non-success status code {StatusCode}.",
                    (int)response.StatusCode);
                throw new LegalAiServiceUnavailableException("Legal AI service warmup failed.");
            }
        }
        catch (LegalAiServiceUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(
                "Legal AI warmup request failed. Host={Host} ExceptionType={ExceptionType} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                ex.GetType().Name,
                watch.ElapsedMilliseconds);
            throw new LegalAiServiceUnavailableException("Legal AI service warmup failed.", ex);
        }
    }

    public async Task<LegalAiInfoResponse> GetInfoAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "legal-info");
        AddInternalTokenHeader(request);
        return await SendForJsonAsync<LegalAiInfoResponse>(request, "Legal AI info request failed.", cancellationToken);
    }

    public async Task<LegalAiHealthResponse> HealthAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "health");
        return await SendForJsonAsync<LegalAiHealthResponse>(request, "Legal AI health request failed.", cancellationToken);
    }

    private async Task<TResponse> SendForJsonAsync<TResponse>(
        HttpRequestMessage request,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            watch.Stop();
            _logger.LogInformation(
                "Legal AI gateway call completed. Host={Host} Method={Method} Path={Path} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                _httpClient.BaseAddress?.Host,
                request.Method.Method,
                request.RequestUri?.ToString(),
                (int)response.StatusCode,
                watch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Legal AI service returned non-success status code {StatusCode}.",
                    (int)response.StatusCode);
                throw new LegalAiServiceUnavailableException(failureMessage);
            }

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
            if (payload is null)
            {
                throw new LegalAiServiceUnavailableException(failureMessage);
            }

            return payload;
        }
        catch (LegalAiServiceUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or NotSupportedException)
        {
            _logger.LogWarning(
                "{FailureMessage} Host={Host} ExceptionType={ExceptionType} ElapsedMs={ElapsedMs}",
                failureMessage,
                _httpClient.BaseAddress?.Host,
                ex.GetType().Name,
                watch.ElapsedMilliseconds);
            throw new LegalAiServiceUnavailableException(failureMessage, ex);
        }
    }

    private void AddInternalTokenHeader(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(_options.HeaderName)
            ? "X-Internal-Service-Token"
            : _options.HeaderName.Trim();

        request.Headers.TryAddWithoutValidation(headerName, _options.ApiKey);
    }

    private static string NormalizeQuery(string query)
    {
        var text = query
            .Trim()
            .Replace("\u0640", string.Empty)
            .Replace("\u0623", "\u0627")
            .Replace("\u0625", "\u0627")
            .Replace("\u0622", "\u0627")
            .Replace("\u0649", "\u064A");

        return Regex.Replace(text, @"\s+", " ");
    }
}
