﻿using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IRepository.ProjectInfo
{
    public interface IProjectInfoRepository:IMongoBaseRepository<ProjectInfoEntity>
    {
       
    }
}
