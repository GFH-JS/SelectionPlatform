using MongoDB.Bson;
using MongoDB.Driver;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using SelectionPlatform.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.ProjectInfo
{
    public class ProjectInfoRepository:MongoBaseRepository<ProjectInfoEntity>,IProjectInfoRepository
    {
        public ProjectInfoRepository(IMongoDatabase mongoDatabase) :base(mongoDatabase)
        { }
    }
}
