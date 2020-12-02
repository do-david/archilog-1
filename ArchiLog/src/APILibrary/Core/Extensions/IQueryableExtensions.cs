using APILibrary.Core.Attributes;
using APILibrary.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace APILibrary.Core.Extensions
{
    public static class IQueryableExtensions
    {
        public enum OperationExpression
        {
            Equals,
            NotEquals,
            Minor,
            MinorEquals,
            Mayor,
            MayorEquals,
            Like,
            Contains,
            Any
        }
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
        //SearchByName(IQueyable<TModel> query, string name)
        public static IQueryable<TModel> SearchByName<TModel>(this IQueryable<TModel> query, string name) where TModel : ModelBase
        {
            var propsInfo = typeof(TModel).GetProperties();
            var result = query;
            foreach (var prop in propsInfo)
            {
                if(prop.PropertyType == typeof(string))
                {
                    var predicate = GetCriteriaWhere<TModel>(prop.Name, OperationExpression.Contains, name);
                    if(query.Where(predicate).Count() > 0)
                    {
                        result = result.Where(predicate);
                    }
                }
            }
            return result;
        }
        //FilterCustomized(IQuerible<TModel>, string type)
        public static IQueryable<TModel> FilterCustomized<TModel>(this IQueryable<TModel> query,string[] fieldNames,string type, string rating, string date) where TModel : ModelBase
        {
            Regex r = new Regex(@"\[\b\d\,\d\b\]");
            Regex rs = new Regex(@"\[\b\d\,\]");
            Regex re = new Regex(@"\[\,\d\b\]");
            Regex d = new Regex(@"\[\b\d{0,4}\-\d{0,2}\-\d{0,2}\,\d{0,4}\-\d{0,2}\-\d{0,2}\b\]");
            Regex ds = new Regex(@"\[\b\d{0,4}\-\d{0,2}\-\d{0,2}\b\,\]");
            Regex de = new Regex(@"\[\,\b\d{0,4}\-\d{0,2}\-\d{0,2}\b\]");
            foreach (var fielName in fieldNames)
            {
                if(fielName == "Type" && !string.IsNullOrWhiteSpace(type))
                {
                    var propInfo = typeof(TModel).GetProperty("Type", BindingFlags.Public |
                   BindingFlags.IgnoreCase | BindingFlags.Instance);
                    var fieldName = propInfo.Name;
                    var predicate = GetCriteriaWhere<TModel>(fieldName, OperationExpression.Equals, type);
                    query = query.Where(predicate);
                }
                if (fielName == "Rating" &&!string.IsNullOrWhiteSpace(rating))
                {
                    var propInfo = typeof(TModel).GetProperty("Rating", BindingFlags.Public |
                        BindingFlags.IgnoreCase | BindingFlags.Instance);
                    var fieldName = propInfo.Name;
                    if (r.IsMatch(rating))
                    {
                        var start = rating[1].ToString();
                        var end = rating[rating.Length - 2].ToString();
                        var predicateStart = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MayorEquals,Convert.ToDecimal(start));
                        var predicateEnd = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MinorEquals, Convert.ToDecimal(end));
                        query = query.Where(predicateStart).Where(predicateEnd);
                    }
                    else if (rs.IsMatch(rating))
                    {
                        var start = rating[1].ToString();
                        var predicateStart = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MayorEquals, Convert.ToDecimal(start));
                        query = query.Where(predicateStart);
                    }
                    else if (re.IsMatch(rating))
                    {
                        var end = rating[rating.Length - 2].ToString();
                        var predicateEnd = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MinorEquals, Convert.ToDecimal(end));
                        query = query.Where(predicateEnd);
                    }
                    else
                    {
                        var predicate = GetCriteriaWhere<TModel>(fieldName, OperationExpression.Equals, Convert.ToDecimal(rating));
                        query = query.Where(predicate);
                    }
                    
                }
                if (fielName == "Date" && !string.IsNullOrWhiteSpace(date))
                {
                    var propInfo = typeof(TModel).GetProperty("CreateAt", BindingFlags.Public |
                   BindingFlags.IgnoreCase | BindingFlags.Instance);
                    var fieldName = propInfo.Name;
                    if (d.IsMatch(date))
                    {
                        var start = date.Substring(1, 10);
                        var end = date.Substring(12,10);
                        var predicateStart = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MayorEquals, Convert.ToDateTime(start));
                        var predicateEnd = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MinorEquals, Convert.ToDateTime(end));
                        query = query.Where(predicateStart).Where(predicateEnd);
                    }
                    else if (ds.IsMatch(date))
                    {
                        var start = date.Substring(1, 10);
                        var predicateStart = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MayorEquals, Convert.ToDateTime(start));
                        query = query.Where(predicateStart);
                    }
                    else if (de.IsMatch(date))
                    {
                        var end = date.Substring(2, 10);
                        var predicateEnd = GetCriteriaWhere<TModel>(fieldName, OperationExpression.MinorEquals, Convert.ToDateTime(end));
                        query = query.Where(predicateEnd);
                    }
                    else
                    {
                        var predicate = GetCriteriaWhere<TModel>(fieldName, OperationExpression.Equals, Convert.ToDateTime(date));
                        query = query.Where(predicate);
                    }
                }
            }
            return query;
        }
        //OrderByTAsc(IQuerible<TModel>, string asc, string desc)
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
        public static Expression<Func<TModel,bool>> GetCriteriaWhere<TModel>(string fieldName, OperationExpression selectedOperator, object fieldValue) where TModel:ModelBase
        {

            var propInfo = typeof(TModel).GetProperty(fieldName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

            var parameter = Expression.Parameter(typeof(TModel), "x");
            var expressionParameter = GetMemberExpression<TModel>(parameter, fieldName);
            if (propInfo != null && fieldValue != null)
            {

                BinaryExpression body = null;

                switch (selectedOperator)
                {
                    case OperationExpression.Equals:
                        body = Expression.Equal(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.NotEquals:
                        body = Expression.NotEqual(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.Minor:
                        body = Expression.LessThan(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.MinorEquals:
                        body = Expression.LessThanOrEqual(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.Mayor:
                        body = Expression.GreaterThan(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.MayorEquals:
                        body = Expression.GreaterThanOrEqual(expressionParameter, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(body, parameter);
                    case OperationExpression.Like:
                        MethodInfo contains = typeof(string).GetMethod("Contains");
                        var bodyLike = Expression.Call(expressionParameter, contains, Expression.Constant(fieldValue, propInfo.PropertyType));
                        return Expression.Lambda<Func<TModel, bool>>(bodyLike, parameter);
                    case OperationExpression.Contains:
                        return Contains<TModel>(fieldName,fieldValue, parameter, expressionParameter);
                    default:
                        throw new Exception("Not implement Operation");
                }
            }
            else
            {
                Expression<Func<TModel, bool>> filter = x => true;
                return filter;
            }
        }

        private static MemberExpression GetMemberExpression<TModel>(ParameterExpression parameter, string propName) where TModel : ModelBase
        {
            if (string.IsNullOrEmpty(propName)) return null;
            else
            {
                return Expression.Property(parameter, propName);
            }
            /*var propertiesName = propName.Split('.');
            if (propertiesName.Count() == 2)
                return Expression.Property(Expression.Property(parameter, propertiesName[0]), propertiesName[1]);*/
        }
        public static Expression<Func<TModel, object>> GetExpression<TModel>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            Expression conversion = Expression.Convert(Expression.Property
            (param, propertyName), typeof(object));   //important to use the Expression.Convert
            return Expression.Lambda<Func<TModel, object>>(conversion, param);
        }
        //à modifier
        private static Expression<Func<TModel, bool>> Contains<TModel>(string fieldName, object fieldValue, ParameterExpression parameterExpression, MemberExpression memberExpression) where TModel:ModelBase
        {
            var propertyExp = Expression.Property(parameterExpression, fieldName);
            if (propertyExp.Type == typeof(string))
            {
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var someValue = Expression.Constant(fieldValue, typeof(string));
                var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                return Expression.Lambda<Func<TModel, bool>>(containsMethodExp, parameterExpression);
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(propertyExp.Type);
                var result = converter.ConvertFrom(fieldValue);
                var someValue = Expression.Constant(result);
                var containsMethodExp = Expression.Equal(propertyExp, someValue);
                return Expression.Lambda<Func<TModel, bool>>(containsMethodExp, parameterExpression);
            }
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
