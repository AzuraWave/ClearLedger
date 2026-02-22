using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class InvoiceNumberCounter : Entity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int Year { get; set; }
        public int LastNumber { get; set; }

        public byte[] RowVersion { get; set; } = null!;
    }
}
