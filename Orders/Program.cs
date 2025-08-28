using Contracts.Messages;
using Dapper;
using Orders.Broker;
using Orders.Configurations;
using Orders.Database;
using Orders.Entities;
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
builder.Services.AddScoped<RabbitMQPublisher>();
builder.Services.AddScoped<DapperContext>();
builder.Services.AddHostedService<OutboxWorkerService>();

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

app.MapPost("/orders", async (PostOrders request, RabbitMQPublisher publisher, DapperContext dbContext) =>
{
	Guid customerId = new Guid("db519d83-8801-41a5-a146-0edb2d0d200b");
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