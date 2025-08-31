
using Invoices.Broker;

namespace Invoices.Services.ListenerWorkerService
{
	public class ListenerWorkerService : BackgroundService
	{
		private readonly ILogger<ListenerWorkerService> _logger;
		private readonly IBroker _broker;

		public ListenerWorkerService(ILogger<ListenerWorkerService> logger, IBroker listener)
		{
			_logger = logger ?? throw new ArgumentNullException("Orders: Logger não encontrado.");
			_broker = listener ?? throw new ArgumentNullException("Broker: Broker não encontrado.");
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				await _broker.ReceiveMessageAsync(async ()=> await Task.CompletedTask);
			}
			catch (Exception ex) 
			{
				_logger.LogError("{DateTime} - {Error}", DateTime.UtcNow, ex.Message);
			}
		}
	}
}
