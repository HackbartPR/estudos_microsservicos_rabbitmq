using Orders.Broker;
using Orders.Database;
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
			int minDelay = 1;
			int maxDelay = 60;
			int currentDelay = minDelay;

			while (stoppingToken.IsCancellationRequested) 
			{
				int processed = 0;

				try
				{
					using var scope = _serviceProvider.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
					var publisher = scope.ServiceProvider.GetRequiredService<RabbitMQPublisher>();

					
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
