# readarr-rresurrected (Automated ARM64 Builds)

This repository serves as a lean, automated compilation hub that builds and maintains native **`linux/arm64`** Docker images for **[readarr-rresurrected](https://github.com/ricetim/readarr-rresurrected)**. 

It is specifically optimized to allow users to run this feature-rich Readarr fork on ARM64 hardware—such as the **Raspberry Pi 5** or Apple Silicon—without encountering `ENOEXEC` or executable format errors.

## 🚀 How It Works

This repository contains no application source code. Instead, it utilizes **GitHub Actions** as a cloud compiler:
1. **Daily Check:** Every morning at 4:00 AM UTC, a background cron job queries the upstream repository.
2. **On-The-Fly Patching:** If a new upstream release is detected, the runner downloads their development branch, injects an explicit patch replacing `linux-musl-x64` references with native `linux-musl-arm64` frameworks, and cross-compiles the binaries.
3. **Registry Publication:** The finished multi-layer image is automatically published straight to the GitHub Container Registry workspace.

## 📦 Deployment via Docker Compose / Portainer

To deploy this native ARM64 image on your Raspberry Pi 5, use the following `docker-compose.yml` stack configuration:

```yaml
services:
  readarr:
    image: ghcr.io/ziilagit104/readarr-rresurrected:latest
    container_name: readarr-rresurrected
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York # Change to your actual timezone
      - GOOGLE_BOOKS_API_KEY= # Optional: improves ebook edition data
    volumes:
      - /home/pi/readarr/config:/config # Adjust paths to match your environment
      - /home/pi/readarr/books:/books
      - /home/pi/readarr/downloads:/downloads
    ports:
      - 8787:8787
    restart: unless-stopped
```

## ⚖️ Credit & Support

* **Core Application & Upstream Logic:** All credit for this excellent, self-hosted metadata-bundled fork goes to **[@ricetim](https://github.com/ricetim/readarr-rresurrected)**. 
* **Support & Bug Reports:** If you experience internal application issues, indexer pagination failures, or database migration bugs, please consult or open an issue directly on the official **[ricetim/readarr-rresurrected Upstream Repository](https://github.com/ricetim/readarr-rresurrected)**.

*This repository only maintains the automated cloud compiling pipeline that translates their upstream source code assets into the `linux/arm64` architecture target.*
