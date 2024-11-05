using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.City;
using SelectionPlatform.IServices.City;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services.City
{
    public class CityServices:BaseServices<CityEntity>,ICityServices
    {
     
        public CityServices(IRepositoryWrapper repositoryWrapper)
        {
            base.baseDal = repositoryWrapper.cityRepository;
        }


        public List<CityEntity> GetAllCountry()
        {
            return FindByExpress(c=>c.level=="1").ToList();
        }
    }
}
