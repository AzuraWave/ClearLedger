using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Invoices
{
    public class InvoiceReadDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public Guid? ClientId { get; set; }
        public string? ClientName { get; set; }
        public Guid? ProjectId { get; set; }    
        public string? ProjectName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reference { get; set; }
        public List<InvoiceLineDto> Lines { get; set; } = new();
    }
}
