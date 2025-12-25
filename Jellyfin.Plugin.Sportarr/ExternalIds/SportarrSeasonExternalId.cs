using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Sportarr.ExternalIds
{
    /// <summary>
    /// Sportarr Season External Id.
    /// </summary>
    public class SportarrSeasonExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => "Sportarr";

        /// <inheritdoc />
        public string Key => "Sportarr";

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.Season;

        /// <inheritdoc />
        public string? UrlFormatString => null;

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Season;
    }
}
