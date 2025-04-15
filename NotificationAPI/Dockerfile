# Etap budowy
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiowanie projektu NotificationAPI
COPY NotificationAPI/*.csproj ./NotificationAPI/
RUN dotnet restore "NotificationAPI/NotificationAPI.csproj"

# Kopiowanie kodu źródłowego
COPY NotificationAPI/. ./
WORKDIR /src/NotificationAPI

# Publikowanie aplikacji
RUN dotnet publish -c Release -o /app

# Etap uruchomienia
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Kopiowanie opublikowanego obrazu
COPY --from=build /app ./

# Ekspozycja portu (np. 5000)
EXPOSE 5000

# Uruchomienie aplikacji
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]
