namespace Invoices.Broker
{
	public interface IBroker : IDisposable
	{
		Task ReceiveMessageAsync(Func<Task> callback);
	}
}
