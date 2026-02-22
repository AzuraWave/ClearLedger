using ApplicationLayer.DTOs.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IUserService
    {
        
        Task<Guid> CreateClientUserAsync(CreateClientUserDto dto);

        
        Task ArchiveUserAsync(Guid userId);

        
        Task DeleteUserAsync(Guid userId);

    }
}
