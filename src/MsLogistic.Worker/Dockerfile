FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MsLogistic.Worker/MsLogistic.Worker.csproj", "src/MsLogistic.Worker/"]
COPY ["src/MsLogistic.Infrastructure/MsLogistic.Infrastructure.csproj", "src/MsLogistic.Infrastructure/"]
COPY ["src/MsLogistic.Application/MsLogistic.Application.csproj", "src/MsLogistic.Application/"]
COPY ["src/MsLogistic.Core/MsLogistic.Core.csproj", "src/MsLogistic.Core/"]
COPY ["src/MsLogistic.Domain/MsLogistic.Domain.csproj", "src/MsLogistic.Domain/"]
RUN dotnet restore "src/MsLogistic.Worker/MsLogistic.Worker.csproj"
COPY . .
WORKDIR "/src/src/MsLogistic.Worker"
RUN dotnet build "./MsLogistic.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MsLogistic.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MsLogistic.Worker.dll"]
