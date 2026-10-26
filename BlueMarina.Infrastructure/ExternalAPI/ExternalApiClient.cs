using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BlueMarina.Application.Interfaces.ExternalAPI;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Infrastructure.ExternalAPI;

public sealed class ExternalApiClient : IExternalApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(
        HttpClient httpClient,
        ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string url,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "[EXT-API] GET {Url} → {StatusCode}. Body: {Body}",
            url,
            (int)response.StatusCode,
            body);

        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} from {url}. Body: {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized
            || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} from {url}. Body: {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(body))
            return default;

        if (typeof(T) == typeof(string))
        {
            return (T)(object)body;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions);
        }
        catch (JsonException jsonEx)
        {
            throw new HttpRequestException(
                $"Failed to deserialize response from {url}. Body: {body}",
                inner: jsonEx,
                statusCode: response.StatusCode);
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url,
        TRequest payload,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "[EXT-API] POST {Url} → {StatusCode}. Body: {Body}",
            url,
            (int)response.StatusCode,
            body);

        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} from {url}. Body: {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized
            || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} from {url}. Body: {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(body))
            return default;

        if (typeof(TResponse) == typeof(string))
        {
            return (TResponse)(object)body;
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
        }
        catch (JsonException jsonEx)
        {
            throw new HttpRequestException(
                $"Failed to deserialize response from {url}. Body: {body}",
                inner: jsonEx,
                statusCode: response.StatusCode);
        }
    }
}