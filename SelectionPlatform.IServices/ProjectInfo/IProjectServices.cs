using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IServices.ProjectInfo
{
    public interface IProjectServices :IMongoBaseServices<ProjectInfoEntity>
    {
        Task<PageList<ProjectInfoEntity>> GetProjects(ProjectQueryParameters projectQueryParameters);

        Task<List<ProjectInfoEntity>> GetProjectsByProjectId(string proid);

        Task<ProjectInfoEntity> GetProjectsByIdAndVersion(string proid,string version);

        Task UpdateProjectInfo(string proid, UpdateProjectDto updateProjectDto);

        Task<List<string>> GetProjectAllVersionById(string proid);
    }
}
