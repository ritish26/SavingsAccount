using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.EventStore;

public class EventStoreAdmin : IEventStoreAdmin
{
    private HttpClient _httpClient;
    private readonly ILogger<EventStoreAdmin> _logger;

    public EventStoreAdmin([NotNull]HttpClient client, [NotNull] IConfiguration configuration,
    [NotNull] ILogger<EventStoreAdmin> logger)
    {
        _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var httpUri= configuration.GetSection("EventStore:HttpUri").Value;
        var username = configuration.GetSection("EventStore:Username").Value;
        var password = configuration.GetSection("EventStore:Password").Value;
        
        var authenticationToken = Convert.ToBase64String(Encoding.UTF8.
            GetBytes($"{username}:{password}"));
        
        _httpClient.BaseAddress = new Uri(httpUri);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authenticationToken);
        
        _logger.LogInformation($"Using HTTP uri: {httpUri}");
    }
    public async Task CreateContinuousProjection(string projectionName, string query, bool enabled, 
        bool emit, bool trackEmittedStreams)
    {
        if(projectionName == null) throw new ArgumentNullException(nameof(projectionName));
        if(query == null) throw new ArgumentNullException(nameof(query));

        if (_httpClient == null)
        {
            throw new InvalidOperationException("EventStore client is not initialized.");
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("CreateContinuousProjection: name {name}, enabled={enabled}, emit= {emit}," +
                             "trackemittedStreams={trackEmittedStreams} query: {query}",
                projectionName, enabled, emit, trackEmittedStreams, query);
        }

        var enabledValue = enabled ? "true" : "false";
        var emitValue = emit ? "true" : "false";
        var trackedStreamsValue = trackEmittedStreams ? "true" : "false";

        var requestUri =
            $"/Projections/continuous?name={Uri.EscapeUriString(projectionName)}&)+" +
            $"type=JS&enabled={enabledValue}&emit={emitValue}&trackedStreams={trackedStreamsValue}";
        
        _logger.LogDebug($"CreateContinuousProjection: requestUri={requestUri}");
        using var responseMessage = await _httpClient.PostAsync(requestUri, 
            new StringContent(query));

        if (responseMessage.IsSuccessStatusCode)
        {
            _logger.LogDebug($"CreateContinuousProjection: response code {(int)responseMessage.StatusCode}");
            return;
        }
        
        _logger.LogDebug($"CreateContinuousProjection Failed: requestUri={requestUri}");
        
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        _logger.LogDebug($"CreateContinuousProjection: responseContent={responseContent}");
    }

    public async Task<bool> ProjectionExists(string projectionName)
    {
        if (_httpClient == null)
        {
            throw new InvalidOperationException("EventStore client is not initialized.");
        }
        
        _logger.LogDebug($"Checking CreateContinuousProjection exists, name {projectionName}");
        
        using var responseMessage = await _httpClient.GetAsync($"/Projections/{projectionName}");
        _logger.LogDebug("response.StatusCode = {statusCode}", responseMessage.StatusCode);

        if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        return true;

    }
}