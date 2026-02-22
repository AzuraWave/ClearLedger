using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities.Base
{
    public class EntityBase<Tid> : IEntityBase<Tid> where Tid : notnull
    {
        public Tid Id { get; set; } = default!;
    
    }
}
