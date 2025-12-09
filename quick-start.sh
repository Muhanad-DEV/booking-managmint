#!/bin/bash
export PATH="/Applications/Docker.app/Contents/Resources/bin:$PATH"
cd "$(dirname "$0")"
echo "Starting Docker containers..."
docker compose up --build
