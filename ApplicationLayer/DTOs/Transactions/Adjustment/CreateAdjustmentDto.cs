using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Adjustment
{
    public class CreateAdjustmentDto
    {
        public Guid ProjectId { get; set; }

        public Guid ClientId { get; set; }
        public decimal Amount { get; set; } 
        public bool IsPositive { get; set; } 
        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;
    }
}
