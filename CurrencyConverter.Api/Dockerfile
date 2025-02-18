# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore
COPY . ./
RUN dotnet restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the application from the build stage
COPY --from=build /app/publish .

# Set environment variable dynamically
ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ENVIRONMENT

# Expose the application port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "CurrencyConverter.Api.dll"]
