# Jellyfin Plugin: Sportarr

<p align="center">
  <img src="images/logo.png" alt="Sportarr Logo" width="200"/>
</p>

A Jellyfin metadata plugin that fetches sports metadata from your [Sportarr](https://github.com/Sportarr/Sportarr) server.

## Features

- 🏆 **Sports Metadata**: Posters, banners, descriptions, and air dates for sports events
- 📺 **Series Support**: Organizes leagues/sports as TV series with year-based seasons
- 🖼️ **Rich Images**: Posters, banners, backdrops, and episode thumbnails
- 🔄 **Live Sync**: Metadata stays in sync with your Sportarr library
- ⚡ **Fast & Lightweight**: Minimal resource usage

## Installation

### Option 1: Add Repository (Recommended)

This is the easiest way to install and receive automatic updates.

1. In Jellyfin, go to **Dashboard** → **Plugins** → **Repositories**
2. Click **Add** and enter:
   - **Repository Name**: `Sportarr`
   - **Repository URL**: `https://raw.githubusercontent.com/AetherVoyager/jellyfin-plugin-sportarr/main/manifest.json`
3. Click **Save**
4. Go to **Catalog** and find **Sportarr** under **Metadata**
5. Click **Install**
6. Restart Jellyfin

### Option 2: Manual Installation

1. Download the latest release from the [Releases page](https://github.com/AetherVoyager/jellyfin-plugin-sportarr/releases)
2. Extract the ZIP file
3. Copy the contents to your Jellyfin plugins directory:
   - **Docker**: `/config/plugins/Sportarr/`
   - **Windows**: `%APPDATA%\Jellyfin\Server\plugins\Sportarr\`
   - **Linux**: `~/.local/share/jellyfin/plugins/Sportarr/`
   - **macOS**: `~/.local/share/jellyfin/plugins/Sportarr/`
4. Restart Jellyfin

### Option 3: Build from Source

```bash
# Clone the repository
git clone https://github.com/AetherVoyager/jellyfin-plugin-sportarr.git
cd jellyfin-plugin-sportarr

# Restore and build
dotnet restore Jellyfin.Plugin.Sportarr/Jellyfin.Plugin.Sportarr.csproj
dotnet build Jellyfin.Plugin.Sportarr/Jellyfin.Plugin.Sportarr.csproj -c Release

# Copy DLL and meta.json to Jellyfin plugins folder
# e.g., cp Jellyfin.Plugin.Sportarr/bin/Release/net8.0/Jellyfin.Plugin.Sportarr.dll /config/plugins/Sportarr/
```

## Configuration

After installation:

1. Go to **Dashboard** → **Plugins** → **Sportarr**
2. Configure the following settings:
   - **Sportarr API URL**: The URL of your Sportarr server (e.g., `http://localhost:5000`)
   - **API Key**: Your Sportarr API key (if authentication is enabled)
3. Click **Test Connection** to verify connectivity
4. Click **Save**

## Library Setup

1. In Jellyfin, go to **Dashboard** → **Libraries**
2. Click **Add Media Library** or edit an existing library
3. Select **Shows** as the content type
4. Add your sports media folder
5. Under **Metadata Downloaders**:
   - Enable **Sportarr**
   - Move **Sportarr** to the top of the list (highest priority)
6. Under **Image Fetchers**:
   - Enable **Sportarr**
   - Move **Sportarr** to the top of the list
7. Click **OK** and scan your library

## File Naming Convention

The plugin works best with Sportarr's file naming format:

### Folder Structure

```
/media/Sports/
├── UFC/
│   ├── Season 2024/
│   │   ├── UFC - S2024E01 - UFC 300 - 1080p.mkv
│   │   └── UFC - S2024E02 - UFC 301 - 1080p.mkv
│   └── Season 2025/
│       └── UFC - S2025E01 - UFC 312 - 1080p.mkv
├── NFL/
│   └── Season 2024/
│       └── NFL - S2024E01 - Super Bowl LVIII - 1080p.mkv
└── Premier League/
    └── Season 2024/
        └── Premier League - S2024E01 - Man United vs Liverpool - 720p.mkv
```

### File Format

```
{Series} - S{Season}E{Episode} - {Title} - {Quality}.ext
```

**Examples:**
- `UFC - S2024E15 - UFC 300 - 1080p WEB-DL.mkv`
- `NFL - S2024E01 - Super Bowl LVIII - 720p.mkv`
- `Premier League - S2024E25 - Arsenal vs Chelsea - 1080p.mkv`

### Multi-Part Events (Fighting Sports)

For events with multiple parts (Early Prelims, Prelims, Main Card):

```
{Series} - S{Season}E{Episode} - pt{Part} - {Title} - {Quality}.ext
```

**Examples:**
- `UFC - S2024E01 - pt1 - UFC 300 Early Prelims - 1080p.mkv`
- `UFC - S2024E01 - pt2 - UFC 300 Prelims - 1080p.mkv`
- `UFC - S2024E01 - pt3 - UFC 300 Main Card - 1080p.mkv`

## How It Works

```
┌─────────────┐     ┌──────────┐     ┌─────────────┐
│   Jellyfin  │────▶│  Plugin  │────▶│  Sportarr   │
│   Library   │     │          │     │   Server    │
│    Scan     │◀────│ Metadata │◀────│    API      │
└─────────────┘     └──────────┘     └─────────────┘
```

1. **Scan**: Jellyfin scans your library and identifies shows/episodes
2. **Search**: Plugin searches Sportarr for matching leagues
3. **Match**: Best match is selected based on name similarity
4. **Fetch**: Full metadata (descriptions, dates, ratings) is retrieved
5. **Images**: Posters, banners, thumbnails are fetched and cached
6. **Display**: Rich metadata appears in your Jellyfin library

## Troubleshooting

### Plugin Not Loading

1. Check Jellyfin logs: **Dashboard** → **Logs**
2. Look for entries containing `[Sportarr]`
3. Verify the DLL is in the correct plugins folder
4. Ensure you're using a compatible Jellyfin version (10.9+)

### No Metadata Found

1. Verify your Sportarr server is running and accessible
2. Test the connection in plugin settings
3. Check that your file naming matches the expected format
4. Ensure the league/sport exists in your Sportarr library

### Images Not Loading

1. Check that your Sportarr server URL is correct (no trailing slash)
2. Verify the API key is correct (if authentication is enabled)
3. Clear the Jellyfin image cache and rescan

### Connection Failed

1. Check that Sportarr is running: `http://your-sportarr-url/api/v1/system/status`
2. Verify network connectivity between Jellyfin and Sportarr
3. If using Docker, ensure containers are on the same network
4. Check firewall settings

## Requirements

- Jellyfin 10.9.0 or later
- Sportarr server running and accessible
- .NET 8.0 runtime (included with Jellyfin 10.9+)

## Development

### Building

```bash
# Clone repository
git clone https://github.com/Sportarr/jellyfin-plugin-sportarr.git
cd jellyfin-plugin-sportarr

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Debug

# Run tests (if any)
dotnet test
```

### Project Structure

```
jellyfin-plugin-sportarr/
├── .github/
│   └── workflows/
│       └── build.yml          # CI/CD pipeline
├── Jellyfin.Plugin.Sportarr/
│   ├── Configuration/
│   │   ├── configPage.html    # Plugin settings UI
│   │   └── PluginConfiguration.cs
│   ├── Providers/
│   │   ├── SportarrSeriesProvider.cs
│   │   ├── SportarrSeasonProvider.cs
│   │   ├── SportarrEpisodeProvider.cs
│   │   └── SportarrImageProvider.cs
│   ├── SportarrPlugin.cs      # Main plugin class
│   ├── meta.json              # Plugin metadata
│   └── Jellyfin.Plugin.Sportarr.csproj
├── docs/
│   └── DOCKER.md              # Docker setup guide
├── images/
│   └── logo.png               # Plugin logo
├── manifest.json              # Repository manifest
└── README.md
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Links

- [Sportarr Main Project](https://github.com/Sportarr/Sportarr)
- [Jellyfin](https://jellyfin.org/)
- [Report Issues](https://github.com/AetherVoyager/jellyfin-plugin-sportarr/issues)
