using DomainLayer.Enums;

namespace ApplicationLayer.DTOs.Client
{
    public class ClientReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? BillingEmail { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public ClientStatus Status { get; set; }
        public decimal Balance { get; set; }
    }
}
