using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<GameTribunal_Web>("web");

builder.AddDevTunnel("tunnel")
       .WithReference(web)
       .WithAnonymousAccess();

builder.Build().Run();