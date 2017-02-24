using System;

namespace Firefly.Models
{
    public interface IEntity
    {
        Guid Id { get; set; }
        object this[string propertyName] { get; set; }
    }
}