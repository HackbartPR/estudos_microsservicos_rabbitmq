using Dapper;
using Orders.Broker;
using Orders.Database;
using Orders.DTOs;
using Orders.Entities;
using Orders.Enums;
using System.Data;
using System.Diagnostics;

namespace Orders.Services.OutboxWorkerService
{
	/// <summary>
	/// Responsável por realizar a leitura da Outbox Table
	/// </summary>
	public class OutboxWorkerService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<OutboxWorkerService> _logger;

		public OutboxWorkerService(IServiceProvider serviceProvider, ILogger<OutboxWorkerService> logger)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException("Orders: Service Provider não encontrado.");
			_logger = logger ?? throw new ArgumentNullException("Orders: Logger não encontrado.");
		}

		/// <summary>
		/// Utiliza Exponential Backoff
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			int minDelay = 1000;
			int maxDelay = 60000;
			int currentDelay = minDelay;

			while (!stoppingToken.IsCancellationRequested) 
			{
				int processed = 0;

				try
				{
					using var scope = _serviceProvider.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
					var publisher = scope.ServiceProvider.GetRequiredService<RabbitMQPublisher>();

					IDbConnection dbConnection = dbContext.Connection;
					
					string query = @"SELECT Id, ExchangeName, RoutingKey, Payload  FROM OutboxMessages 
						WHERE Status = @Status AND (UpdatedAt IS NULL OR UpdatedAt < @Delay)
						ORDER BY ID DESC FOR UPDATE SKIP LOCKED LIMIT 10";
					IEnumerable<OutboxMessageDTO> messages = await dbConnection.QueryAsync<OutboxMessageDTO>(query, new { Status = EStatusOutboxMessage.Pending, Delay = DateTime.UtcNow.AddSeconds(-5) });

					if (messages.Any()) 
					{
						//foreach (OutboxMessageDTO message in messages)
						//	await publisher.SendMessageAsync(message.Payload, message.ExchangeName, message.RoutingKey);

						List<Guid> ids = [.. messages.Select(m => m.Id)];
						query = "UPDATE OutboxMessages SET Status = @Status, UpdatedAt = @UpdatedAt WHERE ID = ANY(@Messages::uuid[])";
						processed = await dbConnection.ExecuteAsync(query, new { Status = EStatusOutboxMessage.Pending, Messages = ids, UpdatedAt = DateTime.UtcNow });
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("{DateTime} - {Error}", DateTime.UtcNow, ex.Message);
				}

				currentDelay = processed > 0 ? minDelay : Math.Min(currentDelay * 2, maxDelay);
				await Task.Delay(currentDelay, stoppingToken);
			}
		}
	}
}
