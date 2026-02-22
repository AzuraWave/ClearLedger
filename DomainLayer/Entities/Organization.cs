using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class Organization : EditableTrackedEntity
    {
        public required string Name { get; set; }

        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<ClientPaymentHeader> PaymentHeaders { get; set; } = new List<ClientPaymentHeader>();
        
        public ICollection<Invoice> Invoices { get; set; } = [];
        public ICollection<Adjustment> Adjustments { get; set; } = [];
        public ICollection<Discount> Discounts { get; set; } = [];

        public ICollection<InvoiceNumberCounter> invoiceNumberCounters { get; set; } = [];

        public string? ApiKeyHash { get; set; }  
        public Guid? DefaultAutomationUserId { get; set; }  
    }
}
