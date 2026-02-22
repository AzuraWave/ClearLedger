using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities.Base
{
    public class TrackedEntity: Entity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
    }
}
