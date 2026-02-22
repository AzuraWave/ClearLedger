using ApplicationLayer.DTOs.Client;
using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Adjustment;
using ApplicationLayer.DTOs.Transactions.Discount;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.DTOs.Transactions.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface ITransactionService
    {
        // WRITES

        //Invoices 
        Task<Guid> CreateInvoiceAsync(CreateInvoiceDto dto, Guid organizationId, Guid createdByUserId);

        

        // Payments
        Task<Guid> CreateProjectPaymentAsync(CreateProjectPaymentDto dto, Guid organizationId, Guid createdByUserId);
        Task<Guid> CreateClientPaymentAsync(CreateClientPaymentDto dto, Guid organizationId, Guid createdByUserId);

        //Discounts
        Task<Guid> ApplyDiscountAsync(CreateDiscountDto dto, Guid organizationId, Guid createdByUserId);

        // Adjustments
        Task<Guid> CreateAdjustmentAsync(CreateAdjustmentDto dto, Guid organizationId, Guid createdByUserId);

        // Voiding
        Task VoidInvoiceAsync(Guid invoiceId, Guid organizationId, Guid userId);
        Task VoidPaymentAsync(Guid paymentId, Guid organizationId, Guid userId);



        // READS
        Task<PagedResult<TransactionDto>> SearchTransactionsAsync(TransactionQueryDto query);

        Task<InvoiceReadDto?> GetInvoiceDetailsAsync(Guid invoiceId, Guid organizationId);
        Task<decimal> GetOrgInvoicesForCurrentMonth(Guid organizationId);
        Task<ClientOverviewDto> GetClientOverviewAsync(Guid organizationId, Guid clientId);
        Task<AdjustmentReadDto?> GetAdjustmentDetailsAsync(Guid adjustmentId, Guid organizationId);
        Task<DiscountReadDto?> GetDiscountDetailsAsync(Guid discountId, Guid organizationId);
        Task<PaymentReadDto?> GetClientPaymentDetailsAsync(Guid paymentId, Guid organizationId);

        Task<StatementOfAccountDto> GetStatementAsync(Guid orgId, Guid clientId, DateTime from, DateTime to);

    }
}
