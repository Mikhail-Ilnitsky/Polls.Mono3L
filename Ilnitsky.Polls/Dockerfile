# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

###-### Базовый образ (base) для запуска (Runtime) из VS в режиме отладки

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


###-### Образ собираемый для продакшена

### ЭТАП 1: Сборка фронтенда (Node.js)
FROM node:lts-alpine3.22 AS build-nodejs
WORKDIR /src/front_app

# Копируем только файлы зависимостей для кэширования слоев
COPY ["Ilnitsky.Polls/front_app/package.json", "Ilnitsky.Polls/front_app/package-lock.json*", "./"]
RUN npm install

# Копируем исходники фронта и собираем
COPY Ilnitsky.Polls/front_app/ .
RUN npm run build


### ЭТАП 2: Сборка .NET проекта
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-dotnet
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файлы проектов для восстановления зависимостей
COPY ["Ilnitsky.Polls/Ilnitsky.Polls.csproj", "Ilnitsky.Polls/"]
COPY ["Ilnitsky.Polls.BusinessLogic/Ilnitsky.Polls.BusinessLogic.csproj", "Ilnitsky.Polls.BusinessLogic/"]
COPY ["Ilnitsky.Polls.Contracts/Ilnitsky.Polls.Contracts.csproj", "Ilnitsky.Polls.Contracts/"]
COPY ["Ilnitsky.Polls.DataAccess/Ilnitsky.Polls.DataAccess.csproj", "Ilnitsky.Polls.DataAccess/"]

# Восстанавливаем пакеты
RUN dotnet restore "./Ilnitsky.Polls/Ilnitsky.Polls.csproj"

# Копируем все исходники бэкенда
COPY . .

# Копируем фронт (собранный Vite на первом этапе) в wwwroot
COPY --from=build-nodejs /src/wwwroot/ ./Ilnitsky.Polls/wwwroot/

# Компилируем исходный код бэкэнда в бинарные файлы (DLL)
WORKDIR "/src/Ilnitsky.Polls"
RUN dotnet build "./Ilnitsky.Polls.csproj" -c $BUILD_CONFIGURATION -o /app/build


### ЭТАП 3: Публикация (сборка) проекта со всеми необходимыми зависимостями в отдельную папку для деплоя
FROM build-dotnet AS publish
ARG BUILD_CONFIGURATION=Release

# Добавляем параметр BuildFrontend=false, чтобы dotnet не пытался сам запустить npm
RUN dotnet publish "./Ilnitsky.Polls.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:BuildFrontend=false


### ЭТАП 4: Финальный образ используемый в производственной среде или при запуске из VS в обычном режиме
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ilnitsky.Polls.dll"]
