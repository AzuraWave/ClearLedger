using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions
{
    public class LedgerEntryReadDto
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        public Guid? ClientId { get; set; }

        public Guid? ProjectId { get; set; }

        public TransactionType Type { get; set; }

        public decimal AmountSigned { get; set; }

        public DateTime Date { get; set; }

        public string? Reference { get; set; }

        public Guid? BatchId { get; set; }

        public bool IsVoided { get; set; }
        public Guid? VoidedByEntryId { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
