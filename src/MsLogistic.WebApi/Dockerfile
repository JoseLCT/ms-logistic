FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MsLogistic.WebApi/MsLogistic.WebApi.csproj", "src/MsLogistic.WebApi/"]
COPY ["src/MsLogistic.Application/MsLogistic.Application.csproj", "src/MsLogistic.Application/"]
COPY ["src/MsLogistic.Core/MsLogistic.Core.csproj", "src/MsLogistic.Core/"]
COPY ["src/MsLogistic.Domain/MsLogistic.Domain.csproj", "src/MsLogistic.Domain/"]
COPY ["src/MsLogistic.Infrastructure/MsLogistic.Infrastructure.csproj", "src/MsLogistic.Infrastructure/"]
RUN dotnet restore "src/MsLogistic.WebApi/MsLogistic.WebApi.csproj"
COPY . .
WORKDIR "/src/src/MsLogistic.WebApi"
RUN dotnet build "./MsLogistic.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MsLogistic.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MsLogistic.WebApi.dll"]
