var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("todo-sql-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var todoDatabase = sqlServer.AddDatabase("todo-db");

var migrationService = builder.AddProject<Projects.TodoApi_MigrationService>("todo-migration-service")
    .WithReference(todoDatabase)
    .WaitFor(sqlServer);

builder.AddProject<Projects.TodoApi>("todo-api")
    .WithReference(todoDatabase)
    .WaitFor(migrationService);

builder.Build().Run();
