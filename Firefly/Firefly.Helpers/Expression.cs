using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Firefly.Helpers
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

    public class ExpressionHelper
    {
        public static PropertyInfo GetPropertyInfo(Expression expression)
        {
            MemberExpression memberExpression;
            var lambda = (LambdaExpression) expression;
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = (MemberExpression) ((UnaryExpression) lambda.Body).Operand;
            }
            else if (lambda.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new InvalidOperationException("Expression must be a MemberExpression or Convert.");
            }
            else
            {
                memberExpression = (MemberExpression) lambda.Body;
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("Expression must be a property reference.");
            }
            return propertyInfo;
        }

        public static Expression<Func<T, bool>> EqualityPredicate<T, TConstant>(Expression<Func<T, TConstant>> member,
            TConstant constant, Type constantType)
        {
            var parameter = Expression.Parameter(typeof(T));
            var comparsion = Expression.Equal(
                Expression.Invoke(member, parameter),
                Expression.Constant(constant, constantType)
            );
            return Expression.Lambda<Func<T, bool>>(comparsion, parameter);
        }

        public static Expression<Func<T, bool>> GreaterPredicate<T, TConstant>(Expression<Func<T, TConstant>> member,
            TConstant constant, Type constantType)
        {
            var parameter = Expression.Parameter(typeof(T));
            var comparsion = Expression.GreaterThan(
                Expression.Invoke(member, parameter),
                Expression.Constant(constant, constantType)
            );
            return Expression.Lambda<Func<T, bool>>(comparsion, parameter);
        }

        public static Expression<Func<T, bool>> GreaterOrEqualPredicate<T, TConstant>(
            Expression<Func<T, TConstant>> member, TConstant constant, Type constantType)
        {
            var parameter = Expression.Parameter(typeof(T));
            var comparsion = Expression.GreaterThanOrEqual(
                Expression.Invoke(member, parameter),
                Expression.Constant(constant, constantType)
            );
            return Expression.Lambda<Func<T, bool>>(comparsion, parameter);
        }

        public static Expression<Func<T, bool>> LessPredicate<T, TConstant>(Expression<Func<T, TConstant>> member,
            TConstant constant, Type constantType)
        {
            var parameter = Expression.Parameter(typeof(T));
            var comparsion = Expression.LessThan(
                Expression.Invoke(member, parameter),
                Expression.Constant(constant, constantType)
            );
            return Expression.Lambda<Func<T, bool>>(comparsion, parameter);
        }

        public static Expression<Func<T, bool>> LessOrEqualPredicate<T, TConstant>(
            Expression<Func<T, TConstant>> member, TConstant constant, Type constantType)
        {
            var parameter = Expression.Parameter(typeof(T));
            var comparsion = Expression.LessThanOrEqual(
                Expression.Invoke(member, parameter),
                Expression.Constant(constant, constantType)
            );
            return Expression.Lambda<Func<T, bool>>(comparsion, parameter);
        }

        public static Expression<Func<T, bool>> CollectionContainsPredicate<T, TConstant>(
            Expression<Func<T, TConstant>> member, ICollection<TConstant> constant)
        {
            var parameter = Expression.Parameter(typeof(T));
            var method = typeof(ICollection<TConstant>).GetMethod("Contains", new[] {typeof(TConstant)});
            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    Expression.Constant(constant, typeof(IList<TConstant>)),
                    method,
                    Expression.Invoke(member, parameter)
                ), parameter
            );
        }
    }
}