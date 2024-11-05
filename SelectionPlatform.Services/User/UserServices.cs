using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.User;
using SelectionPlatform.IServices.User;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services.User
{
    public class UserServices:BaseServices<UserEntity>,IUserServices   //泛型类实现 
    {
        private readonly IUserRepository _userRepository;
        public UserServices(IRepositoryWrapper repositoryWrapper)    ///IUserRepository userRepository
        {
            this._userRepository = repositoryWrapper.userRepository;
            base.baseDal = repositoryWrapper.userRepository;     ///
        }

        public UserEntity GetUserByAccount(string account)
        {
            return FindByExpress((s) => s.Account == account).FirstOrDefault();
        }

        public PageList<UserEntity> GetUsers(UserQueryParameters userQueryParameters)
        {
            return _userRepository.GetUsers(userQueryParameters);
        }
    }
}
