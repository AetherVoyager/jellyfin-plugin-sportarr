using System;
using System.Collections.Generic;
using System.Net.Http;
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
    /// Sportarr Season metadata provider for Jellyfin.
    /// </summary>
    public class SportarrSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>, IHasOrder
    {
        private readonly ILogger<SportarrSeasonProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SportarrSeasonProvider(ILogger<SportarrSeasonProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string Name => "Sportarr";

        public int Order => 0;

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
        /// Search for seasons.
        /// </summary>
        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }

        /// <summary>
        /// Get metadata for a specific season (year).
        /// </summary>
        public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Season>();

            // Get series Sportarr ID
            string? seriesId = null;
            info.SeriesProviderIds?.TryGetValue("Sportarr", out seriesId);

            if (string.IsNullOrEmpty(seriesId))
            {
                _logger.LogDebug("[Sportarr] No series ID for season: {Season}", info.IndexNumber);
                return result;
            }

            if (!info.IndexNumber.HasValue)
            {
                _logger.LogDebug("[Sportarr] Missing season number");
                return result;
            }

            try
            {
                var client = CreateClient();
                var url = $"{ApiUrl}/api/v1/metadata/series/{seriesId}/season/{info.IndexNumber}";

                _logger.LogDebug("[Sportarr] Fetching season: {Url}", url);

                var response = await client.GetStringAsync(url, cancellationToken);
                var json = JsonDocument.Parse(response);
                var root = json.RootElement;

                var season = new Season
                {
                    Name = root.TryGetProperty("title", out var title) ? title.GetString() : $"Season {info.IndexNumber}",
                    IndexNumber = info.IndexNumber
                };

                if (root.TryGetProperty("summary", out var summary))
                {
                    season.Overview = summary.GetString();
                }

                // Use season number (year) as production year
                if (info.IndexNumber >= 1900 && info.IndexNumber <= 2100)
                {
                    season.ProductionYear = info.IndexNumber;
                }

                result.Item = season;
                result.HasMetadata = true;

                _logger.LogInformation("[Sportarr] Updated season: {Name}", season.Name);
            }
            catch (HttpRequestException)
            {
                // Season endpoint may not exist, create basic metadata
                var season = new Season
                {
                    Name = $"Season {info.IndexNumber}",
                    IndexNumber = info.IndexNumber
                };

                if (info.IndexNumber >= 1900 && info.IndexNumber <= 2100)
                {
                    season.ProductionYear = info.IndexNumber;
                }

                result.Item = season;
                result.HasMetadata = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Sportarr] Season metadata error: {Season}", info.IndexNumber);
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
