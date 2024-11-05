using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.City;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IRepository.User;
using SelectionPlatform.Repository.City;
using SelectionPlatform.Repository.ProjectInfo;
using SelectionPlatform.Repository.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private MysqlDbContext _mysqlDbContext; //同一个上下文
        public RepositoryWrapper(MysqlDbContext mysqlDbContext) { 
        
            _mysqlDbContext = mysqlDbContext;
        }

        private IUserRepository _userRepository;
        private IProjectbaseInfoRepository _projectbaseInfoRepository;
        private ICityRepository _cityRepository;

        public IUserRepository userRepository { get { 
            return _userRepository ??= new UserRepository(_mysqlDbContext);
            } }


        public IProjectbaseInfoRepository projectbaseInfoRepository
        {
            get
            {
                return _projectbaseInfoRepository ??= new ProjectbaseinfoRepository(_mysqlDbContext);
            }
        }

        public ICityRepository cityRepository
        {
            get
            {
                return _cityRepository ??= new CityRepsoitory(_mysqlDbContext);
            }
        }


        public Task<int> SaveChangesAsync()
        {
            
            return _mysqlDbContext.SaveChangesAsync();
        }
    }
}
