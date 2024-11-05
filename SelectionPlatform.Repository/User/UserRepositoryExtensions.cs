using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.User
{
    public static class UserRepositoryExtensions
    {
        public static IQueryable<UserEntity> SearchByAccount(this IQueryable<UserEntity> users,string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                return users;
            }

            return users.Where(u=>u.Account.ToLower().Contains(account.ToLower()));
        }
    }
}
