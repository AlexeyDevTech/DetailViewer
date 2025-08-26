# Этап 1: Базовый образ для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этап 2: Сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл решения и все файлы проектов для оптимизации кэша
# Сначала .sln
COPY ["DetailViewer.sln", "."]
# Затем все .csproj из всех папок
COPY ["DetailViewer.Api/DetailViewer.Api.csproj", "DetailViewer.Api/"]
COPY ["DetailViewer.Core/DetailViewer.Core.csproj", "DetailViewer.Core/"]
COPY ["DetailViewer.Global/DetailViewer.Global.csproj", "DetailViewer.Global/"]
# ... добавьте сюда другие проекты, если они есть

# Восстанавливаем зависимости для всего решения
RUN dotnet restore "DetailViewer.sln"

# Копируем все остальные файлы проекта
COPY . .

# Указываем рабочую папку для публикации
WORKDIR "/src/DetailViewer.Api"
# Публикуем конкретный проект
RUN dotnet publish "DetailViewer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DetailViewer.Api.dll"]