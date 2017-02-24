using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Firefly.Helpers;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    public class GuidFilter<TEntity> : BaseFilter<TEntity, Guid>, IFilter<TEntity> where TEntity : class, IEntity
    {
        public GuidFilter(Expression<Func<TEntity, Guid>> property) : base(property)
        {
        }

        public GuidFilter(Expression<Func<TEntity, Guid>> property, string key) : base(property, key)
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

                if (formula.Contains(","))
                {
                    result.Add(GetMany(formula));
                }
                else
                {
                    result.Add(GetEquality(formula));
                }
            }

            return result;
        }

        private Expression<Func<TEntity, bool>> GetEquality(string formula)
        {
            Guid id;
            try
            {
                id = new Guid(formula.Trim());
            }
            catch (Exception crap)
            {
                throw new ArgumentException("Guid parse error: " + crap.Message);
            }
            return ExpressionHelper.EqualityPredicate(Property, id, typeof(Guid));
        }

        private Expression<Func<TEntity, bool>> GetMany(string formula)
        {
            List<Guid> ids;
            try
            {
                var split = formula.Split(',');
                ids = (from s in split select new Guid(s.Trim())).ToList();
            }
            catch (Exception crap)
            {
                throw new ArgumentException("Guid parse error: " + crap.Message);
            }

            return ExpressionHelper.CollectionContainsPredicate(Property, ids);
        }
    }
}