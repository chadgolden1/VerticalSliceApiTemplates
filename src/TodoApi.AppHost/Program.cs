var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("todo-sql-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var todoDatabase = sqlServer.AddDatabase("todo-db");

builder.AddProject<Projects.TodoApi>("todo-api")
    .WithReference(todoDatabase)
    .WaitFor(todoDatabase);

builder.Build().Run();
