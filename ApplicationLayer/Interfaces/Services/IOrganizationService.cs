using ApplicationLayer.DTOs.Organization;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IOrganizationService
    {
        Task<Organization> CreateOrganizationAsync(string name, Guid createdByUserId);
        Task<OrganizationReadDto?> GetOrganizationAsync(Guid organizationId);

        Task<decimal> GetOrganizationBalanceAsync(Guid organizationId);

        Task<string> GenerateApiKeyAsync(Guid organizationId);
    }

}
