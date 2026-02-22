using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Discount
{
    public class CreateDiscountDto
    {
        public Guid ProjectId { get; set; }

        public Guid ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;
    }
}
