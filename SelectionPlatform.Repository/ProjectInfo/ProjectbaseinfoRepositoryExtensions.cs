using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.ProjectInfo
{
    public static class ProjectbaseinfoRepositoryExtensions
    {
        public static IQueryable<ProjectbaseinfoEntity> SearchByID(this IQueryable<ProjectbaseinfoEntity> projs, string proid)
        {
            if (string.IsNullOrWhiteSpace(proid))
            {
                return projs;
            }

            return projs.Where(u => u.ID.ToLower().Contains(proid.ToLower()));
        }
    }
}
