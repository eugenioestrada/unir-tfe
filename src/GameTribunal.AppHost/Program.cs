using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<GameTribunal_Web>("web").WithExternalHttpEndpoints();

builder.Build().Run();