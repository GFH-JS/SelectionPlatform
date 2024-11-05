using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IServices.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using SelectionPlatform.Repository.ProjectInfo;
using SelectionPlatform.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services.ProjectInfo
{
    public class ProjectbaseInfoService:BaseServices<ProjectbaseinfoEntity>, IProjectbaseInfoServices
    {
        private readonly IProjectbaseInfoRepository _projectbaseInfoRepository;
        private readonly IRepositoryWrapper _repositoryWrapper;
        public ProjectbaseInfoService(IRepositoryWrapper repositoryWrapper)
        {
            _projectbaseInfoRepository = repositoryWrapper.projectbaseInfoRepository;
            base.baseDal = _projectbaseInfoRepository;
            _repositoryWrapper = repositoryWrapper;
        }

        public PageList<ProjectbaseinfoEntity> GetProjects(ProjectQueryParameters projectQueryParameters)
        {
            Expression<Func<ProjectbaseinfoEntity, bool>> exp = (u)=> u.CREATE_DATE >= projectQueryParameters.MinCreateTime
                                                                   && u.CREATE_DATE <= projectQueryParameters.MaxCreateTime 
                                                                   && u.DEL_FLAG != "1";
          
            
            return FindByExpress(exp) //日期筛选
              .SearchByID(projectQueryParameters.projectId)
              .OrderByQuery(projectQueryParameters.OrderBy)     //排序
              .ToPageList(projectQueryParameters.pageNmuber, projectQueryParameters.PageSize); //分页
            
        }

        /// <summary>
        /// EF 相同数据不同版本
        /// </summary>
        /// <param name="projectQueryParameters"></param>
        /// <returns></returns>
        public PageList<ProjectbaseinfoEntity> GetProjects2(ProjectQueryParameters projectQueryParameters)
        {
            return FindByExpress(
              u => u.CREATE_DATE >= projectQueryParameters.MinCreateTime
              && u.CREATE_DATE <= projectQueryParameters.MaxCreateTime && u.DEL_FLAG != "1" && u.ID == projectQueryParameters.projectId) //日期筛选
                .OrderByDescending(p=>p.CREATE_DATE)
              .OrderByQuery(projectQueryParameters.OrderBy)     //排序
              .ToPageList(projectQueryParameters.pageNmuber, projectQueryParameters.PageSize); //分页
            //.SearchByAccount(projectQueryParameters.Account)  //字段搜索
        }
    }
}
