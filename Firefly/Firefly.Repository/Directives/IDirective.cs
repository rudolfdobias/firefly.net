using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Firefly.Models.Directives
{
    public interface IDirective<TEntity>
    {
        IQueryable<TEntity> Apply(IQueryable<TEntity> builder, HttpContext context);
    }
}