#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["netmon.cli/netmon.cli.csproj", "netmon.cli/"]
COPY ["netmon.domain/netmon.domain.csproj", "netmon.domain/"]
RUN dotnet restore "netmon.cli/netmon.cli.csproj"
COPY . .
WORKDIR "/src/netmon.cli"
RUN dotnet build "netmon.cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "netmon.cli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "netmon.cli.dll"]