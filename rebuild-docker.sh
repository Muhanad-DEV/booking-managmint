#!/bin/bash
export PATH="/Applications/Docker.app/Contents/Resources/bin:$PATH"
cd "$(dirname "$0")"

echo "Stopping containers..."
docker compose down

echo "Rebuilding containers with latest changes..."
docker compose up --build -d

echo "Containers rebuilt! Check http://localhost:8080"
echo "View logs with: docker compose logs -f webapp"

