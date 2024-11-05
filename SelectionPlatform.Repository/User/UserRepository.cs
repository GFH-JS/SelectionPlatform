using Microsoft.EntityFrameworkCore;
using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.User;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using SelectionPlatform.Respository;
using SelectionPlatform.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.User
{
    public class UserRepository:BaseRepository<UserEntity>,IUserRepository
    {
        public UserRepository(MysqlDbContext mysqlDbContext):base(mysqlDbContext)
        {
            
        }


        public PageList<UserEntity> GetUsers(UserQueryParameters userQueryParameters)
        {
            return  FindByExpress(
                u=>u.CreateTime.Date >= userQueryParameters.MinCreateTime.Date
                && u.CreateTime.Date <= userQueryParameters.MaxCreateTime.Date) //日期筛选
                .SearchByAccount(userQueryParameters.Account)  //字段搜索
                .OrderByQuery(userQueryParameters.OrderBy)     //排序
                .ToPageList(userQueryParameters.pageNmuber, userQueryParameters.PageSize); //分页
        }
        
    }
}
