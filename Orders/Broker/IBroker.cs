namespace Orders.Broker
{
	public interface IBroker : IDisposable
	{
		Task SendMessageAsync(string message);
	}
}
