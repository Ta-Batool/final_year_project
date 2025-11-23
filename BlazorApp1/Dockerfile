# -------- BUILD IMAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy SLN + CSProj
COPY FYP.sln ./
COPY BlazorApp1/BlazorApp1.csproj BlazorApp1/

# Restore dependencies
RUN dotnet restore BlazorApp1/BlazorApp1.csproj

# Copy all files
COPY . .

# Publish Blazor Server App
RUN dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o /app/publish

# -------- RUNTIME IMAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
