using System;
using System.Linq;
using System.Linq.Expressions;
using Firefly.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Firefly.Models.Directives
{
    public class IncludeDirective<TEntity> : IDirective<TEntity> where TEntity : class, IEntity
    {
        private const string IncludeField = "with";
        private readonly Expression<Func<TEntity, object>> _property;
        private readonly string _fieldName;

        public IncludeDirective(Expression<Func<TEntity, object>> navigationProperty)
        {
            _property = navigationProperty;
            _fieldName = ExpressionHelper.GetPropertyInfo(_property).Name;
        }

        public IncludeDirective(Expression<Func<TEntity, object>> navigationProperty, string key)
        {
            _property = navigationProperty;
            _fieldName = key;
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> builder, HttpContext context)
        {
            return IsPresent(context) ? builder.Include(_property) : builder;
        }

        private bool IsPresent(HttpContext context)
        {
            var field = (from f in context.Request.Query
                where string.Equals(f.Key, IncludeField, StringComparison.CurrentCultureIgnoreCase)
                where f.Value.ToString().ToLower().Split(',').Contains(_fieldName.ToLower())
                select f.Value.ToString()).FirstOrDefault();
            return field != null;
        }
    }
}