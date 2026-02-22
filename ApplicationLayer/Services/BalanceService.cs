using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBalanceRepository balanceRepository;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IAdjustmentRepository adjustmentRepository;
        private readonly IDiscountRepository discountRepository;
        private readonly IProjectRepository projectRepository;
        private readonly ILogger<BalanceService> _logger;


        public BalanceService
            (IUnitOfWork unitOfWork,
            IBalanceRepository balanceRepository,
            IDiscountRepository discountRepository,
            IInvoiceRepository invoiceRepository,
            IAdjustmentRepository adjustmentRepository,
            IPaymentRepository paymentRepository,
            IProjectRepository projectRepository,
            ILogger<BalanceService> logger) 
        { 
            this.unitOfWork = unitOfWork;
            this.balanceRepository = balanceRepository;
            this.invoiceRepository = invoiceRepository;
            this.paymentRepository = paymentRepository;
            this.adjustmentRepository = adjustmentRepository;
            this.discountRepository = discountRepository;
            this.projectRepository = projectRepository;
            this._logger = logger;

        }
        public async Task AdjustClientBalanceAsync(Guid clientId, decimal amount, Guid organizationId, bool isPositive)
        {
            _logger.LogInformation("Adjusting client balance: {@OrgId}, {@ClientId}, Amount: {Amount}, Positive: {IsPositive}",
                organizationId, clientId, amount, isPositive);

            await balanceRepository.UpdateClientBalanceAsync(clientId,  organizationId, amount, isPositive);

            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Client balance updated successfully: {@ClientId}, NewAmount: {Amount}", clientId, amount);


        }

        public async Task AdjustProjectBalanceAsync(Guid projectId, decimal amount, Guid organizationId, bool isPositive)
        {
            _logger.LogInformation("Adjusting project balance: {@OrgId}, {@ProjectId}, Amount: {Amount}, Positive: {IsPositive}",
                organizationId, projectId, amount, isPositive);
            await balanceRepository.UpdateProjectBalanceAsync(projectId, organizationId, amount, isPositive);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Project balance updated successfully: {@ProjectId}, NewAmount: {Amount}", projectId, amount);

        }

        public async Task RecalculateClientAsync(Guid clientId, Guid organizationId)
        {
            _logger.LogInformation("Recalculating client balance: {@OrgId}, {@ClientId}", organizationId, clientId);

            var projects = await projectRepository.GetByClientAsync(clientId, organizationId);

            var newBalance = 0m;
            foreach (var project in projects) {
                newBalance += project.Balance;
            }

            await balanceRepository.SetClientBalanceAsync(clientId, organizationId, newBalance);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Client recalculation complete: {@ClientId}, NewBalance: {NewBalance}", clientId, newBalance);


        }

        public async Task RecalculateProjectAsync(Guid projectId, Guid organizationId)
        {
            _logger.LogInformation("Recalculating project balance: {@OrgId}, {@ProjectId}", organizationId, projectId);


            var Invoices = await invoiceRepository.QueryByProject(organizationId, projectId)
                .Where(i => !i.IsVoided)
                .SumAsync(i => i.TotalAmount);

            var Payments =await  paymentRepository.QueryByProject(organizationId, projectId)
                .Where(i => !i.IsVoided)
                .SumAsync(p => p.TotalAmount);

            var PositiveAdjustments = await adjustmentRepository.QueryByProject(organizationId, projectId)
                .Where(a => a.IsPositive)
                .SumAsync(a => a.Amount);

            var NegativeAdjustments = await adjustmentRepository.QueryByProject(organizationId, projectId)
                .Where(a => !a.IsPositive)
                .SumAsync(a => a.Amount);

            var Discounts = await discountRepository.QueryByProject(organizationId, projectId)
                .SumAsync(d => d.Amount);

            var newBalance = Invoices - Payments + PositiveAdjustments - NegativeAdjustments - Discounts;

            await balanceRepository.SetProjectBalanceAsync(projectId, organizationId, newBalance);

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Project recalculation complete: {@ProjectId}, NewBalance: {NewBalance}", projectId, newBalance);

        }
    }
}
