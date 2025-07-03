FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasarMapApp.Api/BasarMapApp.Api.csproj", "BasarMapApp.Api/"]
RUN dotnet restore "BasarMapApp.Api/BasarMapApp.Api.csproj"
COPY . .
WORKDIR "/src/BasarMapApp.Api"
RUN dotnet build "BasarMapApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasarMapApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install PostGIS support
RUN apt-get update && apt-get install -y \
    libgeos-dev \
    libproj-dev \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "BasarMapApp.Api.dll"] 