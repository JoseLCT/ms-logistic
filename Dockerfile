# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and project files
COPY MsLogistic.sln ./
COPY src/MsLogistic.Domain/MsLogistic.Domain.csproj src/MsLogistic.Domain/
COPY src/MsLogistic.Application/MsLogistic.Application.csproj src/MsLogistic.Application/
COPY src/MsLogistic.Infrastructure/MsLogistic.Infrastructure.csproj src/MsLogistic.Infrastructure/
COPY src/MsLogistic.Core/MsLogistic.Core.csproj src/MsLogistic.Core/
COPY src/MsLogistic.WebApi/MsLogistic.WebApi.csproj src/MsLogistic.WebApi/

# Restore dependencies
RUN dotnet restore MsLogistic.sln

# Copy all source files
COPY . .

# Build and publish the application
RUN dotnet publish src/MsLogistic.WebApi/MsLogistic.WebApi.csproj -c Release -o /app/out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime

WORKDIR /app

RUN dotnet tool install --global dotnet-ef --version 9.0.0

ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy the published output from the build stage
COPY --from=build /app/out .

COPY --from=build /src ./src

EXPOSE 8080

ENTRYPOINT ["dotnet", "MsLogistic.WebApi.dll"]