# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all source
#COPY ../NotificationShared ./NotificationShared
COPY . ./

# Restore and publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "NotificationPushReceiver.dll"] 
