FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# 複製 sln csproj and restore nuget
COPY *.sln .

# 目標路徑資料夾要與來源路徑資料夾名稱相同(因 sln 已儲存專案路徑)
COPY point-cloud-analyzer-web/*.csproj ./point-cloud-analyzer-web/

# nuget restore
RUN dotnet restore

# 複製其他檔案
COPY point-cloud-analyzer-web/. ./point-cloud-analyzer-web/
WORKDIR /app/point-cloud-analyzer-web

# 使用 Release 建置專案並輸出至 out
RUN dotnet publish -c Release -o out
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app

# 將產出物複製至 app 下
COPY --from=build /app/point-cloud-analyzer-web/out ./

# 啟動 TestDokcer
ENTRYPOINT ["dotnet", "point-cloud-analyzer-web.dll"]