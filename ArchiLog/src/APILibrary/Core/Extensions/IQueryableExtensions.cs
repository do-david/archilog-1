using APILibrary.Core.Attributes;
using APILibrary.Core.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace APILibrary.Core.Extensions
{
    public static class IQueryableExtensions
    {
        public static object SelectObject(object value, string[] fields)
        {
            var expo = new ExpandoObject() as IDictionary<string, object>;
            var valueType = value.GetType();

            foreach (var field in fields)
            {
                var prop = valueType.GetProperty(field, BindingFlags.Public |
                    BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (prop != null)
                {
                    var isPresentAttribute = prop.CustomAttributes
                         .Any(x => x.AttributeType == typeof(NotJsonAttribute));
                    if (!isPresentAttribute)
                      expo.Add(prop.Name, prop.GetValue(value));
                }
                else
                {
                    throw new Exception($"Property {field} does not exist.");
                }
            }
            return expo;
        }
        //OrderByTAsc(IQuerible<TModel>, string[] asc)
        public static IQueryable<TModel> OrderByAscOrDesc<TModel>(this IQueryable<TModel> query, string asc, string desc) where TModel : ModelBase
        {
            if (string.IsNullOrWhiteSpace(desc)) 
            { 
                var propInfo = typeof(TModel).GetProperty(asc, BindingFlags.Public |
                    BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (propInfo is null)
                    throw new InvalidOperationException("Please provide a valid property name");
                else
                {
                    var keySelector = GetExpression<TModel>(propInfo.Name);
                    query = query.OrderBy(keySelector);
                }
            }
            else if (string.IsNullOrWhiteSpace(asc))
            {
                var propInfo = typeof(TModel).GetProperty(desc, BindingFlags.Public |
                   BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (propInfo is null)
                    throw new InvalidOperationException("Please provide a valid property name");
                else
                {
                    var keySelector = GetExpression<TModel>(propInfo.Name);
                    query = query.OrderByDescending(keySelector);
                }
            }
            else
            {
                var propInfoAsc = typeof(TModel).GetProperty(asc, BindingFlags.Public |
                  BindingFlags.IgnoreCase | BindingFlags.Instance);
                var propInfoDesc = typeof(TModel).GetProperty(desc, BindingFlags.Public |
                  BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (propInfoAsc is null || propInfoDesc is null)
                    throw new InvalidOperationException("Please provide a valid property name");
                else
                {
                    var keySelector1 = GetExpression<TModel>(propInfoAsc.Name);
                    var keySelector2 = GetExpression<TModel>(propInfoDesc.Name);
                    query = query.OrderBy(keySelector1).ThenByDescending(keySelector2);
                }
            }
            return query;
        }
        public static Expression<Func<TModel, object>> GetExpression<TModel>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            Expression conversion = Expression.Convert(Expression.Property
            (param, propertyName), typeof(object));   //important to use the Expression.Convert
            return Expression.Lambda<Func<TModel, object>>(conversion, param);
        }
        public static IQueryable<dynamic> SelectDynamic<TModel>(this IQueryable<TModel> query, string[] fields) where TModel : ModelBase
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            var membersExpression = fields.Select(y => Expression.Property(parameter, y));

            var membersAssignment = membersExpression.Select(z => Expression.Bind(z.Member, z));

            var body = Expression.MemberInit(Expression.New(typeof(TModel)), membersAssignment);

            var lambda = Expression.Lambda<Func<TModel, dynamic>>(body, parameter);

            return query.Select(lambda);
        }

        public static IQueryable<TModel> SelectModel<TModel>(this IQueryable<TModel> query, string[] fields) where TModel : ModelBase
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");

            var membersExpression = fields.Select(y => Expression.Property(parameter, y));

            var membersAssignment = membersExpression.Select(z => Expression.Bind(z.Member, z));

            var body = Expression.MemberInit(Expression.New(typeof(TModel)), membersAssignment);

            var lambda = Expression.Lambda<Func<TModel, TModel>>(body, parameter);

            return query.Select(lambda);
        }

        
    }
}
