# ===========================
# 1. Build stage
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY FYP.sln ./
COPY API/API.csproj API/
COPY Model/Model.csproj Model/
COPY BlazorApp1/BlazorApp1.csproj BlazorApp1/

# Restore dependencies
RUN dotnet restore ./FYP.sln

# Copy the rest of the source
COPY . .

# Publish the API project
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# ===========================
# 2. Runtime stage
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Make Kestrel listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

# Copy published output from build stage
COPY --from=build /app/publish ./

# Expose container port
EXPOSE 8080

# Start the API
ENTRYPOINT ["dotnet", "API.dll"]
