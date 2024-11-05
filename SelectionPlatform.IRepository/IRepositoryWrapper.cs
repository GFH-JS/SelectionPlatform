using SelectionPlatform.IRepository.City;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IRepository.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IRepository
{
    public interface IRepositoryWrapper
    {
        IUserRepository userRepository { get; }   //所有服务类
        IProjectbaseInfoRepository projectbaseInfoRepository { get; }
        ICityRepository cityRepository { get; }
        Task<int> SaveChangesAsync();
       
    }
}
