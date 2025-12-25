using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Sportarr
{
    /// <summary>
    /// Plugin configuration for Sportarr.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the Sportarr API URL.
        /// </summary>
        public string SportarrApiUrl { get; set; } = "http://localhost:5000";

        /// <summary>
        /// Gets or sets the Sportarr API Key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether to enable debug logging.
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Gets or sets the image cache duration in hours.
        /// </summary>
        public int ImageCacheHours { get; set; } = 24;
    }
}
