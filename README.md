# TodoApi
This project is an example solution template of an ASP.NET Core HTTP API that organizes code by capability (vertical slice architecture).

## Purpose
Often in codebases when creating a feature or changing an existing one, there's a need to change many different projects, files, and layers through out the system.  

In this project, code is organized by feature, rather than layers. Each feature (request) within the system is built around a distinct use case, meaning a single feature groups and encapsulates all of its concerns within a single "vertical slice."  

Using this approach, we avoid making changes across horizontal layers and rather only touch the feature we're interested in. We avoid "shared layer" abstractions such as repositories, services, etc. that require we make changes that affect more than just the feature we're presently working on. New features mean only adding code, rather than making changes to shared code and worrying about cascading side effects.

![Vertical slice among different layers of the system](./docs/vertical-slice-0.png)

Because each feature is organized this way, each feature in the system can decide for itself how to best fulfill a request.

![Vertical slices using best implementation for a given request](./docs/vertical-slice-1.png)

Using this approach, we'll find that we *minimize* coupling *between* features, but *maximize* the coupling *within* a feature. Sharing code becomes a purposeful decision instead of the default. You may also find that this simplifies project structure. For example, with a simple API, we may only need to create a single .NET project or assembly and separate things out by folder structure and namespace, as opposed to creating a .NET project per horizontal layer.

## Technologies showcased
* Vertical slice architecture
* FastEndpoints + REPR pattern + Swagger + FluentValidation
* Entity Framework Core (SQL Server)
* .NET Aspire for cloud-native development
* Integration tests

## Getting started
To work with this project, you need:
* .NET SDK
* PowerShell 7+
* .NET Aspire workload (for local development with Aspire)

### How to run
You have two options for running the project:

#### Option 1: Using .NET Aspire (Recommended)
```
dotnet run --project src/TodoApi.AppHost/TodoApi.AppHost.csproj
```
This will start the Aspire AppHost which orchestrates:
- SQL Server container
- Database migration service (runs migrations automatically)
- TodoApi

#### Option 2: Direct API execution
You can still run the API directly for simpler scenarios:
```
dotnet run --project src/TodoApi/TodoApi.csproj
```
Note: When running directly, you'll need to ensure SQL Server is available and handle migrations manually.

### How to test
To run all the tests, use the Text Explorer within Visual Studio 2022 17.8+ or the `dotnet` CLI:
```
dotnet test
```

You may alternatively run the build script, `build.ps1`, which runs all the tests by default.

Currently, there's one test project, `TodoApi.Tests`, where all the integration tests live. Integration tests are preferred as they will typically execute vertical slices or features against each other just as users would, mimicking the production scenarios very closely as the tests use actual dependency injection (DI) registrations, pipeline configuration, validation, real SQL Server DDL/DML, and so on.  

This project may also contain unit tests for testing at a more granular level, if desired.

### Build process
This project makes use of a single build script, `build.ps1`. The build script takes care of all the tasks related to continuous integration, such as:
* Cleaning previous binaries
* Verifying code analysis rules, code style
* Building
* Running integration tests
* Creating deployment artifact(s)

## Database migrations
This project uses EF Core's code-first migrations with a dedicated migration service following .NET Aspire best practices.

### Architecture
Migrations are separated into a dedicated `TodoApi.MigrationService` project that:
- Runs as a hosted service before the API starts
- Ensures database schema is up-to-date before accepting requests
- Provides better separation of concerns and deployment flexibility
- Automatically applies migrations when running with Aspire

### Scaffolding new migration
```
dotnet ef migrations add <MigrationName> --project ./src/TodoApi.MigrationService/TodoApi.MigrationService.csproj --startup-project ./src/TodoApi/TodoApi.csproj
```

### Manually apply migrations
```
dotnet ef database update --project ./src/TodoApi.MigrationService/TodoApi.MigrationService.csproj --startup-project ./src/TodoApi/TodoApi.csproj
```

### Running migrations in production
The migration service can be deployed separately and run as an init container or job in containerized environments, ensuring migrations complete before the API starts.

### References
Inspired by:
1. [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
2. [ContosoUniversityDotNetCore-Pages](https://github.com/jbogard/ContosoUniversityDotNetCore-Pages)
3. [Database migrations with Entity Framework Core sample app](https://github.com/dotnet/aspire-samples/tree/main/samples/DatabaseMigrations)
