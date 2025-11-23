FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files first (for layer caching)
COPY BlazorApp1/BlazorApp1.csproj BlazorApp1/
COPY Model/Model.csproj Model/

# Restore only the Blazor project (which will follow its ProjectReference to Model)
RUN dotnet restore BlazorApp1/BlazorApp1.csproj

# Now copy the rest of the source
COPY . .

# Publish Blazor Server app
RUN dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o /app/publish

# -------- RUNTIME IMAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
