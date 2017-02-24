using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    public class StringFilter<TEntity> : BaseFilter<TEntity, string>, IFilter<TEntity> where TEntity : class, IEntity
    {
        private const string LikeDeterminant = "~";
        public StringFilter(Expression<Func<TEntity, string>> property) : base(property) { }
        public StringFilter(Expression<Func<TEntity, string>> property, string key) : base(property, key) { }

        public IEnumerable<Expression<Func<TEntity, bool>>> Build(HttpContext context)
        {
            var result = new List<Expression<Func<TEntity, bool>>>();

            foreach (var formula in GetUrlFilters(context))
            {
                if (string.IsNullOrEmpty(formula))
                {
                    return result;
                }

                if (formula.StartsWith(LikeDeterminant) || formula.EndsWith(LikeDeterminant))
                {
                    result.Add(IlikePredicate(formula));
                }
                else
                {
                    result.Add(InsensitiveEqualityPredicate(formula));
                }
            }

            return result;
        }

        private Expression<Func<TEntity, bool>> InsensitiveEqualityPredicate(string formula)
        {
            return Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    Expression.Call(Property.Body, "ToLower", Type.EmptyTypes),
                    Expression.Constant(formula.ToLower(), typeof(string))
                ),
                Property.Parameters
            );
        }

        private Expression<Func<TEntity, bool>> IlikePredicate(string formula)
        {
            string method;
            if (formula.StartsWith(LikeDeterminant) && formula.EndsWith(LikeDeterminant))
            {
                method = "Contains";
            }
            else if (formula.EndsWith(LikeDeterminant))
            {
                method = "StartsWith";
            }
            else
            {
                method = "EndsWith";
            }
            var filter = formula.Replace(LikeDeterminant, "").ToLower();

            return Expression.Lambda<Func<TEntity, bool>>(
                Expression.Call(
                    Expression.Call(Property.Body, "ToLower", Type.EmptyTypes),
                    typeof(string).GetMethods().FirstOrDefault(m => m.Name == method && m.GetParameters().ToList().Count == 1),
                    Expression.Constant(filter)
                    ),
                Property.Parameters);
        }




    }
}