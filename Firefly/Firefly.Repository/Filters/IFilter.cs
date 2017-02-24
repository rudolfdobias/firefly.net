using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    public interface IFilter<T> where T : class, IEntity
    {
        IEnumerable<Expression<Func<T, bool>>> Build(HttpContext context);
        string GetPropertyName();
    }
}