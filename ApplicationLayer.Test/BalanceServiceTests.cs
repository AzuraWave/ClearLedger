using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using MockQueryable.Moq;
using MockQueryable;
namespace ApplicationLayer.Test
{
    public class BalanceServiceTests
    {

        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<IBalanceRepository> _balanceRepoMock;
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<IPaymentRepository> _paymentRepoMock;
        private readonly Mock<IAdjustmentRepository> _adjustmentRepoMock;
        private readonly Mock<IDiscountRepository> _discountRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<ILogger<BalanceService>> _loggerMock;

        private readonly BalanceService _service;

        public BalanceServiceTests()
        {
            _unitMock = new Mock<IUnitOfWork>();
            _balanceRepoMock = new Mock<IBalanceRepository>();
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _adjustmentRepoMock = new Mock<IAdjustmentRepository>();
            _discountRepoMock = new Mock<IDiscountRepository>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _loggerMock = new Mock<ILogger<BalanceService>>();

            _service = new BalanceService(
                _unitMock.Object,
                _balanceRepoMock.Object,
                _discountRepoMock.Object,
                _invoiceRepoMock.Object,
                _adjustmentRepoMock.Object,
                _paymentRepoMock.Object,
                _projectRepoMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AdjustClientBalanceAsync_Calls_Repo_And_Save()
        {
            var clientId = Guid.NewGuid();
            var orgId = Guid.NewGuid();

            _balanceRepoMock.Setup(r =>
                r.UpdateClientBalanceAsync(clientId, orgId, 100m, true))
                .Returns(Task.CompletedTask);

            _unitMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _service.AdjustClientBalanceAsync(clientId, 100m, orgId, true);

            _balanceRepoMock.Verify(r =>
                r.UpdateClientBalanceAsync(clientId, orgId, 100m, true), Times.Once);

            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task AdjustProjectBalanceAsync_Calls_Repo_And_Save()
        {
            var projectId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            decimal amount = 200m;
            bool isPositive = true;

            _balanceRepoMock.Setup(r =>
                r.UpdateProjectBalanceAsync(projectId, orgId, amount, isPositive))
                .Returns(Task.CompletedTask);

            _unitMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _service.AdjustProjectBalanceAsync(projectId, amount, orgId, isPositive);

            _balanceRepoMock.Verify(r =>
                r.UpdateProjectBalanceAsync(projectId, orgId, amount, isPositive), Times.Once);

            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RecalculateClientAsync_Sums_Project_Balances()
        {
            var projects = new List<Project>
    {
        new Project { Balance = 100m },
        new Project { Balance = 50m }
    };

            _projectRepoMock.Setup(r =>
                r.GetByClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(projects);

            _balanceRepoMock.Setup(r =>
                r.SetClientBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), 150m))
                .Returns(Task.CompletedTask);

            _unitMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _service.RecalculateClientAsync(Guid.NewGuid(), Guid.NewGuid());

            _balanceRepoMock.Verify(r =>
                r.SetClientBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), 150m),
                Times.Once);
        }
        [Fact]
        public async Task RecalculateProjectAsync_Computes_Correct_Balance()
        {
            var invoices = new[] { new Invoice { TotalAmount = 200m, IsVoided = false } }.BuildMock();
            var payments = new[] { new ClientPaymentHeader { TotalAmount = 50m, IsVoided = false } }.BuildMock();
            var adjustments = new[]
            {
        new Adjustment { Amount = 30m, IsPositive = true },
        new Adjustment { Amount = 10m, IsPositive = false }
    }.BuildMock();
            var discounts = new[] { new Discount { Amount = 20m } }.BuildMock();

            _invoiceRepoMock.Setup(r => r.QueryByProject(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(invoices);

            _paymentRepoMock.Setup(r => r.QueryByProject(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(payments);

            _adjustmentRepoMock.Setup(r => r.QueryByProject(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(adjustments);

            _discountRepoMock.Setup(r => r.QueryByProject(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(discounts);

            _balanceRepoMock.Setup(r =>
                r.SetProjectBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), 150m))
                .Returns(Task.CompletedTask);

            _unitMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _service.RecalculateProjectAsync(Guid.NewGuid(), Guid.NewGuid());

            
            _balanceRepoMock.Verify(r =>
                r.SetProjectBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), 150m),
                Times.Once);
        }



    }


}
