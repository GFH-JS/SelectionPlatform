using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.Models
{
    public class ProjectbaseinfoEntity
    {
        public string ID { get; set; } = Guid.NewGuid().ToString("N");
        public string? PROJECT_NAME { get; set; }
        public int? CITY { get; set; }
        public int? COUNTRY { get; set; }
        public string? CITY_NO { get; set; }
        public DateTime CREATE_DATE { get; set; } = DateTime.Now;
        public int? CREATE_BY { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public string? REMARK { get; set; }
        public string? city_name { get; set; }
        public int? province { get; set; }
        public string? type { get; set; } = "1";   //1 离心机  2螺杆机
        public string? PROJECT_NO { get; set; }
        public string? DEL_FLAG { get; set; } = "0";

    }
}
