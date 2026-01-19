using X39.Software.ExamMaker.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("branch-environment");

var db = builder.AddPostgres("pgsql")
    .WithDataVolume();
var authDb = db.AddDatabase("auth-db");
var examDb = db.AddDatabase("exam-db");
var api = builder.AddProject<Projects.X39_Software_ExamMaker_Api>("api")
    .WithAwaitedReference(authDb)
    .WithAwaitedReference(examDb);
var app = builder.AddProject<Projects.X39_Software_ExamMaker_WebApp>("app")
    .WaitFor(api);
builder.Build()
    .Run();
