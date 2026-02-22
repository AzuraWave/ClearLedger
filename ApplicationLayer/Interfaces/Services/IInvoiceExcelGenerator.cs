using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Transactions.Invoices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IInvoiceExcelGenerator
    {
        byte[] Generate(InvoiceReadDto invoice, ClientReadDto client);
    }
}
