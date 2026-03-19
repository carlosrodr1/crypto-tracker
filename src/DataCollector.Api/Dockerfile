FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/DataCollector.Shared/DataCollector.Shared.csproj", "src/DataCollector.Shared/"]
COPY ["src/DataCollector.Api/DataCollector.Api.csproj", "src/DataCollector.Api/"]
RUN dotnet restore "src/DataCollector.Api/DataCollector.Api.csproj"

COPY . .
WORKDIR "/src/src/DataCollector.Api"
RUN dotnet publish "DataCollector.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /data
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DataCollector.Api.dll"]
