FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copy sln csproj and restore nuget
COPY *.sln .

COPY ./src/point-cloud-analyzer-web/*.csproj ./src/point-cloud-analyzer-web/

# nuget restore
RUN dotnet restore


COPY src/point-cloud-analyzer-web/. ./src/point-cloud-analyzer-web/
WORKDIR /app/src/point-cloud-analyzer-web

#release to target folder
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 
#FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

COPY --from=build /app/src/point-cloud-analyzer-web/out ./

ENTRYPOINT ["dotnet", "point-cloud-analyzer-web.dll", "--environment=Development"]