﻿FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NotificationService/*.csproj ./NotificationService/
WORKDIR /src/NotificationService

RUN dotnet restore

COPY NotificationService/. ./ 
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app ./

ENTRYPOINT ["dotnet", "NotificationService.dll"]
