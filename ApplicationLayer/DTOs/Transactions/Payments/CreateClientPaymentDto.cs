using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Payments
{
    public class CreateClientPaymentDto
    {
        public Guid ClientId { get; set; }
        public decimal TotalAmount { get; set; }  // > 0
        public DateTime Date { get; set; }
        public string? Reference { get; set; }
        public List<PaymentAllocationDto> Allocations { get; set; } = new();
    }
}
