using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Payments
{
    public class CreateProjectPaymentDto
    {
        public Guid ProjectId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Reference { get; set; }
    }
}
