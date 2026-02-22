using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Payments
{
    public class PaymentAllocationDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }

        public ProjectStatus status { get; set; }
        public bool IsSelected { get; set; } = true;


    }
}
