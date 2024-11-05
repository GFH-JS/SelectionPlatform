using MongoDB.Bson;
using SelectionPlatform.Models.Models;
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
    public class ProjectInfoDto
    {
        public ObjectId id { get; set; }
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string projectVersion { get; set; }
        public DateTime createTime { get; set; }
    }

    public class AddProjectDto
    {
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string projectVersion { get; set; }
       
    }

    public class ProjectInputDto
    {
        public string projectId { get; set; }
        public string? projectName { get; set; }
        public string? projectVersion { get; set; }
        public string metricInch { get; set; }
        public string? triggerSource { get; set; }
        public int modelType { get; set; } = 1;  //0模块机 1 离心机
        public ProofData proofData { get; set; } = new ProofData();
    }

    public class UpdateProjectDto
    {
        public string ProjectName { get; set; }
        //public string ProjectVersion { get; set; }
    }




    public class LoadPointInputDto
    {
        public string projectId { get; set; }
        public string? projectName { get; set; }
        public string? projectVersion { get; set; }
        public string metricInch { get; set; }
        public string? triggerSource { get; set; }
        public ProofData proofData { get; set; } = new ProofData();
    }
   
}
