using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities.Base
{
    public class EditableTrackedEntity : TrackedEntity
    {
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
