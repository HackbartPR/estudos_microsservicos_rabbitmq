using Invoices.Broker;
using Invoices.Broker.RabbitMQ;
using Invoices.Configurations;
using Invoices.Observability;
using Invoices.Services.ListenerWorkerService;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<AppSettings.Database>(builder.Configuration.GetSection(AppSettings.Database.Identifier));
builder.Services.Configure<AppSettings.Broker>(builder.Configuration.GetSection(AppSettings.Broker.Identifier));
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<IBroker, RabbitMQListener>();
builder.Services.AddHostedService<ListenerWorkerService>();

builder.Services.AddOpenTelemetry()
	.WithTracing(tracerProviderBuilder =>
	{
		tracerProviderBuilder
			.AddSource(OpenTelemetryExtension.ServiceName)
			.SetResourceBuilder(
				ResourceBuilder.CreateDefault()
					.AddService(serviceName: OpenTelemetryExtension.ServiceName, serviceVersion: OpenTelemetryExtension.ServiceVersion))
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation()
			.AddRabbitMQInstrumentation()
			.AddNpgsql()
			.AddOtlpExporter(opt =>
			{
				opt.Endpoint = new Uri("http://localhost:4317");
			});
	});


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
