using DomainLayer.Entities.Base;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class Client : EditableTrackedEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        public string Name { get; set; } = null!;
        public ClientStatus Status { get; set; } = ClientStatus.Active;


        public string? BillingEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }

        public decimal Balance { get; set; } = 0m;

        public ICollection<Project> Projects { get; set; } = [];
        public ICollection<ClientPaymentHeader> PaymentHeaders { get; set; } = [];
        public ICollection<Invoice> Invoices { get; set; } = [];
        public ICollection<Adjustment> Adjustments { get; set; } = [];
        public ICollection<Discount> Discounts { get; set; } = [];
        public ICollection<InvoiceNumberCounter> invoiceNumberCounters { get; set; } = [];
    }
}
