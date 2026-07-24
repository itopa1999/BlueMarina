#!/bin/bash
set -e

echo "📦 Starting BlueMarina API entrypoint..."

echo "⏳ Waiting for PostgreSQL to be ready..."
while ! nc -z postgres 5432; do
  sleep 1
done
echo "✅ PostgreSQL is ready!"

 dotnet ef database update --project /src/BlueMarina.Infrastructure/BlueMarina.Infrastructure.csproj --startup-project /app

echo "🚀 Starting the API..."
exec dotnet BlueMarina.Api.dll