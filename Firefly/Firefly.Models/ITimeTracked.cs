using System;

namespace Firefly.Models
{
    public interface ITimeTracked
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}