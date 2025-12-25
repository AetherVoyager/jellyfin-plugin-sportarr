using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Sportarr
{
    /// <summary>
    /// Sportarr Series (League) metadata provider for Jellyfin.
    /// </summary>
    public class SportarrSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly ILogger<SportarrSeriesProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SportarrSeriesProvider(ILogger<SportarrSeriesProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "Sportarr";

        public int Order => 0; // Primary provider

        private string ApiUrl => SportarrPlugin.Instance?.Configuration.SportarrApiUrl?.TrimEnd('/') ?? "http://localhost:5000";
        private string ApiKey => SportarrPlugin.Instance?.Configuration.ApiKey ?? string.Empty;

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
            }
            return client;
        }

        /// <summary>
        /// Search for series (leagues) matching the query.
        /// </summary>
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();

            if (string.IsNullOrEmpty(searchInfo.Name))
            {
                return results;
            }

            try
            {
                var client = CreateClient();
                var url = $"{ApiUrl}/api/v1/metadata/search?title={Uri.EscapeDataString(searchInfo.Name)}&type=league";

                if (searchInfo.Year.HasValue)
                {
                    url += $"&year={searchInfo.Year}";
                }

                _logger.LogDebug("[Sportarr] Searching: {Url}", url);

                var response = await client.GetStringAsync(url, cancellationToken);
                var json = JsonDocument.Parse(response);

                if (json.RootElement.TryGetProperty("results", out var resultsElement))
                {
                    foreach (var item in resultsElement.EnumerateArray())
                    {
                        var result = new RemoteSearchResult
                        {
                            Name = item.GetProperty("title").GetString(),
                            ProviderIds = new Dictionary<string, string>
                            {
                                { "Sportarr", item.GetProperty("id").GetString() ?? "" }
                            },
                            SearchProviderName = Name
                        };

                        if (item.TryGetProperty("year", out var yearElement) && yearElement.ValueKind == JsonValueKind.Number)
                        {
                            result.ProductionYear = yearElement.GetInt32();
                        }

                        if (item.TryGetProperty("posterUrl", out var posterElement))
                        {
                            result.ImageUrl = posterElement.GetString();
                        }

                        results.Add(result);
                        _logger.LogDebug("[Sportarr] Found: {Name} (ID: {Id})", result.Name, result.ProviderIds["Sportarr"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Sportarr] Search error");
            }

            return results;
        }

        /// <summary>
        /// Get metadata for a specific series (league).
        /// </summary>
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();

            // Get Sportarr ID from provider IDs or search
            string? sportarrId = null;
            info.ProviderIds?.TryGetValue("Sportarr", out sportarrId);

            if (string.IsNullOrEmpty(sportarrId) && !string.IsNullOrEmpty(info.Name))
            {
                // Search for the series
                var searchResults = await GetSearchResults(info, cancellationToken);
                var enumerator = searchResults.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    enumerator.Current.ProviderIds?.TryGetValue("Sportarr", out sportarrId);
                }
            }

            if (string.IsNullOrEmpty(sportarrId))
            {
                _logger.LogWarning("[Sportarr] No ID found for: {Name}", info.Name);
                return result;
            }

            try
            {
                var client = CreateClient();
                var url = $"{ApiUrl}/api/v1/metadata/series/{sportarrId}";

                _logger.LogDebug("[Sportarr] Fetching series: {Url}", url);

                var response = await client.GetStringAsync(url, cancellationToken);
                var json = JsonDocument.Parse(response);
                var root = json.RootElement;

                var series = new Series
                {
                    Name = root.GetProperty("title").GetString(),
                    Overview = root.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                    OfficialRating = root.TryGetProperty("contentRating", out var rating) ? rating.GetString() : null
                };

                // Set provider ID
                series.SetProviderId("Sportarr", sportarrId);

                // Year
                if (root.TryGetProperty("year", out var yearElement) && yearElement.ValueKind == JsonValueKind.Number)
                {
                    series.ProductionYear = yearElement.GetInt32();
                    series.PremiereDate = new DateTime(yearElement.GetInt32(), 1, 1);
                }

                // Genres
                if (root.TryGetProperty("genres", out var genres))
                {
                    foreach (var genre in genres.EnumerateArray())
                    {
                        var genreStr = genre.GetString();
                        if (!string.IsNullOrEmpty(genreStr))
                        {
                            series.AddGenre(genreStr);
                        }
                    }
                }
                else
                {
                    series.AddGenre("Sports");
                }

                // Studios
                if (root.TryGetProperty("studio", out var studio) && !string.IsNullOrEmpty(studio.GetString()))
                {
                    series.AddStudio(studio.GetString()!);
                }

                result.Item = series;
                result.HasMetadata = true;

                _logger.LogInformation("[Sportarr] Updated series: {Name}", series.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Sportarr] Get metadata error for ID: {Id}", sportarrId);
            }

            return result;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return client.GetAsync(url, cancellationToken);
        }
    }
}
