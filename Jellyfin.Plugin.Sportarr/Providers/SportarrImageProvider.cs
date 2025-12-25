using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Sportarr
{
    /// <summary>
    /// Sportarr Image provider for Jellyfin.
    /// Provides posters, banners, fanart for series and thumbnails for episodes.
    /// </summary>
    public class SportarrImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly ILogger<SportarrImageProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SportarrImageProvider(ILogger<SportarrImageProvider> logger, IHttpClientFactory httpClientFactory)
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
        /// Check if this provider supports the item type.
        /// </summary>
        public bool Supports(BaseItem item)
        {
            return item is Series || item is Season || item is Episode;
        }

        /// <summary>
        /// Get supported image types for an item.
        /// </summary>
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            if (item is Series)
            {
                return new[] { ImageType.Primary, ImageType.Banner, ImageType.Backdrop, ImageType.Thumb };
            }
            else if (item is Season)
            {
                return new[] { ImageType.Primary };
            }
            else if (item is Episode)
            {
                return new[] { ImageType.Primary, ImageType.Thumb };
            }

            return Array.Empty<ImageType>();
        }

        /// <summary>
        /// Get available images for an item.
        /// </summary>
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var images = new List<RemoteImageInfo>();

            string? sportarrId = null;
            item.ProviderIds?.TryGetValue("Sportarr", out sportarrId);

            if (item is Series series)
            {
                if (string.IsNullOrEmpty(sportarrId))
                {
                    return images;
                }

                try
                {
                    var client = CreateClient();
                    var url = $"{ApiUrl}/api/v1/metadata/series/{sportarrId}";
                    var response = await client.GetStringAsync(url, cancellationToken);
                    var json = JsonDocument.Parse(response);
                    var root = json.RootElement;

                    // Poster
                    if (root.TryGetProperty("posterUrl", out var poster) && !string.IsNullOrEmpty(poster.GetString()))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = poster.GetString(),
                            Type = ImageType.Primary,
                            ProviderName = Name
                        });
                    }

                    // Banner
                    if (root.TryGetProperty("bannerUrl", out var banner) && !string.IsNullOrEmpty(banner.GetString()))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = banner.GetString(),
                            Type = ImageType.Banner,
                            ProviderName = Name
                        });
                    }

                    // Fanart/Backdrop
                    if (root.TryGetProperty("fanartUrl", out var fanart) && !string.IsNullOrEmpty(fanart.GetString()))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = fanart.GetString(),
                            Type = ImageType.Backdrop,
                            ProviderName = Name
                        });
                    }

                    // Thumb
                    if (root.TryGetProperty("thumbUrl", out var thumb) && !string.IsNullOrEmpty(thumb.GetString()))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = thumb.GetString(),
                            Type = ImageType.Thumb,
                            ProviderName = Name
                        });
                    }

                    _logger.LogDebug("[Sportarr] Found {Count} images for series: {Name}", images.Count, series.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Sportarr] Error fetching series images");
                }
            }
            else if (item is Season season)
            {
                // Use series poster for season
                var seriesId = season.Series?.GetProviderId("Sportarr");
                if (!string.IsNullOrEmpty(seriesId))
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{ApiUrl}/api/v1/images/league/{seriesId}/poster",
                        Type = ImageType.Primary,
                        ProviderName = Name
                    });
                }
            }
            else if (item is Episode episode)
            {
                // Get episode thumbnail
                if (!string.IsNullOrEmpty(sportarrId))
                {
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{ApiUrl}/api/v1/images/event/{sportarrId}/thumb",
                        Type = ImageType.Primary,
                        ProviderName = Name
                    });
                    images.Add(new RemoteImageInfo
                    {
                        Url = $"{ApiUrl}/api/v1/images/event/{sportarrId}/thumb",
                        Type = ImageType.Thumb,
                        ProviderName = Name
                    });
                }
            }

            return images;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var client = CreateClient();
            return client.GetAsync(url, cancellationToken);
        }
    }
}
