using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.DTO
{
    /// <summary>
    /// 数据库到前端
    /// </summary>
    public class ProjectbaseInfoDto
    {
        public string ID { get; set; }
        public string PROJECT_NAME { get; set; }
        public int CITY { get; set; }
        public int COUNTRY { get; set; }
        public string CITY_NO { get; set; }
        public DateTime CREATE_DATE { get; set; } = DateTime.Now;
        public int CREATE_BY { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public string REMARK { get; set; }
        public string city_name { get; set; }
        public int province { get; set; }
        public string type { get; set; } = "1";   //1 离心机  2螺杆机
        public string PROJECT_NO { get; set; }
        public List<string>? Versions { get; set; }
    }

    /// <summary>
    /// 前端到数据库
    /// </summary>
    public class AddProjectbaseInfoDto
    {
        public string PROJECT_NAME { get; set; }
        public int CITY { get; set; }   //城市
        public int COUNTRY { get; set; } //国家
        public string CITY_NO { get; set; }
        public int CREATE_BY { get; set; }
        public string REMARK { get; set; }
        public string city_name { get; set; }
        public int province { get; set; }  //省会
        public string type { get; set; } = "1";   //1 离心机  2螺杆机
        public string PROJECT_NO { get; set; }
    }

    /// <summary>
    /// 前端到数据库
    /// </summary>
    public class UpdateProjectbaseInfoDto
    {
        public string PROJECT_NAME { get; set; }
        public int CITY { get; set; }
        public int COUNTRY { get; set; }
        public string CITY_NO { get; set; }
        public int CREATE_BY { get; set; }
        public string REMARK { get; set; }
        public string city_name { get; set; }
        public int province { get; set; }
        public string type { get; set; } = "1";   //1 离心机  2螺杆机
        public string PROJECT_NO { get; set; }
    }
}
