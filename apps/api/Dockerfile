# File: apps/api/Dockerfile
# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

# Copy project files
COPY ECommerce.Api/*.csproj ECommerce.Api/
COPY ECommerce.Application/*.csproj ECommerce.Application/
COPY ECommerce.Domain/*.csproj ECommerce.Domain/
COPY ECommerce.Infrastructure/*.csproj ECommerce.Infrastructure/
COPY ECommerce.Shared/*.csproj ECommerce.Shared/

# Restore dependencies
RUN dotnet restore ECommerce.Api/ECommerce.Api.csproj

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish ECommerce.Api/ECommerce.Api.csproj \
    --configuration Release \
    --output /app \
    --no-restore \
    -p:PublishTrimmed=false \
    -p:PublishSingleFile=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

# Copy published output
COPY --from=build /app .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install ICU libraries for globalization
RUN apk add --no-cache icu-libs

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerce.Api.dll"]
