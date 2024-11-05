using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.ResponseFeatures
{
    public class PageList<T>:List<T>
    {
        public PageMetaData MetaData { get; set; }
        public PageList(IEnumerable<T> items,int totalCount,int currentPage,int pageSize) { 
          MetaData = new PageMetaData { 
          
              TotalCount = totalCount,
              PageSize = pageSize,
              CurrentPage = currentPage,
              TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
          };
            AddRange(items);
        }
    }
}
