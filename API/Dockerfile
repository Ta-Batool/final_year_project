# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["API/API.csproj", "API/"]
COPY ["Model/Model.csproj", "Model/"]
RUN dotnet restore "API/API.csproj"

# Copy everything and publish
COPY . .
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output from build image
COPY --from=build /app/publish .

# Render expects your app to listen on the PORT env variable.
# ASP.NET will use this automatically if ASPNETCORE_URLS is not set
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
