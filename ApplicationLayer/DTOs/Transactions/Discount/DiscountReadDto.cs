using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Discount
{
    public class DiscountReadDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ProjectId { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }
}
