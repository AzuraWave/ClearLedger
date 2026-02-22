using ApplicationLayer.DTOs.Client;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IClientService
    {
        Task<Guid> CreateClientAsync(Guid organizationId, ClientCreateDto dto, Guid createdByUserId);

        Task<ClientReadDto?> GetClientAsync(Guid clientId, Guid organizationId, bool getArchived = false);

        Task<IEnumerable<ClientReadDto>> GetClientsByOrganizationAsync(Guid organizationId, bool getArchived = false);

        Task<int> GetClientsTotal(Guid organizationId, bool getArchived = false);
        Task<ClientReadDto> GetClientByProject(Guid projectId, Guid organizationId, bool getArchived = false);

        Task ArchiveClientAsync(Guid clientId, Guid organizationId);
        Task UpdateClientAsync(Guid organizationId, ClientUpdateDto dto, Guid modifiedByUserId);

        Task DeleteClientAsync(Guid organizationID, Guid clientId);
    }

}
