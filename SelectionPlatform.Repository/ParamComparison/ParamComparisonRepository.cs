using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository.ParamComparison;
using SelectionPlatform.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.ParamComparison
{
    public class ParamComparisonRepository : BaseRepository<SelectionPlatform.Models.Models.ParamComparison>, IParamComparisonRepository
    {
        public ParamComparisonRepository(MysqlDbContext dbContext) : base(dbContext)
        {
        }
    }
}
