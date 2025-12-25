using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Sportarr.ExternalIds
{
    /// <summary>
    /// Sportarr Episode External Id.
    /// </summary>
    public class SportarrEpisodeExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => "Sportarr";

        /// <inheritdoc />
        public string Key => "Sportarr";

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.Episode;

        /// <inheritdoc />
        public string? UrlFormatString => null;

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Episode;
    }
}
