using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ApplicationLayer.Test
{
    public class ClientServiceTests
    {
        private readonly Mock<IClientRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<ILogger<ClientService>> _loggerMock;
        private readonly ClientService _service;

        public ClientServiceTests()
        {
            _repoMock = new Mock<IClientRepository>();
            _unitMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ClientService>>();
            _service = new ClientService(_unitMock.Object, _repoMock.Object, _loggerMock.Object);
        }

        private Client CreateDefaultClient(
        Guid? id = null,
        string? name = null,
        bool withProjects = false,
        bool withPayments = false)
        {
            var orgid = Guid.NewGuid();

            var client = new Client
            {
                Id = id ?? Guid.NewGuid(),
                Name = name ?? "Test Client",
                Status = ClientStatus.Active,
                Organization = new Organization
                {
                    Id = orgid,
                    Name = "Org1"
                },
                OrganizationId = orgid,
                BillingEmail = "test@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Test St",
                Notes = "Sample notes",
                Balance = 0m,
                Projects = withProjects ? new List<Project> { new Project() } : new List<Project>(),
                PaymentHeaders = withPayments ? new List<ClientPaymentHeader> { new ClientPaymentHeader() } : new List<ClientPaymentHeader>(),
                Invoices = new List<Invoice>(),
                Adjustments = new List<Adjustment>(),
                Discounts = new List<Discount>(),
                invoiceNumberCounters = new List<InvoiceNumberCounter>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            };

            return client;
        }

        [Fact]
        public async Task CreateClientAsync_CreatesClientAndReturnsId()
        {
            var dto = new ClientCreateDto { Name = "Test Client" };
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var clientId = await _service.CreateClientAsync(orgId, dto, userId);

            Assert.NotEqual(Guid.Empty, clientId);
            _repoMock.Verify(r => r.AddAsync(It.Is<Client>(c => c.Name == dto.Name && c.OrganizationId == orgId)), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetClientAsync_ReturnsDto_WhenClientExists()
        {
            var client = new Client { Id = Guid.NewGuid(), Name = "Client1" };
            _repoMock.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<Guid>(), false)).ReturnsAsync(client);

            var dto = await _service.GetClientAsync(client.Id, Guid.NewGuid());

            Assert.NotNull(dto);
            Assert.Equal(client.Name, dto!.Name);
        }

        [Fact]
        public async Task GetClientAsync_ReturnsNull_WhenClientNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).ReturnsAsync((Client?)null);

            var dto = await _service.GetClientAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task UpdateClientAsync_UpdatesExistingClient()
        {
            var client = new Client { Id = Guid.NewGuid(), Name = "Old" };
            var dto = new ClientUpdateDto { Id = client.Id, Name = "Updated", Status = ClientStatus.Active };
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(client.Id, orgId)).ReturnsAsync(client);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.UpdateClientAsync(orgId, dto, userId);

            Assert.Equal("Updated", client.Name);
            _repoMock.Verify(r => r.UpdateAsync(client), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ArchiveClientAsync_ArchivesClient_WhenExists()
        {
            var client = CreateDefaultClient(Guid.NewGuid(), "ArchiveClient");
            _repoMock.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<Guid>(), false)).ReturnsAsync(client);
            _repoMock.Setup(r => r.ArchiveAsync(client)).Callback<Client>(c => c.Status = ClientStatus.Archived).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.ArchiveClientAsync(client.Id, client.OrganizationId);

            Assert.Equal(ClientStatus.Archived, client.Status);

            _repoMock.Verify(r => r.ArchiveAsync(client), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteClientAsync_Deletes_WhenNoProjectsOrLedger()
        {
            var client = CreateDefaultClient(Guid.NewGuid(), "DeleteClient");
            _repoMock.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<Guid>())).ReturnsAsync(client);
            _repoMock.Setup(r => r.DeleteAsync(client)).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteClientUserAsync(client.Id)).Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.DeleteClientAsync(client.OrganizationId, client.Id);

            
            _repoMock.Verify(r => r.DeleteAsync(client), Times.Once);
            _repoMock.Verify(r => r.DeleteClientUserAsync(client.Id), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
    }
}
