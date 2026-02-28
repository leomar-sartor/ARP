# Imagem base de runtime (Linux)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage de build (Linux)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ARP/ARP.csproj", "ARP/"]
RUN dotnet restore "ARP/ARP.csproj"
COPY . .
WORKDIR "/src/ARP"
RUN dotnet build "ARP.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage de publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ARP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ARP.dll"]