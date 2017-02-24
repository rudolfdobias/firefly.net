using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Firefly.Helpers;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    
    public abstract class BaseFilter<TEntity, TProperty> where TEntity : class, IEntity
    {
        protected readonly Expression<Func<TEntity, TProperty>> Property;
        protected string Field;


        protected BaseFilter(Expression<Func<TEntity, TProperty>> property)
        {
            Property = property;
            Field = ExpressionHelper.GetPropertyInfo(property).Name;
        }

        protected BaseFilter(Expression<Func<TEntity, TProperty>> property, string key)
        {
            Property = property;
            Field = key;
        }

        public string GetPropertyName()
        {
            return Field;
        }

        protected List<string> GetUrlFilters(HttpContext context)
        {
            var field = (from f in context.Request.Query
                         where string.Equals(f.Key, Field, StringComparison.CurrentCultureIgnoreCase)
                         select f.Value.ToList()).FirstOrDefault();
            return field ?? new List<string>();
        }
    }

}