using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace SelectionPlatform.Utility.Extensions
{
    public static class RepositoryExtesions
    {
        /// <summary>
        /// 根据字段 排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="sources"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static IQueryable<TSource> OrderByQuery<TSource>(this IQueryable<TSource> sources, string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return sources;
            }

            var orderParams = queryString.Trim().Split(',');
            var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder orderquerysb = new StringBuilder();
            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                {
                    continue;
                }

                var propertyFromQueryName = param.Split(" ")[0];
                var objProperty = propertyInfos.FirstOrDefault(p => p.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objProperty == null) continue;

                var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
                orderquerysb.Append($"{objProperty.Name} {sortingOrder},");
            }

            return sources.OrderBy(orderquerysb.ToString().TrimEnd(',', ' '));
        }


        /// <summary>
        /// 根据字段 排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="sources"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> OrderByQuery<TSource>(this IEnumerable<TSource> sources, string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return sources;
            }

            var orderParams = queryString.Trim().Split(',');
            var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder orderquerysb = new StringBuilder();
            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                {
                    continue;
                }

                var propertyFromQueryName = param.Split(" ")[0];
                var objProperty = propertyInfos.FirstOrDefault(p => p.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objProperty == null) continue;

                var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
                orderquerysb.Append($"{objProperty.Name} {sortingOrder},");
            }

            return sources.AsQueryable().OrderBy(orderquerysb.ToString().TrimEnd(',', ' '));
        }
    }
}
