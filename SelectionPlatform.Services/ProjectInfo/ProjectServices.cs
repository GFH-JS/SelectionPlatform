using MongoDB.Bson;
using MongoDB.Driver;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IServices.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using SelectionPlatform.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services.ProjectInfo
{
    public class ProjectServices:MongoBaseServices<ProjectInfoEntity>,IProjectServices
    {
        private IProjectInfoRepository _repository;
        public ProjectServices(IProjectInfoRepository projectInfoRepository) :base(projectInfoRepository) 
        {
            _repository = projectInfoRepository;
        }

        public async Task<PageList<ProjectInfoEntity>> GetProjects(ProjectQueryParameters projectQueryParameters)
        {
        
            if (projectQueryParameters.ShowLatest)
            {
                var pipline = new BsonDocument[] {

                   new BsonDocument("$match", new BsonDocument{
                        {"createTime",new BsonDocument
                          {
                           { "$gte",projectQueryParameters.MinCreateTime},
                           { "$lte",projectQueryParameters.MaxCreateTime}
                          }
                        },
                    }),           
                  new BsonDocument("$sort",new BsonDocument{

                { "projectId",1},
                { "createTime",-1}
            }),
                            new BsonDocument("$group",new BsonDocument
            {
               { "_id","$projectId"},
               { "latestRecord",new BsonDocument("$first","$$ROOT")},

            }),
                            new BsonDocument("$replaceRoot",new BsonDocument("newRoot","$latestRecord"))

            };

                var projlist = await FindByBsonDocument(pipline);
                return projlist.AsQueryable()
                .OrderByQuery(projectQueryParameters.OrderBy)
                .ToPageList(projectQueryParameters.pageNmuber, projectQueryParameters.PageSize);
            }

            var prolist = await FindByExpress(
                u => u.createTime >= projectQueryParameters.MinCreateTime
                && u.createTime <= projectQueryParameters.MaxCreateTime);

            return prolist.AsQueryable()
                .OrderByQuery(projectQueryParameters.OrderBy)
                .ToPageList(projectQueryParameters.pageNmuber, projectQueryParameters.PageSize);
           
        }

        public async Task<ProjectInfoEntity> GetProjectsByIdAndVersion(string proid, string version)
        {
            var builder = Builders<ProjectInfoEntity>.Filter;
            var filter = builder.And(builder.Eq("projectId", proid),builder.Eq("projectVersion", version));
            //_repository.FindByExpress(s=>s.projectId == proid);
            var result = await _repository.FindByFilter(filter);

            return result.FirstOrDefault();
        }

        public Task<List<ProjectInfoEntity>> GetProjectsByProjectId(string proid)
        {
            var builder = Builders<ProjectInfoEntity>.Filter;
            var filter = builder.And(builder.Eq("projectId", proid));
            return _repository.FindByFilter(filter);
        }

        public Task UpdateProjectInfo(string proid,UpdateProjectDto updateProjectDto)
        {
            var builder = Builders<ProjectInfoEntity>.Filter;
            var filter = builder.And(builder.Eq("projectId", proid));

            var update = Builders<ProjectInfoEntity>.Update.Set(u => u.projectId, updateProjectDto.ProjectName);

            return _repository.Update(filter, update);
        }

        public async Task<List<string>> GetProjectAllVersionById(string proid)
        {
            List<string> version = new List<string>();
            var projs = await GetProjectsByProjectId(proid);

            foreach (var item in projs)
            {
                version.Add(item.projectVersion);
            }

            return version;
        }
    }
}
