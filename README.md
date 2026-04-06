# MsLogistic

A microservice-based logistics platform built with .NET, containerized with Docker and orchestrated via Docker Compose.

---

## Getting Started

Clone the repository and set up your environment file before running the application:

```bash
git clone https://github.com/joselct/ms_logistic.git
cd ms_logistic
cp .env.example .env
```

Edit `.env` with your configuration values, then follow the [Docker Compose](#running-with-docker-compose) instructions
to spin up the stack.

---

## Docker

### Running the Container

To run a pre-built image of the MsLogistic application:

```bash
docker container run -d -p 8080:8080 --name ms_logistic_app joselct/ms_logistic:1.0.0
```

### Building & Publishing Images

**API**

```bash
docker build -f src/MsLogistic.WebApi/Dockerfile -t joselct/ms_logistic_api:<version> .
docker push joselct/ms_logistic_api:<version>
```

**Worker**

```bash
docker build -f src/MsLogistic.Worker/Dockerfile -t joselct/ms_logistic_worker:<version> .
docker push joselct/ms_logistic_worker:<version>
```

Replace `<version>` with your target version tag (e.g., `1.0.0`).

### Environment Variables

Copy the example file and fill in your values:

```bash
cp .env.example .env
```

> **Note:** Never commit your `.env` file to version control. It is listed in `.gitignore` by default.

### Running with Docker Compose

Start all services in detached mode:

```bash
docker compose --env-file .env up -d
```

To stop and remove containers:

```bash
docker compose down
```

---

## Database Migrations

> Ensure your connection string in `.env` (or `appsettings.json`) points to the correct database before running
> migrations.

**Apply the latest migrations:**

```bash
dotnet ef database update \
  --project src/MsLogistic.Infrastructure \
  --startup-project src/MsLogistic.WebApi \
  --context DomainDbContext
```

**Create a new migration:**

```bash
dotnet ef migrations add <MigrationName> \
  --project src/MsLogistic.Infrastructure \
  --startup-project src/MsLogistic.WebApi \
  --context DomainDbContext
```

Replace `<MigrationName>` with a descriptive name (e.g., `AddShipmentStatusColumn`).

---

## Testing

### Requirements

Code coverage reports require the **ReportGenerator** global tool. Install it once with:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Running Tests

Run the unit test suite:

```bash
dotnet test src/MsLogistic.UnitTest/MsLogistic.UnitTest.csproj
```

### Code Coverage

**Collect coverage data** (outputs a Cobertura XML file):

```bash
dotnet test src/MsLogistic.UnitTest/MsLogistic.UnitTest.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

**Generate an HTML report** from the coverage data:

```bash
reportgenerator \
  -reports:"./src/MsLogistic.UnitTest/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html
```

Open `coveragereport/index.html` in your browser to view the full coverage report.
