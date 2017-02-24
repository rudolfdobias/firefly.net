using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models;
using Microsoft.AspNetCore.Http;

namespace Firefly.Repository.Filters
{
    public class DateFilter<TEntity> : BaseFilter<TEntity, DateTime?>, IFilter<TEntity> where TEntity : class, IEntity
    {
        public DateFilter(Expression<Func<TEntity, DateTime?>> property) : base(property)
        {
        }

        public DateFilter(Expression<Func<TEntity, DateTime?>> property, string key) : base(property, key)
        {
        }

        const string LesserThan = "<";
        const string LesserThanEqual = "<=";
        const string GreaterThan = ">";
        const string GreaterThanEqual = ">=";

        public IEnumerable<Expression<Func<TEntity, bool>>> Build(HttpContext context)
        {
            var result = new List<Expression<Func<TEntity, bool>>>();
            foreach (var formula in GetUrlFilters(context))
            {
                if (string.IsNullOrEmpty(formula))
                {
                    return result;
                }
                try
                {
                    string operation;
                    if (formula.Contains(new[] {LesserThanEqual, GreaterThanEqual, LesserThan, GreaterThan},
                        out operation))
                    {
                        result.Add(Comparsion(formula, operation));
                    }
                    else
                    {
                        DateTime? value = DateTime.Parse(formula);
                        result.Add(ExpressionHelper.EqualityPredicate(Property, value, typeof(DateTime?)));
                    }
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Cannot parse date: " + formula);
                }
            }
            return result;
        }


        private Expression<Func<TEntity, bool>> Comparsion(string formula, string operation)
        {
            Expression<Func<TEntity, bool>> predicate;
            formula = formula.Substring(operation.Length);
            DateTime? value = DateTime.Parse(formula);
            switch (operation)
            {
                case LesserThan:
                    predicate = ExpressionHelper.LessPredicate(Property, value, typeof(DateTime?));
                    break;
                case LesserThanEqual:
                    predicate = ExpressionHelper.LessOrEqualPredicate(Property, value, typeof(DateTime?));
                    break;
                case GreaterThan:
                    predicate = ExpressionHelper.GreaterPredicate(Property, value, typeof(DateTime?));
                    break;
                case GreaterThanEqual:
                    predicate = ExpressionHelper.GreaterOrEqualPredicate(Property, value, typeof(DateTime?));
                    break;
                default:
                    throw new ArgumentException("Unknown operation " + operation);
            }

            return predicate;
        }
    }
}