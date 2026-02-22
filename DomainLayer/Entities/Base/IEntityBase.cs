using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities.Base
{
    public interface IEntityBase<Tid>
    {
        Tid Id { get; set; }
    }
}
