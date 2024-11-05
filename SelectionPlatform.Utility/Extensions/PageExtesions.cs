using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Utility.Extensions
{
    public static class PageExtesions
    {
        public static PageList<TSource> ToPageList<TSource>(this IQueryable<TSource> sources, int pageNumber, int pageSize)
        {
            try
            {
                var count = sources.Count();
                var items = sources.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return new PageList<TSource>(items, count, pageNumber, pageSize);
            }
            catch (Exception ex)
            {

                throw;
            }
         
        }
    }
}
