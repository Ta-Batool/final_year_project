FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FYP.sln ./
COPY BlazorApp1/BlazorApp1.csproj BlazorApp1/
COPY Model/Model.csproj Model/
COPY API/API.csproj API/

RUN dotnet restore ./FYP.sln

COPY . .

RUN dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
