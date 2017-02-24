using System;
using System.Linq.Expressions;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Firefly.Repository.Filters
{
    public class EnumFilter<TEntity, TEnum> : BaseFilter<TEntity, TEnum>, IFilter<TEntity>
        where TEntity : class, IEntity
    {
        public EnumFilter(Expression<Func<TEntity, TEnum>> property) : base(property)
        {
        }

        public EnumFilter(Expression<Func<TEntity, TEnum>> property, string key) : base(property, key)
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

                result.Add(EqualityPredicate(formula));
            }
            return result;
        }

        private Expression<Func<TEntity, bool>> EqualityPredicate(string formula)
        {
            TEnum filter;
            try
            {
                filter = formula.AsEnum<TEnum>();
            }
            catch (Exception crap)
            {
                throw new ArgumentException(crap.Message);
            }
            return ExpressionHelper.EqualityPredicate(Property, filter, typeof(TEnum));
        }
    }
}