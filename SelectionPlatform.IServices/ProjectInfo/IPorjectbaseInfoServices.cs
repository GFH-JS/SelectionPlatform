using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IServices.ProjectInfo
{
    public interface IProjectbaseInfoServices:IBaseServices<ProjectbaseinfoEntity>
    {
        PageList<ProjectbaseinfoEntity> GetProjects(ProjectQueryParameters projectQueryParameters);
    }
}
