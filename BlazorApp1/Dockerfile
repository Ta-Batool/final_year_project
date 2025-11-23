FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and only the projects BlazorApp1 needs
COPY FYP.sln ./
COPY BlazorApp1/BlazorApp1.csproj BlazorApp1/
COPY Model/Model.csproj Model/

# Restore dependencies (solution-level)
RUN dotnet restore ./FYP.sln

# Copy the rest of the source
COPY . .

# Publish only the Blazor Server app
RUN dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o /app/publish

# -------- RUNTIME IMAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
