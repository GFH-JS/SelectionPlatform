using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository.City;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository.City
{
    public class CityRepsoitory:BaseRepository<CityEntity>,ICityRepository
    {
        public CityRepsoitory(MysqlDbContext mysqlDbContext) : base(mysqlDbContext) 
        {

        }
    }
}
