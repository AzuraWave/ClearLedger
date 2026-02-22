using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Payments
{
    public class PaymentReadDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid ClientId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reference { get; set; }
        public List<PaymentAllocationDto> Allocations { get; set; } = new();
    }
}
