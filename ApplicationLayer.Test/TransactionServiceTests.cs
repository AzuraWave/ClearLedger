using ApplicationLayer.DTOs.Query;
using ApplicationLayer.DTOs.Transactions;
using ApplicationLayer.DTOs.Transactions.Adjustment;
using ApplicationLayer.DTOs.Transactions.Discount;
using ApplicationLayer.DTOs.Transactions.Invoices;
using ApplicationLayer.DTOs.Transactions.Payments;
using ApplicationLayer.DTOs.Transactions.Query;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Test
{
    public class TransactionServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<IDiscountRepository> _discountRepoMock;
        private readonly Mock<IAdjustmentRepository> _adjustmentRepoMock;
        private readonly Mock<IPaymentRepository> _paymentRepoMock;
        private readonly Mock<IClientRepository> _clientRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IBalanceService> _balanceServiceMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly Mock<ITransactionService> _serviceMock;

        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _discountRepoMock = new Mock<IDiscountRepository>();
            _adjustmentRepoMock = new Mock<IAdjustmentRepository>();
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _clientRepoMock = new Mock<IClientRepository>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _balanceServiceMock = new Mock<IBalanceService>();
            _unitMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<TransactionService>>();
            _serviceMock = new Mock<ITransactionService>();

            _service = new TransactionService(
                _invoiceRepoMock.Object,
                _discountRepoMock.Object,
                _adjustmentRepoMock.Object,
                _paymentRepoMock.Object,
                _clientRepoMock.Object,
                _projectRepoMock.Object,
                _balanceServiceMock.Object,
                _unitMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateInvoiceAsync_CreatesInvoiceAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            var dto = new CreateInvoiceDto
            {
                ProjectId = projectId,
                Date = DateTime.UtcNow,
                Reference = "Sand",
                Lines = new List<CreateInvoiceLineDto>
        {
            new CreateInvoiceLineDto { Description = "Test", Quantity = 2, UnitPrice = 50m }
        }
            };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });

            _invoiceRepoMock.Setup(r => r.GetNextInvoiceNumberAsync(orgId, clientId))
                .ReturnsAsync("INV-001");

            _invoiceRepoMock.Setup(r => r.AddAsync(It.IsAny<Invoice>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, 100m, orgId, true))
                .Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, 100m, orgId, true))
                .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var invoiceId = await _service.CreateInvoiceAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, invoiceId);
            _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, 100m, orgId, true), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, 100m, orgId, true), Times.Once);
        }

        [Fact]
        public async Task CreateInvoiceAsync_Throws_WhenProjectNotFound()
        {
            var dto = new CreateInvoiceDto { ProjectId = Guid.NewGuid(), Lines = new List<CreateInvoiceLineDto> { new() } };
            _projectRepoMock.Setup(r => r.GetByIdAsync(dto.ProjectId, It.IsAny<Guid>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.CreateInvoiceAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateProjectPaymentAsync_CreatesPaymentAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var amount = 150m;

            var dto = new CreateProjectPaymentDto
            {
                ProjectId = projectId,
                Amount = amount,
                Date = DateTime.UtcNow,
                Reference = "Sand"
            };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<ClientPaymentHeader>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, amount, orgId, false)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, amount, orgId, false)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var paymentId = await _service.CreateProjectPaymentAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, paymentId);
            _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<ClientPaymentHeader>()), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, amount, orgId, false), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, amount, orgId, false), Times.Once);
        }

        [Fact]
        public async Task CreateProjectPaymentAsync_Throws_WhenAmountNonPositive()
        {
            var dto = new CreateProjectPaymentDto { ProjectId = Guid.NewGuid(), Amount = 0m };
            _projectRepoMock.Setup(r => r.GetByIdAsync(dto.ProjectId, It.IsAny<Guid>()))
                .ReturnsAsync(new Project { Id = dto.ProjectId, ClientId = Guid.NewGuid() });
            _clientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Client { Id = Guid.NewGuid() });

            await Assert.ThrowsAsync<Exception>(() => _service.CreateProjectPaymentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateClientPaymentAsync_CreatesPaymentAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var dto = new CreateClientPaymentDto
            {
                ClientId = clientId,
                TotalAmount = 150m,
                Date = DateTime.UtcNow,
                Reference = "Sand",
                Allocations = new List<PaymentAllocationDto>
        {
            new PaymentAllocationDto { ProjectId = projectId, Amount = 150m }
        }
            };

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<ClientPaymentHeader>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, 150m, orgId, false)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, 150m, orgId, false)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var paymentId = await _service.CreateClientPaymentAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, paymentId);
            _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<ClientPaymentHeader>()), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, 150m, orgId, false), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, 150m, orgId, false), Times.Once);
        }

        [Fact]
        public async Task CreateClientPaymentAsync_Throws_WhenAmountNonPositive()
        {
            var dto = new CreateClientPaymentDto { ClientId = Guid.NewGuid(), TotalAmount = 0, Allocations = new List<PaymentAllocationDto> { new() } };
            await Assert.ThrowsAsync<Exception>(() => _service.CreateClientPaymentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateClientPaymentAsync_Throws_WhenNoAllocations()
        {
            var dto = new CreateClientPaymentDto { ClientId = Guid.NewGuid(), TotalAmount = 100, Allocations = new List<PaymentAllocationDto>() };
            await Assert.ThrowsAsync<Exception>(() => _service.CreateClientPaymentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateClientPaymentAsync_Throws_WhenAllocationTotalMismatch()
        {
            var dto = new CreateClientPaymentDto
            {
                ClientId = Guid.NewGuid(),
                TotalAmount = 150,
                Allocations = new List<PaymentAllocationDto> { new() { Amount = 100, ProjectId = Guid.NewGuid() } }
            };
            await Assert.ThrowsAsync<Exception>(() => _service.CreateClientPaymentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateClientPaymentAsync_Throws_WhenProjectInvalid()
        {
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var dto = new CreateClientPaymentDto
            {
                ClientId = clientId,
                TotalAmount = 100,
                Allocations = new List<PaymentAllocationDto> { new() { ProjectId = projectId, Amount = 100 } }
            };

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<Guid>())).ReturnsAsync(new Client { Id = clientId });
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.CreateClientPaymentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ApplyDiscountAsync_CreatesDiscountAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var dto = new CreateDiscountDto
            {
                ClientId = clientId,
                ProjectId = projectId,
                Amount = 100m,
                Date = DateTime.UtcNow,
                Reason = "Test discount"
            };


            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });

            _discountRepoMock.Setup(r => r.AddAsync(It.IsAny<Discount>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, false)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, false)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var discountId = await _service.ApplyDiscountAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, discountId);
            _discountRepoMock.Verify(r => r.AddAsync(It.IsAny<Discount>()), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, false), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, false), Times.Once);
        }

        [Fact]
        public async Task ApplyDiscountAsync_Throws_WhenAmountNonPositive()
        {
            var dto = new CreateDiscountDto { ClientId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), Amount = 0, Date = DateTime.UtcNow };
            await Assert.ThrowsAsync<Exception>(() => _service.ApplyDiscountAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ApplyDiscountAsync_Throws_WhenProjectNotFound()
        {
            var projectId = Guid.NewGuid();
            var dto = new CreateDiscountDto { ProjectId = projectId, Amount = 50, Date = DateTime.UtcNow };
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.ApplyDiscountAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ApplyDiscountAsync_Throws_WhenClientNotFound()
        {
            var projectId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var dto = new CreateDiscountDto { ProjectId = projectId, Amount = 50, Date = DateTime.UtcNow };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>()))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });

            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<Guid>())).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.ApplyDiscountAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAdjustmentAsync_CreatesPositiveAdjustmentAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var dto = new CreateAdjustmentDto
            {
                ClientId = clientId,
                ProjectId = projectId,
                Amount = 100m,
                IsPositive = true,
                Date = DateTime.UtcNow,
                Reason = "Positive adjustment"
            };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });
            _adjustmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Adjustment>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, true)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, true)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var adjustmentId = await _service.CreateAdjustmentAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, adjustmentId);
            _adjustmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Adjustment>()), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, true), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, true), Times.Once);
        }

        [Fact]
        public async Task CreateAdjustmentAsync_CreatesNegativeAdjustmentAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var dto = new CreateAdjustmentDto
            {
                ClientId = clientId,
                ProjectId = projectId,
                Amount = 50m,
                IsPositive = false,
                Date = DateTime.UtcNow,
                Reason = "Negative adjustment"
            };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId))
                .ReturnsAsync(new Client { Id = clientId });
            _adjustmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Adjustment>())).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, false)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, false)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var adjustmentId = await _service.CreateAdjustmentAsync(dto, orgId, userId);

            Assert.NotEqual(Guid.Empty, adjustmentId);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, dto.Amount, orgId, false), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, dto.Amount, orgId, false), Times.Once);
        }

        [Fact]
        public async Task CreateAdjustmentAsync_Throws_WhenAmountNonPositive()
        {
            var dto = new CreateAdjustmentDto { Amount = 0, ClientId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAdjustmentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAdjustmentAsync_Throws_WhenProjectNotFound()
        {
            var dto = new CreateAdjustmentDto { ProjectId = Guid.NewGuid(), Amount = 10, ClientId = Guid.NewGuid() };
            _projectRepoMock.Setup(r => r.GetByIdAsync(dto.ProjectId, It.IsAny<Guid>())).ReturnsAsync((Project?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAdjustmentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAdjustmentAsync_Throws_WhenClientNotFound()
        {
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var dto = new CreateAdjustmentDto { ProjectId = projectId, Amount = 20, ClientId = clientId };

            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>())).ReturnsAsync(new Project { Id = projectId, ClientId = clientId });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<Guid>())).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.CreateAdjustmentAsync(dto, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidInvoiceAsync_VoidsInvoiceAndAdjustsBalances()
        {
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var invoiceId = Guid.NewGuid();

            var invoice = new Invoice
            {
                Id = invoiceId,
                ProjectId = projectId,
                ClientId = clientId,
                TotalAmount = 200m,
                IsVoided = false
            };

            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, orgId)).ReturnsAsync(invoice);
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, orgId)).ReturnsAsync(new Project { Id = projectId, ClientId = clientId });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId)).ReturnsAsync(new Client { Id = clientId });
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, invoice.TotalAmount, orgId, false)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, invoice.TotalAmount, orgId, false)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.VoidInvoiceAsync(invoiceId, orgId, userId);

            Assert.True(invoice.IsVoided);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, invoice.TotalAmount, orgId, false), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, invoice.TotalAmount, orgId, false), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task VoidInvoiceAsync_Throws_WhenInvoiceNotFound()
        {
            var invoiceId = Guid.NewGuid();
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<Guid>())).ReturnsAsync((Invoice?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.VoidInvoiceAsync(invoiceId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidInvoiceAsync_Throws_WhenInvoiceAlreadyVoided()
        {
            var invoiceId = Guid.NewGuid();
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Invoice { Id = invoiceId, IsVoided = true });

            await Assert.ThrowsAsync<Exception>(() => _service.VoidInvoiceAsync(invoiceId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidInvoiceAsync_Throws_WhenProjectNotFound()
        {
            var invoiceId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Invoice { Id = invoiceId, ProjectId = projectId });
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.VoidInvoiceAsync(invoiceId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidInvoiceAsync_Throws_WhenClientNotFound()
        {
            var invoiceId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<Guid>()))
                .ReturnsAsync(new Invoice { Id = invoiceId, ProjectId = projectId });
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId, It.IsAny<Guid>()))
                .ReturnsAsync(new Project { Id = projectId, ClientId = clientId });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<Guid>())).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.VoidInvoiceAsync(invoiceId, Guid.NewGuid(), Guid.NewGuid()));
        }


        [Fact]
        public async Task VoidPaymentAsync_VoidsPaymentAndAdjustsBalances()
        {
            var paymentId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var payment = new ClientPaymentHeader
            {
                Id = paymentId,
                ClientId = clientId,
                Allocations = new List<ClientPaymentAllocation>
        {
            new ClientPaymentAllocation { ProjectId = projectId, Amount = 100m }
        },
                IsVoided = false
            };

            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, orgId)).ReturnsAsync(payment);
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, orgId)).ReturnsAsync(new Client { Id = clientId });
            _balanceServiceMock.Setup(b => b.AdjustProjectBalanceAsync(projectId, 100m, orgId, true)).Returns(Task.CompletedTask);
            _balanceServiceMock.Setup(b => b.AdjustClientBalanceAsync(clientId, 100m, orgId, true)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.VoidPaymentAsync(paymentId, orgId, userId);

            Assert.True(payment.IsVoided);
            _balanceServiceMock.Verify(b => b.AdjustProjectBalanceAsync(projectId, 100m, orgId, true), Times.Once);
            _balanceServiceMock.Verify(b => b.AdjustClientBalanceAsync(clientId, 100m, orgId, true), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task VoidPaymentAsync_Throws_WhenPaymentNotFound()
        {
            var paymentId = Guid.NewGuid();
            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<Guid>())).ReturnsAsync((ClientPaymentHeader?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.VoidPaymentAsync(paymentId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidPaymentAsync_Throws_WhenPaymentAlreadyVoided()
        {
            var paymentId = Guid.NewGuid();
            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<Guid>()))
                .ReturnsAsync(new ClientPaymentHeader { Id = paymentId, IsVoided = true });

            await Assert.ThrowsAsync<Exception>(() => _service.VoidPaymentAsync(paymentId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task VoidPaymentAsync_Throws_WhenClientNotFound()
        {
            var paymentId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<Guid>()))
                .ReturnsAsync(new ClientPaymentHeader { Id = paymentId, ClientId = clientId, Allocations = new List<ClientPaymentAllocation>() });
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<Guid>())).ReturnsAsync((Client?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.VoidPaymentAsync(paymentId, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAdjustmentDetailsAsync_ReturnsDto_WhenFound()
        {
            var adjustmentId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var adjustment = new Adjustment
            {
                Id = adjustmentId,
                ClientId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Amount = 100m,
                Reason = "Test reason",
                Date = DateTime.UtcNow
            };

            _adjustmentRepoMock.Setup(r => r.GetByIdAsync(adjustmentId, orgId)).ReturnsAsync(adjustment);

            var result = await _service.GetAdjustmentDetailsAsync(adjustmentId, orgId);

            Assert.NotNull(result);
            Assert.Equal(adjustment.Id, result!.Id);
            Assert.Equal(adjustment.Amount, result.Amount);
            Assert.Equal(adjustment.Reason, result.Reference);
        }

        [Fact]
        public async Task GetAdjustmentDetailsAsync_ReturnsNull_WhenNotFound()
        {
            var adjustmentId = Guid.NewGuid();
            _adjustmentRepoMock.Setup(r => r.GetByIdAsync(adjustmentId, It.IsAny<Guid>())).ReturnsAsync((Adjustment?)null);

            var result = await _service.GetAdjustmentDetailsAsync(adjustmentId, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClientPaymentDetailsAsync_ReturnsDto_WhenFound()
        {
            var paymentId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var payment = new ClientPaymentHeader
            {
                Id = paymentId,
                ClientId = clientId,
                Date = DateTime.UtcNow,
                Reference = "Test payment",
                TotalAmount = 200m,
                Allocations = new List<ClientPaymentAllocation>
        {
            new() { ProjectId = projectId, Amount = 200m }
        }
            };

            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, orgId)).ReturnsAsync(payment);

            var result = await _service.GetClientPaymentDetailsAsync(paymentId, orgId);

            Assert.NotNull(result);
            Assert.Equal(payment.Id, result!.Id);
            Assert.Equal(payment.TotalAmount, result.TotalAmount);
            Assert.Single(result.Allocations);
            Assert.Equal(projectId, result.Allocations.First().ProjectId);
        }

        [Fact]
        public async Task GetClientPaymentDetailsAsync_ReturnsNull_WhenNotFound()
        {
            var paymentId = Guid.NewGuid();
            _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<Guid>())).ReturnsAsync((ClientPaymentHeader?)null);

            var result = await _service.GetClientPaymentDetailsAsync(paymentId, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDiscountDetailsAsync_ReturnsDto_WhenFound()
        {
            var discountId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var discount = new Discount
            {
                Id = discountId,
                ClientId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Amount = 50m,
                Reason = "Promo",
                Date = DateTime.UtcNow
            };

            _discountRepoMock.Setup(r => r.GetByIdAsync(discountId, orgId)).ReturnsAsync(discount);

            var result = await _service.GetDiscountDetailsAsync(discountId, orgId);

            Assert.NotNull(result);
            Assert.Equal(discount.Id, result!.Id);
            Assert.Equal(discount.Amount, result.Amount);
            Assert.Equal(discount.Reason, result.Reference);
        }

        [Fact]
        public async Task GetDiscountDetailsAsync_ReturnsNull_WhenNotFound()
        {
            _discountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Discount?)null);

            var result = await _service.GetDiscountDetailsAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetInvoiceDetailsAsync_ReturnsDto_WhenFound()
        {
            var invoiceId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var invoice = new Invoice
            {
                Id = invoiceId,
                InvoiceNumber = "INV001",
                ClientId = clientId,
                ProjectId = projectId,
                Client = new Client { Id = clientId, Name = "Test Client" },
                Project = new Project { Id = projectId, Name = "Test Project" },
                TotalAmount = 100m,
                Reference = "Ref001",
                Date = DateTime.UtcNow,
                Lines = new List<InvoiceLine>
        {
            new InvoiceLine { Id = Guid.NewGuid(), Description = "Item1", Quantity = 2, UnitPrice = 25m, LineTotal = 50m }
        }
            };

            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId, orgId)).ReturnsAsync(invoice);

            var result = await _service.GetInvoiceDetailsAsync(invoiceId, orgId);

            Assert.NotNull(result);
            Assert.Equal(invoice.Id, result!.Id);
            Assert.Equal(invoice.TotalAmount, result.TotalAmount);
            Assert.Single(result.Lines);
            Assert.Equal(invoice.Lines.First().Description, result.Lines.First().Description);
        }

        [Fact]
        public async Task GetInvoiceDetailsAsync_ReturnsNull_WhenNotFound()
        {
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Invoice?)null);

            var result = await _service.GetInvoiceDetailsAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchTransactionsAsync_ReturnsAllTransactionTypes()
        {
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

   
            var invoices = new List<Invoice>
             {
                 new Invoice
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        ClientId = clientId,
                        ProjectId = projectId,
                        Client = new Client { Id = clientId, Name = "ClientA" },
                        Project = new Project { Id = projectId, Name = "ProjectA" },
                        Lines = new List<InvoiceLine> { new InvoiceLine { Quantity = 2, UnitPrice = 50, LineTotal = 100 } },
                        Date = DateTime.UtcNow,
                        Reference = "InvoiceRef",
                        IsVoided = false
                    }
                }.BuildMock();

            var discounts = new List<Discount>
                {
                    new Discount
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        ClientId = clientId,
                        ProjectId = projectId,
                        Client = new Client { Id = clientId, Name = "ClientA" },
                        Project = new Project { Id = projectId, Name = "ProjectA" },
                        Amount = 10,
                        Date = DateTime.UtcNow,
                        Reason = "DiscountReason"
                    }
                }.BuildMock();

            var adjustments = new List<Adjustment>
            {
                new Adjustment
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    ClientId = clientId,
                    ProjectId = projectId,
                    Client = new Client { Id = clientId, Name = "ClientA" },
                    Project = new Project { Id = projectId, Name = "ProjectA" },
                    Amount = 20,
                    IsPositive = true,
                    Date = DateTime.UtcNow,
                    Reason = "AdjustmentReason"
                }
            }.BuildMock();

            var payments = new List<ClientPaymentHeader>
                {
                    new ClientPaymentHeader
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        ClientId = clientId,
                        Client = new Client { Id = clientId, Name = "ClientA" },
                        Allocations = new List<ClientPaymentAllocation>
                        {
                            new ClientPaymentAllocation
                            {
                                ProjectId = projectId,
                                Project = new Project { Id = projectId, Name = "ProjectA" },
                                Amount = 50
                            }
                        },
                        Date = DateTime.UtcNow,
                        Reference = "PaymentRef",
                        IsVoided = false
                    }
                }.BuildMock();

            _invoiceRepoMock.Setup(r => r.QueryAll(orgId)).Returns(invoices);
            _discountRepoMock.Setup(r => r.QueryAll(orgId)).Returns(discounts);
            _adjustmentRepoMock.Setup(r => r.QueryAll(orgId)).Returns(adjustments);
            _paymentRepoMock.Setup(r => r.QueryAll(orgId)).Returns(payments);

            var queryDto = new TransactionQueryDto
            {
                OrganizationId = orgId,
                ClientId = clientId,
                Page = 1,
                PageSize = 10
            };

            var result = await _service.SearchTransactionsAsync(queryDto);

            Assert.NotNull(result);
            Assert.Equal(4, result.Items.Count());
            Assert.Contains(result.Items, t => t.Type == TransactionType.Invoice);
            Assert.Contains(result.Items, t => t.Type == TransactionType.Discount);
            Assert.Contains(result.Items, t => t.Type == TransactionType.Adjustment);
            Assert.Contains(result.Items, t => t.Type == TransactionType.Payment);
        }

        [Fact]
        public async Task GetOrgInvoicesForCurrentMonth_ReturnsValue()
        {
            var orgId = Guid.NewGuid();
            _invoiceRepoMock.Setup(r => r.GetOrgInvoicesForCurrentMonth(orgId)).ReturnsAsync(500m);

            var result = await _service.GetOrgInvoicesForCurrentMonth(orgId);

            Assert.Equal(500m, result);
        }

        [Fact]
        public async Task GetClientOverviewAsync_ReturnsCorrectTotals()
        {
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var projectId = Guid.NewGuid();


            var invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        ClientId = clientId,
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine { LineTotal = 100 },
                            new InvoiceLine { LineTotal = 50 }
                        },
                        IsVoided = false
                    }
                }.BuildMock();

            _invoiceRepoMock.Setup(r => r.QueryByClient(orgId, clientId)).Returns(invoices);

                        var payments = new List<ClientPaymentHeader>
                {
                    new ClientPaymentHeader
                    {
                        ClientId = clientId,
                        Allocations = new List<ClientPaymentAllocation>
                        {
                            new ClientPaymentAllocation { Amount = 70, ProjectId = projectId }
                        },
                        IsVoided = false
                    }
                }.BuildMock();

            _paymentRepoMock.Setup(r => r.QueryByClient(orgId, clientId)).Returns(payments);

            var projects = new List<Project>
            {
                new Project { ClientId = clientId, Status = ProjectStatus.Active },
                new Project { ClientId = clientId, Status = ProjectStatus.Archived },
                new Project { ClientId = Guid.NewGuid(), Status = ProjectStatus.Active } 
            }.BuildMock();

            _projectRepoMock.Setup(r => r.QueryAll(orgId)).ReturnsAsync(projects);

            var result = await _service.GetClientOverviewAsync(orgId, clientId);

            Assert.Equal(150m, result.TotalInvoiced); 
            Assert.Equal(70m, result.TotalPaid);
            Assert.Equal(1, result.ActiveProjects);  
        }
        [Fact]
        public async Task GetStatementAsync_ReturnsCorrectStatement()
        {
            var orgId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);
            var serviceMock = new Mock<TransactionService>(
                _invoiceRepoMock.Object,
                _discountRepoMock.Object,
                _adjustmentRepoMock.Object,
                _paymentRepoMock.Object,
                _clientRepoMock.Object,
                _projectRepoMock.Object,
                _balanceServiceMock.Object,
                _unitMock.Object,
                _loggerMock.Object
            ){ CallBase = true };

            serviceMock
        .Setup(s => s.SearchTransactionsAsync(It.Is<TransactionQueryDto>(d => d.To == from.AddDays(-1))))
        .ReturnsAsync(new PagedResult<TransactionDto>(
            new List<TransactionDto>
            {
                new TransactionDto { Amount = 100 },
                new TransactionDto { Amount = -30 }
            }, 2));

            serviceMock
                .Setup(s => s.SearchTransactionsAsync(It.Is<TransactionQueryDto>(d => d.From == from && d.To == to)))
                .ReturnsAsync(new PagedResult<TransactionDto>(
                    new List<TransactionDto>
                    {
                new TransactionDto { Amount = 50 },
                new TransactionDto { Amount = -20 }
                    }, 2));

            var client = new Client { Id = clientId, Name = "Test Client" };
            _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);

            var result = await serviceMock.Object.GetStatementAsync(orgId, clientId, from, to);

            Assert.Equal(clientId, result.ClientId);
            Assert.Equal("Test Client", result.ClientName);
            Assert.Equal(70, result.OpeningBalance);
            Assert.Equal(2, result.Transactions.Count);
            Assert.Equal(120, result.Transactions[0].RunningBalance); 
            Assert.Equal(100, result.Transactions[1].RunningBalance); 
        }

    }
}
