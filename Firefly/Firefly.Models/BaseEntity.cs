using System;
using System.Reflection;

namespace Firefly.Models
{
    public abstract class BaseEntity : IEntity, ITimeTracked
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

        public object this[string propertyName]
        {
            get
            {
                var property = GetType().GetProperty(propertyName);
                return property.GetValue(this, null);
            }
            set
            {
                var property = GetType().GetProperty(propertyName);
                property.SetValue(this, value, null);
            }
        }

        public PropertyInfo[] GetProperties()
        {
            return GetType().GetProperties();
        }
    }
}