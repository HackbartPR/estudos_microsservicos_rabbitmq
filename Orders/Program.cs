using Contracts.Messages;
using Dapper;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Broker;
using Orders.Broker.RabbitMQ;
using Orders.Configurations;
using Orders.Database;
using Orders.Entities;
using Orders.Observability;
using Orders.Requests;
using Orders.Services.OutboxWorkerService;
using Scalar.AspNetCore;
using System.Data;
using System.Text.Json;
using System.Transactions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<AppSettings.Database>(builder.Configuration.GetSection(AppSettings.Database.Identifier));
builder.Services.Configure<AppSettings.Broker>(builder.Configuration.GetSection(AppSettings.Broker.Identifier));
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddScoped<IBroker, RabbitMQPublisher>();
builder.Services.AddScoped<DapperContext>();
builder.Services.AddHostedService<OutboxWorkerService>();

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

app.MapPost("/orders", async (PostOrders request, IBroker publisher, DapperContext dbContext) =>
{
	Guid customerId = new Guid("63a034b3-13c5-4a9d-9fbc-270ddfb3164d");
	Order order = new()
	{
		Amount = request.Amount,
		CustomerId = customerId,
	};

	OrderCreatedMessage message = new(order.Id, order.Amount, new OrderCreatedMessage.CustomerRecord(customerId));
	OutboxMessage outbox = new()
	{
		RoutingKey = "orders",
		Payload = JsonSerializer.Serialize(message)
	};

	IDbConnection dbConnection = dbContext.Connection;
	using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
	{
		string query = "INSERT INTO Orders (Id, CustomerId, Amount, Status, CreatedAt) VALUES (@Id, @CustomerId, @Amount, @Status, @CreatedAt)";
		await dbConnection.ExecuteAsync(query, order);

		query = "INSERT INTO OutboxMessages (Id, ExchangeName, RoutingKey, Payload, Status, UpdatedAt, CreatedAt) VALUES (@Id, @ExchangeName, @RoutingKey, @Payload::json, @Status, @UpdatedAt, @CreatedAt)";
		await dbConnection.ExecuteAsync(query, outbox);

		scope.Complete(); 
	}

    return Results.Created();
});

app.Run();