using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Invoices
{
    public class CreateInvoiceDto
    {
        public Guid ProjectId { get; set; }

        public Guid ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Reference { get; set; }

        public List<CreateInvoiceLineDto> Lines { get; set; } = new();
    }
}
