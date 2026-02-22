using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationLayer.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork, ILogger<OrganizationService> logger)
        {
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<string> GenerateApiKeyAsync(Guid organizationId)
        {
            var apiKey = Guid.NewGuid().ToString("N");
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            var hashedApi = Convert.ToBase64String(hash);

            await _organizationRepository.AddApiKeyAsync(organizationId, hashedApi);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("API key generated for OrgId: {OrgId}, Time: {Time}", organizationId, DateTime.UtcNow);

            return apiKey;

        }

        public async Task<Organization> CreateOrganizationAsync(string name, Guid createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Organization name is required");

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
                UpdatedBy = createdByUserId,
                DefaultAutomationUserId = createdByUserId
            };

            await _organizationRepository.AddAsync(organization);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Organization created: {OrgId}, Name: {Name}, CreatedBy: {UserId}, Time: {Time}",
                organization.Id, name, createdByUserId, DateTime.UtcNow);

            return organization;
        }

        public async Task<OrganizationReadDto?> GetOrganizationAsync(Guid organizationId)
        {
            var org = await _organizationRepository.GetByIdAsync(organizationId);

            if (org == null)
            {
                _logger.LogDebug("GetOrganizationAsync: Organization not found: {OrgId}", organizationId);
                return null;
            }

            _logger.LogDebug("GetOrganizationAsync: Retrieved OrgId: {OrgId}, Name: {Name}", org.Id, org.Name);

            return new OrganizationReadDto
            {
                Id = org.Id,
                Name = org.Name
            };
        }

        public async Task<decimal> GetOrganizationBalanceAsync(Guid organizationId)
        {
            var balance = await _organizationRepository.GetOrganizationBalanceAsync(organizationId);
            _logger.LogDebug("GetOrganizationBalanceAsync: OrgId: {OrgId}, Balance: {Balance}", organizationId, balance);
            return balance;
        }
    }

}
