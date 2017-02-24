using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Firefly.Extensions
{
    public enum SortDirection { Asc, Desc }
    public static class Linq
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string propertyName, SortDirection? sortDirection)
        {
            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            if (propertyInfo == null){
                throw new ArgumentException("Property \"" + propertyName + "\" does not exist.");
            }
            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new[] { arg });
            
            string overrideIdentifier = "OrderBy";
            if (sortDirection != null && sortDirection == SortDirection.Desc){
                overrideIdentifier = "OrderByDescending";
            }

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == overrideIdentifier && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     // 
                     return m.GetParameters().ToList().Count == 2;
                 })
                 .Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*
            Call query.OrderBy(selector), with query and selector: x=> x.PropName
            Note that we pass the selector as Expression to the method and we don't compile it.
            By doing so EF can extract "order by" columns and generate SQL for it.
            */

            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IQueryable<TSource> Include<TSource>(this IEnumerable<TSource> query, string navigationPropertyName)
        {
            var entityType = typeof(TSource);
            var propertyInfo = entityType.GetProperty(navigationPropertyName);
            if (propertyInfo == null){
                throw new ArgumentException("Navigation property \"" + navigationPropertyName + "\" does not exist.");
            }
            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, navigationPropertyName);
            var selector = Expression.Lambda(property, new[] { arg });
            
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "Include" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     return m.GetParameters().ToList().Count == 2;
                 })
                 .Single();
            
            MethodInfo genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);

            var newQuery = (IQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }
    }
}