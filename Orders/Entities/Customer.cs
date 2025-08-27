namespace Orders.Entities
{
	public class Customer
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = null!;

		public string Email { get; set; } = null!;

		public string Address { get; set; } = null!;

		public string State { get; set; } = null!;

		public string ZipCode { get; set; } = null!;

		public string Country { get; set; } = null!;

		public DateTime? DateOfBirth { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
