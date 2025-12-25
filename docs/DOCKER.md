# Docker Setup

## Using with Docker Compose

If you're running both Jellyfin and Sportarr in Docker, here's a sample configuration:

```yaml
version: "3.8"

services:
  sportarr:
    image: sportarr/sportarr:latest
    container_name: sportarr
    ports:
      - "5000:5000"
    volumes:
      - ./sportarr-config:/config
      - /path/to/sports:/media/sports
    restart: unless-stopped
    networks:
      - media

  jellyfin:
    image: jellyfin/jellyfin:latest
    container_name: jellyfin
    ports:
      - "8096:8096"
    volumes:
      - ./jellyfin-config:/config
      - ./jellyfin-cache:/cache
      - /path/to/sports:/media/sports:ro
    restart: unless-stopped
    networks:
      - media

networks:
  media:
    driver: bridge
```

## Plugin Installation in Docker

### Method 1: Volume Mount (Recommended for Development)

1. Download or build the plugin
2. Mount the plugin directory:

```yaml
jellyfin:
  volumes:
    - ./plugins/Sportarr:/config/plugins/Sportarr:ro
```

### Method 2: Custom Dockerfile

Create a Dockerfile that includes the plugin:

```dockerfile
FROM jellyfin/jellyfin:latest

# Create plugin directory
RUN mkdir -p /config/plugins/Sportarr

# Copy plugin files
COPY Jellyfin.Plugin.Sportarr.dll /config/plugins/Sportarr/
COPY meta.json /config/plugins/Sportarr/
```

### Method 3: Init Container Script

Add an init script to download the plugin on startup:

```yaml
jellyfin:
  volumes:
    - ./init-plugins.sh:/init-plugins.sh:ro
  entrypoint: ["/bin/bash", "-c", "/init-plugins.sh && /jellyfin/jellyfin"]
```

`init-plugins.sh`:
```bash
#!/bin/bash
PLUGIN_DIR="/config/plugins/Sportarr"
RELEASE_URL="https://github.com/Sportarr/jellyfin-plugin-sportarr/releases/latest/download/jellyfin-plugin-sportarr.zip"

if [ ! -f "$PLUGIN_DIR/Jellyfin.Plugin.Sportarr.dll" ]; then
    echo "Installing Sportarr plugin..."
    mkdir -p "$PLUGIN_DIR"
    curl -L "$RELEASE_URL" -o /tmp/plugin.zip
    unzip -o /tmp/plugin.zip -d "$PLUGIN_DIR"
    rm /tmp/plugin.zip
    echo "Sportarr plugin installed!"
fi
```

## Network Configuration

When Jellyfin connects to Sportarr, use the container name as the hostname:

- **Sportarr API URL**: `http://sportarr:5000`

This works because both containers are on the same Docker network.

## Troubleshooting

### Cannot Connect to Sportarr

1. Verify both containers are on the same network:
   ```bash
   docker network inspect media
   ```

2. Test connectivity from Jellyfin container:
   ```bash
   docker exec jellyfin curl http://sportarr:5000/api/v1/system/status
   ```

3. Check Sportarr logs:
   ```bash
   docker logs sportarr
   ```

### Plugin Not Loading

1. Check plugin files exist:
   ```bash
   docker exec jellyfin ls -la /config/plugins/Sportarr/
   ```

2. Check Jellyfin logs:
   ```bash
   docker logs jellyfin | grep -i sportarr
   ```

3. Restart Jellyfin:
   ```bash
   docker restart jellyfin
   ```
