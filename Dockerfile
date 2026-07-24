# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["BlueMarina.Api/BlueMarina.Api.csproj", "BlueMarina.Api/"]
COPY ["BlueMarina.Application/BlueMarina.Application.csproj", "BlueMarina.Application/"]
COPY ["BlueMarina.Domain/BlueMarina.Domain.csproj", "BlueMarina.Domain/"]
COPY ["BlueMarina.Infrastructure/BlueMarina.Infrastructure.csproj", "BlueMarina.Infrastructure/"]
COPY ["BlueMarina.Shared/BlueMarina.Shared.csproj", "BlueMarina.Shared/"]
RUN dotnet restore "BlueMarina.Api/BlueMarina.Api.csproj"

# Copy the rest and build
COPY . .
WORKDIR "/src/BlueMarina.Api"
RUN dotnet build "BlueMarina.Api.csproj" -c Release -o /app/build

# ---- Publish stage ----
FROM build AS publish
RUN dotnet publish "BlueMarina.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy entrypoint script and set execution permission
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Copy published files
COPY --from=publish /app/publish .

# Set environment (can be overridden at runtime)
ENV ASPNETCORE_ENVIRONMENT=Production

# Use the entrypoint script
ENTRYPOINT ["/app/entrypoint.sh"]