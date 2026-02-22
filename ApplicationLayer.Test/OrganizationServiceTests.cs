using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Test
{
    public class OrganizationServiceTests
    {
        private readonly Mock<IOrganizationRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _unitMock;
        private readonly Mock<ILogger<OrganizationService>> _loggerMock;
        private readonly OrganizationService _service;

        public OrganizationServiceTests()
        {
            _repoMock = new Mock<IOrganizationRepository>();
            _unitMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrganizationService>>();
            _service = new OrganizationService(_repoMock.Object, _unitMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateOrganizationAsync_Should_Create_And_Return_Organization()
        {
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Organization>()))
                     .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            var result = await _service.CreateOrganizationAsync("TestOrg", userId);

            Assert.Equal("TestOrg", result.Name);
            Assert.Equal(userId, result.CreatedBy);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Organization>()), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateOrganizationAsync_Should_Throw_When_Name_Invalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrganizationAsync("", Guid.NewGuid()));
        }

        [Fact]
        public async Task GetOrganizationAsync_Returns_Dto_When_Found()
        {
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org1" };
            _repoMock.Setup(r => r.GetByIdAsync(org.Id)).ReturnsAsync(org);

            var result = await _service.GetOrganizationAsync(org.Id);

            Assert.NotNull(result);
            Assert.Equal(org.Name, result!.Name);
        }

        [Fact]
        public async Task GetOrganizationAsync_Returns_Null_When_NotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Organization?)null);

            var result = await _service.GetOrganizationAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GenerateApiKeyAsync_Should_Call_Repository_And_Save()
        {
            var orgId = Guid.NewGuid();

            _repoMock.Setup(r => r.AddApiKeyAsync(orgId, It.IsAny<string>()))
                     .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            var apiKey = await _service.GenerateApiKeyAsync(orgId);

            Assert.False(string.IsNullOrWhiteSpace(apiKey));
            _repoMock.Verify(r => r.AddApiKeyAsync(orgId, It.IsAny<string>()), Times.Once);
            _unitMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrganizationBalanceAsync_Returns_Balance()
        {
            var orgId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetOrganizationBalanceAsync(orgId))
                     .ReturnsAsync(150m);

            var result = await _service.GetOrganizationBalanceAsync(orgId);

            Assert.Equal(150m, result);
        }
    }
}
