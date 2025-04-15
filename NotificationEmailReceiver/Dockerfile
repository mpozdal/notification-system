FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NotificationEmailReceiver/*.csproj ./NotificationEmailReceiver/
WORKDIR /src/NotificationEmailReceiver

RUN dotnet restore

COPY NotificationEmailReceiver/. ./ 
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app ./

ENTRYPOINT ["dotnet", "NotificationEmailReceiver.dll"]
