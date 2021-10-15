#FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY ./src /src
WORKDIR /src

# nuget restore
RUN dotnet restore
#release to target folder
RUN dotnet publish point-cloud-analyzer-web -o /publish --configuration Release

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "point-cloud-analyzer-web.dll", "--environment=Development"]