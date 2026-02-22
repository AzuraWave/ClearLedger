using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ClientId { get; set; }

        public string? ClientName { get; set; }
        public string? ProjectName { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }

        public decimal RunningBalance { get; set; } = 0;

        public bool IsVoided = false;
    }
}
