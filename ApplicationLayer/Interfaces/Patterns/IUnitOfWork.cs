using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Patterns
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
