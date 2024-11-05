using SelectionPlatform.IRepository.City;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IServices.City
{
    public interface ICityServices :ICityRepository
    {
        List<CityEntity> GetAllCountry();
    }
}
