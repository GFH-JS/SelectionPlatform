using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.ResponseFeatures
{

    public class PageMetaData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }


    public class ProjectsResposne
    {
        public ProjectsResposne()
        {
            code = 200;
            msg = "成功";
        }
        public object list;
        public PageMetaData page;
        public int code { get; set; }
        public string msg { get; set; }

    }
}
