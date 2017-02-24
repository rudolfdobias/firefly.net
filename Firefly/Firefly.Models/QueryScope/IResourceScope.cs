using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Firefly.Models{
    public interface IResourceScope<T> where T:IEntity{
        IQueryable<T> QueryCreating(IQueryable<T> builder, HttpContext context);
        T EntityCreating(T entity, HttpContext context);
    }
}