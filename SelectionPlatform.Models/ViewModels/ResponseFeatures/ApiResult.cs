using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.ResponseFeatures
{
    public class ApiResult
    {
        public ApiResult() {

            code = 200;
            msg = "成功";
        }
        public int code { get; set; }
        public string msg { get; set; }
        public object data;
       

    }
}
