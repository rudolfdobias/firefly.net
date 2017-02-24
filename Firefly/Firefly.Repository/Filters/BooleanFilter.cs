using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Firefly.Helpers;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    public class BooleanFilter<TEntity> : BaseFilter<TEntity, bool>, IFilter<TEntity> where TEntity : class, IEntity
    {
        public BooleanFilter(Expression<Func<TEntity, bool>> property) : base(property)
        {
        }

        public BooleanFilter(Expression<Func<TEntity, bool>> property, string key) : base(property, key)
        {
        }

        public IEnumerable<Expression<Func<TEntity, bool>>> Build(HttpContext context)
        {
            var result = new List<Expression<Func<TEntity, bool>>>();
            foreach (var formula in GetUrlFilters(context))
            {
                if (string.IsNullOrEmpty(formula))
                {
                    return result;
                }
                var value = bool.Parse(formula);
                result.Add(ExpressionHelper.EqualityPredicate(Property, value, typeof(bool)));
            }
            return result;
        }
    }
}