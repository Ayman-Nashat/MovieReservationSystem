FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet publish "Movie Reservation System.csproj" -o /published /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /published .

ENTRYPOINT ["dotnet", "Movie Reservation System.dll"]
