using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Firefly.Models
{
    public class PropertyScope<TEntity> : IResourceScope<TEntity> where TEntity : IEntity
    {
        public delegate TEntity InsertFilter(TEntity entity, HttpContext context);
        public delegate IQueryable<TEntity> QueryFilter(IQueryable<TEntity> query, HttpContext context);
        private readonly InsertFilter _insertFilter;
        private readonly QueryFilter _queryFilter;

        public PropertyScope(QueryFilter queryFilter, InsertFilter insertFilter)
        {
            _insertFilter = insertFilter;
            _queryFilter = queryFilter;
        }

        public TEntity EntityCreating(TEntity entity, HttpContext context)
        {
            entity = _insertFilter(entity, context);
            return entity;
        }

        public IQueryable<TEntity> QueryCreating(IQueryable<TEntity> builder, HttpContext context)
        {
            return _queryFilter(builder, context);
        }
    }
}