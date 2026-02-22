using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.DTOs.Transactions.Invoices;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IInvoiceDocumentGenerator
    {
        byte[] Generate(InvoiceReadDto invoice, ClientReadDto client, OrganizationReadDto org);
    }
}
