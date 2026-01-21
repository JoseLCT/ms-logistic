# MsLogistic

### Docker

To build the Docker image for the MsLogistic application, use the following command:

```bash
docker container run -d -p 8080:8080 --name ms_logistic_app joselct/ms_logistic:1.0.0
```

### Migrations

To apply the latest migrations to the database, run the following command:

```bash
dotnet ef database update --project src/MsLogistic.Infrastructure --startup-project src/MsLogistic.WebApi --context DomainDbContext
```

To create a new migration, use the following command:

```bash
dotnet ef migrations add [NAME] --project src/MsLogistic.Infrastructure --startup-project src/MsLogistic.WebApi --context DomainDbContext
```

## Testing

### Requirements

To generate code coverage reports, ensure you have the ReportGenerator tool installed. You can install it globally using
the following command:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Running Tests and Generating Coverage Reports

To run the tests for the MsLogistic application, use the following command:

```bash
dotnet test src/MsLogistic.UnitTest/MsLogistic.UnitTest.csproj
```

To generate a code coverage report, use the following command:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

To generate html report from the coverage data, use the following command:

```bash
reportgenerator -reports:"./src/MsLogistic.UnitTest/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```