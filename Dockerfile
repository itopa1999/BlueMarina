# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BlueMarina.sln ./
COPY BlueMarina.Api/BlueMarina.Api.csproj BlueMarina.Api/
COPY BlueMarina.Application/BlueMarina.Application.csproj BlueMarina.Application/
COPY BlueMarina.Domain/BlueMarina.Domain.csproj BlueMarina.Domain/
COPY BlueMarina.Infrastructure/BlueMarina.Infrastructure.csproj BlueMarina.Infrastructure/
COPY BlueMarina.Shared/BlueMarina.Shared.csproj BlueMarina.Shared/

# Restore API and referenced projects
RUN dotnet restore BlueMarina.Api/BlueMarina.Api.csproj

# Install EF Core CLI.
# Ideally, match this version to your EF Core package version.
RUN dotnet tool install --global dotnet-ef --version 8.0.10
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy the remaining source, including Infrastructure migrations
COPY . .

# Publish API
RUN dotnet publish BlueMarina.Api/BlueMarina.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Generate a migration executable
RUN dotnet ef migrations bundle \
    --project BlueMarina.Infrastructure \
    --startup-project BlueMarina.Api \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained \
    --output /app/migrate


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY --from=build /app/migrate /app/migrate
COPY docker-entrypoint.sh /app/docker-entrypoint.sh

RUN chmod +x /app/migrate /app/docker-entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
