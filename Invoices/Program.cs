using Invoices.Broker;
using Invoices.Broker.RabbitMQ;
using Invoices.Configurations;
using Invoices.Services.ListenerWorkerService;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<AppSettings.Database>(builder.Configuration.GetSection(AppSettings.Database.Identifier));
builder.Services.Configure<AppSettings.Broker>(builder.Configuration.GetSection(AppSettings.Broker.Identifier));
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<IBroker, RabbitMQListener>();
builder.Services.AddHostedService<ListenerWorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
	return Results.Ok();
});

app.MapGet("/health", () =>
{
	return Results.Ok();
});

app.Run();
