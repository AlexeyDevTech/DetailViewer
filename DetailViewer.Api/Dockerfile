FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DetailViewer.Api/DetailViewer.Api.csproj", "DetailViewer.Api/"]
RUN dotnet restore "DetailViewer.Api/DetailViewer.Api.csproj"
COPY . .
WORKDIR "/src/DetailViewer.Api"
RUN dotnet build "DetailViewer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DetailViewer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DetailViewer.Api.dll"]
