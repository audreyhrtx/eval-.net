using Pipeline;
using Pipeline.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<PipelineProvider>();
builder.Services.AddSingleton<IPipelineScheduler, PipelineScheduler>();
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
