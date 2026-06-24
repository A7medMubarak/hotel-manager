FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY HotelManager.sln .
COPY src/HotelManager.Domain/*.csproj src/HotelManager.Domain/
COPY src/HotelManager.Application/*.csproj src/HotelManager.Application/
COPY src/HotelManager.Infrastructure/*.csproj src/HotelManager.Infrastructure/
COPY src/HotelManager.API/*.csproj src/HotelManager.API/
RUN dotnet restore
COPY . .
RUN dotnet publish src/HotelManager.API -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HotelManager.API.dll"]
