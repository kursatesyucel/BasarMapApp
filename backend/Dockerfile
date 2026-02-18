FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasarMapApp.Api/BasarMapApp.Api.csproj", "BasarMapApp.Api/"]
RUN dotnet restore "BasarMapApp.Api/BasarMapApp.Api.csproj"
COPY . .
WORKDIR "/src/BasarMapApp.Api"
RUN dotnet build "BasarMapApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasarMapApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BasarMapApp.Api.dll"]
