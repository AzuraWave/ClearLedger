using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTOs.Client
{
    public class ClientCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? BillingEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
    }
}
