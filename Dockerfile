# 基底映像：ASP.NET 8.0 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# 建置階段：使用 .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製 csproj 並還原相依套件
COPY ["testAPI.csproj", "./"]
RUN dotnet restore "./testAPI.csproj"

# 複製剩餘程式碼並建置
COPY . .
RUN dotnet publish "testAPI.csproj" -c Release -o /app/publish

# 組成最後映像
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "testAPI.dll"]
