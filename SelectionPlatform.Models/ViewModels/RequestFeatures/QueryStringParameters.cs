using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.RequestFeatures
{
    public abstract class QueryStringParameters
    {
        private const int maxPageSize = 100;
        public int pageNmuber { get; set; } = 1;
        private int _pageSize =10;

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }

        public string? OrderBy { get; set; }

        public string? Fields { get; set; }
    }
}
