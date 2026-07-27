# 階段一：Build 應用程式
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製 backend 資料夾下的所有內容並發布
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 階段二：運行環境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 執行 backend.dll
ENTRYPOINT ["dotnet", "backend.dll"]