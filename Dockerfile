FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["booking-managmint.csproj", "./"]
RUN dotnet restore "booking-managmint.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "booking-managmint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "booking-managmint.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/docs/sqlserver_init.sql ./docs/sqlserver_init.sql
ENTRYPOINT ["dotnet", "booking-managmint.dll"]

