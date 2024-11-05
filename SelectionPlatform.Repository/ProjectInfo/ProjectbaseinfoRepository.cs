using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.ProjectInfo
{
    public class ProjectbaseinfoRepository:BaseRepository<ProjectbaseinfoEntity>,IProjectbaseInfoRepository
    {
        public ProjectbaseinfoRepository(MysqlDbContext mysqlDbContext) : base(mysqlDbContext)
        {

        }
    }
}
