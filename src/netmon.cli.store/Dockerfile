#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["netmon.cli.store/netmon.cli.store.csproj", "netmon.cli.store/"]
COPY ["netmon.cli.shared/netmon.cli.shared.csproj", "netmon.cli.shared/"]
COPY ["netmon.domain/netmon.domain.csproj", "netmon.domain/"]
RUN dotnet restore "netmon.cli.store/netmon.cli.store.csproj"
COPY . .
WORKDIR "/src/netmon.cli.store"
RUN dotnet build "netmon.cli.store.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "netmon.cli.store.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "netmon.cli.store.dll"]